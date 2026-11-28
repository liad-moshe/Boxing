# Task 10: Playable fight — keyboard control of the player boxer
Issue: #10   Branch: task/10-player-controls   Type: mechanic   Route: table (New combat mechanic)

## Request
Start a fight from the menu and control the human player's boxer with a small set of boxing moves:
a few punches and a few defensive moves. Keep the key set small so the keyboard is not crowded.
Keyboard only, no mouse. The opponent just stands still: no AI behaviour in this task.

## Roles run (in order)
- game-designer · opus · new combat mechanic: spec, controls, acceptance criteria, playtest checklist
- boxing-consultant · sonnet · realism review of the moves (jab, cross, guard, slip) and their timing
- developer · sonnet · plan.md, then TDD: src/Core move/state logic + thin game/ adapters, fight scene, input map
- qa · sonnet · acceptance tests, build/test/smoke, Godot-3 drift scan, playtest checklist
- kb-keeper · haiku · input-map / feature-map / ui-map updates

## Roles skipped
- tech-artist · Milestone 1 (never routed in M1); boxers stay flat-colour primitives

## Gates
G1 (route) · G2 (spec) · G3 (playtest + client-approval deployment) · G4 (client merges)

## Model overrides
- game-designer · opus · rule (a) new combat mechanic · 2026-11-28

## Constraints
- Milestone 1 scope (docs/roadmap.md): moves are jab, cross, guard, slip, plus basic movement.
- Out of scope: stamina, health/KO, round timer, decision, opponent AI (opponent stands idle).
- Keyboard only, no mouse. Small key set.
- No external assets: ring and boxers from primitives/CSG.
- Files likely touched: src/Core (fighter state/moves), game/scenes (fight scene), game/scripts,
  project.godot [input], tests/, docs/specs/player-controls.md, docs/kb/*.
- Labels used: type:mechanic, needs:boxing, needs:design.
