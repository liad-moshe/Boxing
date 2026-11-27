# Governance proposal (task 5): menu_left and menu_right in project.godot

project.godot is outside the developer's write paths. The select screen needs two new input actions (AC22).

Until this is applied, `InputActions.EnsureRegistered()` (game/scripts/InputActions.cs) adds the two actions
at runtime if they are missing, so the game works either way. Once project.godot has them, that helper is a no-op.

Add this to the `[input]` section of project.godot, directly before `menu_confirm={`:

```
menu_left={
"deadzone": 0.2,
"events": [Object(InputEventKey,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"pressed":false,"keycode":0,"physical_keycode":4194319,"key_label":0,"unicode":0,"location":0,"echo":false,"script":null)
]
}
menu_right={
"deadzone": 0.2,
"events": [Object(InputEventKey,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"pressed":false,"keycode":0,"physical_keycode":4194321,"key_label":0,"unicode":0,"location":0,"echo":false,"script":null)
]
}
```

(4194319 = Key.Left, 4194321 = Key.Right, physical keycodes, same format as menu_up / menu_down.)
