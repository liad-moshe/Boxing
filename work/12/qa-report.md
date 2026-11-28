# QA report: #12 stance (orthodox / southpaw)

VERDICT: PASS

First QA cycle on this task (not a 2nd FAIL). No production code or tests changed by QA; no new tests were needed.

## Commands (docs/process/commands.md, in order)
| # | Command | Exit | Result |
|---|---|---|---|
| 1 | `dotnet build Boxing.sln -warnaserror` | 0 | 0 Warning(s), 0 Error(s) |
| 2 | `dotnet test tests/Core.Tests` | 0 | Failed 0, Passed 166, Total 166 |
| 3 | `godot --headless --path . --build-solutions --quit` | 0 | git tree stayed clean afterwards (no tab rewrite of Main.cs) |
| 4 | `godot --headless --path . res://game/scenes/smoke/Smoke.tscn` | 0 | `SMOKE: PASS after 1479 physics frames (fight script included)`; grep for `ERROR`, `SCRIPT ERROR`, `Unhandled exception` found nothing |

## Other checks
- **API-drift scan** (changed .cs in src/, game/, tests/): no hits for Spatial, KinematicBody, .Translation, yield, Godot.Object, OS.GetTicksMsec, Engine.EditorHint, [Remote], [Puppet], string Connect(...). Clean.
- **No external assets:** diff adds no image, model, font, audio or .import files. Clean.
- **Protected paths:** diff touches none of .claude, .github, CLAUDE.md, docs/process, global.json, .gitignore, .gitattributes. project.godot is unchanged.
- **Absolute/user-specific paths** in added lines: none.
- **Fight logic unchanged:** `git diff main...HEAD -- src/Core/PlayerFighter.cs` is empty; FighterInput and the timing numbers are also untouched. `PlayerFighter.cs` contains no mention of stance. The only change to player-moves.json is the new `stance.default: "orthodox"` block plus `_comment` text; every frame count, reach and cancelInto is the same as main.
- **KB valid:** every backticked repo path in docs/kb exists, and every project.godot [input] action is listed in docs/kb/input-map.md. Note for kb-keeper (not a failure): docs/kb/input-map.md line 10 still says menu_up / menu_down do nothing on Character Select, and lines 20-21 name only `Menu._UnhandledInput`. Both are now stale (CharacterSelect.cs line ~124 toggles stance on them). That is the kb-keeper's step.

## Acceptance criteria
| AC | How verified | Result |
|---|---|---|
| AC1 | MoveSetTests (existing malformed-data and unknown-cancelInto cases) + StanceTests: ShippedData_DefaultStanceIsOrthodox, DefaultStance_ReadsTheDataValue, BadDefaultStance_Throws_NamingTheKey, NonStringDefaultStance..., MissingDefaultStance... | PASS |
| AC2-AC16 | Existing #10 Core tests, which run unchanged against unchanged `PlayerFighter` and unchanged move numbers | PASS (166/166) |
| AC17 | StanceTests: Orthodox_/Southpaw_ helper tests, SouthpawPose_IsTheMirrorOfOrthodox_OnEveryTick_OfEveryMove, Jab_MovesTheLeadGlove_Cross_TheRear_InBothStances; PoseBlendTests updated | PASS |
| AC18 | CharacterSelectFlowStanceTests (7 tests: defaults, toggle, confirm, Back, Done rejected) + StanceTests.OneTwo_RunsTheSameTicks... (PlayerFighter takes no stance) | PASS |
| AC19 | Smoke SelectScript: line text, Up, Down, Left/Right no change, click toggles without confirming, FocusMode None. "Held Up toggles once" is by code reading: CharacterSelect uses IsActionPressed, which ignores echo by default. Not driven in smoke, left to playtest item 1. | PASS (held-key part by inspection) |
| AC20 | Smoke SelectScript: step 2 "You: ... (Southpaw)", step-2 stance line Orthodox. Escape back to step 1 with the stance kept is covered at Core level (Back test); the UI path is for playtest item 5. | PASS |
| AC21 | Smoke southpaw pass (tags, lead foot, face cues, build checks) | PASS |
| AC22 | project.godot not in diff; all fight_* actions and menu actions present | PASS |
| AC23 | Smoke: held key throws one move, A+D no move, arrow keys no move (orthodox pass). Playtest item 4. | PASS |
| AC24 | Smoke "Orthodox player ok" (lead glove +0.451 m on jab, rear +0.563 m on cross, torso turn) | PASS |
| AC25 | Smoke "Southpaw player ok" (same turn size 0.0362, mirrored) | PASS |
| AC26 | StanceTests.OneTwo_... (both stances) + smoke one-two | PASS |
| AC27 | Smoke slip, guard and Idle-return checks in both stances | PASS |
| AC28 | Smoke: "Contact: Jab" / "Contact: Cross" printed, no contact from the start position | PASS |
| AC29 | Escape then New Game reset to defaults: not driven in smoke (GameSession.Clear() is exercised; GameSession and the flow default are unit/inspection-level). Playtest item 5. | PLAYTEST |
| AC30 | Smoke direct-launch orthodox pass: tags, jab and cross complete and return to Idle, no errors | PASS |

## Defects
None.

## Playtest checklist for the client (G3)
1. New Game: under the two cards a line reads "Stance: Orthodox (Up / Down to change)". Up gives Southpaw, Down gives Orthodox, clicking the line also flips it without confirming a card, Left / Right only move the highlight, and holding Up changes it only once. Pick Southpaw, confirm Ghost: step 2 shows "You: Mateo Rivera (Southpaw)" and its own line starts on Orthodox.
2. Leave the opponent on Orthodox, confirm Bruno: tags read "YOU (Southpaw)" and "CPU (Orthodox)"; your right foot is forward and the opponent's left foot is forward.
3. As southpaw, walk close with D. J throws the glove nearer the camera (your right) and shows "Contact: Jab". K throws the other glove, the left shoulder comes forward, it feels slightly slower, and "Contact: Cross" shows. At the start distance neither shows contact text.
4. J then quickly K flows as a one-two. Mashing or holding keys never punches faster than the moves allow. Hold S: gloves rise, walking slows, J and K do nothing. Q / E dip to that side (E toward the camera); J or K right as the slip comes back fires out of it.
5. Escape, New Game, both Orthodox: J throws the left glove (away from the camera), K the right glove with the right shoulder turning, with the same rhythm as in southpaw. Escape in step 2 brings back step 1 with your stance kept. After Escape from the ring, New Game starts on Orthodox again.

VERDICT: PASS
