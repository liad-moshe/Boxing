# Feature map

Maps each player-facing feature to the code, scenes, inputs and tests behind it, so any agent can find
the right place to change. Maintained by kb-keeper. Rules: no line numbers; every backticked repo path
must exist (CI kb-check).

| Feature | Player-facing behaviour | Code anchors (`path` + `Type.Member`) | Scenes | Inputs | Tests |
|---|---|---|---|---|---|
| Main menu | Launch shows a "Boxing" title with New Game (selected first) and Exit. Up/Down wrap; mouse hover selects; Enter or left-click activates. New Game opens the ring; Exit quits | `src/Core/MenuSelection.cs` + `MenuSelection.MoveUp`, `MenuSelection.MoveDown`, `MenuSelection.Hover`; `game/scripts/Menu.cs` + `Menu._Ready` (builds the controls), `Menu._UnhandledInput` (keys), `Menu.Activate` (New Game or Exit) | `game/scenes/Menu.tscn` (main scene in project.godot) | menu_up; menu_down; menu_confirm | `tests/Core.Tests/MenuSelectionTests.cs`; `game/scenes/smoke/Smoke.cs` + `Smoke._Ready` (checks the menu builds with its two buttons); `game/scenes/smoke/Smoke.tscn` (headless smoke) |
| Escape back to menu (screen flow) | In the ring, Escape returns to the menu with New Game selected; the game stays open. A held Escape counts once. Escape in the ring never quits the app | `src/Core/ScreenFlow.cs` + `ScreenFlow.OnBack` (Ring returns GoToMenu); `game/scripts/Main.cs` + `Main._UnhandledInput` (performs the scene change to the menu) | `game/scenes/Main.tscn`; `game/scenes/Menu.tscn` | ui_back | `tests/Core.Tests/ScreenFlowTests.cs`; ring-to-menu scene change is manual (playtest), not in smoke |
| Quit game | The Exit item quits. Escape on the menu quits (Menu screen returns QuitApp) | `game/scripts/Menu.cs` + `Menu.Activate` (Exit); `game/scripts/Menu.cs` + `Menu._UnhandledInput` (Escape on menu, via `src/Core/ScreenFlow.cs` + `ScreenFlow.OnBack`) | `game/scenes/Menu.tscn` | menu_confirm; ui_back | — (manual) |
| Ring scene (pipeline proof) | Reached by New Game (not the launch scene). Shows a flat ring floor seen side-on, lit by one light | `game/scripts/Main.cs` + `Main._Ready` (builds floor, light, camera from code) | `game/scenes/Main.tscn` | — | `game/scenes/smoke/Smoke.tscn` (headless smoke) |
| Round clock | Not visible yet. A 3-minute round counts down while the ring runs | `src/Core/RoundClock.cs` + `RoundClock.Tick`; `game/scripts/Main.cs` + `Main._Process` | `game/scenes/Main.tscn` | — | `tests/Core.Tests/RoundClockTests.cs`; `game/scenes/smoke/Smoke.cs` + `Smoke._PhysicsProcess` (checks the clock advanced) |
