// Deterministic classifier mirroring the `paths-ignore` contract enforced by
// ci.yml, deploy.yml, and label-check.yml. This is NOT consumed at workflow
// run time -- GitHub evaluates paths-ignore itself. It exists so the
// passive-doc boundary is testable and reviewable as code (see
// docs-only-diff.test.mjs) instead of only living inside YAML comments.
//
// Keep ALLOWLIST in exact sync with the `paths-ignore:` lists in:
//   - .github/workflows/ci.yml
//   - .github/workflows/deploy.yml
//   - .github/workflows/label-check.yml
// The test file asserts that sync directly by reading those YAML files.

/**
 * The full, narrow passive-doc boundary for this repo.
 * Verified NOT to be copied into any deployable build output:
 *   - GardenAdvisor.API/Dockerfile only COPYs GardenAdvisor.API/ before
 *     `dotnet publish`, which does not bundle root README.md.
 *   - garden-advisor-ui/Dockerfile COPYs the whole UI folder into the build
 *     stage, but `npm run build` (Create React App) only emits src/ + public/
 *     output into build/; garden-advisor-ui/README.md is never emitted.
 * Any file NOT in this exact list is treated as automation-required,
 * including: anything under .github/**, any other *.md file anywhere in the
 * repo (new markdown paths are NOT implicitly docs-only -- they must be
 * explicitly added here and to the workflow paths-ignore lists after the
 * same "does it end up in a deployable artifact?" review), and any non-doc
 * source/config/infra file.
 */
export const ALLOWLIST = Object.freeze(['README.md', 'garden-advisor-ui/README.md']);

/** GitHub's documented path-filter evaluation limit. */
export const GITHUB_PATH_FILTER_FILE_LIMIT = 3000;

/**
 * Classify a changed-file list the same way the passive-doc contract intends.
 *
 * @param {string[] | null | undefined} files - repo-relative changed file
 *   paths for the diff being classified (e.g. a PR's full file list, or a
 *   push's compare file list).
 * @param {{ truncated?: boolean }} [options] - set `truncated: true` when the
 *   caller knows its own file listing was cut off before it could enumerate
 *   every changed file (e.g. a paginated API call that gave up early).
 * @returns {'docs-only' | 'requires-automation' | 'indeterminate'}
 *
 * Classification rules (in order):
 *   1. Unknown/empty/truncated input, or a file count at or beyond GitHub's
 *      documented 3,000-file path-filter evaluation limit, is INDETERMINATE.
 *      Per GitHub's own docs (Workflow syntax for GitHub Actions ->
 *      "Git diff comparisons"): if a push has more than 1,000 commits, or
 *      diff generation times out, the workflow will ALWAYS run (safe,
 *      over-triggers). But if the generated diff contains more than 3,000
 *      files and the file(s) the filter would match are NOT among the first
 *      3,000 returned, the workflow will NOT run -- i.e. GitHub's own
 *      mechanism can silently under-trigger (falsely skip) in that specific
 *      case, it is not a guaranteed fail-safe. We do not claim
 *      paths-ignore is unconditionally fail-closed at this scale: our local
 *      classifier instead refuses to guess and reports INDETERMINATE
 *      whenever it cannot see (or trust) the complete file list, so callers
 *      can apply the operational response in DOCS_ONLY_CI.md (split the
 *      change, freeze + do a full local diff, or manually
 *      validate/dispatch the affected workflow) instead of assuming either
 *      "ran" or "skipped" was correct.
 *   2. If every changed file is in ALLOWLIST, it's DOCS-ONLY.
 *   3. Otherwise, REQUIRES-AUTOMATION (this includes any mix of docs +
 *      non-docs changes -- there is no partial-credit "mostly docs" case).
 */
export function classifyDiff(files, options = {}) {
  const { truncated = false } = options;

  if (truncated) return 'indeterminate';
  if (!Array.isArray(files)) return 'indeterminate';
  if (files.length === 0) return 'indeterminate';
  if (files.length >= GITHUB_PATH_FILTER_FILE_LIMIT) return 'indeterminate';

  const allDocsOnly = files.every((f) => ALLOWLIST.includes(f));
  return allDocsOnly ? 'docs-only' : 'requires-automation';
}
