---
name: inbox
description: Show what is waiting for the client - PRs needing playtest/approval or merge, issues labeled status:needs-client or status:blocked, open questions in the current task log - and recommend ONE next action. Read-only.
model: haiku
disallowed-tools: Edit, Write, NotebookEdit, Agent
---

# /inbox — the client's to-do list (read-only)

Use only read-only commands. Never change anything here.

## Collect
1. **Open PRs:** `gh pr list --state open --json number,title,headRefName,url --limit 20`.
2. For each PR: `gh pr checks <number>`. Sort it into one bucket:
   - **Waiting for your approval (G3):** `client-approval` is pending or waiting, and build-test and kb-check
     passed.
   - **Ready to merge (G4):** every required check passed (build-test, kb-check, client-approval).
   - **CI failing:** a required check failed. The Producer must fix it, so it isn't on the client.
   - **CI running:** checks are still in progress.
3. **Issues waiting on the client:**
   - `gh issue list --label status:needs-client --state open --limit 20`
   - `gh issue list --label status:blocked --state open --limit 20`
4. **Open questions on the current branch:**
   - Run `git rev-parse --abbrev-ref HEAD`.
   - If it is `task/<n>-*`, grep `work/<n>/log.md` and `work/<n>/*.md` for `QUESTIONS:` lines that aren't
     `none`, and for gates that are still pending (G1/G2).

If a gh command fails (offline, or a label doesn't exist), say so in one line and continue.

## Output (at most 20 lines)
```
📥 Client inbox — <date>
G3 approve:   #<pr> <title> — <url>           (or "none")
G4 merge:     #<pr> <title> — <url>           (or "none")
Needs you:    #<issue> <title> [needs-client|blocked]
Questions:    <task n>: <question>
FYI:          <CI failing / running, one line each>

👉 Recommended next action: <exactly ONE concrete action, e.g. "Playtest PR #12, then Review deployments →
   client-approval → Approve and deploy (docs/process/gates.md, G3)">
```
Priority for the one action: an unanswered gate question, then G3 approve, then G4 merge, then a blocked
issue, then "start the next roadmap task with /task".

End with `▶ Now: … | ⏳ Waiting on you: … | ➡ Next: …`.
