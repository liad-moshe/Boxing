# Task 8: Producer opens the PR automatically after QA passes
Issue: #8   Branch: task/8-auto-open-pr   Type: docs (process/governance)   Route: lean

## Request
The client wants PRs opened automatically when development of a task is finished, instead of the client
having to type /ship. The task flow (task skill step 9) already says to continue with the ship skill, but
the project skill `ship` shares its name with Claude Code's built-in `/ship`, whose description forbids
model invocation, so the Producer stops and waits. Fix: rename the project skill to `ship-task` and make
step 9 explicit. All affected files are protected, so the output is a governance proposal.

## Roles run (in order)
- Producer · opus (session model, see note) · writes work/8/governance-proposal.md (Producer-allowed path)

## Roles skipped
- game-designer · no gameplay, UI or balance change
- boxing-consultant · no real-boxing claim
- tech-artist · Milestone 1 / no visuals
- developer · no src/, game/, tests/ or data/ change
- qa · no code to verify; the proposal is reviewed by the client
- kb-keeper · no src/, game/ or project.godot change

## Gates
- G1: answered by the client on 2026-11-27 ("Yes, write it") for this lean route.
- G2: n/a (no spec).
- Client applies the governance proposal themselves (protected paths).
- G3/G4: only if a PR is opened from this branch after the client applies the change.

## Model overrides
- none delegated. Note: the session ran on claude-opus-5-5 instead of Sonnet; the client was warned.

## Constraints
- Protected paths only (.claude/**, CLAUDE.md, docs/process/**). The Producer never edits them.
