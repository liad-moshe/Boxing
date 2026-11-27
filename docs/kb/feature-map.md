# Feature map

Maps each player-facing feature to the code, scenes, inputs and tests behind it, so any agent can find
the right place to change. Maintained by kb-keeper. Rules: no line numbers; every backticked repo path
must exist (CI kb-check).

| Feature | Player-facing behaviour | Code anchors (`path` + `Type.Member`) | Scenes | Inputs | Tests |
|---|---|---|---|---|---|
| Main scene (pipeline proof) | Launching the game shows a flat ring floor seen side-on, lit by one light | `game/scripts/Main.cs` + `Main._Ready` (builds floor, light, camera from code) | `game/scenes/Main.tscn` (main scene in project.godot) | — | `game/scenes/smoke/Smoke.tscn` (headless smoke) |
| Round clock | Not visible yet. A 3-minute round counts down while Main runs | `src/Core/RoundClock.cs` + `RoundClock.Tick`; `game/scripts/Main.cs` + `Main._Process` | `game/scenes/Main.tscn` | — | `tests/Core.Tests/RoundClockTests.cs`; `game/scenes/smoke/Smoke.cs` + `Smoke._PhysicsProcess` (checks the clock advanced) |
| Quit game | Pressing Escape closes the game | `game/scripts/Main.cs` + `Main._UnhandledInput` | `game/scenes/Main.tscn` | ui_quit_game | — (manual) |
