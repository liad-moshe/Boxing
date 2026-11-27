# Spec: Main menu

Status: DRAFT   Issue: #2

## Goal
Give the game a start screen where the player picks New Game or Exit, by mouse or keyboard. This is the first thing the player sees, so launch-to-play stays a few seconds (vision pillar 3).

## Player story
As a player, I want to choose New Game or Exit with the mouse or the arrow keys and Enter, so that I can start or leave the game without reading any instructions.

## Menu items
Fixed order, top to bottom. Index 0 is first.

| Index | Label | Activation result |
|---|---|---|
| 0 | New Game | Placeholder: switch to the existing ring scene (`game/scenes/Main.tscn`, the primitive ring and side-on camera). When a real game scene exists, only this target changes. |
| 1 | Exit | Quit the application (same effect as the existing `ui_quit_game`). |

Licenses (listed in roadmap M1) is OUT OF SCOPE here; see Open questions.

## Selection behaviour
One selected index (0 or 1) is shared by mouse and keyboard. There is never "no selection".
- Initial state: index 0 (New Game) is selected when the menu appears.
- Up moves the index by -1, Down by +1. Wrap-around: Up on index 0 goes to the last item; Down on the last item goes to index 0.
- Mouse hover over an item makes that item the selected index immediately (hover and keyboard highlight are the same highlight, never two at once).
- Mouse leave: leaving an item does NOT clear the selection. The last selected item stays highlighted, so Enter still works after the mouse rests outside the buttons.
- Keyboard after hover: Up/Down move from whatever index hover last set.
- Mouse left-click on an item activates that item (and selects it). Enter activates the selected item.
- Keyboard input does not move the mouse cursor; a stationary mouse never overrides a keyboard selection (only mouse movement onto an item, i.e. a new hover event, does).
- Activation is ignored if the menu is already leaving (no double activation from a double click or Enter held down).

## Controls
Actions to add to the input map (names for project.godot and docs/kb/input-map.md):

| Action | Key | Notes |
|---|---|---|
| menu_up | Up arrow | Select previous item, wraps. |
| menu_down | Down arrow | Select next item, wraps. |
| menu_confirm | Enter (also keypad Enter) | Activate selected item. |
| (mouse) | Hover / left click | Handled by Control mouse signals, no InputMap action. |

Existing `ui_quit_game` (Escape) is unchanged and still quits from the menu. No clash with docs/kb/input-map.md. Do not reuse Godot's built-in `ui_up`, `ui_down`, `ui_accept`, so the game's own menu actions stay explicit and remappable later.

## Visual description (code-built, no external assets)
- Built entirely from Control nodes in code: a full-screen `ColorRect` background, a title `Label`, and a `VBoxContainer` with two `Button`-style items centred on screen. Default Godot theme and font; no images, fonts or sounds.
- Background: flat dark colour (suggested near-black blue-grey). Title "Boxing" (working title) large, light colour, above the items.
- Items: equal width and height, stacked vertically with a clear gap, labels centred.
- Normal item: dark grey fill, light text. Selected item: clearly different fill (suggested warm red or amber) with brighter text, plus a visible border, so it is readable without relying on colour alone. Pressed (mouse down) may darken the selected fill slightly.
- The exact colours and sizes are the developer's call; they are constants in one place, not a style decision (art style is M2).
- Keyboard focus and the selected index must never disagree: the highlighted item is always the one Enter activates.

## Acceptance criteria
Selection logic lives in src/Core as a pure type (no Godot reference), for example `MenuSelection(itemCount)` with `SelectedIndex`, `MoveUp()`, `MoveDown()`, `Hover(index)`. AC1 to AC8 are xUnit tests on that type; AC9 to AC13 are checked in the running game.
- AC1. **Given** a new selection over 2 items, **when** it is created, **then** `SelectedIndex` is 0.
- AC2. **Given** index 0, **when** MoveDown is called, **then** index is 1.
- AC3. **Given** index 1 (last), **when** MoveDown is called, **then** index wraps to 0.
- AC4. **Given** index 0, **when** MoveUp is called, **then** index wraps to 1 (last).
- AC5. **Given** index 0, **when** Hover(1) is called, **then** index is 1, and a following MoveDown gives 0 (keyboard continues from hover).
- AC6. **Given** index 1 after a hover, **when** the mouse leaves the item (no selection call is made), **then** index stays 1.
- AC7. **Given** any state, **when** Hover is called with an index below 0 or at/above the item count, **then** the index is unchanged and no exception is thrown.
- AC8. **Given** a selection constructed with an item count below 1, **when** it is created, **then** it is rejected (argument exception), so an empty menu cannot exist.
- AC9. **Given** the game is launched, **when** the first frame shows, **then** the main menu is displayed (not the ring), with "New Game" first and highlighted and "Exit" second.
- AC10. **Given** the menu is shown, **when** the player presses Down then Enter, **then** the application quits.
- AC11. **Given** the menu is shown, **when** the player presses Enter with the initial selection, **then** the ring scene loads (placeholder New Game) and the menu is gone.
- AC12. **Given** the mouse is over Exit, **when** the player left-clicks, **then** the application quits; **and given** the mouse is over New Game, **when** clicked, **then** the ring scene loads.
- AC13. **Given** the mouse hovers New Game then moves to empty background, **when** the player presses Enter, **then** New Game (the last highlighted item) activates; and Up/Down keep working from that item.
- AC14. **Given** the menu is shown, **when** the player presses Escape, **then** the application quits (existing `ui_quit_game` behaviour preserved).
- AC15. **Given** the project is built, **when** `project.godot` is inspected, **then** `menu_up`, `menu_down`, `menu_confirm` exist with the keys above and the main scene is the menu scene.

## Balance values
No gameplay numbers. UI colours, sizes and spacing are constants in the menu script, not tunables. Pointer: none (no data/balance file).

## Boxing notes
none. No real-boxing claims.

## Out of scope
- Licenses screen (roadmap M1); gamepad, rebinding, settings, volume, difficulty, save/continue.
- Real game flow behind New Game; returning from the ring to the menu (Escape in the ring still quits for now).
- Art style, custom fonts, animation, transitions, sound, logo.
- Touch input, mouse wheel, Tab/focus-navigation beyond the Up/Down/Enter above.

## Open questions
1. Licenses (roadmap M1 menu: Start, Quit, Licenses) is deliberately NOT part of this task, per the task request. Proposed follow-up task: add a third item "Licenses" (index 2) showing the engine license notice from docs/licenses/README.md. The selection type already supports any item count, so that task adds a row, not new logic. Client: confirm a follow-up task.
2. New Game placeholder loads the existing pipeline-proof ring scene (`game/scenes/Main.tscn`) because it is the only scene. The client may prefer a simple "Coming soon" message instead; this spec chooses the ring because it is playable proof the button works.
3. In the ring, Escape quits the whole app, not back to the menu. Return-to-menu is left for the game-flow task.

## Playtest checklist
Five things the client checks by hand at G3.
1. Launch the game: the menu appears first, with "New Game" highlighted at the top and "Exit" below it.
2. Press Down, Down, Down, Up with no mouse: the highlight alternates and wraps between the two items (never gets stuck or disappears).
3. Move the mouse over each item: the item under the pointer highlights; move the pointer onto empty background: the last item stays highlighted, and pressing Up/Down continues from it.
4. Click New Game (or Enter on it): the ring scene appears. Relaunch, click Exit (or select it and press Enter): the game closes.
5. Relaunch and press Escape on the menu: the game closes. Also check that holding Enter or double-clicking New Game does not cause errors or a double load.
