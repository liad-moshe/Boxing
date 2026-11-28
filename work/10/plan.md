# Plan: task 10 player controls (spec docs/specs/player-controls.md)

Order: failing xUnit test, then code, `dotnet test tests/Core.Tests` green, tick. AC numbers are the spec's.

## Core (src/Core, tests/Core.Tests)
- [x] 1. MoveSet.Parse: shipped data parses; bad/missing frame counts, speed, reach, unknown cancelInto, bounds excluding playerX throw with the key named (AC1). Test project links player-moves.json.
- [x] 2. PlayerFighter timeline for Jab, Cross, SlipLeft, SlipRight: exact startup/active/recovery then Idle; cross total ratio 1 to 1.5 (AC2, AC3); IsEvading/EvadeSide only in slip Active (AC4).
- [x] 3. Ignore rules, no buffering, cancels from Recovery only: jab->cross, slip->jab/cross, cross none, slip->slip none (AC5, AC6, AC7, AC8).
- [x] 4. Guard hold: GuardRaising for raiseFrames, Guarding, release -> Idle same tick, punches ignored, slip from guard, guard resumes after move (AC9, AC10).
- [x] 5. Walking, guard slow-down, no walking in moves, resumes on free tick, bounds clamp (AC11, AC12, AC13).
- [x] 6. Contact on first Active tick in reach, at most one, none out of reach (AC14); same-tick priority incl. guard wins (AC15); opponent X fixed (AC16).
- [x] 7. ContactBanner (text, countdown in ticks, newer contact restarts) for the Contact label logic (AC21, AC23 no leftover text on a fresh fight).
- [x] 8. PoseBlend: per-tick pose amounts (lead/rear extend, guard, slip, side) from State/Phase/PhaseFrame; zero in Idle (AC20 logic).

## Godot adapters (game/, project.godot)
- [x] 9. project.godot [input]: fight_forward D, fight_back A, fight_jab J, fight_cross K, fight_guard S, fight_slip_left Q, fight_slip_right E (physical keycodes, one key each); names in InputActions.cs (AC18).
- [x] 10. GameSession.Moves loader (player-moves.json + ring positions); FightController (input -> FighterInput -> Tick once per _PhysicsProcess, root X, tickRate check, Contact label + GD.Print, controls hint); FighterAnimator (poses existing primitive parts: guard, jab/cross, slips); wire into Main._Ready (AC17, AC19 to AC24).
- [x] 11. Smoke: scripted fight driven by Input.ActionPress/Release: AC17 first-D moves, AC18 InputMap, AC19 hold J one jab / A+D no move, AC20 poses, AC21 contact text, AC22 no contact at start and stop at min separation, AC24 direct launch plays a jab.

## Finish
- [x] 12. Run the four checks from docs/process/commands.md; leave the build green; report.

## Notes
- Tuned `visual.punchExtendH` 0.30 -> 0.24 (within the spec's 30 percent) so a punch at gap == reach ends at the opponent's head front; the cross glove travels further by exactly (cross.reachM - jab.reachM), because one value serves both punches. The smoke asserts both.
- Not in smoke (manual playtest): AC23 (Escape / new fight), the physical keys themselves.
- Diff size: new code about 2.3k lines: Core 570, adapters 330, smoke 340, tests 1000. Over the ~400 line guide (mechanic route, client-approved full route, so reported not re-classified); touches Core, adapters and the input map.
