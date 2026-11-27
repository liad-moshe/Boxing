# Setup progress — agentic environment

Branch: `setup/agentic-env`. Spec: docs/process/environment-spec.md.
Rule: at the start of a stage, read this file. Complete stage → report and stop. Partial stage →
continue from the first unchecked item. Tick, commit ("setup(stage N): …") and push after each item.

Stage prompts are pasted one at a time. Sub-items for Stages 2–7 are filled in from each stage prompt
when it is pasted, so this file never guesses at future work.

## Stage 0 — Preflight (read-only)
- [x] Tool versions: git 2.46.0, gh 2.97.0, claude 2.1.329, dotnet 10.0.202
- [x] godot --version = 4.7.2.stable.mono (after an app restart cleared a stale PATH)
- [x] Repo: remote, clean tree, only README.md, PUBLIC, default branch main
- [x] gh auth: liad-moshe with repo + workflow scopes
- [x] git identity set, noreply email
- [x] opus, sonnet and haiku all selectable
- [x] No other project path in ~/.claude/CLAUDE.md
- [x] Bash tool is Git Bash (msys)

## Stage 1 — Foundation files
- [x] 1. Branch setup/agentic-env + this progress file (pushed with -u)
- [x] 2. docs/process/environment-spec.md (MASTER CONTEXT verbatim)
- [x] 3. .gitignore
- [x] 4. .gitattributes
- [x] 5. Docs skeletons
  - [x] docs/vision.md (DRAFT)
  - [x] docs/roadmap.md (M1, M2, M1 skips)
  - [x] docs/process/workflow.md
  - [x] docs/process/routing.md
  - [x] docs/process/failure-playbook.md
  - [x] docs/process/gates.md
  - [x] docs/boxing/sources.md + docs/boxing/README.md
  - [x] docs/kb/feature-map.md, input-map.md, ui-map.md
  - [x] docs/specs/README.md (spec template)
  - [x] docs/decisions/README.md + ADR-0001-engine-and-versions.md
  - [x] docs/art/style-guide.md
  - [x] docs/licenses/README.md
- [x] 6. CLAUDE.md (Producer manual, < 150 lines)
- [x] 7. SETUP.md
- [x] 8. README.md overview

## Stage 2 — Engine skeleton
- [x] 1. Root files: global.json, Directory.Build.props, Directory.Packages.props, Boxing.sln
- [x] 2. Boxing.csproj (Godot.NET.Sdk/4.7.2, excludes src/** tests/**, refs Core) + TargetFramework in ADR-0001
- [x] 3. src/Core/Boxing.Core.csproj + RoundClock + .gdignore in src/ and tests/
- [x] 4. tests/Core.Tests (xUnit, 3 RoundClock tests)
- [x] 5. project.godot by hand + `godot --headless --import --path .` clean
- [x] 6. game/scenes/Main.tscn + game/scripts/Main.cs
- [x] 7. game/scenes/smoke/Smoke.tscn + Smoke.cs
- [x] 8. Local checks pass + docs/process/commands.md (+ SETUP.md §6 smoke line)
- [x] 9. export_presets.cfg ("Windows Desktop") — templates not installed locally; preset validated (fails only on missing templates); CI proves export
- [x] 10. docs/licenses/godot-LICENSE.txt
- [x] 11. docs/kb entries for Main (quit action, Main scene)

## Carry-overs (open)
- [x] Switched all 3 projects to net10.0 (2026-11-21): CI run 44953758554 green incl. Windows export; zip bundles .NET runtime 10.0.15; ADR-0001 updated
- [x] Client machine: Godot editor Indent Type = Spaces (set 2026-11-21, verified: import leaves Main.cs unchanged) (else headless runs rewrite game/scripts/Main.cs with tabs)

## Stage 3 — Claude Code environment
- [x] 0. Read official docs: sub-agents, skills, hooks, settings, permissions (+ commands for name clashes)
- [x] A. .claude/hooks/studio-hook.sh (bash-guard, path-guard, session-context, build-gate) + ≥20-case test table
- [x] B. .claude/agents/*.md (6 agent files: frontmatter per spec §3, path-guard hooks, developer build-gate)
- [x] C. .claude/skills/*/SKILL.md (task, resume-task [renamed from "resume": built-in /resume clash, client chose 2026-11-21], inbox, ship, plan-milestone, playtest-feedback,
      godot-csharp-api, boxing-sources, kb-update)
- [x] D. .claude/settings.json + .claude/settings.local.example.json
- [x] E. Verification table (ALL PASS) (agent → name → model → tools → maxTurns vs spec §3) + skills list

## Stage 4 — Workflows
- [x] 0. Action versions checked (gh release view) + setup-godot action.yml read at its tag
- [x] 0b. docs/decisions/ADR-0002-ci-versions.md
- [x] 1. .github/workflows/ci.yml (build-test, kb-check, governance, client-approval)
- [x] 2. .github/workflows/release.yml (playtest-N prerelease, keep newest 5)
- [x] 3. Commit + push (full gh login with workflow scope)
- [x] 4. PR #1 setup/agentic-env → main "Agentic studio environment (setup)" (CLIENT BRIEFING body)
- [x] 5. CI green: build-test, kb-check, governance, client-approval all pass (run 44948729756; client-approval passed without waiting: environment auto-created with no reviewer until Stage 5)
- [x] 6. Artifact Boxing-windows-37036af.zip verified: Boxing.exe (PE32+ GUI), Boxing.pck, data_Boxing_windows_x86_64/ (187 files incl. Boxing.dll, Boxing.Core.dll, GodotSharp.dll, self-contained net8.0 runtime), godot-LICENSE.txt

## Stage 5 — GitHub settings (each change shown first, applied only after the client's "yes")
- [x] 1. Labels (17, gh label create --force) — created and verified
- [x] 2. Repo merge settings (squash only, delete branch on merge, no auto-merge) — applied and re-read
- [x] 3. Actions settings (default workflow permissions read; fork-PR approval for all external contributors) — applied and re-read
- [x] 4. Environment client-approval (reviewer liad-moshe, prevent_self_review false, can_admins_bypass false) — reviewer via API; admin bypass unticked by the client in the UI (field not in the REST docs); re-read OK
- [x] 5. GitHub Actions app id read from a check run on the PR head: 15368 (github-actions), commit 7ea333b
- [x] 6. Ruleset protect-main (no bypass; deletion, non_fast_forward, PR squash-only, strict required checks) — id 28200762, active
- [x] 7. PR checks re-ran (push 8400364, run 45912170910): client-approval waiting for review (reviewer liad-moshe); PR merge state BLOCKED
- [x] 8. docs/process/github-settings.md

## Stage 6 — Lock-down
- [x] 0. Rule syntax checked against code.claude.com permissions/tools docs (/path = project root in project
      settings; Edit rules cover Write/NotebookEdit; mcp__<server>__* valid; tool name "PowerShell";
      disableBypassPermissionsMode does not affect auto mode)
- [x] 0b. Hook test suite re-run before the lock-down
- [x] 1-3. .claude/settings.json: PreToolUse Bash → bash-guard; disableBypassPermissionsMode; deny list (27 rules)
      (shown to the client, written only after "yes"; after the write, protected paths are read-only for
      every session, including the setup session)
- [ ] 4. Commit + push settings.json; verify the lock-down live (blocked edit + blocked gh api)
- [ ] 5. Client, in their own PowerShell window: fine-grained PAT → gh auth logout / login --with-token /
      setup-git / status → remove git:https://github.com from Credential Manager → restart claude → STAGE 7

## Stage 7
- [ ] (sub-items added when the Stage 7 prompt is pasted)
