# Input map

Lists every InputMap action: its default keys, where it is defined and which code handles it.
Maintained by kb-keeper. CI kb-check fails if an action in project.godot is missing here.

menu_left and menu_right are NOT in project.godot. They are registered in code at startup by
`game/scripts/InputActions.cs` + `InputActions.EnsureRegistered` (called from `game/scripts/CharacterSelect.cs` + `CharacterSelect._Ready`),
which adds them only if they are missing. They are listed here because the select screen uses them.

menu_up and menu_down move the selection on the main menu, and on the Character Select screen they toggle the stance (Orthodox <-> Southpaw) of the boxer picked in the current step. They are not used on the ring.

The fight_* actions (ring only) are defined in project.godot with physical keycodes, one key each. They are
read every physics tick by `game/scripts/FightController.cs` + `FightController._PhysicsProcess`: "held"
actions use IsActionPressed, "fresh press" actions use IsActionJustPressed. The arrow keys are not bound to
any fight action. Escape (ui_back) is unchanged on the ring.

| Action | Default keys/buttons | Defined in project.godot | Handled in (`path` + `Type.Member`) |
|---|---|---|---|
| ui_back | Escape (physical key) | yes, [input] section | `game/scripts/Menu.cs` + `Menu._UnhandledInput`; `game/scripts/CharacterSelect.cs` + `CharacterSelect._UnhandledInput`; `game/scripts/Main.cs` + `Main._UnhandledInput` |
| menu_up | Up arrow (physical key) | yes, [input] section | `game/scripts/Menu.cs` + `Menu._UnhandledInput` (moves selection up); `game/scripts/CharacterSelect.cs` + `CharacterSelect._UnhandledInput` (fresh press toggles the stance via `CharacterSelect.ToggleStance`) |
| menu_down | Down arrow (physical key) | yes, [input] section | `game/scripts/Menu.cs` + `Menu._UnhandledInput` (moves selection down); `game/scripts/CharacterSelect.cs` + `CharacterSelect._UnhandledInput` (fresh press toggles the stance via `CharacterSelect.ToggleStance`) |
| menu_confirm | Enter and keypad Enter (physical keys) | yes, [input] section | `game/scripts/Menu.cs` + `Menu._UnhandledInput`; `game/scripts/CharacterSelect.cs` + `CharacterSelect._UnhandledInput` |
| menu_left | Left arrow (physical key) | no, registered in code by `game/scripts/InputActions.cs` + `InputActions.EnsureRegistered` | `game/scripts/CharacterSelect.cs` + `CharacterSelect._UnhandledInput` (highlight previous card, wraps) |
| menu_right | Right arrow (physical key) | no, registered in code by `game/scripts/InputActions.cs` + `InputActions.EnsureRegistered` | `game/scripts/CharacterSelect.cs` + `CharacterSelect._UnhandledInput` (highlight next card, wraps) |
| fight_forward | D (physical key), held | yes, [input] section | `game/scripts/FightController.cs` + `FightController._PhysicsProcess` (walk toward the opponent; with fight_back cancels out) |
| fight_back | A (physical key), held | yes, [input] section | `game/scripts/FightController.cs` + `FightController._PhysicsProcess` (walk away from the opponent) |
| fight_jab | J (physical key), fresh press | yes, [input] section | `game/scripts/FightController.cs` + `FightController._PhysicsProcess` (jab with the lead hand: left glove in orthodox, right in southpaw; key repeat and holding ignored) |
| fight_cross | K (physical key), fresh press | yes, [input] section | `game/scripts/FightController.cs` + `FightController._PhysicsProcess` (cross with the rear hand: right glove in orthodox, left in southpaw; fresh press only) |
| fight_guard | S (physical key), held | yes, [input] section | `game/scripts/FightController.cs` + `FightController._PhysicsProcess` (guard up while held, down on release) |
| fight_slip_left | Q (physical key), fresh press | yes, [input] section | `game/scripts/FightController.cs` + `FightController._PhysicsProcess` (slip to the boxer's left) |
| fight_slip_right | E (physical key), fresh press | yes, [input] section | `game/scripts/FightController.cs` + `FightController._PhysicsProcess` (slip to the boxer's right) |
