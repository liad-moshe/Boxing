VERDICT: FAIL (1 defect, AC22 as written; everything else passes)

First FAIL on task 5.

## Acceptance criteria
| AC | How verified | Result |
|---|---|---|
| AC1 | FighterRosterTests.Shipped_HasAnvilThenGhost_WithAllStatsInRange | PASS |
| AC2 | FighterRosterTests.Shipped_StatTotalsEqual | PASS |
| AC3 | FighterRosterTests.Shipped_NeitherBoxerDominates | PASS |
| AC4 | MissingStat_Throws, StatOutOfRange_Throws(0, 11), DuplicateIds_Throws, EmptyList_Throws (messages name the problem) | PASS |
| AC5 | CharacterSelectFlowTests.New_StartsAtPickPlayer_NoIds | PASS |
| AC6 | ConfirmPlayer_AdvancesToOpponent | PASS |
| AC7 | SameBoxerAllowed_ReachesDone | PASS |
| AC8 | Back_FromPlayerStep_GoesToMenu | PASS |
| AC9 | Back_FromOpponentStep_ReturnsToPlayerStep_ClearsOpponent | PASS |
| AC10 | Confirm_UnknownId_Rejected..., Confirm_AfterDone_Rejected... (returns false, state unchanged) | PASS |
| AC11-AC21 | Code review of CharacterSelect.cs / Main.cs and playtest needed. Headless smoke confirms both cards (Card_anvil, Card_ghost) build. Not automatable beyond that. Code matches spec: single highlight via MenuSelection, hover gated on real mouse motion, 350 ms click cooldown plus DoubleClick guard, `_leaving` guard, echo ignored by IsActionPressed, step-2 highlight set to the player's pick, YOU tag and "You: <Name>" line, GameSession cleared on return to the menu. | PASS by review; playtest pending |
| AC22 | Inspected project.godot and docs/kb/input-map.md | FAIL as written (see below) |
| AC23 | Smoke launches Main.tscn directly, checks PlayerFighter and OpponentFighter exist, no errors | PASS |

## Commands
| # | Command | Exit | Result |
|---|---|---|---|
| 1 | dotnet build Boxing.sln -warnaserror | 0 | 0 Warning(s), 0 Error(s) |
| 2 | dotnet test tests/Core.Tests | 0 | Failed 0, Passed 35 |
| 3 | godot --headless --path . --build-solutions --quit | 0 | ok |
| 4 | godot --headless ... Smoke.tscn | 0 | `SMOKE: PASS after 120 physics frames`; 0 matches for ERROR / SCRIPT ERROR / Unhandled exception |

## Other checks
- API-drift scan (game/, src/ .cs): no hits for Spatial, KinematicBody, .Translation, yield, string Connect, Godot.Object, OS.GetTicksMsec, EditorHint, [Remote], [Puppet]. Clean.
- External assets: the diff adds no image, model, font or audio files. Clean.
- KB validity: every backticked path in docs/kb exists. Every project.godot [input] action (ui_back, menu_up, menu_down, menu_confirm) is listed in input-map.md. Valid now. This will break the moment the governance proposal is applied and menu_left/menu_right are not added to input-map.md, so kb-keeper must add them in the same change.
- No tests added by QA: the developer's tests already cover AC1-AC10 fully. I did not add a test that reads project.godot, because it would fail CI until the client applies the proposal.

## Defects
1. AC22 not met as written. project.godot has no `menu_left` / `menu_right`, and docs/kb/input-map.md does not list them. Reproduce: grep `menu_left` in project.godot and in docs/kb/input-map.md; both return nothing.
   - Judgment on the fallback: functionally acceptable. `InputActions.EnsureRegistered()` adds the actions (physical Left/Right) at runtime in CharacterSelect._Ready. It is idempotent, so it becomes a no-op once project.godot defines them. It uses valid Godot 4.7 APIs. The game behaves as specced. It does not satisfy the AC's wording, which says project.godot is inspected.
   - Recommended path: the client applies work/5/governance-proposal.md (or accepts the fallback explicitly), and kb-keeper adds both rows to input-map.md. Then AC22 flips to PASS with no code change. If the client accepts the fallback, the Producer should record the AC22 waiver.

## Risks, not verified
- Export of data/balance/fighters.json. GameSession reads `res://data/balance/fighters.json` with FileAccess. export_presets.cfg has export_filter="all_resources" and include_filter="". Godot normally does not pack non-resource files (such as .json) under that setting without an include filter such as `*.json`. The exported game may then fail to load the roster. I could not test it: export templates are not installed on this machine (the export command failed with "templates missing"). The spec asks the developer to verify this. Recommend adding include_filter="data/balance/*.json" (export_presets.cfg is not in the protected list, but it is outside QA's write paths) or checking it on a machine with templates. It does not affect editor runs or CI smoke.

## Playtest checklist for the client (G3)
Note: Left/Right work even before project.godot is updated, because of the runtime fallback.
1. Click New Game: "Choose your boxer" shows two cards side by side, each with name, style (Slugger / Out-boxer) and six stat bars with numbers. Left/Right and mouse hover move one highlight, wrapping. Both boxers' stats stay visible.
2. Pick Mateo "Ghost" Rivera (Enter or click): header becomes "Choose your opponent", shows "You: Mateo Rivera", and his card has a "YOU" tag and the highlight. A double click must not skip the opponent step.
3. Pick Bruno "The Anvil" Kessler as opponent: the ring shows the blue, tall, slim fighter on the left labelled "YOU" and the red, short, wide fighter on the right labelled "CPU", facing each other, fully visible.
4. Escape to the menu, New Game again (fresh: default highlight on the first card, no YOU tag), pick the same boxer for both: both fighters look identical, labelled YOU and CPU, no warning.
5. Escape steps back one level at a time (opponent step, then boxer step, then menu). Holding Escape about 2 s moves only one step and never quits. Stats change nothing about the ring.
