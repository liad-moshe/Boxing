# QA report: task 2 main menu

VERDICT: PASS

First QA cycle (not a repeat FAIL). One follow-up is required before the PR: the KB docs are stale (see "KB check").

## Acceptance criteria
| AC# | How verified | Result |
|---|---|---|
| AC1-AC8 | xUnit `MenuSelectionTests` (17 tests pass in total) | PASS |
| AC9 | Menu.tscn is the main scene in project.godot. Menu.cs builds New Game then Exit, with index 0 highlighted by `Refresh()`. Smoke instantiates the menu and finds both buttons. Visual check is the client's playtest. | PASS (code and smoke), playtest to confirm |
| AC10 | Code review: Down moves to index 1, Enter calls `Activate(1)`, which calls `Quit()`. Needs a window. | PLAYTEST |
| AC11 | Code review: `Activate(0)` calls `ChangeSceneToFile(Main.tscn)`. | PLAYTEST |
| AC12 | Code review: `button.Pressed` calls `Activate(index)`. Needs a real mouse. | PLAYTEST |
| AC13 | Code review: `MouseEntered` calls `Hover`, and there is no mouse-exit handler, so the selection stays. Needs a real mouse. | PLAYTEST |
| AC14 | Code review: `ui_back` in the menu goes through `ScreenFlow.OnBack(Menu)` and calls `Quit()`. | PLAYTEST |
| AC15 | project.godot inspected. `run/main_scene` is Menu.tscn. `ui_back` is physical 4194305 (Escape). `menu_up` is 4194320 (Up). `menu_down` is 4194322 (Down). `menu_confirm` is 4194309 and 4194310 (Enter and keypad Enter). `ui_quit_game` is absent. | PASS |
| AC16, AC17 | xUnit `ScreenFlowTests` | PASS |
| AC18 | Code review: `Main._UnhandledInput` calls `OnBack(Ring)`, which returns GoToMenu, then `ChangeSceneToFile(Menu)`. The menu is built fresh with index 0. | PLAYTEST |
| AC19 | Code review: `IsActionPressed` ignores echo by default, and `_leaving` guards a second press. Needs a held key. | PLAYTEST |
| AC20 | Code review: the scene is freed on change and state is per instance. Needs a real run. | PLAYTEST |

Only AC1-AC8, AC15, AC16 and AC17 are fully machine-verified. AC9 is partly machine-verified (code and smoke). AC10-AC14 and AC18-AC20 need the client's playtest, as the spec says. I wrote no extra tests under `tests/`, because the developer's tests already cover AC1-AC8, AC16 and AC17.

## Commands
| # | Command | Exit | Result |
|---|---|---|---|
| 1 | `dotnet build Boxing.sln -warnaserror` | 0 | 0 Warning(s), 0 Error(s) |
| 2 | `dotnet test tests/Core.Tests` | 0 | Passed 17, Failed 0 |
| 3 | `godot --headless --path . --build-solutions --quit` | 0 | OK. `git status` is clean afterwards (no tab rewrite). |
| 4 | `godot --headless ... Smoke.tscn` | 0 | Output contains `SMOKE: PASS`. Zero matches for ERROR, SCRIPT ERROR or Unhandled exception. |

## API-drift scan
I grepped all `.cs` files under game/, src/ and tests/ for Spatial, KinematicBody, .Translation, yield, string Connect, Godot.Object, OS.GetTicksMsec, EditorHint, [Remote] and [Puppet]. There were no hits.

## ui_quit_game references
- Gone from project.godot, `src/`, `game/` and `tests/`. PASS.
- It remains in `docs/kb/input-map.md`, `docs/kb/feature-map.md` and `docs/kb/ui-map.md` (stale).
- It also remains in `.claude/skills/godot-csharp-api/SKILL.md` as an example string. That path is protected, so it is for the client to decide.

## No external assets
The diff adds no image, model, font or audio file. PASS.

## KB check
- Every backticked path in `docs/kb` exists. PASS.
- `docs/kb/input-map.md` does NOT list `ui_back`, `menu_up`, `menu_down` or `menu_confirm`, and it still lists `ui_quit_game`.
- `ui-map.md` and `feature-map.md` have no Menu entries.
- This fails the KB-still-valid rule as it stands. brief.md schedules kb-keeper after QA, so I did not count it as a task defect.
- kb-keeper MUST run before the PR is opened. The CI kb-check would fail otherwise.

## Defects
None in the implementation.

## Notes
- Smoke covers only that the menu builds and has its two buttons. The ring-to-menu scene change is not covered headless, and the spec allows that.
- The Smoke queue-frees the menu immediately. This is harmless.

## Playtest checklist for the client
1. Launch the game. The menu appears first, with "New Game" highlighted (warm red with a border) and "Exit" below it. Press Down, Down, Down, Up with no mouse. The highlight alternates and wraps, and never disappears.
2. Hover each item with the mouse. The item under the pointer highlights. Move onto empty background. The last item stays highlighted. Up and Down continue from it, and Enter activates it.
3. Click New Game, or press Enter on it. The ring appears. Press Escape once. You are back at the menu with New Game highlighted, and the game is still open.
4. In the ring, hold Escape for about 2 seconds. You land on the menu once and the game stays open. Then do New Game, Escape, New Game again. The ring loads each time with no errors.
5. On the menu, press Escape. The game closes. Relaunch and click Exit (or select it and press Enter). The game closes. Also hold Enter and double-click New Game. There should be no errors and no double load.

## Re-check after review fixes

Commands (exit code, result):
| # | Command | Exit | Result |
|---|---|---|---|
| 1 | `dotnet build Boxing.sln -warnaserror` | 0 | 0 Warning(s), 0 Error(s) |
| 2 | `dotnet test tests/Core.Tests` | 0 | Passed 16, Failed 0 (17 before, minus the removed vacuous test) |
| 3 | `godot --headless --path . --build-solutions --quit` | 0 | OK, `git status` clean afterwards |
| 4 | `godot --headless ... Smoke.tscn` | 0 | `SMOKE: PASS`, 0 matches for ERROR, SCRIPT ERROR or Unhandled exception |

Review fixes checked by code:
- Keyboard and click work with no mouse motion. `_UnhandledInput` (Up, Down, Confirm, Back) and `button.Pressed` call `Activate` and never read `_mouseMoved`. PASS.
- Hover is ignored until the first mouse motion. `_Input` sets `_mouseMoved` on `InputEventMouseMotion`, and `OnHover` returns early while it is false. PASS.
- Scene paths: the only `res://game/scenes` strings in `.cs` files are in `ScenePaths.cs`. The match in `Smoke.cs` is a comment. Menu.cs and Main.cs use `ScenePaths.Ring` and `ScenePaths.Menu`. PASS.
- `ui_quit_game` has no hits in `src/`, `game/`, `tests/` or `project.godot`. The dead Quit branch is gone from `Main.cs`. PASS.
- Styleboxes are cached as readonly fields, and `ApplyStyle` reuses them. PASS.
- API-drift scan over all `.cs` files: 0 hits. PASS.
- External assets: the diff adds no image, model, font or audio file. PASS.

Not re-run: the KB check (docs only). Its stale-KB follow-up from the first cycle is unchanged, and the later wip commit "kb update" was not re-audited here.
The first-motion guard needs a real mouse, so it stays on the client's playtest (checklist items 1 and 2 above).

VERDICT: PASS
