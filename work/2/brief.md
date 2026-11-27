# Task 2: Main menu (New Game / Exit)
Issue: #2   Branch: task/2-main-menu   Type: ui   Route: table (UI change)
## Request
Add a main menu where the player chooses to start a new game or exit. Navigate with the mouse or the Up/Down arrow keys; Enter selects.
## Roles run (in order)
- game-designer · sonnet (light) · UI spec + 5-item playtest checklist
- developer · sonnet · plan.md, TDD (menu-selection logic in src/Core), thin Godot adapter in game/
- qa · sonnet · acceptance tests, build/test/headless smoke, Godot-3 drift scan
- kb-keeper · haiku · update input-map/ui-map/feature-map (skipped only if no src/game/project.godot change)
## Roles skipped
- boxing-consultant · no real-boxing claim in a menu
- tech-artist · Milestone 1; no style change (menu built from code-only Control nodes)
## Gates
G2 (spec OK), G3 (playtest), G4 (client merges). G1 not needed: classification is unambiguous.
## Model overrides
none
## Constraints
Milestone 1 scope; no external assets (no fonts/images/sounds; default theme + code-built styling only); likely touches game/ (menu scene/script), src/Core (selection logic + tests), project.godot (main scene, input actions). Label note: all labels existed.
