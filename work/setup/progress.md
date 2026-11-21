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
- [ ] 8. README.md overview

## Stage 2
- [ ] (sub-items added when the Stage 2 prompt is pasted)
- Known carry-over: TargetFramework decision for ADR-0001 (net8.0 vs net10.0)

## Stage 3
- [ ] (sub-items added when the Stage 3 prompt is pasted)

## Stage 4
- [ ] (sub-items added when the Stage 4 prompt is pasted)

## Stage 5
- [ ] (sub-items added when the Stage 5 prompt is pasted)

## Stage 6
- [ ] (sub-items added when the Stage 6 prompt is pasted)

## Stage 7
- [ ] (sub-items added when the Stage 7 prompt is pasted)
