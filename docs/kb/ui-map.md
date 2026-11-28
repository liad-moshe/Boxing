# UI map

Lists every screen and control, the signal or event it raises, and the code that reacts to it.
Maintained by kb-keeper. No line numbers; backticked repo paths must exist (CI kb-check).

The menu and the Character Select controls are built in code (no .tscn child nodes beyond the root Control).
Menu node names are the labels with spaces removed, so the node paths are Column/NewGame and Column/Exit (relative to Menu).
The Pressed and MouseEntered connections for the menu are made in code in `Menu._Ready`, not in the scene file.
Character Select cards are named Card_<id> (ids from `data/balance/fighters.json`, e.g. Card_anvil, Card_ghost).
Their MouseEntered and GuiInput connections are made in code in `CharacterSelect.BuildCard`.
Stat rows are named Stat_<stat> and hold a Bar (Back and Fill ColorRects) and a Value label; they are built once and not updated.

| Screen | Control | Signal/event | Handler (`path` + `Type.Member`) | Effect |
|---|---|---|---|---|
| Menu (`game/scenes/Menu.tscn`) | Column/NewGame (Button) | Pressed (connected in `Menu._Ready`) | `game/scripts/Menu.cs` + `Menu.Activate` | Selects New Game, clears the picks (`game/scripts/GameSession.cs` + `GameSession.Clear`) and changes scene to Character Select (`game/scenes/CharacterSelect.tscn`) |
| Menu | Column/NewGame (Button) | MouseEntered (connected in `Menu._Ready`) | `game/scripts/Menu.cs` + `Menu.OnHover` | Selects New Game (the one highlight) |
| Menu | Column/Exit (Button) | Pressed (connected in `Menu._Ready`) | `game/scripts/Menu.cs` + `Menu.Activate` | Quits the game |
| Menu | Column/Exit (Button) | MouseEntered (connected in `Menu._Ready`) | `game/scripts/Menu.cs` + `Menu.OnHover` | Selects Exit (the one highlight) |
| Menu | — (keyboard) | menu_up / menu_down action (InputEvent) | `game/scripts/Menu.cs` + `Menu._UnhandledInput` | Moves the selection up or down, wrapping |
| Menu | — (keyboard) | menu_confirm action (InputEvent) | `game/scripts/Menu.cs` + `Menu._UnhandledInput` | Activates the selected item |
| Menu | — (keyboard) | ui_back action (InputEvent) | `game/scripts/Menu.cs` + `Menu._UnhandledInput` | Quits the game (via `src/Core/ScreenFlow.cs` + `ScreenFlow.OnBack`) |
| Character Select, step 1 and 2 (`game/scenes/CharacterSelect.tscn`) | Header (Label) | — (text set on each step) | `game/scripts/CharacterSelect.cs` + `CharacterSelect.Refresh` | Shows "Choose your boxer" (step 1) or "Choose your opponent" (step 2) |
| Character Select | YouLine (Label) | — (text set on each step) | `game/scripts/CharacterSelect.cs` + `CharacterSelect.Refresh` | Step 2 only: shows "You: <player boxer name>" |
| Character Select | Column/Cards/Card_<id> (PanelContainer) | MouseEntered (connected in `CharacterSelect.BuildCard`) | `game/scripts/CharacterSelect.cs` + `CharacterSelect.OnHover` | Highlights the card (the one highlight). Ignored until the mouse has moved and during a screen change |
| Character Select | Column/Cards/Card_<id> (PanelContainer) | GuiInput, left button pressed (connected in `CharacterSelect.BuildCard`) | `game/scripts/CharacterSelect.cs` + `CharacterSelect.OnCardInput`; `game/scripts/CharacterSelect.cs` + `CharacterSelect.Confirm` | Highlights and confirms the card: step 1 stores the player's boxer and moves to step 2; step 2 stores the opponent and changes to the ring (`game/scenes/Main.tscn`). Double click and clicks within 350 ms of the last step are ignored |
| Character Select | Column/Cards/Card_<id>/.../YouTag (Label) | — (visibility set on each step) | `game/scripts/CharacterSelect.cs` + `CharacterSelect.Refresh` | Step 2 only: shows "YOU" on the card of the player's boxer |
| Character Select | Column/Cards/Card_<id>/.../Stat_<stat> (HBoxContainer: Label, Bar, Value) | — (built once) | `game/scripts/CharacterSelect.cs` + `CharacterSelect.BuildStatRow` | Shows the stat label, a bar filled to value / statMax, and the number. Display only |
| Character Select | Column/Hint (Label) | — (static text) | `game/scripts/CharacterSelect.cs` + `CharacterSelect._Ready` | Shows the key hint line |
| Character Select | — (keyboard) | menu_left / menu_right action (InputEvent) | `game/scripts/CharacterSelect.cs` + `CharacterSelect._UnhandledInput` | Moves the single highlight to the previous or next card, wrapping |
| Character Select | — (keyboard) | menu_confirm action (InputEvent) | `game/scripts/CharacterSelect.cs` + `CharacterSelect._UnhandledInput`; `game/scripts/CharacterSelect.cs` + `CharacterSelect.Confirm` | Confirms the highlighted card (same effect as a click). Held Enter confirms once |
| Character Select | — (keyboard) | ui_back action (InputEvent) | `game/scripts/CharacterSelect.cs` + `CharacterSelect._UnhandledInput`; `game/scripts/CharacterSelect.cs` + `CharacterSelect.GoBack` (uses `src/Core/CharacterSelectFlow.cs` + `CharacterSelectFlow.Back`) | Step 2: back to step 1 with the player's boxer highlighted, opponent pick discarded. Step 1: back to the menu, picks discarded |
| Ring (`game/scenes/Main.tscn`, 3D view plus two HUD labels, no buttons) | Tag (Label3D) on PlayerFighter and OpponentFighter | — (text set at build) | `game/scripts/FighterBuilder.cs` + `FighterBuilder.Build` | Shows "YOU" above the left fighter and "CPU" above the right one |
| Ring | Hud/ContactLabel (Label, top of screen, hidden at start) | — (text and visibility set every physics tick) | `game/scripts/FightController.cs` + `FightController._PhysicsProcess` | Shows "Contact: Jab" or "Contact: Cross" near the top for the set number of ticks after a punch reaches; a newer contact replaces it and restarts the count; hidden otherwise |
| Ring | Hud/ControlsHint (Label, bottom of screen) | — (static text, built once) | `game/scripts/FightController.cs` + `FightController._Ready` | Shows the one-line controls hint: A / D move, J jab, K cross, S guard (hold), Q / E slip left / right, Esc menu |
| Ring | — (keyboard) | fight_forward, fight_back, fight_guard (held); fight_jab, fight_cross, fight_slip_left, fight_slip_right (fresh press) (InputEvent via the InputMap, read each physics tick) | `game/scripts/FightController.cs` + `FightController._PhysicsProcess` | Walks the player boxer, holds guard, throws a jab or cross, or slips. The opponent does not respond. Arrow keys do nothing here |
| Ring | — (keyboard only) | ui_back action (InputEvent) | `game/scripts/Main.cs` + `Main._UnhandledInput` | Clears the picks and returns to the menu (`game/scenes/Menu.tscn`); never quits |
