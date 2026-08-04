# Docs-only CI/CD gating -- contract and known limitations

This document is the source of truth for the "docs-only diffs skip CI/CD"
behavior implemented via `paths-ignore` in `ci.yml`, `deploy.yml`, and
`label-check.yml`. Read it before adding a file to, or removing one from, the
passive-doc allowlist.

## The allowlist (exhaustive)

```
README.md
garden-advisor-ui/README.md
```

These are the **only** two Markdown files in this repository today. Each was
verified not to be copied into a deployable build artifact:

- `GardenAdvisor.API/Dockerfile` only `COPY`s `GardenAdvisor.API/` before
  `dotnet publish`; `dotnet publish` does not bundle the repo-root
  `README.md`.
- `garden-advisor-ui/Dockerfile` does `COPY . .` into its build stage (so
  `garden-advisor-ui/README.md` *is* present during `npm run build`), but
  Create React App's build only emits `src/` + `public/` output into
  `build/`; a project-root `README.md` is never emitted into that output.

**Nothing else is docs-only by default.** In particular, out of scope for the
allowlist (i.e. automation-required even though the diff "looks like docs"):

- Anything under `.github/**` (workflows, this file, the PR template, issue
  templates) -- these are automation-defining files, not passive prose.
- Any *new* Markdown file at any other path (e.g. a future `docs/**`
  directory, a `CHANGELOG.md`, or a Markdown file inside
  `GardenAdvisor.API/` or `garden-advisor-ui/src/`) until it has been
  reviewed with the same "does this end up in a deployable artifact, or does
  any pipeline read it operationally?" question and explicitly added here
  *and* to all three workflows' `paths-ignore` lists (kept in sync and
  enforced by `.github/scripts/docs-only-diff.test.mjs`).
- Any mixed diff -- if a PR/push touches even one file outside the
  allowlist, **the entire diff is treated as automation-required**. There is
  no partial-credit "mostly docs" classification.

## How the gating actually works

Each of `ci.yml`, `deploy.yml`, and `label-check.yml` uses GitHub's native
`paths-ignore:` trigger filter -- **not** a job that runs and reports
success/no-op. This is a deliberate constraint: a "skip job" still consumes a
GitHub-hosted runner (even if only for a few seconds), which violates the
"docs-only diffs cost zero runner minutes" requirement. `paths-ignore`
evaluation happens entirely on GitHub's side before any runner is claimed.

`paths-ignore` fails closed by construction: GitHub skips the workflow only
when *every* changed file in the diff matches an ignored pattern. One
non-matching file is enough to make the whole workflow run normally.

## GitHub's documented path-filter limitation (>=3,000 files)

Per GitHub's own docs ([Workflow syntax for GitHub Actions --
"Git diff comparisons"](https://docs.github.com/en/actions/reference/workflows-and-actions/workflow-syntax#git-diff-comparisons)),
`paths`/`paths-ignore` evaluation for `push`/`pull_request` triggers has
documented limits:

- If a push contains more than 1,000 commits, the workflow will **always**
  run.
- If generating the diff times out, the workflow will **always** run.
- **If the generated diff contains more than 3,000 files and the file(s) the
  workflow filter matches are not in the first 3,000 files returned by the
  filter, the workflow will NOT run.**

The first two cases are safe over-triggers (automation runs when it maybe
didn't need to). The third is **not** a guaranteed fail-safe: at that scale,
GitHub's own path-filter evaluation can under-trigger and silently skip a
workflow that should have run, because it never saw the non-doc file past
the 3,000-file cutoff. This repo's automation does not claim, and this
document does not claim anywhere, that `paths-ignore` is unconditionally
fail-closed once a diff crosses that size -- it is fail-closed only below the
limit.

`.github/scripts/docs-only-diff.mjs`'s `classifyDiff()` reflects this
directly: any diff at or beyond 3,000 changed files, or any diff whose file
listing is known to be incomplete/truncated, is classified `indeterminate`
-- **never** `docs-only`, and never asserted as a confident
`requires-automation` either, since the classifier cannot see enough of the
diff to know. Treat `indeterminate` as **non-success** for any
decision-making purpose (i.e. do not proceed as if the diff were verified
docs-only, and do not assume GitHub's trigger behaved correctly).

**Operational guidance if a change ever approaches or exceeds this size:**

- **Prefer splitting** the oversized change into smaller PRs/pushes before it
  reaches `main`/`develop`, so no single diff ever nears the 3,000-file
  threshold.
- If an oversized diff has already reached `main` (e.g. a large merge or
  history import), **treat the release as frozen**: do not assume CI/CD ran
  (or was correctly skipped) based on `paths-ignore` alone. Compute the full
  local diff (`git diff --name-only <base>...<head>` with no truncation) to
  classify it yourself, and **manually validate and/or manually dispatch**
  the affected workflows (`workflow_dispatch` where available, or
  `gh workflow run`) rather than trusting the automatic trigger result.

## Branch-protection caveat (read before relying on this for merges)

Required status checks configured on `main`/`develop` branch protection are
evaluated by whether GitHub ever *receives* a status/check-run report for
those exact context names -- not by whether the workflow was "supposed to"
run. If a workflow is skipped by `paths-ignore` (as designed for docs-only
diffs), its job(s) never report a status at all, and any branch-protection
"required" context tied to that workflow will show as
**"Expected — Waiting for status to be reported"** and can block merging
indefinitely (this repo also has `enforce_admins: true` on both protected
branches, meaning even a repo admin cannot bypass this via the merge button).

`main`'s required contexts are `Build and Test`, `Frontend Build`, and
`Check for release label`; `develop`'s are `Build and Test` and
`Frontend Build`. As of this change, the `ci.yml` job `name:` fields were
renamed to match exactly (`build-api` -> `Build and Test`,
`build-and-test-ui` -> `Frontend Build`), fixing a pre-existing mismatch
(previously `Build API` / `Build and Test UI`) without changing what branch
protection enforces. `label-check.yml`'s job was already named
`Check for release label`, matching `main`'s third required context.

This does **not** eliminate the underlying caveat: a genuine docs-only PR
still causes these jobs to be skipped (by design, to cost zero runner
minutes), so their required contexts still won't report for that PR, and it
will still show "Expected — waiting for status to be reported" and be
blocked from merging through the normal PR UI. Fixing the name mismatch only
ensures that on any PR where these jobs *do* run, they satisfy the exact
required-context names branch protection expects. Whether/how to let
docs-only PRs merge despite required-but-skipped checks (e.g. reconfiguring
which contexts are required, or accepting that docs-only PRs must be merged
via a path that tolerates pending checks) is a branch-protection policy
decision explicitly left to the repository administrator/coordinator; this
change intentionally does **not** modify branch protection, rulesets, or
required-check configuration -- that requires an admin-scoped token
(`GH_SETUP_TOKEN`, see `setup-repository.yml`) and is out of scope here.

## Testing

`.github/scripts/docs-only-diff.mjs` implements the classification contract
above as plain code, and `.github/scripts/docs-only-diff.test.mjs` (run via
`node --test .github/scripts/docs-only-diff.test.mjs`) both exercises that
logic against representative diffs (pure docs, pure source, mixed,
oversized/indeterminate, truncated) and asserts the three workflow files'
literal `paths-ignore:` lists stay byte-for-byte in sync with the allowlist,
so a future edit to one without the other is caught by CI rather than
silently drifting.
