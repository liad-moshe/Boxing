# UI map

Lists every screen and control, the signal or event it raises, and the code that reacts to it.
Maintained by kb-keeper. No line numbers; backticked repo paths must exist (CI kb-check).

The menu controls are built in code by `game/scripts/Menu.cs` (no .tscn child nodes). Button node names
are the labels with spaces removed, so the node paths are Column/NewGame and Column/Exit (relative to Menu). The Pressed and
MouseEntered connections are made in code in `Menu._Ready`, not in the scene file.

| Screen | Control | Signal/event | Handler (`path` + `Type.Member`) | Effect |
|---|---|---|---|---|
| Menu (`game/scenes/Menu.tscn`) | Column/NewGame (Button) | Pressed (connected in `Menu._Ready`) | `game/scripts/Menu.cs` + `Menu.Activate` | Selects New Game and changes scene to the ring (`game/scenes/Main.tscn`) |
| Menu | Column/NewGame (Button) | MouseEntered (connected in `Menu._Ready`) | `game/scripts/Menu.cs` + `Menu.OnHover` | Selects New Game (the one highlight) |
| Menu | Column/Exit (Button) | Pressed (connected in `Menu._Ready`) | `game/scripts/Menu.cs` + `Menu.Activate` | Quits the game |
| Menu | Column/Exit (Button) | MouseEntered (connected in `Menu._Ready`) | `game/scripts/Menu.cs` + `Menu.OnHover` | Selects Exit (the one highlight) |
| Menu | — (keyboard) | menu_up / menu_down action (InputEvent) | `game/scripts/Menu.cs` + `Menu._UnhandledInput` | Moves the selection up or down, wrapping |
| Menu | — (keyboard) | menu_confirm action (InputEvent) | `game/scripts/Menu.cs` + `Menu._UnhandledInput` | Activates the selected item |
| Menu | — (keyboard) | ui_back action (InputEvent) | `game/scripts/Menu.cs` + `Menu._UnhandledInput` | Quits the game (via `src/Core/ScreenFlow.cs` + `ScreenFlow.OnBack`) |
| Ring (`game/scenes/Main.tscn`, 3D view, no UI controls yet) | — (keyboard only) | ui_back action (InputEvent) | `game/scripts/Main.cs` + `Main._UnhandledInput` | Returns to the menu (`game/scenes/Menu.tscn`); never quits |
