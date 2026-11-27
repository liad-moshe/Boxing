---
name: kb-update
description: Formats and procedure for the docs/kb knowledge base (feature-map, input-map, ui-map) - how to derive input actions from project.godot [input], signal connections from .tscn [connection] lines and C# event subscriptions, and the rules CI kb-check enforces. Use when updating docs/kb after a code change.
user-invocable: false
---

# Knowledge base update (environment-spec.md §9)

## Table formats (keep these headers exactly)
**docs/kb/feature-map.md**
`| Feature | Player-facing behaviour | Code anchors (`path` + `Type.Member`) | Scenes | Inputs | Tests |`

**docs/kb/input-map.md**
`| Action | Default keys/buttons | Defined in project.godot | Handled in (`path` + `Type.Member`) |`

**docs/kb/ui-map.md**
`| Screen | Control | Signal/event | Handler (`path` + `Type.Member`) | Effect |`

## Anchor rules
- Code anchor = a backticked repo-relative path, then `+`, then a backticked `Type.Member`. For example:
  `` `game/scripts/Main.cs` + `Main._UnhandledInput` ``.
- **Never use line numbers.** Lines move; type and member names are stable.
- Backtick only real repo paths and `Type.Member` names. Write action names and key names unformatted.
- Several anchors go in one cell, separated by `;`.

## CI kb-check fails when
1. A backticked repo path anywhere in docs/kb does not exist, or
2. An action in project.godot's `[input]` section is missing from input-map.md.

Check both yourself before you finish. Use Glob to confirm each path, and compare the action lists.

## Deriving entries (use Grep / Glob / Read)
**Input actions (project.godot):**
- The `[input]` section runs from `[input]` to the next `[section]` line. Each action starts at column 0
  as `name={`.
- Key: `"physical_keycode":<n>` (or `"keycode"`) inside `InputEventKey`. Letters and digits are their
  ASCII codes (`74` = J). Special keys are 4194304 + k, e.g. `4194305` = Escape. To confirm, search
  GodotSharp.xml for `F:Godot.Key.` or read the `Key` enum in the 4.7 docs.
- Mouse buttons and joypad events: `InputEventMouseButton` (`"button_index"`),
  `InputEventJoypadButton`.
- Handlers: Grep the C# for `"<action>"` and for `StringName` constants that hold it
  (`IsActionPressed`, `IsActionJustPressed`, `Input.GetAxis`, ...).

**Signal connections in scenes (.tscn):**
- Lines like `[connection signal="pressed" from="Menu/StartButton" to="." method="OnStartPressed"]`.
  That means: Screen = the scene, Control = `from`, Signal = `signal`, Handler = the `method` on the
  script attached to `to`.

**Signal subscriptions in C#:**
- Grep `+= ` on events, e.g. `startButton.Pressed += OnStartPressed;`, and `Connect(` with
  `SignalName.` arguments.
- Custom signals are declared as `[Signal] public delegate void XEventHandler(...)`. The event is `X`.

**Features:**
- Start from the task's spec (docs/specs/) for the player-facing wording.
- Anchor the Core type and member that implements the rule, plus the Godot adapter that triggers it.
- Tests: the test file paths in tests/, and the smoke scene if it covers the feature.

## Procedure
1. Get the changed files from the Producer's brief (or `git diff --name-only main...HEAD`).
2. For each change: add rows, update rows, and delete rows for removed code.
3. Verify both CI rules. Report rows added, changed and removed per file.
