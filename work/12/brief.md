# Task 12: Left and right versions of jab and cross (4 punch keys)
Issue: #12   Branch: task/12-left-right-punches   Type: mechanic   Route: table (full; labels type:mechanic, needs:boxing, needs:design)

## Request
Split jab and cross into left-hand and right-hand versions: 4 punch keys (left jab, right jab, left cross,
right cross; today J = jab, K = cross). Keep total key count small and all fight keys reachable (arrow
walking was dropped in task 10). Boxing-consultant checks which hand may throw which punch (lead vs rear,
stance). Mirrored animation for the other glove; same tick timings unless review says otherwise.
Out of scope: opponent AI, damage, stamina.

## Roles run (in order)
- game-designer · opus · new combat mechanic, update docs/specs/player-controls.md (rule a)
- boxing-consultant · sonnet · needs:boxing, lead/rear hand realism
- developer · sonnet · implement (TDD) in src/Core + thin game/ adapters
- qa · sonnet · verify
- kb-keeper · haiku · input-map/feature-map change

## Roles skipped
- tech-artist · Milestone 1, no style change (mirrored pose only)

## Gates
G1 G2 G3 G4

## Model overrides
- game-designer · opus · rule (a) · 2026-11-28

## Constraints
M1 scope, no external assets. Files likely touched: data/balance/player-moves.json, project.godot [input],
src/Core/MoveSet.cs, PlayerFighter.cs, game/scripts/{InputActions,FightController,FighterAnimator}.cs,
docs/specs/player-controls.md, docs/kb/*, tests/Core.Tests.
