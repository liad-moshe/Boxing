---
name: plan-milestone
description: Turn the client's direction for a milestone into a proposal - roadmap changes plus a list of GitHub issues with type labels and suggested routes - and, only after the client approves it, update docs/roadmap.md and create the issues.
argument-hint: "<milestone, e.g. M1, plus any direction>"
model: opus
disable-model-invocation: true
---

# /plan-milestone — propose a milestone plan (client approval required)

Direction: **$ARGUMENTS**

You are the Producer. This skill runs on opus. Follow CLAUDE.md.

## 1. Read
- docs/vision.md, including the client decisions.
- docs/roadmap.md.
- docs/process/routing.md: the task types and usage units.
- docs/kb/feature-map.md: what already exists.
- Open issues: `gh issue list --state open --limit 50 --json number,title,labels`.

## 2. Draft the proposal (no GitHub changes yet)
Write `work/plan-<milestone>/proposal.md` on the current branch. Don't commit it yet. It contains:
1. **Goal**: one sentence, plus what "done" looks like for the client in a playtest.
2. **Roadmap changes**: the exact text to add, change or remove in docs/roadmap.md, as a before/after.
3. **Issues**, in build order. One row each:
   `| # | Title | type:<mechanic/ui/bug/balance/refactor/visual> | needs:boxing? | Route | Est. units | Depends on |`
   - Keep each issue small: about one PR, under ~400 changed lines, and at most 3 systems.
   - The type comes from routing.md. M1 never uses tech-artist or type:visual.
   - Every issue states its player-facing outcome in one sentence.
4. **Deliberately not in this milestone**: a bullet list.
5. **Total estimated usage units**, plus the riskiest assumption.

## 3. Ask for approval
Show the client the issue table and the total, then ask exactly one question: **"Approve this milestone
plan?"** Then stop. Accept edits and redraft until the client says yes.

## 4. After an explicit "yes"
1. Create a tracking issue: `gh issue create --title "Plan <milestone>" --body "<goal + link list>"`.
   It gets no type label, because it is planning, not a work type. Use its number `<p>` for
   `git switch -c task/<p>-plan-<milestone>` from an up-to-date main.
2. Apply the roadmap changes to docs/roadmap.md.
3. Create each issue in order:
   `gh issue create --title "<title>" --body "<outcome, acceptance idea, depends on #..>" --label type:<type>`
   Add `--label needs:boxing` where it applies. Record the issue numbers in the tracking issue with
   `gh issue comment <p> --body ...`.
4. Commit `docs(plan): <milestone> roadmap`. Then ship it as a lean, no-code PR with the ship-task skill, with
   QA reduced to "docs only" and the reason noted in brief.md.

## Rules
- Never create issues or edit the roadmap before the client's explicit approval.
- No external assets. Stay inside docs/vision.md's pillars and the client decisions.

End with `▶ Now: … | ⏳ Waiting on you: … | ➡ Next: …`.
