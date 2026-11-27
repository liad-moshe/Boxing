VERDICT: PASS

Scope: humanoid look change (AC24-AC29) plus regression over AC1-AC23. First FAIL: no (this is a PASS on the first QA run of this change).

## Acceptance criteria
| AC | How verified | Result |
|---|---|---|
| AC1-AC10, AC29 | xUnit in tests/Core.Tests (FighterRosterTests, 32 tests incl. look/body/scale/colour validation throws) | PASS (50/50 total) |
| AC11-AC23 | Regression: unchanged flow code, smoke loads Menu, CharacterSelect (both cards), Main with two fighters, clock advance; xUnit flow/selection tests | PASS |
| AC24 | Smoke CheckParts: all 25 named parts on ring fighters and on freshly built Anvil/Ghost; fails if any missing | PASS |
| AC25 | Smoke CheckColours: Shorts, GloveL/R, Hair, Head, Waistband albedo vs JSON hex, tolerance 0.01 | PASS |
| AC26 | Smoke: Ghost total height > Anvil, Anvil shoulder span > Ghost, Ghost glove-to-shoulder reach > Anvil (world AABBs) | PASS |
| AC27 | Smoke CheckPose: foot lowest point within 1 cm of floor, head above torso, gloves above shorts top and in front of torso by facing, glove radius >= 1.5x forearm radius | PASS |
| AC28 | Smoke: eyes/nose/mouth on facing side of head; player on -X facing +X, opponent facing -X | PASS |
| Visual look | Playtest (item 3) | pending client |

Smoke assertions measure real geometry and match the spec wording, so I judged them strict enough and added no tests. Minor note (not a defect): the ring-fighter colour check compares both ring fighters against roster[0]; it passes because the default pair is the same boxer. Both shipped builds are also checked individually against their own definitions, so coverage holds.

## Commands
| Command | Exit | Result |
|---|---|---|
| dotnet build Boxing.sln -warnaserror | 0 | 0 Warning(s), 0 Error(s) |
| dotnet test tests/Core.Tests | 0 | Failed: 0, Passed: 50 |
| godot --headless --path . --build-solutions --quit | 0 | ok |
| godot --headless --path . res://game/scenes/smoke/Smoke.tscn | 0 | `SMOKE: PASS after 120 physics frames`; no ERROR / SCRIPT ERROR / Unhandled exception |

Working tree was clean after the commands (no Main.cs tab rewrite).

## Other checks
- API-drift scan (game/ and src/ .cs): no Godot 3 names found. PASS.
- External assets: diff bdc9c16..HEAD adds no image, model, font or audio files. PASS.
- KB: every backticked path in docs/kb exists (symbol names such as `Menu.Activate` are not paths); all project.godot input actions are in docs/kb/input-map.md. PASS.

## Defects
None.

## Playtest checklist for the client
Wording unchanged from the approved spec (it already describes the humanoid look). Names match data: Mateo Rivera is the tall, lean, blue-shorted boxer; Bruno Kessler is the stocky, broad, red-shorted one.
1. Click New Game: a "Choose your boxer" screen shows two cards side by side, each with name, style (Slugger / Out-boxer) and six stat bars with numbers. Left/Right and mouse hover move the highlight and wrap; both boxers' stats stay visible.
2. Pick Mateo Rivera (Enter or click): the header changes to "Choose your opponent", shows "You: Mateo Rivera", and his card carries a "YOU" tag.
3. Pick Bruno Kessler as opponent: the ring shows two human-looking boxers, each with head, eyes, nose, mouth, hair, neck, shoulders, two arms with big gloves up in a guard, shorts with a waistband, legs and feet. A tall, lean, blue-shorted fighter is on the left ("YOU"), a stocky, broad, red-shorted one on the right ("CPU"), standing ready and facing each other.
4. Escape to the menu, New Game again, pick the same boxer for both: both look identical but are labelled YOU and CPU; nothing warns or blocks you.
5. Escape steps back one level at a time (opponent, boxer, menu), a held Escape moves only one step, and the stats shown change nothing about ring behaviour.
