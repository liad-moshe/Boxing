# Task 5: Character select
Issue: #5   Branch: task/5-character-select   Type: ui (ambiguous, proposed; could be mechanic)   Route: table (pending G1)
## Request
Starting a new game from the menu: player chooses a boxer for themself and for the AI opponent from 2 fighters (same fighter allowed for both). Fighters differ in boxing parameters; the selection screen shows each fighter's parameters. After choosing, the ring shows both fighters ready to fight.
## Roles run (in order)
- game-designer · sonnet · fighter roster, stat set, selection-screen spec, balance data in data/balance/
- developer · sonnet · plan, TDD in src/Core, thin Godot adapters
- qa · sonnet · verification
- kb-keeper · haiku · docs/kb update
## Roles skipped
- boxing-consultant · stats are gameplay abstractions; revisit if designer makes real-boxing claims
- tech-artist · Milestone 1, primitives only
## Gates
G1 (ambiguous class), G2, G3, G4
## Model overrides
none
## Constraints
M1 scope, no external assets, fighters are code-built primitives. Files likely touched: src/Core (roster/selection), game/ (select screen, ring), data/balance/, tests/Core.Tests, docs/kb.
