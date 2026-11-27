# UI map

Lists every screen and control, the signal or event it raises, and the code that reacts to it.
Maintained by kb-keeper. No line numbers; backticked repo paths must exist (CI kb-check).

| Screen | Control | Signal/event | Handler (`path` + `Type.Member`) | Effect |
|---|---|---|---|---|
| Main (3D view, no UI controls yet) | — (keyboard only) | ui_quit_game action (InputEvent) | `game/scripts/Main.cs` + `Main._UnhandledInput` | Quits the game |
