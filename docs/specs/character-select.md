# Spec: Character select

Status: DRAFT   Issue: #5

## Goal
After New Game, let the player choose a boxer for themself and a boxer for the AI opponent from a roster of exactly 2, seeing each boxer's stats while browsing, then show the ring with both fighters standing ready. This gives the game its first player choice and the data foundation for later combat.

## Player story
As a player, I want to compare two boxers and pick one for me and one for my opponent (possibly the same), so that I can start a fight with the matchup I want.

## Roster (data-driven)
Defined in `data/balance/fighters.json`; the code must not hard-code names or numbers.

| Id | Name | Archetype | Stats (see data file) | Look |
|---|---|---|---|---|
| anvil | Bruno "The Anvil" Kessler | Slugger | high Power and Chin, low Speed and Defense | red trunks and gloves, shorter and wider |
| ghost | Mateo "Ghost" Rivera | Out-boxer | high Speed, Defense and Reach, low Power and Chin | blue trunks and gloves, taller and slimmer |

- Stat set, in display order: Power, Speed, Stamina, Defense, Reach, Chin. Each is an integer 1 to `statMax` (10). Both boxers have the same stat total, so neither is strictly better.
- The roster order in the file is the order on screen (left to right).
- **Stats are display-only in this task.** There is no combat, so no stat changes any gameplay yet. The select screen must not claim otherwise. (Do not add a "stats apply in fight" message; just show them.)
- The only on-screen consequences of stats: the stat bars and numbers, and the fighters' primitive bodies, which differ visibly in colour and size (see Ring). Body size follows `look.heightScale` / `widthScale` in the data file, not the stat values by formula.

## Screen flow
One new screen, **Character Select**, with two steps. Two cards (one per boxer) are side by side at all times; each card always shows its boxer's name, archetype and all stats, so the player compares both while browsing. One card is highlighted.

Flow: Menu -> (New Game) -> Select step 1 "Choose your boxer" -> Select step 2 "Choose your opponent" -> Ring.

- Step 1: header "Choose your boxer". Highlight starts on `ui.defaultHighlightId` (anvil, the first card). Confirm stores the player's pick and moves to step 2.
- Step 2: header "Choose your opponent". A small line on screen shows "You: <player boxer name>". Highlight starts on the player's own pick (so choosing the same boxer is one Enter). Confirm stores the opponent pick and loads the ring.
- Same boxer for both is allowed and must work with no warning.
- The player's pick is marked on its card in step 2 with a "YOU" tag (text, not colour alone).

| Current screen | Escape result |
|---|---|
| Menu | Quit app (unchanged). |
| Select step 1 | Return to the main menu (New Game highlighted, picks discarded). |
| Select step 2 | Return to step 1; the player's pick is kept as the highlight, the opponent pick is discarded. |
| Ring | Return to the main menu (unchanged). Picks are discarded; New Game starts select again from step 1 with the default highlight. |

Escape ignores key repeat and is ignored during a screen change, exactly as in main-menu.md.

## Selection behaviour
Same rules as the main menu, with the 2 cards as items (index 0 and 1) and horizontal movement.
- Exactly one card is highlighted at all times. Left moves -1, Right +1, wrapping.
- Mouse hover over a card highlights it (same single highlight). Leaving a card does not clear it. A stationary mouse never overrides a keyboard move.
- Left click on a card highlights and confirms it. Enter confirms the highlighted card.
- Confirm is ignored while a screen change is in progress (no double confirm from double click or held Enter).
- Reuse `MenuSelection` for the highlight (count = roster size).

## Controls
| Action | Key | Notes |
|---|---|---|
| menu_left | Left arrow (physical) | NEW. Highlight previous card, wraps. |
| menu_right | Right arrow (physical) | NEW. Highlight next card, wraps. |
| menu_confirm | Enter, keypad Enter | Existing. Confirm highlighted card (step 1 or 2). |
| ui_back | Escape | Existing. Per the Escape table above. Ignores key repeat. |
| (mouse) | Hover / left click | Control mouse signals, no InputMap action. |

No clash with docs/kb/input-map.md (menu_up and menu_down stay for the main menu; they do nothing on this screen). menu_left and menu_right are registered in code at startup (game/scripts/InputActions.EnsureRegistered), not in project.godot, so no project.godot edit is required. The developer lists both in docs/kb/input-map.md, noting they are code-registered.

## Visual description (code-built, no external assets)
- Control nodes in code, same style constants approach as the menu: dark background, header `Label` at top, two cards side by side centred.
- Card contents, top to bottom: name (with nickname), archetype, then six stat rows. Each stat row: label, a horizontal bar filled to value / `statMax` of its width (a `ColorRect` over a darker `ColorRect`), and the number (e.g. "9") to the right. Rows in `statOrder`.
- Highlighted card: clearly different fill plus a visible border (as in the menu, not colour alone). The "YOU" tag shows in step 2.
- Default Godot theme and font; no images or sounds. Exact colours and sizes are the developer's call.

## Ring: fighters ready to fight
- On entering the ring, build two fighters from primitives (e.g. capsule/cylinder body, sphere head, two sphere gloves raised in a guard pose, box trunks). No external assets.
- Player at `ring.playerX`, opponent at `ring.opponentX`, both on the floor, facing each other, side-on camera sees both fully. Idle: static pose is enough (a gentle idle bob is optional).
- Each fighter uses its boxer's `look` (skin, trunks and glove colour, height and width scale relative to `ring.baseHeight` / `ring.baseWidth`). When both picks are the same boxer, the two fighters look identical; a small text label above each ("YOU" and "CPU") tells them apart. Labels are code-generated (`Label3D` or screen `Label`).
- The round clock and existing floor/light/camera keep working.

## Hand-off of choices
- Pure logic in `src/Core` (no Godot reference):
  - `FighterDef` (id, name, nickname, archetype, stats, look) and `FighterRoster` loaded from the JSON text (parsing in Core, file reading in the Godot adapter). Invalid data (no fighters, duplicate ids, stat outside 1..statMax, missing stat from `statOrder`) throws a clear exception.
  - `CharacterSelectFlow`: step (`PickPlayer`, `PickOpponent`, `Done`), `Confirm(fighterId)`, and `Back()` returning `GoToMenu`, `GoToPlayerStep` or `Stay`. Exposes `PlayerId` and `OpponentId`.
- Godot side: a static `GameSession` (or equivalent) in `game/` holds `PlayerId` and `OpponentId`. Select writes both before changing scene to `Main.tscn`. `Main` reads them in `_Ready` and builds the fighters. If nothing was set (smoke test, direct launch of the ring), both default to the first roster entry. `GameSession` is cleared when returning to the menu.
- The data file must be readable at runtime in the editor and in the exported build (developer to verify it is included in the export).

## Acceptance criteria
AC1 to AC10 are xUnit tests in `tests/Core.Tests`. The rest are checked in the running game (playtest, headless smoke where practical).
- AC1. **Given** the shipped `fighters.json`, **when** the roster is loaded, **then** it contains exactly 2 fighters (anvil, ghost) in that order, each with all six stats between 1 and `statMax`.
- AC2. **Given** the shipped roster, **when** stat totals are summed, **then** both fighters have the same total.
- AC3. **Given** the shipped roster, **when** compared stat by stat, **then** each boxer is strictly higher than the other in at least one stat (neither dominates).
- AC4. **Given** roster JSON with a missing stat, a stat of 0 or above `statMax`, duplicate ids, or an empty fighter list, **when** loaded, **then** loading throws and names the problem.
- AC5. **Given** a new `CharacterSelectFlow`, **when** created, **then** the step is `PickPlayer` and no ids are set.
- AC6. **Given** step `PickPlayer`, **when** `Confirm("ghost")`, **then** `PlayerId` is "ghost" and the step is `PickOpponent`.
- AC7. **Given** step `PickOpponent`, **when** `Confirm("ghost")` with the player also "ghost", **then** `OpponentId` is "ghost" and the step is `Done` (same boxer allowed).
- AC8. **Given** step `PickPlayer`, **when** `Back()`, **then** the result is `GoToMenu`.
- AC9. **Given** step `PickOpponent` with the player picked, **when** `Back()`, **then** the result is `GoToPlayerStep`, the step is `PickPlayer`, and the opponent id is cleared.
- AC10. **Given** `Confirm` is called with an id not in the roster, or after the step is `Done`, **when** evaluated, **then** it is rejected (exception or no change, documented) and the state is unchanged.
- AC11. **Given** the menu, **when** the player activates New Game, **then** Character Select step 1 is shown ("Choose your boxer") with both cards visible, the first card highlighted, and each card showing name, archetype and six stat rows with a bar and a number matching the data file.
- AC12. **Given** step 1, **when** the player presses Right, Right, Left, **then** the highlight alternates and wraps with no stuck or missing highlight; stat panels of both boxers stay visible throughout.
- AC13. **Given** step 1, **when** the mouse hovers the second card then rests on empty background and Enter is pressed, **then** the second card is confirmed and step 2 shows.
- AC14. **Given** step 1 with the player confirming Ghost, **when** step 2 shows, **then** the header reads "Choose your opponent", "You: Mateo Rivera" is shown, Ghost's card has a "YOU" tag, and Ghost's card is highlighted.
- AC15. **Given** step 2, **when** the player presses Enter on the highlighted (same) boxer, **then** the ring loads with two fighters that both use that boxer's look.
- AC16. **Given** player Anvil and opponent Ghost, **when** the ring loads, **then** the left fighter is red and shorter and wider, the right fighter is blue and taller and slimmer, both stand idle facing each other, fully visible, labelled "YOU" (left) and "CPU" (right).
- AC17. **Given** step 2, **when** Escape is pressed once, **then** step 1 shows with the player's earlier pick highlighted, and the app is still running.
- AC18. **Given** step 1, **when** Escape is pressed once, **then** the main menu shows with New Game highlighted.
- AC19. **Given** the ring after a select, **when** Escape is pressed, **then** the menu shows; **and when** New Game is chosen again, **then** step 1 starts fresh (default highlight, no "YOU" tag, no earlier picks).
- AC20. **Given** any select step, **when** Escape is held about two seconds, **then** it goes back exactly one step and stops (no repeat skipping through steps or quitting).
- AC21. **Given** a double click or held Enter on a card, **when** processed, **then** only one step advance happens (no skipped opponent step, no double ring load).
- AC22. **Given** the project is built, **when** `InputActions.EnsureRegistered` has run at startup, **then** `menu_left` (Left arrow, physical) and `menu_right` (Right arrow, physical) exist in the InputMap, and docs/kb/input-map.md lists both and notes they are code-registered (not in project.godot).
- AC23. **Given** the ring is launched directly (smoke scene, no select), **when** it loads, **then** it shows two fighters using the first roster entry and no error occurs.

## Balance values
All numbers live in `data/balance/fighters.json`, not in this spec or in code.
Pointer: `data/balance/fighters.json` -> `statMax`, `statOrder`, `statLabels`, `fighters[].stats`, `fighters[].look`, `ring.playerX`, `ring.opponentX`, `ring.baseHeight`, `ring.baseWidth`, `ui.defaultHighlightId`.

## Boxing notes
none. No real-boxing claims are made.
GAMEPLAY ABSTRACTION: boxers are described by six 1 to 10 stats and two archetypes instead of real measured attributes, because the screen must be readable at a glance and combat does not exist yet. The stats are placeholders for later combat tuning and have no effect now.

## Out of scope
- Combat, punches, damage, stamina use, AI behaviour, round logic, scoring. Stats affect nothing yet.
- More than 2 boxers, unlocking, customisation, names entry, saving the last choice.
- Animated previews, 3D rotating models on the select screen (cards are text and bars only).
- Gamepad, rebinding, sound, art style, fades, any external asset.
- Fighter animation beyond an optional idle bob.

## Open questions
- Is the "Ghost" / "The Anvil" naming and the red/blue colouring acceptable, or does the client want other names? (Working defaults are in the data file and are easy to change.)

## Playtest checklist
Five things the client checks by hand at G3.
1. Click New Game: a "Choose your boxer" screen shows two cards side by side, each with name, style (Slugger / Out-boxer) and six stat bars with numbers. Press Left/Right and hover with the mouse: the highlight moves and wraps, and both boxers' stats stay visible.
2. Pick Mateo Rivera (Enter or click): the header changes to "Choose your opponent", shows "You: Mateo Rivera", and his card carries a "YOU" tag.
3. Pick Bruno Kessler as the opponent: the ring appears with a blue, tall, slim fighter on the left ("YOU") and a red, short, wide one on the right ("CPU"), standing ready and facing each other.
4. Go back to the menu with Escape, then New Game again and pick the same boxer for both: both fighters look identical but are still labelled YOU and CPU, and nothing warns or blocks you.
5. Escape steps back one level at a time (opponent step, then boxer step, then menu), a held Escape moves only one step, and the stats shown change nothing about how the ring behaves.
