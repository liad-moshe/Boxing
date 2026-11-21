# ADR-0002: CI actions and versions

- Status: Accepted
- Date: 2026-11-21

## Context
CI (.github/workflows/ci.yml) and releases (.github/workflows/release.yml) run on GitHub-hosted runners,
which are free for this public repo. All CI logic lives inside the workflow files. There are no composite
actions or helper scripts, because files outside .github/workflows can be written with the agents' token.

## Decision: actions, pinned to their current major tag
Checked 2026-11-21 with `gh release view -R <repo>`. Each major tag was confirmed to exist with
`git ls-remote --tags`.

| Action | Latest release | Pinned as | Inputs we use |
|---|---|---|---|
| actions/checkout | v7.0.1 (2026-07-20) | `@v7` | `persist-credentials: false` |
| actions/setup-dotnet | v6.1.0 (2026-11-03) | `@v6` | `global-json-file: global.json` (SDK 10.0.202) |
| actions/upload-artifact | v7.0.2 (2026-10-07) | `@v7` | `path`, `retention-days: 14`, `archive: false` (uploads our zip as-is, no zip-in-zip), `if-no-files-found: error` |
| chickensoft-games/setup-godot | v2.4.3 (2026-10-01) | `@v2` | `version` (read from global.json), `use-dotnet: true`, `include-templates: true` |

setup-godot v2.4.3 `action.yml` inputs (read at the tag): `version` (required; also accepts `global`),
`path`, `downloads-path`, `bin-path`, `godot-sharp-release`, `use-dotnet` (default true),
`include-templates` (default false), `cache` (default true). It puts `godot` on PATH and sets
`GODOT`/`GODOT4`.

## Other choices
- **Runner:** `ubuntu-24.04`, pinned rather than `ubuntu-latest`, so an image upgrade is a deliberate
  change. The Windows export is cross-built on Linux. Fallback if that ever fails: a `windows-latest`
  runner for the export step (free on public repos), only with client approval.
- **Godot version** comes from `global.json` → `msbuild-sdks` → `Godot.NET.Sdk`, read with `sed` into a step
  output, so global.json stays the single source of truth.
- **SDK version in Boxing.csproj too:** setup-godot's README suggests leaving the version out of
  `Sdk="Godot.NET.Sdk"` when global.json pins it. Our spec pins it in both places. The local build is
  clean with both (0 warnings), so we keep both. If they ever disagree, the build tells us.
- **Permissions:** the workflow default is `contents: read`. Only `governance` gets
  `pull-requests: write`, and it never checks out PR code. `client-approval` has `permissions: {}`. Only
  release.yml's release job gets `contents: write`.
- **Tag pinning vs SHA pinning:** we pin major tags (spec). Upgrading to a new major is a new ADR or an
  update to this one.

## Consequences
- A new major version of any action needs a deliberate PR that edits the workflows and this ADR. Workflow
  edits are protected paths, so the client makes them, or a session the client starts with
  `--setting-sources user`.
- The PAT on the client's machine has no Workflows permission, so agents can't change CI.
