VERDICT: PASS

Second QA pass on task 5. The first pass failed AC22 only. AC22 was then amended: menu_left and menu_right are registered in code, not in project.godot.

## Acceptance criteria
| AC | How verified | Result |
|---|---|---|
| AC1 | FighterRosterTests.Shipped_HasAnvilThenGhost_WithAllStatsInRange | PASS |
| AC2 | FighterRosterTests.Shipped_StatTotalsEqual | PASS |
| AC3 | FighterRosterTests.Shipped_NeitherBoxerDominates | PASS |
| AC4 | MissingStat_Throws, StatOutOfRange_Throws, DuplicateIds_Throws, EmptyList_Throws | PASS |
| AC5-AC10 | CharacterSelectFlowTests (all 35 tests pass) | PASS |
| AC11-AC21 | Code review plus smoke (both cards build); playtest pending | PASS by review; playtest pending |
| AC22 (amended) | game/scripts/InputActions.cs `EnsureRegistered` adds menu_left (Key.Left, PhysicalKeycode) and menu_right (Key.Right, PhysicalKeycode) if missing. It is called from CharacterSelect._Ready (line 47). docs/kb/input-map.md has both rows and a note that they are code-registered and not in project.godot. | PASS |
| AC23 | Smoke launches Main.tscn directly with both fighters, no errors | PASS |

## Commands
| # | Command | Exit | Result |
|---|---|---|---|
| 1 | dotnet build Boxing.sln -warnaserror | 0 | 0 Warning(s), 0 Error(s) |
| 2 | dotnet test tests/Core.Tests | 0 | Failed 0, Passed 35 |
| 3 | godot --headless --path . --build-solutions --quit | 0 | ok |
| 4 | godot --headless ... Smoke.tscn | 0 | `SMOKE: PASS after 120 physics frames`; no ERROR / SCRIPT ERROR / Unhandled exception |

## Other checks
- API-drift scan (game/, src/ .cs): no hits for any Godot 3 name. Clean.
- External assets: none added.
- KB validity: every backticked path in docs/kb exists. Every project.godot [input] action is listed in input-map.md.
- Minor doc nit, not a failure: the XML comment in InputActions.cs still says the actions "belong in project.godot (see work/5/governance-proposal.md)". It is stale after the amendment, and work/5 is deleted at ship. The kb-keeper may want it reworded.

## Observations and risks (not failures)
- kb-keeper observation: `ScreenFlow.OnBack(CharacterSelect)` is not used by game code. No spec AC is broken.
- Unverified risk: export packing of `data/balance/fighters.json`. export_presets.cfg uses export_filter="all_resources" with an empty include_filter, so Godot may not pack the .json file. GameSession reads it with FileAccess. Export templates are not installed here, so I could not test it. It does not affect editor runs or CI smoke. Recommend include_filter="data/balance/*.json" or a check on a machine with templates before any release export.

## Defects
None.

## Playtest checklist for the client (G3)
1. Click New Game: "Choose your boxer" shows two cards side by side, each with name, style (Slugger / Out-boxer) and six stat bars with numbers. Left/Right and mouse hover move one highlight, wrapping.
2. Pick Mateo "Ghost" Rivera (Enter or click): header becomes "Choose your opponent", shows "You: Mateo Rivera", and his card has a "YOU" tag. A double click must not skip the opponent step.
3. Pick Bruno "The Anvil" Kessler as opponent: the ring shows the blue, tall, slim fighter labelled "YOU" and the red, short, wide fighter labelled "CPU", facing each other, fully visible.
4. Escape to the menu, New Game again (fresh: default highlight on the first card, no YOU tag), pick the same boxer for both: both look identical, labelled YOU and CPU, no warning.
5. Escape steps back one level at a time (opponent, boxer, menu). Holding Escape about 2 s moves only one step and never quits.
