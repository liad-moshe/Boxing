# Task 12 (playtest fixes, PR #13): feedback round 1
Issue: #12   Branch: task/12-left-right-punches   Type: playtest fix   Route: playtest fix

## Request (client comment on PR #13, 2026-11-28)
1. Question: after a slip left or right, an immediate jab/cross works with the right timing; in real boxing
   can you punch to the other side of the slip? -> answer from boxing-consultant (no code). Any restriction
   found becomes a NEW issue, not a change here.
2. Tweak: guard barely moves the hands; it is not obvious the boxer is covering his face -> clearer guard pose.

## Roles run (in order)
- boxing-consultant · sonnet · item 1 (research answer)
- game-designer · sonnet · item 2 (guard pose values/spec; no new mechanic, so no opus)
- developer · sonnet · item 2
- qa · sonnet
- kb-keeper · haiku · only if game/ behavior map changes

## Roles skipped
- tech-artist · Milestone 1

## Gates
G3 again (client re-approves client-approval after new commits), G4.

## Model overrides
none

## Constraints
M1, no external assets. Item 2 touches guard visuals only (data/balance/player-moves.json visual, FighterAnimator,
spec guard section). Guard timings/logic unchanged unless the designer says otherwise. >400 lines or >3 systems:
propose a new issue instead.
