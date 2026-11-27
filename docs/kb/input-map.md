# Input map

Lists every InputMap action: its default keys, where it is defined and which code handles it.
Maintained by kb-keeper. CI kb-check fails if an action in project.godot is missing here.

menu_left and menu_right are NOT in project.godot. They are registered in code at startup by
`game/scripts/InputActions.cs` + `InputActions.EnsureRegistered` (called from `game/scripts/CharacterSelect.cs` + `CharacterSelect._Ready`),
which adds them only if they are missing. They are listed here because the select screen uses them.

menu_up and menu_down do nothing on the Character Select screen (only the main menu handles them).

| Action | Default keys/buttons | Defined in project.godot | Handled in (`path` + `Type.Member`) |
|---|---|---|---|
| ui_back | Escape (physical key) | yes, [input] section | `game/scripts/Menu.cs` + `Menu._UnhandledInput`; `game/scripts/CharacterSelect.cs` + `CharacterSelect._UnhandledInput`; `game/scripts/Main.cs` + `Main._UnhandledInput` |
| menu_up | Up arrow (physical key) | yes, [input] section | `game/scripts/Menu.cs` + `Menu._UnhandledInput` |
| menu_down | Down arrow (physical key) | yes, [input] section | `game/scripts/Menu.cs` + `Menu._UnhandledInput` |
| menu_confirm | Enter and keypad Enter (physical keys) | yes, [input] section | `game/scripts/Menu.cs` + `Menu._UnhandledInput`; `game/scripts/CharacterSelect.cs` + `CharacterSelect._UnhandledInput` |
| menu_left | Left arrow (physical key) | no, registered in code by `game/scripts/InputActions.cs` + `InputActions.EnsureRegistered` | `game/scripts/CharacterSelect.cs` + `CharacterSelect._UnhandledInput` (highlight previous card, wraps) |
| menu_right | Right arrow (physical key) | no, registered in code by `game/scripts/InputActions.cs` + `InputActions.EnsureRegistered` | `game/scripts/CharacterSelect.cs` + `CharacterSelect._UnhandledInput` (highlight next card, wraps) |
