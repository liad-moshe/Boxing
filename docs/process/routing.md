# Routing

The Producer classifies every task with this table, then records the route in work/<n>/brief.md.
Source: environment-spec.md §4–5. This file is protected: agents never edit it, except that the client
may ask for real /usage observations to be added below.

## Routing table
| Task type | Designer | Boxing | Tech Artist | Developer | QA | KB | Gates |
|---|---|---|---|---|---|---|---|
| New combat mechanic | yes (opus) | yes | if visual (M2+) | yes | yes | yes | G1 G2 G3 G4 |
| UI change | yes (sonnet, light) | no | if style changes | yes | yes | yes | G2 G3 G4 |
| Bug fix | no | no | no | yes (repro test first) | yes | if behavior map changes | G3 G4 |
| Balance tweak | yes (edits data/balance) | if a realism claim changes | no | no | yes | no | G3 G4 |
| Refactor | no | no | no | yes (opus if >3 systems) | yes | yes | G1 G3 G4 |
| Visual feature (M2+) | yes | no | yes | if code needed | yes | yes | G1 G2 G3 G4 |

G1 is also asked whenever the classification is ambiguous.

## Forcing a route
The client can override the table with "full route" / "lean route", or with the labels `route:full` /
`route:lean`. The Producer records the override in brief.md.

## Model overrides (log every one in work/<n>/brief.md)
Default models come from each agent file. The Producer may pass `model: "opus"` only:
- (a) to game-designer for a new combat mechanic or milestone design;
- (b) to developer after 2 failed QA/CI cycles on the same task, for a refactor touching more than 3
  systems, or for a new architecture ADR;
- (c) to tech-artist for a new shader system.

The /plan-milestone skill always runs on opus.

## Escalation rules
Any agent may end its report with `ESCALATE: <role> — <reason>`. Automatic triggers:
| Trigger | Action |
|---|---|
| Spec is ambiguous | → game-designer |
| A claim about real boxing | → boxing-consultant |
| A visual decision | → tech-artist (M2+) |
| 2 failed QA/CI cycles | → developer re-run on opus |
| 3rd failure | → label status:blocked, ask the client |
| Diff over ~400 lines or more than 3 systems | → re-classify as "New combat mechanic", re-ask G1 |

## Usage estimates
Relative usage units per task (Sonnet = 1, Opus ≈ 1.7, Haiku ≈ 0.35):

| Task type | Estimated units |
|---|---|
| New combat mechanic | ≈ 6.5 |
| UI change | ≈ 3.5 |
| Bug fix | ≈ 2.3 |
| Balance tweak | ≈ 1.7 |
| Refactor | ≈ 3.0 |
| Visual feature | ≈ 3.8 |

## Real /usage observations
Record actual numbers here after real tasks, so the estimates can be corrected.

| Date | Task # | Type | Route | Models | /usage before → after | Notes |
|---|---|---|---|---|---|---|
| | | | | | | |
