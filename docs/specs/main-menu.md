# Spec: Main menu

Status: DRAFT   Issue: #2

## Goal
Give the game a start screen where the player picks New Game or Exit, by mouse or keyboard, and a way back to it from the ring. This is the first thing the player sees, so launch-to-play stays a few seconds (vision pillar 3).

## Player story
As a player, I want to choose New Game or Exit with the mouse or the arrow keys and Enter, and to press Escape in the ring to go back to the menu, so that I can start, leave, or restart without reading any instructions.

## Menu items
Fixed order, top to bottom. Index 0 is first.

| Index | Label | Activation result |
|---|---|---|
| 0 | New Game | Placeholder: switch to the existing ring scene (`game/scenes/Main.tscn`, the primitive ring and side-on camera). Client confirmed this target. When a real game scene exists, only this target changes. |
| 1 | Exit | Quit the application. |

Licenses (listed in roadmap M1) is OUT OF SCOPE here; it is a separate follow-up task.

## Selection behaviour
One selected index (0 or 1) is shared by mouse and keyboard. There is never "no selection".
- Initial state: index 0 (New Game) is selected every time the menu appears, including when the player returns from the ring.
- Up moves the index by -1, Down by +1. Wrap-around: Up on index 0 goes to the last item; Down on the last item goes to index 0.
- Mouse hover over an item makes that item the selected index immediately (hover and keyboard highlight are the same highlight, never two at once).
- Mouse leave: leaving an item does NOT clear the selection. The last selected item stays highlighted, so Enter still works after the mouse rests outside the buttons.
- Keyboard after hover: Up/Down move from whatever index hover last set.
- Mouse left-click on an item activates that item (and selects it). Enter activates the selected item.
- Keyboard input does not move the mouse cursor; a stationary mouse never overrides a keyboard selection (only mouse movement onto an item, i.e. a new hover event, does).
- Activation is ignored if the menu is already leaving (no double activation from a double click or Enter held down).

## Screen flow and Escape
Two screens: Menu and Ring. Escape is one action with a different result per screen:

| Current screen | Escape result |
|---|---|
| Menu | Quit the application (unchanged from today; no conflict with the new flow, because the menu is the top of the flow and there is nothing further back). |
| Ring | Return to the main menu (CHANGED: previously quit the app). The ring scene is unloaded, the menu is shown fresh with New Game selected. |

- Escape while in the ring never quits the app. To quit from the ring the player goes Escape, then Exit (or Escape twice, with separate key presses).
- Key repeat does not count: a held Escape produces one action. Holding Escape in the ring must return to the menu and stop there; it must not carry on and quit the app. Only a fresh press (not an echo) triggers Escape.
- Escape is ignored while a screen change is already in progress (no double transition).
- Returning to the menu discards the ring state; New Game starts the ring again from scratch (the ring is a placeholder with only a round clock, so nothing is saved).
- Mouse and Enter in the menu behave exactly as in "Selection behaviour"; Escape in the ring is keyboard only.

## Controls
Actions to add to the input map (names for project.godot and docs/kb/input-map.md):

| Action | Key | Notes |
|---|---|---|
| menu_up | Up arrow | Select previous item, wraps. |
| menu_down | Down arrow | Select next item, wraps. |
| menu_confirm | Enter (also keypad Enter) | Activate selected item. |
| ui_back | Escape (physical key) | Replaces `ui_quit_game` (same key, same physical binding). Menu: quit app. Ring: return to menu. Ignores key repeat. |
| (mouse) | Hover / left click | Handled by Control mouse signals, no InputMap action. |

The old action `ui_quit_game` is renamed to `ui_back` because "quit" no longer describes what Escape does in the ring. It must be removed from project.godot and from docs/kb/input-map.md (the row is replaced, not duplicated), and the code that handled it in `game/scripts/Main.cs` is updated to the new meaning. No other key clashes with docs/kb/input-map.md. Do not reuse Godot's built-in `ui_up`, `ui_down`, `ui_accept`, so the game's own menu actions stay explicit and remappable later.

## Visual description (code-built, no external assets)
- Built entirely from Control nodes in code: a full-screen `ColorRect` background, a title `Label`, and a `VBoxContainer` with two `Button`-style items centred on screen. Default Godot theme and font; no images, fonts or sounds.
- Background: flat dark colour (suggested near-black blue-grey). Title "Boxing" (working title) large, light colour, above the items.
- Items: equal width and height, stacked vertically with a clear gap, labels centred.
- Normal item: dark grey fill, light text. Selected item: clearly different fill (suggested warm red or amber) with brighter text, plus a visible border, so it is readable without relying on colour alone. Pressed (mouse down) may darken the selected fill slightly.
- The exact colours and sizes are the developer's call; they are constants in one place, not a style decision (art style is M2).
- Keyboard focus and the selected index must never disagree: the highlighted item is always the one Enter activates.
- No on-screen hint or button is required for "Escape to go back" in the ring (no UI in the ring yet).

## Acceptance criteria
Logic lives in src/Core as pure types (no Godot reference), so it is unit-tested:
- Selection: `MenuSelection(itemCount)` with `SelectedIndex`, `MoveUp()`, `MoveDown()`, `Hover(index)`.
- Back navigation: a pure function, for example `ScreenFlow.OnBack(Screen current)`, where `Screen` is `Menu` or `Ring`, returning a result of `QuitApp` or `GoToMenu`. The Godot adapters only call it and perform the returned action (quit the tree, or change scene).

AC1 to AC8 and AC16 to AC17 are xUnit tests on these types. AC9 to AC15 and AC18 to AC20 are checked in the running game (playtest, and the headless smoke scene where practical).
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
- AC11. **Given** the menu is shown, **when** the player presses Enter with the initial selection, **then** the ring scene (`Main.tscn`) loads and the menu is gone.
- AC12. **Given** the mouse is over Exit, **when** the player left-clicks, **then** the application quits; **and given** the mouse is over New Game, **when** clicked, **then** the ring scene loads.
- AC13. **Given** the mouse hovers New Game then moves to empty background, **when** the player presses Enter, **then** New Game (the last highlighted item) activates; and Up/Down keep working from that item.
- AC14. **Given** the menu is shown, **when** the player presses Escape, **then** the application quits.
- AC15. **Given** the project is built, **when** `project.godot` is inspected, **then** `menu_up`, `menu_down`, `menu_confirm` and `ui_back` (Escape) exist with the keys above, `ui_quit_game` no longer exists, and the main scene is the menu scene.
- AC16. **Given** the current screen is Menu, **when** `OnBack` is evaluated, **then** the result is `QuitApp`.
- AC17. **Given** the current screen is Ring, **when** `OnBack` is evaluated, **then** the result is `GoToMenu` (never `QuitApp`).
- AC18. **Given** the ring is shown (reached via New Game), **when** the player presses Escape once, **then** the ring disappears, the main menu is shown with New Game highlighted, and the application is still running.
- AC19. **Given** the ring is shown, **when** the player holds Escape down for about two seconds, **then** the app goes back to the menu exactly once and stays running (key repeat does not quit it).
- AC20. **Given** the player went menu, ring, menu, **when** they press Enter on New Game again, **then** the ring loads again and works the same as the first time (no error, no leftover nodes from the first visit).

How the ring to menu transition is verified: the decision (Ring maps to GoToMenu) is covered by the AC16/AC17 unit tests; the actual scene change is covered by AC18 to AC20 in the playtest. The developer may additionally extend the headless smoke scene (`game/scenes/smoke/Smoke.tscn`) to load the ring, trigger the back action, and assert the menu scene is current. That is optional and does not replace the playtest.

## Balance values
No gameplay numbers. UI colours, sizes and spacing are constants in the menu script, not tunables. Pointer: none (no data/balance file).

## Boxing notes
none. No real-boxing claims.

## Out of scope
- Licenses screen (roadmap M1): a follow-up task. Also gamepad, rebinding, settings, volume, difficulty, save/continue.
- Real game flow behind New Game: pause menu, "are you sure" confirmation, resuming a bout, saving progress. Escape in the ring goes straight back to the menu and discards the ring.
- Art style, custom fonts, animation, transitions (fades), sound, logo.
- Touch input, mouse wheel, Tab/focus-navigation beyond the Up/Down/Enter/Escape above.

## Open questions
none. (Licenses scope, the New Game target, and Escape behaviour in the ring were all answered by the client.)

## Playtest checklist
Five things the client checks by hand at G3.
1. Launch the game: the menu appears first, with "New Game" highlighted at the top and "Exit" below it. Press Down, Down, Down, Up with no mouse: the highlight alternates and wraps between the two items (never gets stuck or disappears).
2. Move the mouse over each item: the item under the pointer highlights; move the pointer onto empty background: the last item stays highlighted, and pressing Up/Down continues from it.
3. Click New Game (or Enter on it): the ring scene appears. Press Escape once: you are back at the main menu with New Game highlighted, and the game is still running (it did not close).
4. From the ring, hold Escape for a couple of seconds: you land on the menu once and the game stays open. Then go New Game, Escape, New Game again: the ring loads each time without errors.
5. On the menu, press Escape: the game closes. Relaunch, click Exit (or select it and press Enter): the game closes. Also check that holding Enter or double-clicking New Game does not cause errors or a double load.
