# docs/specs — feature specs

Written by **game-designer**. The Producer may write drafts only. One file per feature:
`docs/specs/<feature>.md`. The client approves a spec at gate G2 before development starts.

Copy the template below into a new file.

---

```markdown
# Spec: <feature name>

Status: DRAFT | APPROVED (G2, <date>)   Issue: #<n>

## Goal
One or two sentences: what this feature adds and why it makes the game better.

## Player story
As a player, I want <action>, so that <outcome>.

## Controls
| Action | Key | Notes |
|---|---|---|

## Acceptance criteria
Each one is testable by QA, and by an automated test where possible.
- AC1. **Given** <starting state>, **when** <player/opponent action>, **then** <observable result>.
- AC2. **Given** …, **when** …, **then** ….

## Balance values
All tunable numbers live in data/balance/<file>, not in this spec or in code.
Pointer: data/balance/<file> → <keys>.

## Boxing notes
Real-boxing basis with [S#] citations from docs/boxing/sources.md, or "none".
GAMEPLAY ABSTRACTION: <each deliberate departure from real boxing, with the reason>.

## Out of scope
What this spec deliberately does not cover.

## Playtest checklist
Five things the client checks by hand at G3.
1.
2.
3.
4.
5.
```
