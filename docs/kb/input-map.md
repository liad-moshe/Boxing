# Input map

Lists every InputMap action: its default keys, where it is defined and which code handles it.
Maintained by kb-keeper. CI kb-check fails if an action in project.godot is missing here.

| Action | Default keys/buttons | Defined in project.godot | Handled in (`path` + `Type.Member`) |
|---|---|---|---|
| ui_quit_game | Escape (physical key) | yes, [input] section | `game/scripts/Main.cs` + `Main._UnhandledInput` |
