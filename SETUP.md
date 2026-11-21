# Setup — new computer

Everything shared lives in this repo. This page sets up the machine-only parts. Windows, PowerShell.

## 1. Tools (pinned)
| Tool | Version | Notes |
|---|---|---|
| Git for Windows | ≥ 2.46.0 | Includes **Git Bash**, which is required: all hooks run in bash |
| GitHub CLI (`gh`) | ≥ 2.97.0 | |
| Claude Code | ≥ 2.1.329 | |
| .NET SDK | 10.0.202 | Pinned by global.json |
| Godot | 4.7.2 stable, **.NET build** | From godotengine.org only (not Steam or itch.io) |
| Godot .NET export templates | 4.7.2 | Only for local exports: Editor → Manage Export Templates |

**Make `godot` work on the command line:**
1. Unzip the Godot .NET build to a folder of your choice. It contains a `GodotSharp` folder and a
   `*_console.exe` file.
2. Copy the `_console.exe` as `godot.exe` into the same folder, next to `GodotSharp`.
3. Add that folder to PATH. Restart any open terminals and the Claude app.
4. `godot --version` must print `4.7.2.stable.mono…`. If it says `official` without `mono`, an older
   non-.NET Godot comes first on PATH.
5. Open the Godot editor once. In **Editor → Editor Settings → Text Editor → Behavior → Indent**, set
   **Type: Spaces** and **Size: 4**. Otherwise headless editor runs rewrite C# files with tabs
   (docs/process/commands.md, "Known noise").

**Optional IDE:** VS Code with the base C# extension, or Visual Studio Community. JetBrains Rider is free
only for non-commercial use.

## 2. Logins
1. **Claude:** run `claude` and log in.
2. **Create the GitHub token.** On github.com go to Settings → Developer settings → Personal access tokens →
   **Fine-grained tokens** → Generate new token:
   - Resource owner: liad-moshe. Repository access: **Only select repositories → liad-moshe/Boxing**.
   - Expiration: 366 days or less.
   - Repository permissions:
     - Contents: **Read and write**
     - Pull requests: **Read and write**
     - Issues: **Read and write**
     - Actions: **Read**
     - Commit statuses: **Read**
     - Metadata: **Read** (selected automatically)
   - Leave **everything else at No access**. In particular: Administration, Workflows, Deployments,
     Environments, Secrets, Variables and Webhooks. Actions must not be write. These missing permissions
     are what stop agents from approving, merging around the rules, or editing CI.
3. **Give the token to gh,** in your own terminal (never paste it into Claude):
   ```powershell
   gh auth logout
   ```
   ```powershell
   gh auth login --with-token
   ```
   Paste the token, press Enter, then press **Ctrl+Z** and Enter to finish the input. Then:
   ```powershell
   gh auth setup-git
   ```
   `gh auth status` shows no "scopes" for a fine-grained token. That is normal.
4. **Remove old git credentials:** in Windows Credential Manager → Windows Credentials, delete any
   `git:https://github.com` entry, or run:
   ```powershell
   cmdkey /delete:git:https://github.com
   ```
5. **Browser:** log in to github.com. Watch the repo (Watch → All Activity) and check that Settings →
   Notifications has email or web notifications on. That way you hear when a deployment waits for your
   review (docs/process/gates.md, G3).

## 3. Git identity
Use your GitHub noreply address (github.com → Settings → Emails):
```powershell
git config --global user.name "Your Name"
```
```powershell
git config --global user.email "<id>+liad-moshe@users.noreply.github.com"
```

## 4. Optional machine settings (never commit)
`.claude/settings.local.json` is git-ignored. Example, if `godot` is not on PATH:
```json
{ "env": { "GODOT": "C:/path/to/godot.exe" } }
```

## 5. Where things live
| On GitHub (server) | In the repo (git) | Only on this machine |
|---|---|---|
| Ruleset `protect-main`, Environment `client-approval` with you as reviewer, Actions runs and artifacts, playtest prereleases, issues, PRs | All code, scenes, docs, process, agents, skills, hooks, `.claude/settings.json`, CI workflows, pinned versions | Claude login, the PAT in the gh keyring, git identity, Godot/.NET installs and export templates, `.claude/settings.local.json`, `CLAUDE.local.md`, `~/.claude` |

> ⚠ **Keep the repo public.** On the free plan, Environment required reviewers (the G3 approval gate)
> and rulesets only work on public repositories. Making it private silently removes the gate.

There is no `.env`. No secrets are needed.

## 6. First-run check
**Model for the Producer:** the Producer runs on **Sonnet**.
- **Terminal (`claude`):** sessions get Sonnet automatically from `.claude/settings.json`.
- **Claude desktop app (Code tab):** the app starts each session with the model picked in its picker,
  and that outranks settings.json. Pick **Sonnet** in the picker under the prompt box *before* the first
  message of every new session.

```
1. claude (or a new Code session)  → opens in the repo; the session-start summary shows branch + open PRs
2. ask "which studio subagents are available?"
                                   → game-designer, boxing-consultant, developer, qa, kb-keeper, tech-artist
                                     (/agents no longer lists them: since Claude Code 2.1.198 it only
                                     prints a reminder)
3. /inbox                          → shows what is waiting for you (may be empty)
4. dotnet test tests/Core.Tests                                  → all tests pass
5. godot --headless --path . --build-solutions --quit
   godot --headless --path . res://game/scenes/smoke/Smoke.tscn  → prints "SMOKE: PASS", exit 0
```
All commands and their pass criteria: docs/process/commands.md.
