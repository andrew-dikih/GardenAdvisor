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

GitHub's own path-filter evaluation for `push`/`pull_request` triggers has a
hard limit: if a diff touches 3,000 files or more, GitHub cannot reliably
evaluate `paths`/`paths-ignore` against the full file list. **When that
happens, GitHub's documented fallback is to always run the workflow** -- i.e.
GitHub's own failure mode is "never wrongly skip", not "wrongly skip". This
repo's automation inherits that same posture and does not attempt to
override or second-guess it.

Do not treat this document, or `.github/scripts/docs-only-diff.mjs`, as an
unqualified guarantee that a docs-only diff will always be detected as such.
The local `classifyDiff()` helper mirrors GitHub's own fail-closed behavior:
any diff at or beyond 3,000 changed files, or any diff whose file listing is
known to be incomplete/truncated, is classified `indeterminate` -- never
`docs-only`. Treat `indeterminate` the same as `requires-automation` for any
decision-making purpose.

**Operational guidance if a change ever approaches this size:** split the
oversized change into smaller PRs/pushes before it reaches `main`/`develop`.
If an oversized diff has already reached `main` (e.g. a large merge or
history import), treat the release as frozen and manually validate/dispatch
the affected workflows (`workflow_dispatch` where available, or a manual
`gh workflow run`) rather than trusting that `paths-ignore` classified it
correctly.

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

At the time this gating was added, `main`'s required contexts are
`Build and Test`, `Frontend Build`, and `Check for release label`; `develop`'s
are `Build and Test` and `Frontend Build`. **These do not match this
repository's actual job names** (`Build API` and `Build and Test UI` in
`ci.yml`, `Check for release label` in `label-check.yml` -- the last one does
match). This mismatch predates this change and already means required
checks were not reliably satisfied before docs-only gating existed; adding
`paths-ignore` does not newly break anything that was working, but it does
mean a genuine docs-only PR will likely be unmergeable through the normal PR
UI until an administrator either updates the required-context names to match
reality, or reconfigures which checks are required for doc-only paths, or a
non-doc commit is pushed to make the real jobs run at least once. This gating
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
