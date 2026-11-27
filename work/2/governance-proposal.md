# Governance proposal (task 2): project.godot edit

My path-guard hook only allows src/, game/, tests/, data/, docs/decisions/, work/. project.godot is outside it,
though the task and spec AC15 require changing it. The client (or the Producer, if allowed) must apply this.

## Why
Main scene must become the menu, and the Escape action is renamed ui_quit_game -> ui_back, plus 3 new menu actions.
game/scripts/Menu.cs and Main.cs already use these names.

## Exact change
1. `[application]`: `run/main_scene="res://game/scenes/Menu.tscn"` (was Main.tscn).
2. `[input]`: remove `ui_quit_game`; add these actions, each `"deadzone": 0.2`, events are InputEventKey with
   `keycode:0` and the physical_keycode below (same Object(...) form as the existing ui_quit_game entry):
   - `ui_back`: physical_keycode 4194305 (Escape)
   - `menu_up`: 4194320 (Up)
   - `menu_down`: 4194322 (Down)
   - `menu_confirm`: 4194309 (Enter) and 4194310 (KP Enter)

Simplest: rename the existing `ui_quit_game=` key to `ui_back=`, then append the three new blocks copied from it
with the physical_keycode values changed.
