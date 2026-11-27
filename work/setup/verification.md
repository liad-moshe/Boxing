# Stage 7 — Verification results (2026-11-27)

## A. Configuration

### A1. Agent/model table (from .claude/agents/*.md frontmatter; main model from .claude/settings.json)
| Agent | model | tools | maxTurns | preloaded skills | path-guard write paths | Spec §3 |
|---|---|---|---|---|---|---|
| (main) Producer | sonnet (`settings.json` "model") | — | — | — | (permission deny rules + bash-guard) | match |
| game-designer | sonnet | Read, Grep, Glob, Edit, Write | 25 | boxing-sources | docs/specs/ data/balance/ work/ | match |
| boxing-consultant | sonnet | Read, Grep, Glob, Edit, Write, WebSearch, WebFetch | 30 | boxing-sources | docs/boxing/ work/ | match |
| developer | sonnet | Read, Grep, Glob, Edit, Write, Bash, WebFetch | 60 | godot-csharp-api | src/ game/ tests/ data/ docs/decisions/ work/ (+ Stop build-gate) | match |
| qa | sonnet | Read, Grep, Glob, Edit, Write, Bash, WebFetch | 40 | godot-csharp-api | tests/ work/ | match |
| kb-keeper | haiku | Read, Grep, Glob, Edit, Write | 15 | kb-update | docs/kb/ | match |
| tech-artist | sonnet | Read, Grep, Glob, Edit, Write, Bash, WebFetch | 50 | godot-csharp-api | game/art/ game/shaders/ src/Procedural/ docs/art/ work/ | match |

env: CLAUDE_CODE_DISABLE_EXPLORE_PLAN_AGENTS=1, CLAUDE_CODE_EXPERIMENTAL_AGENT_TEAMS=0 (match).
Note: this verification session itself ran on claude-opus-5-5 (picked in the desktop app model picker,
which overrides settings.json). The SessionStart hook warned about it correctly.

## Results
| # | Check | Expected | Observed | Result |
|---|---|---|---|---|
| A1 | Agent/model table vs spec §3 | all match | all 7 rows match | PASS |
| A2 | `gh auth status` shows fine-grained token | fine-grained PAT | agent cannot run it: bash-guard blocks `gh auth` (by design) → client runs it (C) | PENDING (client) |
| B3 | Edit tool on .claude/settings.json | denied | "File is in a directory that is denied by your permission settings." | PASS |
| B4 | `echo test >> CLAUDE.md` | hook block | bash-guard: "redirecting output into a protected path" | PASS |
| B5 | `git push origin HEAD:main` | hook/deny block | bash-guard: "pushing to main/master is not allowed" | PASS |
| B6 | `gh api user` | blocked | bash-guard: "this gh command is reserved for the client" | PASS |
| B7a | qa subagent `gh pr list` | subagent guard block | bash-guard: "subagent 'qa' may not use git push or gh; only the Producer talks to GitHub" | PASS |
| B7b | qa subagent Write src/x.cs | path-guard block | path-guard: "src/x.cs is outside this agent's write paths (tests/ work/)" | PASS |
| B7c | `git status` after B7 | clean, no src/x.cs | clean (one transient `.claude/settings.local.json.tmp.*` from the app's atomic write of settings.local.json, gone seconds later) | PASS |
| B8 | developer adds [Obsolete] call in Main.cs on scratch/build-gate | build-gate forces fix or BLOCKED | used `AnimationPlayer.AnimationProcessCallback.Idle`; Stop build-gate ran `dotnet build Boxing.sln -warnaserror` → CS0618; developer removed the call; Producer re-check: `git diff` empty, build 0 warnings / 0 errors; branch deleted | PASS |
| D1 | grep tracked files for absolute paths, "tzone", tokens | none | only placeholders (`C:/path/to/godot.exe` in SETUP.md + settings.local.example.json), a "D:/ vs /d/" comment in studio-hook.sh, and `res://` Godot paths; no "tzone" outside this work/ file; no token patterns | PASS |
| D2 | `git check-ignore -v` local files | both ignored | `.gitignore:20 .claude/settings.local.json`, `.gitignore:21 CLAUDE.local.md` | PASS |
