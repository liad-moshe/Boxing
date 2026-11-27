---
name: ship
description: Finish a studio task whose QA verdict is PASS - assemble the CLIENT BRIEFING PR body, delete work/<n>/, push the task branch, open the PR with labels, tell the client how to playtest/approve/merge, and recommend /clear.
---

# /ship — open the PR as a client briefing

You are the Producer. Follow CLAUDE.md. Never merge, and never approve.

## 1. Preconditions (stop and report if any fails)
- The current branch is `task/<n>-<slug>`.
- work/<n>/qa-report.md says `VERDICT: PASS`.
- kb-keeper ran, or brief.md records why it was skipped.
- The four checks in docs/process/commands.md pass locally right now. Run them.
- `git status --short` is clean, apart from work/<n>/.

## 2. Collect facts
- `git diff --stat main...HEAD` and `git log --oneline main..HEAD`.
- Check for protected paths:
  `git diff --name-only main...HEAD | grep -E '^(\.claude/|\.github/|CLAUDE\.md$|docs/process/|global\.json$|\.gitignore$|\.gitattributes$)'`
- From brief.md: the route, the roles run and skipped, the models, and the overrides.
- From the spec and qa-report: the acceptance criteria, their status, the keys, and the playtest checklist.

## 3. Write the CLIENT BRIEFING to a temp file outside the repo
`body="$(mktemp)"`, then write this into it:
```markdown
<⚠ GOVERNANCE banner, only if protected paths changed:
> ⚠ **Governance change.** This PR touches protected paths: <list>. Review these files yourself before approving.>

## What changes for the player
<2-4 plain sentences, no code talk>

## How to playtest
1. On this PR open **Checks** → **build-test** run → **Artifacts** → download the playtest build, unzip, run `Boxing.exe`.
2. Keys: <action → key, from docs/kb/input-map.md>
3. Checklist:
   - [ ] <item 1>  … (exactly 5 items, from qa-report.md)
4. If it plays right: on the run page, **Review deployments** → tick **client-approval** → **Approve and deploy** (docs/process/gates.md).

## Acceptance criteria
| AC | Status | Verified by |
|---|---|---|

## Studio roles
| Role | Ran? | Model | Note |
|---|---|---|---|
<every role: run, or skipped with the reason; opus overrides with their rule>

## Risks
<bullets, or "none known">

## Open questions
<bullets, or "none">

Closes #<n>
```

## 4. Clean up, push, open the PR
1. `git rm -r -q work/<n>` and then `git commit -m "chore(<n>): ship — remove work files"`.
2. `git push`.
3. Open the PR:
   ```
   gh pr create --base main --head task/<n>-<slug> --title "<type>(#<n>): <short title>" --body-file "$body"
   --label type:<type>
   ```
   Add `--label needs:boxing` / `--label needs:art` if they apply. If adding a label fails, create the
   PR without it and mention that.
4. `gh pr view --json url -q .url`, then delete the temp file.

## 5. Tell the client (at most 10 lines)
- The PR URL, plus one line on what changed for the player.
- What to do now:
  1. Wait for CI (build-test, kb-check, governance) to go green. GitHub notifies you.
  2. **G3:** playtest with the checklist in the PR, then approve `client-approval`.
  3. **G4:** click **Squash and merge**. release.yml then publishes the `playtest-N` prerelease.
  4. Not happy? Comment on the PR and run `/playtest-feedback <PR number>`.
- Recommend: "Run **/clear** before the next task, so each task gets a fresh session."

End with `▶ Now: … | ⏳ Waiting on you: … | ➡ Next: …`.
