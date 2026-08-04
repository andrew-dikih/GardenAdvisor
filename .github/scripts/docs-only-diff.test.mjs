import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import path from 'node:path';

import { classifyDiff, ALLOWLIST, GITHUB_PATH_FILTER_FILE_LIMIT } from './docs-only-diff.mjs';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const workflowsDir = path.join(__dirname, '..', 'workflows');

function readWorkflow(name) {
  return readFileSync(path.join(workflowsDir, name), 'utf8');
}

// Extract the literal list under a `paths-ignore:` block. Deliberately a
// simple line-based scan (not a YAML parser) since we control the exact
// formatting we write; this is a drift detector, not a general YAML tool.
function extractPathsIgnore(yamlText) {
  const lines = yamlText.split(/\r?\n/);
  const start = lines.findIndex((l) => l.trim() === 'paths-ignore:');
  assert.ok(start !== -1, 'expected a paths-ignore: block');
  const entries = [];
  for (let i = start + 1; i < lines.length; i++) {
    const line = lines[i];
    const match = line.match(/^\s*-\s*'([^']+)'\s*$/);
    if (!match) break;
    entries.push(match[1]);
  }
  return entries;
}

// --- classifyDiff behavior -------------------------------------------------

test('docs-only: every changed file is on the allowlist', () => {
  assert.equal(classifyDiff(['README.md']), 'docs-only');
  assert.equal(classifyDiff(['garden-advisor-ui/README.md']), 'docs-only');
  assert.equal(classifyDiff([...ALLOWLIST]), 'docs-only');
});

test('requires-automation: a single non-doc source file', () => {
  assert.equal(classifyDiff(['GardenAdvisor.API/Program.cs']), 'requires-automation');
});

test('requires-automation: workflow file changes', () => {
  assert.equal(classifyDiff(['.github/workflows/ci.yml']), 'requires-automation');
});

test('requires-automation: a markdown file NOT on the explicit allowlist', () => {
  // New docs paths are not implicitly safe -- they must be added deliberately.
  assert.equal(classifyDiff(['docs/architecture.md']), 'requires-automation');
  assert.equal(classifyDiff(['GardenAdvisor.API/NOTES.md']), 'requires-automation');
});

test('requires-automation: mixed docs + non-docs diff (no partial credit)', () => {
  assert.equal(
    classifyDiff(['README.md', 'GardenAdvisor.API/Program.cs']),
    'requires-automation',
  );
});

test('indeterminate: empty file list', () => {
  assert.equal(classifyDiff([]), 'indeterminate');
});

test('indeterminate: null/undefined input', () => {
  assert.equal(classifyDiff(null), 'indeterminate');
  assert.equal(classifyDiff(undefined), 'indeterminate');
});

test('indeterminate: explicitly-flagged truncated listing', () => {
  assert.equal(classifyDiff(['README.md'], { truncated: true }), 'indeterminate');
});

test('indeterminate: file count at/above GitHub\'s 3,000-file path-filter limit', () => {
  const huge = Array.from({ length: GITHUB_PATH_FILTER_FILE_LIMIT }, () => 'README.md');
  assert.equal(classifyDiff(huge), 'indeterminate');

  const justUnder = Array.from({ length: GITHUB_PATH_FILTER_FILE_LIMIT - 1 }, () => 'README.md');
  assert.equal(classifyDiff(justUnder), 'docs-only');
});

// --- drift detection against the actual workflow YAML ----------------------

test('ci.yml paths-ignore matches ALLOWLIST exactly', () => {
  const entries = extractPathsIgnore(readWorkflow('ci.yml'));
  assert.deepEqual(entries, [...ALLOWLIST]);
});

test('deploy.yml paths-ignore matches ALLOWLIST exactly', () => {
  const entries = extractPathsIgnore(readWorkflow('deploy.yml'));
  assert.deepEqual(entries, [...ALLOWLIST]);
});

test('label-check.yml paths-ignore matches ALLOWLIST exactly', () => {
  const entries = extractPathsIgnore(readWorkflow('label-check.yml'));
  assert.deepEqual(entries, [...ALLOWLIST]);
});

// --- trigger-block shape sanity (guards against runner-consuming skip jobs) -

test('ci.yml / deploy.yml / label-check.yml do not gate jobs with a skip-job pattern', () => {
  // The contract requires trigger-level filtering only -- no job that always
  // runs just to report success/no-op for docs-only diffs.
  for (const name of ['ci.yml', 'deploy.yml', 'label-check.yml']) {
    const text = readWorkflow(name);
    assert.ok(
      !/changed-files|dorny\/paths-filter|tj-actions\/changed-files/i.test(text),
      `${name} should rely on native paths-ignore, not a runner-consuming changed-files action`,
    );
  }
});
