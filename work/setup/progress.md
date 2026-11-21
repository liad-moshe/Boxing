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
- [ ] Switch all 3 projects to net10.0 once an export with net10.0 succeeds (local templates or CI); update ADR-0001 table
- [x] Client machine: Godot editor Indent Type = Spaces (set 2026-11-21, verified: import leaves Main.cs unchanged) (else headless runs rewrite game/scripts/Main.cs with tabs)

## Stage 3 — Claude Code environment
- [x] 0. Read official docs: sub-agents, skills, hooks, settings, permissions (+ commands for name clashes)
- [x] A. .claude/hooks/studio-hook.sh (bash-guard, path-guard, session-context, build-gate) + ≥20-case test table
- [x] B. .claude/agents/*.md (6 agent files: frontmatter per spec §3, path-guard hooks, developer build-gate)
- [x] C. .claude/skills/*/SKILL.md (task, resume-task [renamed from "resume": built-in /resume clash, client chose 2026-11-21], inbox, ship, plan-milestone, playtest-feedback,
      godot-csharp-api, boxing-sources, kb-update)
- [x] D. .claude/settings.json + .claude/settings.local.example.json
- [x] E. Verification table (ALL PASS) (agent → name → model → tools → maxTurns vs spec §3) + skills list

## Stage 4
- [ ] (sub-items added when the Stage 4 prompt is pasted)

## Stage 5
- [ ] (sub-items added when the Stage 5 prompt is pasted)

## Stage 6
- [ ] (sub-items added when the Stage 6 prompt is pasted)

## Stage 7
- [ ] (sub-items added when the Stage 7 prompt is pasted)
