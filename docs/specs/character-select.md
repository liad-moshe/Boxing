# Spec: Character select

Status: DRAFT   Issue: #5   (look revised after client feedback: fighters must read as human boxers; pending client G2)   Amended by #12: stance choice per step (see "Amendment for #12: stance" below; full rules and ACs in docs/specs/player-controls.md)

## Goal
After New Game, let the player choose a boxer for themself and a boxer for the AI opponent from a roster of exactly 2, seeing each boxer's stats while browsing, then show the ring with both fighters standing ready. This gives the game its first player choice and the data foundation for later combat.

## Player story
As a player, I want to compare two boxers and pick one for me and one for my opponent (possibly the same), so that I can start a fight with the matchup I want.

## Roster (data-driven)
Defined in `data/balance/fighters.json`; the code must not hard-code names or numbers.

| Id | Name | Archetype | Stats (see data file) | Look |
|---|---|---|---|---|
| anvil | Bruno "The Anvil" Kessler | Slugger | high Power and Chin, low Speed and Defense | red shorts and gloves, dark buzz-cut hair, stocky and broad (big neck, wide shoulders, thick arms, shorter reach) |
| ghost | Mateo "Ghost" Rivera | Out-boxer | high Speed, Defense and Reach, low Power and Chin | blue shorts and gloves, brown short hair, tall and lean (narrow shoulders, long thin arms and legs) |

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

No clash with docs/kb/input-map.md (menu_up and menu_down stay for the main menu; **since #12 they toggle the stance on this screen**, see the amendment below). menu_left and menu_right are registered in code at startup (game/scripts/InputActions.EnsureRegistered), not in project.godot, so no project.godot edit is required. The developer lists both in docs/kb/input-map.md, noting they are code-registered.

## Visual description (code-built, no external assets)
- Control nodes in code, same style constants approach as the menu: dark background, header `Label` at top, two cards side by side centred.
- Card contents, top to bottom: name (with nickname), archetype, then six stat rows. Each stat row: label, a horizontal bar filled to value / `statMax` of its width (a `ColorRect` over a darker `ColorRect`), and the number (e.g. "9") to the right. Rows in `statOrder`.
- Highlighted card: clearly different fill plus a visible border (as in the menu, not colour alone). The "YOU" tag shows in step 2.
- Default Godot theme and font; no images or sounds. Exact colours and sizes are the developer's call.

## Ring: fighters ready to fight
- On entering the ring, build two fighters that clearly read as **human boxers** (not capsule-and-ball dummies), from code primitives only (sphere, capsule, cylinder, box, optional CSG or ArrayMesh). No external assets.
- Required body parts, each its own named `MeshInstance3D` (or CSG node) under the fighter root, so the smoke test can find them by name. `L` = the boxer's left side, `R` = right:
  - Head: `Head` (slightly taller than wide), `Hair` (cap shape per `look.hairStyle`: `buzz` thin, `short` slightly thicker), `Neck`.
  - Face cues (small, on the side facing the opponent): `EyeL`, `EyeR`, `Nose`, `Mouth` (a thin dark box), plus optional brow and ears.
  - Upper body: `Torso` (wider at the shoulders than at the waist, e.g. a scaled capsule or tapered shape), `ShoulderL`, `ShoulderR`.
  - Arms in a guard stance: `UpperArmL`, `UpperArmR`, `ForearmL`, `ForearmR`, each bent at the elbow (`body.guardElbowBendDeg`) so both forearms point up and forward, and `GloveL`, `GloveR`, large (`body.gloveRadiusH`, clearly bigger than a fist) in front of the chin/face at about `body.guardGloveHeightH` of the fighter's height. A slim `CuffL` / `CuffR` (wrist band, glove colour or white) is optional.
  - Lower body: `Shorts` (box or tapered shape, covers hips to mid-thigh), `Waistband` (a thin band in `look.waistbandColor` around the top of the shorts), `UpperLegL`, `UpperLegR`, `LowerLegL`, `LowerLegR`, `FootL`, `FootR` (flat boxes longer than wide, resting on the floor, so no part of the legs ends in mid-air).
  - Label: `Tag` (see below).
- Colours: skin on head, neck, torso, arms, legs, feet. Hair in `hairColor`. Shorts in `trunksColor` (the data key keeps its old name; the player sees "shorts"). Gloves in `gloveColor`. Eyes, mouth and nose use `body.eyeColor`, `body.mouthColor` and skin colour.
- Stance (static pose, GAMEPLAY ABSTRACTION: a simple fixed ready pose instead of real boxing footwork [no claim made]; chosen for readability): upright, knees slightly bent is optional, feet apart by `body.stanceFootSpreadW`, the lead foot (the one nearer the opponent) forward by `body.stanceLeadFootForwardW`, torso and head turned to face the opponent, both gloves up by the face. Facing is mirrored for the right-hand fighter so both face each other.
- Builds differ through `look`: `heightScale`, `widthScale`, `headScale`, `neckScale`, `shoulderScale`, `armLengthScale`, `armThicknessScale`, `legThicknessScale`, multiplied onto the shared `body` proportions (fractions of `ring.baseHeight` / `ring.baseWidth`). Bruno must read as stocky and broad (visibly wider shoulders, thicker neck and arms, shorter arms); Mateo as tall and lean (longer, thinner limbs). Both must be recognisable as the same kind of figure at the camera distance, with the six-part silhouette (head, torso, two arms with gloves, two legs) clear.
- Player at `ring.playerX`, opponent at `ring.opponentX`, both on the floor (lowest foot point at floor level), facing each other, side-on camera sees both fully. Idle: static pose is enough (a gentle idle bob is optional).
- Each fighter uses its boxer's `look`. When both picks are the same boxer, the two fighters look identical; a small text label above each ("YOU" and "CPU") tells them apart. Labels are code-generated (`Label3D` or screen `Label`).
- All proportions and colours are in `data/balance/fighters.json` (`fighters[].look` and `body`); the builder hard-codes none of them. Exact meshes and fine proportions are the developer's call within the part list above.
- The round clock and existing floor/light/camera keep working.

## Hand-off of choices
- Pure logic in `src/Core` (no Godot reference):
  - `FighterDef` (id, name, nickname, archetype, stats, look) and `FighterRoster` loaded from the JSON text (parsing in Core, file reading in the Godot adapter). Invalid data (no fighters, duplicate ids, stat outside 1..statMax, missing stat from `statOrder`) throws a clear exception.
  - `CharacterSelectFlow`: step (`PickPlayer`, `PickOpponent`, `Done`), `Confirm(fighterId)`, and `Back()` returning `GoToMenu`, `GoToPlayerStep` or `Stay`. Exposes `PlayerId` and `OpponentId`.
- Godot side: a static `GameSession` (or equivalent) in `game/` holds `PlayerId` and `OpponentId`. Select writes both before changing scene to `Main.tscn`. `Main` reads them in `_Ready` and builds the fighters. If nothing was set (smoke test, direct launch of the ring), both default to the first roster entry. `GameSession` is cleared when returning to the menu.
- The data file must be readable at runtime in the editor and in the exported build (developer to verify it is included in the export).

## Amendment for #12: stance
Spec change made for issue #12. The authoritative rules and the acceptance criteria (AC17 to AC21, AC29, AC30) are in docs/specs/player-controls.md, section "Stance choice (Character Select)". Summary of what changes on this screen:
- Each step has one **stance line** under the cards: `Stance: Orthodox   (Up / Down to change)` (or Southpaw). It sets the stance of the boxer picked in that step, whatever card is highlighted.
- **Up / Down** (existing `menu_up` / `menu_down`, fresh press only) or a left click on the stance line toggle Orthodox <-> Southpaw. The line takes no keyboard focus and a click on it confirms nothing. No new key or InputMap action.
- Both steps start on `stance.default` from `data/balance/player-moves.json` (orthodox). Confirm stores pick and stance. Step 2 shows `You: <name> (<stance>)`.
- Escape from step 2 keeps the player's pick and stance; discards the opponent's. Escape to the menu (from step 1 or the ring) discards all.
- `CharacterSelectFlow` gains `PlayerStance`, `OpponentStance`, `ToggleStance()`; `GameSession` gains `PlayerStance`, `OpponentStance` (default `stance.default` on direct launch).
- Ring: the fighter `Tag` text becomes `YOU (<stance>)` / `CPU (<stance>)` (AC16 and playtest item 4 of this spec now read "YOU" and "CPU" as the start of the tag). A southpaw fighter is built as the mirror of the orthodox build: `FootR` leads by `body.stanceLeadFootForwardW` and `GloveR` takes the lead-glove place. All part names, colours and AC24 to AC28 checks are unchanged.

## Acceptance criteria
AC1 to AC10 and AC29 are xUnit tests in `tests/Core.Tests`. AC24 to AC28 are checked by the headless smoke test (node names, colours, measured positions) plus the playtest. The rest are checked in the running game (playtest, headless smoke where practical).
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
- AC16. **Given** player Anvil and opponent Ghost, **when** the ring loads, **then** the left fighter is red-shorted and shorter and broader, the right fighter is blue-shorted and taller and leaner, both stand idle facing each other, fully visible, labelled "YOU" (left) and "CPU" (right).
- AC17. **Given** step 2, **when** Escape is pressed once, **then** step 1 shows with the player's earlier pick highlighted, and the app is still running.
- AC18. **Given** step 1, **when** Escape is pressed once, **then** the main menu shows with New Game highlighted.
- AC19. **Given** the ring after a select, **when** Escape is pressed, **then** the menu shows; **and when** New Game is chosen again, **then** step 1 starts fresh (default highlight, no "YOU" tag, no earlier picks).
- AC20. **Given** any select step, **when** Escape is held about two seconds, **then** it goes back exactly one step and stops (no repeat skipping through steps or quitting).
- AC21. **Given** a double click or held Enter on a card, **when** processed, **then** only one step advance happens (no skipped opponent step, no double ring load).
- AC22. **Given** the project is built, **when** `InputActions.EnsureRegistered` has run at startup, **then** `menu_left` (Left arrow, physical) and `menu_right` (Right arrow, physical) exist in the InputMap, and docs/kb/input-map.md lists both and notes they are code-registered (not in project.godot).
- AC23. **Given** the ring is launched directly (smoke scene, no select), **when** it loads, **then** it shows two fighters using the first roster entry and no error occurs.
- AC24. **Given** the ring has loaded (smoke test), **when** each fighter root is inspected, **then** it has a child named each of: `Head`, `Hair`, `Neck`, `EyeL`, `EyeR`, `Nose`, `Mouth`, `Torso`, `ShoulderL`, `ShoulderR`, `UpperArmL`, `UpperArmR`, `ForearmL`, `ForearmR`, `GloveL`, `GloveR`, `Shorts`, `Waistband`, `UpperLegL`, `UpperLegR`, `LowerLegL`, `LowerLegR`, `FootL`, `FootR`, `Tag`; and the smoke test fails if any is missing.
- AC25. **Given** both fighters are built, **when** their part materials are read, **then** `Shorts`, `GloveL`/`GloveR`, `Hair` and `Head` use the boxer's `trunksColor`, `gloveColor`, `hairColor` and `skinColor`, and `Waistband` uses `waistbandColor`.
- AC26. **Given** player Anvil and opponent Ghost, **when** bounding boxes are measured, **then** Ghost's total height is greater than Anvil's, Anvil's shoulder span (`ShoulderL` to `ShoulderR`) is wider than Ghost's, and Ghost's glove-to-shoulder arm reach is longer than Anvil's.
- AC27. **Given** a built fighter, **when** the parts are measured, **then** the lowest point of `FootL` and `FootR` is at floor level (within 1 cm), `Head` is above `Torso`, both gloves are higher than the shorts and in front of the torso on the side facing the opponent, and each glove is at least 1.5 times the radius of its forearm.
- AC28. **Given** the ring, **when** the opponent's side is viewed, **then** each fighter's face cues (eyes, nose, mouth) are on the side facing the other fighter, and the fighter at `playerX` faces +X while the fighter at `opponentX` faces -X.
- AC29. **Given** `data/balance/fighters.json`, **when** loaded by the roster loader, **then** each fighter's `look` contains `hairColor`, `hairStyle`, `waistbandColor` and all six `*Scale` build values, and `body` is present; a missing or unparsable colour or non-positive scale throws and names the problem. (xUnit, in `tests/Core.Tests` with AC1 to AC4.)

## Balance values
All numbers live in `data/balance/fighters.json`, not in this spec or in code.
Pointer: `data/balance/fighters.json` -> `statMax`, `statOrder`, `statLabels`, `fighters[].stats`, `fighters[].look` (colours, hair, build scales), `body` (shared human proportions, guard pose, face), `ring.playerX`, `ring.opponentX`, `ring.baseHeight`, `ring.baseWidth`, `ui.defaultHighlightId`.

## Boxing notes
none. No real-boxing claims are made.
GAMEPLAY ABSTRACTION: boxers are described by six 1 to 10 stats and two archetypes instead of real measured attributes, because the screen must be readable at a glance and combat does not exist yet. The stats are placeholders for later combat tuning and have no effect now.

## Out of scope
- Combat, punches, damage, stamina use, AI behaviour, round logic, scoring. Stats affect nothing yet.
- More than 2 boxers, unlocking, customisation, names entry, saving the last choice.
- Animated previews, 3D rotating models on the select screen (cards are text and bars only).
- Boxer portraits or face art on the cards (issue #7, M2).
- Fine detail: fingers, clothing folds, muscles, facial animation, skeletal rigs, real footwork or punch animation. Art style polish is M2.
- Gamepad, rebinding, sound, art style, fades, any external asset.
- Fighter animation beyond an optional idle bob.

## Open questions
- Is the "Ghost" / "The Anvil" naming and the red/blue colouring acceptable, or does the client want other names? (Working defaults are in the data file and are easy to change.)

## Playtest checklist
Five things the client checks by hand at G3.
1. Click New Game: a "Choose your boxer" screen shows two cards side by side, each with name, style (Slugger / Out-boxer) and six stat bars with numbers. Press Left/Right and hover with the mouse: the highlight moves and wraps, and both boxers' stats stay visible.
2. Pick Mateo Rivera (Enter or click): the header changes to "Choose your opponent", shows "You: Mateo Rivera", and his card carries a "YOU" tag.
3. Pick Bruno Kessler as the opponent: the ring appears with two human-looking boxers, each with a head with eyes, nose and mouth, hair, neck, shoulders, two arms with big gloves up in a guard, shorts with a waistband, legs and feet. A tall, lean, blue-shorted fighter is on the left ("YOU") and a stocky, broad, red-shorted one on the right ("CPU"), standing ready and facing each other.
4. Go back to the menu with Escape, then New Game again and pick the same boxer for both: both fighters look identical but are still labelled YOU and CPU, and nothing warns or blocks you.
5. Escape steps back one level at a time (opponent step, then boxer step, then menu), a held Escape moves only one step, and the stats shown change nothing about how the ring behaves.
