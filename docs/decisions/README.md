# docs/decisions — Architecture Decision Records (ADRs)

An ADR records one significant technical decision: the context, the choice and its consequences. It lets
a later session (human or agent) see *why* something is the way it is, without having to re-argue it.
Written by **developer**. A new architecture ADR may run on opus (docs/process/routing.md).

- File name: `ADR-NNNN-short-title.md`, numbered in order. Numbers are never reused.
- To change a decision, write a new ADR with "Supersedes ADR-NNNN" and set the old one's status to
  "Superseded by ADR-MMMM". Don't rewrite history.

## Template

```markdown
# ADR-NNNN: <title>

- Status: Proposed | Accepted | Superseded by ADR-MMMM
- Date: YYYY-MM-DD
- Task: #<n> (if any)

## Context
What forces are at play: requirements, constraints, facts (with links to official docs).

## Decision
What we do, stated plainly.

## Alternatives considered
- <option>: why not.

## Consequences
What becomes easier, what becomes harder, and what to watch for.
```
