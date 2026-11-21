---
name: resume-task
description: Continue an interrupted studio task (after a usage limit, crash or /clear) from its branch, brief, log and plan, starting at the first unchecked item. With no task branch checked out, lists open task branches and PRs. Not the built-in /resume, which reopens a past conversation.
---

# /resume-task — pick up where the studio left off

You are the Producer. Follow CLAUDE.md.

## 1. Where are we?
Run `git status --short` and `git rev-parse --abbrev-ref HEAD`.

### On `task/<n>-<slug>`
1. Read `work/<n>/brief.md` (route, roles, gates, overrides), `work/<n>/log.md` (what happened) and
   `work/<n>/plan.md`, if it exists.
2. Check for uncommitted changes. If a role was interrupted mid-step, its files may be half-written. List
   them, and either re-run that role's step or commit what is complete as `wip(<n>): <step> (partial)`.
3. Work out the next step:
   - the first unchecked plan.md item; or
   - if there is no plan, the next role in the route after the last `log.md` entry; or
   - a gate that is still pending (G1/G2), in which case ask the client again and stop.
4. Tell the client in 3-5 lines what was done, what's next, and whether anything waits on them.
5. Continue with the task skill's steps from that point. Keep the same checkpoint protocol.

### On `setup/*`
Read `work/setup/progress.md`, report the unchecked items, and wait for the client to paste the next
stage prompt.

### On `main`, or anything else
List what could be resumed, read-only:
- `git branch --list "task/*"` and `git branch -r --list "origin/task/*"`
- `gh pr list --state open --limit 10`
- `gh issue list --label status:blocked --limit 10`

Then ask the client which task to resume, and `git switch task/<n>-<slug>` only after they answer.

## Rules
- Never start a new task here. Use /task for that.
- If brief.md and log.md disagree, trust log.md and the git history, and say so.

End with `▶ Now: … | ⏳ Waiting on you: … | ➡ Next: …`.
