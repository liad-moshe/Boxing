# Input map

Lists every InputMap action: its default keys, where it is defined and which code handles it.
Maintained by kb-keeper. CI kb-check fails if an action in project.godot is missing here.

| Action | Default keys/buttons | Defined in project.godot | Handled in (`path` + `Type.Member`) |
|---|---|---|---|
| ui_back | Escape (physical key) | yes, [input] section | `game/scripts/Menu.cs` + `Menu._UnhandledInput`; `game/scripts/Main.cs` + `Main._UnhandledInput` |
| menu_up | Up arrow (physical key) | yes, [input] section | `game/scripts/Menu.cs` + `Menu._UnhandledInput` |
| menu_down | Down arrow (physical key) | yes, [input] section | `game/scripts/Menu.cs` + `Menu._UnhandledInput` |
| menu_confirm | Enter and keypad Enter (physical keys) | yes, [input] section | `game/scripts/Menu.cs` + `Menu._UnhandledInput` |
