# QA report: task 12, playtest fix round 1, item 2 (clearer guard pose)

Branch task/12-left-right-punches, head 288c170 plus one new QA test file. First QA FAIL on this fix: no (this is the first QA pass of the guard fix).

## Commands (docs/process/commands.md, in order)
| # | Command | Exit | Result |
|---|---|---|---|
| 1 | dotnet build Boxing.sln -warnaserror | 0 | 0 Warning(s), 0 Error(s) |
| 2 | dotnet test tests/Core.Tests | 0 | Failed 0, Passed 180 (179 dev + 1 QA test) |
| 3 | godot --headless --path . --build-solutions --quit | 0 | ok |
| 4 | godot --headless --path . res://game/scenes/smoke/Smoke.tscn | 0 | `SMOKE: PASS after 1479 physics frames`; 0 lines match ERROR, SCRIPT ERROR or Unhandled exception |

## Acceptance criteria (guard fix)
| AC | How verified | Result |
|---|---|---|
| AC31 (rise at least 0.8 x raiseH x H, glove between chin and head top, 0.02-0.20 H ahead of head front) | Smoke `ExpectGuardCovers` on the Guarding player (default stance, plus Orthodox and Southpaw runs); Core `GuardMoveTests.FullGuard_RaisesGloveAtLeast80Percent...`; QA `GuardArmGeometryTests` (same checks on every roster fighter, both arms, shipped data) | PASS |
| AC32 (monotonic raise, release returns to Idle within 1 mm, glove never crosses centre line) | Centre-line sign check in smoke `ExpectGuardCovers`, both stances. Monotonic raise: `GuardMoveTests.Raise_IsMonotonicInTheBlend`. Idle-after-release: existing smoke `ExpectPoseAtRest` after guard release (unchanged lines), and `PoseBlend` is unchanged. | PASS (the per-tick raise over exactly raiseFrames in the scene is not separately asserted by the new code; the blend curve is unchanged from main) |
| AC33 parse (raiseH >= 0.08, 3 new keys >= 0, missing/negative throws naming the key) | `MoveSetTests.GuardVisualKeys_AreRead...`, `NewGuardKey_MissingOrNegative_Throws_NamingTheKey`, `GuardGloveRaise_BelowMinimum_Throws...` | PASS |
| AC33 limb length within 15 percent | Smoke `ExpectGuardCovers` (UpperArm and Forearm basis length vs rest, both arms, both stances); `TwoBoneArmTests.Elbow_KeepsBothLimbLengths`; QA test | PASS |
| AC33 elbow lower than glove, elbow inboard of shoulder | The developer's tests did NOT cover this (xUnit only checks offsets, smoke does not check it). Gap closed by new QA test `tests/Core.Tests/GuardArmGeometryTests.cs`, which rebuilds the arm with the FighterBuilder and FighterAnimator arithmetic from shipped data for every roster fighter and both arms. It is a Core-level replica, not a scene read. | PASS (see residual note) |
| AC34 (southpaw mirrors orthodox) | Guard offsets are per anatomical arm (`GuardMove.Sideways`, `inwardSign` from the wrist side), so there is no stance-dependent branch. `GuardMoveTests.Sideways_...Mirrors`, `StanceTests.SouthpawPose_IsTheMirrorOfOrthodox...`, and the smoke runs the full guard check under the Southpaw stance. | PASS |

Residual note: the elbow-lower and elbow-inboard checks run on a replica of the scene arithmetic, not on the live nodes. If FighterBuilder's arm layout changes, the replica must follow. Low risk.

## Scope checks
- `PlayerFighter.cs` (src/Core): not in `git diff main...HEAD`, so unchanged vs main. Guard timings, logic, lockout and slow-down live there and in data/balance/player-moves.json `guard`, `walk` and so on. The data diff touches only `visual` keys and the visual `_comment`.
- Idle pose unchanged: `FighterAnimator` only runs the arm solver when `guard > 0`. At guard 0 the offsets are exactly 0 (`GuardMoveTests.AtBlendZero_NothingMoves`), and smoke `ExpectPoseAtRest` still passes after every move.
- Full suite: all stance tests (AC1-AC30) pass in the 180-test run, and the smoke Orthodox and Southpaw scenarios pass.
- Changed src/Core files: GuardMove.cs (new), TwoBoneArm.cs (new), MoveSet.cs (3 keys and a min-raise rule). No `using Godot` in Core.
- Godot 3 API drift scan on changed .cs files (Spatial, KinematicBody, .Translation, yield, Connect(.., this, ..), Godot.Object, OS.GetTicksMsec, EditorHint, [Remote], [Puppet]): 0 hits.
- External assets: the diff adds no image, model, font or audio file.
- Protected paths: none changed (.claude, .github, CLAUDE.md, docs/process, global.json, .gitignore, .gitattributes).
- Absolute or user-specific paths in added lines: none.
- KB: all backticked paths in docs/kb exist, and every project.godot input action is in docs/kb/input-map.md.

## Does the guard read as "clearly covering the face"? (I cannot see the screen)
Measurable evidence only, from the smoke assertions and the data:
- Each glove rises 0.11 H (about 0.20 m at H 1.8 m), against 0.04 H before. That is a 2.75x larger vertical change.
- Glove centre ends at about 0.95 H, inside the head's height span (0.86-1.015 H). The smoke asserts this.
- Gloves are pulled back to about 0.16 H ahead of the root, which is 0.02-0.20 H in front of the head's front surface. The smoke asserts this.
- Gloves stay on their own side of the centre line (inward 0.14, the 0.22 value was rejected because it crossed the line). Elbows stay low and inboard, and the limbs are not stretched.
Judgement from numbers: geometrically the gloves sit at face height just in front of the face, so it should read as covering. Whether the gloves hide enough of the face from the side-on camera, and whether the inward 0.14 reads from the front, is a visual call only the client can make.

## Defects
None.

## Playtest checklist for the client
1. Select a fighter and start a fight (Orthodox). Stand still and check Idle, then hold Guard: the fists should move visibly up and back until they sit at face height just in front of the head. The change from Idle should be obvious at first glance.
2. Release Guard: the fists drop straight back to the Idle stance with no leftover or jitter. Hold Guard and walk: you move slower, and punches are locked out while guarding (all unchanged).
3. Slip left and right, then throw a jab and a cross straight out of Guard: the gloves go up and back into Guard cleanly, and the punch and slip poses look the same as before.
4. Look at the guard from the side-on camera and from any angle you can reach: the arms should look like arms (no stretched or bent-through limbs), the elbows stay tucked low near the ribs, and neither glove pokes through the head or crosses the middle of the face.
5. Repeat the guard in Southpaw (pick it on the character select) and for a second fighter body type: the guard should be the mirror image of Orthodox and still cover the face. Tell me if it still reads too weak, too high (hides the eyes), or too tight, and the three guard numbers get re-tuned.

## Tests written
- `tests/Core.Tests/GuardArmGeometryTests.cs` (new, 1 test, passes).

VERDICT: PASS
