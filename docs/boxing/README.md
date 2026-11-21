# docs/boxing — realism research

Owned by the **boxing-consultant** agent. It runs when a task has the label `needs:boxing`, or when any
role makes a claim about real boxing (see the escalation rules in docs/process/routing.md).

## What lives here
- **sources.md**: the tiered source registry. Every claim in this folder cites an `[S#]` from it.
- **Topic notes** (added as needed), one file per topic, e.g. `rules-scoring.md`, `technique-jab.md`,
  `stamina-physiology.md`. Each note has:
  1. **Summary**: what real boxing does, in plain words, with `[S#]` citations.
  2. **Game implications**: what this means for our mechanics.
  3. **GAMEPLAY ABSTRACTION:** lines for each place the game deliberately departs from reality.

## Reviews
For a task's realism review, the consultant writes `work/<n>/boxing-review.md` (deleted with work/<n>/
at ship time). Findings worth keeping are promoted into a topic note here.
