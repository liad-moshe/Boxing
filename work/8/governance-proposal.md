# Governance proposal — task #8: open the PR automatically after QA passes

## What
Rename the project skill `ship` to `ship-task`. Make the task flow run it straight away once QA says PASS
and kb-keeper is done (or skipped), without waiting for the client. Update every reference to the old name.

## Why
- `.claude/skills/task/SKILL.md` step 9 already says to continue with the ship skill after QA PASS.
- Claude Code has a **built-in** skill that is also called `ship`. Its description says *"Never call it
  yourself: only a line that the user types starts it."* Because of the shared name, the Producer sees
  that rule when it reaches step 9. So it stops and asks the client to type `/ship`.
- A unique name removes the clash. The new step 9 wording says plainly that opening a PR is **not** a gate.
- Gates do not change: G3 (playtest + approve `client-approval`) and G4 (merge) stay with the client, and
  GitHub enforces both. The Producer still never merges and never approves.
- `Skill(ship-task)` goes into the committed allow-list. Without it, the automatic step stops on a
  permission prompt. Today `Skill(ship)` is only in your own gitignored `settings.local.json`.

## How to apply
Run this in a terminal from the repo root, on branch `task/8-auto-open-pr`. Alternatively, start a session
with `claude --setting-sources user` and ask it to apply this file.

```
git mv .claude/skills/ship .claude/skills/ship-task
```

Then make the edits below by hand. Each one is a small hunk.

### 1. `.claude/skills/ship-task/SKILL.md` (after the move)
```diff
 ---
-name: ship
-description: Finish a studio task whose QA verdict is PASS - assemble the CLIENT BRIEFING PR body, delete work/<n>/, push the task branch, open the PR with labels, tell the client how to playtest/approve/merge, and recommend /clear.
+name: ship-task
+description: Finish a studio task whose QA verdict is PASS - assemble the CLIENT BRIEFING PR body, delete work/<n>/, push the task branch, open the PR with labels, tell the client how to playtest/approve/merge, and recommend /clear. The Producer runs it automatically from /task step 9; the client may also type /ship-task.
 ---
 
-# /ship — open the PR as a client briefing
+# /ship-task — open the PR as a client briefing
 
 You are the Producer. Follow CLAUDE.md. Never merge, and never approve.
+Run this as soon as its preconditions hold. Do not ask the client first: opening the PR is not a gate.
```

### 2. `.claude/skills/task/SKILL.md`
```diff
-description: Start a new studio task from the client's request or a GitHub issue - classify the route, create the issue/branch/brief, run the client gates, delegate to the subagents in route order with checkpoints, then continue to /ship.
+description: Start a new studio task from the client's request or a GitHub issue - classify the route, create the issue/branch/brief, run the client gates, delegate to the subagents in route order with checkpoints, then open the PR with the ship-task skill.
```
```diff
 ## 9. Ship
-When QA says PASS and kb-keeper is done (or skipped), continue with the **ship** skill.
+When QA says PASS and kb-keeper is done (or skipped), run the **ship-task** skill right away, in the same
+turn. Do not stop to ask the client: opening the PR is not a gate. G3 and G4 stay with the client.
```

### 3. `.claude/skills/plan-milestone/SKILL.md`
```diff
-4. Commit `docs(plan): <milestone> roadmap`. Then ship it as a lean, no-code PR with the ship skill, with
+4. Commit `docs(plan): <milestone> roadmap`. Then ship it as a lean, no-code PR with the ship-task skill, with
    QA reduced to "docs only" and the reason noted in brief.md.
```

### 4. `.claude/settings.json` (inside `permissions.allow`)
```diff
+      "Skill(ship-task)",
       "Bash(gh pr create *)",
```

### 5. `CLAUDE.md` — section `## Shipping`
```diff
 ## Shipping
-- /ship builds the PR body as the CLIENT BRIEFING:
+- The ship-task skill builds the PR body as the CLIENT BRIEFING. The Producer runs it automatically when
+  QA says PASS and kb-keeper is done (or skipped). Opening the PR is not a gate, so never wait for the
+  client to ask. The briefing contains:
```

### 6. `docs/process/workflow.md`
```diff
-[10] /ship ─► PR whose body is your CLIENT BRIEFING; work/<n>/ deleted in the final commit;
+[10] ship-task (automatic) ─► PR whose body is your CLIENT BRIEFING; work/<n>/ deleted in the final commit;
         │      Producer recommends /clear
```

### 7. `docs/process/environment-spec.md`
```diff
-10. /ship → the PR body is the CLIENT BRIEFING: player-facing change, how to playtest (artifact link, keys,
+10. ship-task (run automatically after 8-9) → the PR body is the CLIENT BRIEFING: player-facing change, how to playtest (artifact link, keys,
```

### 8. `docs/process/failure-playbook.md`
```diff
-- **Do:** one task per session. After `/ship`, run `/clear` before the next task. Long sessions are
+- **Do:** one task per session. After the PR is opened (ship-task), run `/clear` before the next task. Long sessions are
```

### 9. Optional, your local file only (gitignored, not committed): `.claude/settings.local.json`
```diff
-      "Skill(ship)",
+      "Skill(ship-task)",
```

## Check after applying
```
git grep -nE "/ship([^-]|$)|ship skill|name: ship$" -- . ":!work"
```
This should print nothing. The new names are `ship-task`, `/ship-task` and `ship-task skill`. Then commit
the changes on this branch, e.g. `chore(8): rename ship skill to ship-task`. Once that commit is pushed,
the Producer can open the PR for task #8 with `ship-task`. That PR is the first real test of the new flow.
Because it touches protected paths, it gets the ⚠ governance banner.

## Risk
- I can't fully verify that the rename fixes the stop until a real task reaches step 9. The PR for this
  task is the first check: if the Producer opens it without asking you, the fix works.
