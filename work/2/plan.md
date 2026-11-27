# Plan: task 2 main menu

- [x] 1. MenuSelection (AC1-AC8): tests in tests/Core.Tests/MenuSelectionTests.cs, then src/Core/MenuSelection.cs
- [x] 2. ScreenFlow.OnBack (AC16-AC17): tests in ScreenFlowTests.cs, then src/Core/ScreenFlow.cs
- [ ] 3. project.godot: main scene = Menu.tscn, actions menu_up/menu_down/menu_confirm, ui_quit_game -> ui_back (AC15). Test: grep project.godot  (BLOCKED: hook forbids project.godot; see governance-proposal.md)
- [x] 4. game/scripts/Menu.cs + game/scenes/Menu.tscn: code-built Controls, hover/click/keys, leaving guard, Escape quits (AC9-AC14)
- [x] 5. Main.cs: Escape (no echo) -> ScreenFlow.OnBack(Ring) -> change scene to menu, leaving guard (AC18-AC20)
- [x] 6. Smoke: also load the menu scene headless and assert it builds (optional extension)
- [x] 7. Run the four checks from docs/process/commands.md
