# GardenAdvisor — Agent Instructions

## Project structure

- `GardenAdvisor.API/` — .NET 10 API (no `.sln` at repo root; use `working-directory: GardenAdvisor.API` for all `dotnet` commands in CI)
- `garden-advisor-ui/` — React/TypeScript frontend (npm workspace)

## Compound learnings

| Date | Category | What happened | Prevention |
|---|---|---|---|
| 2026-07-24 | CI / working-directory | `dotnet restore` and `dotnet build` in both `ci.yml` and `copilot-setup-steps.yml` ran from repo root, failing with "no project or solution file" because no `.sln` exists at root. | Always set `working-directory: GardenAdvisor.API` for `dotnet` steps. |
| 2026-07-24 | Actions / Node runtime | `actions/checkout@v4`, `setup-node@v4`, `setup-dotnet@v4`, `docker/login-action@v3`, `docker/build-push-action@v5`, `azure/login@v2` all use `node20` which is EOL and removed from runners 2026-09-16. `actions/github-script@v9` is still `node20` — no node24 version yet. | Node24 floors: checkout→v5, setup-node→v6, setup-dotnet→v6, docker/login→v4, docker/build-push→v7, azure/login→v3. Audit with `grep -rnE 'uses:' .github/workflows/`. |
| 2026-07-24 | CI / UI tests | `Build and Test UI` fails with `SyntaxError: Unexpected token 'export'` in react-leaflet — ESM-only package, Jest not configured to transform it. Pre-existing since at least July 2026. | Fix: add `transformIgnorePatterns` override in Jest config or upgrade to Jest ESM mode. Separate issue from action version upgrades. |
