# Roadmap

The Producer keeps this file up to date. Milestones are proposals until the client approves the plan
(see docs/process/gates.md).

## M1 — "Sparring Prototype"
Goal: one complete, playable sparring round that proves the core loop is fun.
- A ring made from primitives: canvas, ropes, posts and corners.
- Two boxers built from procedural primitives (player and opponent).
- Moves: **jab**, **cross**, **guard**, **slip**.
- **Stamina**: punches and slips spend it, and it recovers over time. Low stamina makes punches weaker and
  slower.
- **3-minute round timer.**
- **Decision**: KO (the opponent's health is depleted) or a points decision when time runs out.
- **Keyboard controls** (the key map lives in docs/kb/input-map.md once it exists in project.godot).
- **Menu**: Start, Quit and Licenses (the engine license notice, see docs/licenses/README.md).
- **Opponent AI**: a simple state machine (for example: approach, attack, defend, recover), tuned
  **forgiving** by default (client decision, docs/vision.md).
- **Camera**: side-on, like a classic fighting game (client decision, docs/vision.md).

## M2 — "Look & Feel"
Goal: make M1 look and sound good. This brings in the tech-artist role.
- Shaders for the boxers, the ring and lighting.
- Procedural audio: punch impacts, crowd bed and bell (AudioStreamGenerator).
- HUD: health, stamina and round timer, styled to match the art direction (docs/art/style-guide.md).

## What M1 deliberately skips
- Multiple rounds, corners between rounds, judges' scorecards shown in detail.
- Hooks, uppercuts, body shots, clinching, footwork beyond basic movement.
- Knockdowns with a referee count. M1 uses a KO end state only.
- Gamepad support, rebinding controls, settings menu.
- Sound and visual polish beyond flat colors (that is M2).
- Difficulty levels, career mode, character customisation, multiplayer.
- Save data of any kind.
