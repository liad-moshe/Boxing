# Spec: Player controls (playable fight, idle opponent)

Status: DRAFT   Issue: #10

## Goal
When the player confirms both boxers in Character Select, the ring becomes a fight scene: the player's boxer walks forward and back and throws a jab, a cross, a held guard and a slip, all from the keyboard. The opponent stands idle. This is the first time the game is played rather than looked at, and it sets the timing rules (startup, active, recovery, cancels) that stamina, damage and AI will build on later.

## Player story
As a player, I want to step in and out of range and throw a jab, a cross, a guard and a slip with a handful of keys, so that I can feel the rhythm of the basic boxing moves before the opponent fights back.

## Scene flow (how the fight is entered)
- Unchanged flow: Menu -> New Game -> Select step 1 -> Select step 2 -> confirm -> `game/scenes/Main.tscn`. The ring scene **is** the fight scene; no new scene file is required. The player fighter (left, "YOU") gets the controls; the opponent (right, "CPU") stays idle.
- Controls are live on the first physics tick of the ring. There is no countdown, intro or "Fight!" banner (vision pillar 3: launch to first punch in seconds).
- The confirm key (Enter) is not a fight key, so a held Enter from the select screen does nothing in the fight.
- Escape (`ui_back`) behaves exactly as today: back to the menu, picks cleared. It works in any move state. New Game again starts a fresh fight with the player back at `ring.playerX` in Idle.
- Direct launch of `Main.tscn` (smoke test) still works: both fighters fall back to the first roster entry and the player is controllable.
- The existing invisible round clock keeps running unchanged; nothing happens when it reaches zero (round logic is out of scope).
- A one-line controls hint is shown at the bottom of the screen in the fight (code-built `Label`, default font): `A / D  move    J  jab    K  cross    S  guard (hold)    Space  slip    Esc  menu`. The labels name the key **positions** on a QWERTY layout (see Controls).

## Controls
Six new actions, all bound by **physical keycode** (key position, so AZERTY and other layouts get the same hand shape; on AZERTY the "A" position is Q). They are added to the `[input]` section of project.godot (not code-registered), so the kb-check covers them.

| Action | Key (physical) | Type | Notes |
|---|---|---|---|
| fight_forward | D | held | Walk toward the opponent (+X for the player on the left). |
| fight_back | A | held | Walk away from the opponent (-X). A and D together cancel out (no movement). |
| fight_jab | J | press | Lead-hand straight punch. Fresh press only; key repeat (echo) and holding never throw a second jab. |
| fight_cross | K | press | Rear-hand straight punch. Fresh press only. |
| fight_guard | S | **hold** | Guard is up while held, down on release. Hold was chosen over toggle so guard state always matches the finger, and so there is no "forgot my guard was on" state. |
| fight_slip | Space | press | Slip (head off the line). Fresh press only. One slip direction (see Slip). |
| ui_back | Escape | press | Existing, unchanged. |

**Key count: 6 fight keys + Escape = 7 keys.** Justification:
- Movement 2: the side-on camera shows one axis (toward / away from the opponent), so up/down movement would add keys without readable value. No lateral footwork in M1 (roadmap).
- Offence 2 (jab, cross) and defence 2 (guard, slip): exactly the four M1 moves. Guard is a hold, so it needs no "off" key. Slip uses one key and one direction instead of slip-left / slip-right.
- Hand layout: left hand on A, S, D (move and guard), left thumb on Space (slip), right hand on J, K (punches). Two hands, no stretching, no mouse.

**No clash with docs/kb/input-map.md.** The existing keys are Escape, Up, Down, Enter, keypad Enter (project.godot) and Left, Right (code-registered menu_left / menu_right). None of the six new actions uses them, and the fight scene does not react to the arrow keys or Enter. W, L and the arrows are deliberately left free for later moves.

## Fight model (pure logic, src/Core)
All rules below live in `src/Core` with no Godot reference and are covered by xUnit tests. Suggested types (names are the developer's call): `MoveSet` (parsed from the JSON text, validated), `FighterInput` (one tick of input: `MoveAxis` -1/0/+1, `GuardHeld`, `JabPressed`, `CrossPressed`, `SlipPressed`, where the three `Pressed` flags are true only on the tick of a fresh press), and `PlayerFighter` with `Tick(FighterInput)` exposing `State`, `Phase`, `PhaseFrame`, `X`, `IsBlocking`, `IsEvading` and a per-tick `Contact` result (none, Jab or Cross).

### Time
- The simulation runs in whole ticks at `tickRate` from the data file. Core never uses floating-point seconds for move timing. The Godot adapter calls `Tick` exactly once per `_PhysicsProcess`, and the project's physics tick rate must equal `tickRate` (Godot's default; the adapter logs an error with `GD.PushError` if they differ).
- **Timeline rule:** a move that starts on tick N occupies ticks N to N + startup + active + recovery - 1: first its Startup ticks, then its Active ticks, then its Recovery ticks. On the next tick the fighter is free and that tick's input is processed normally.

### States
| State | Phases | Meaning |
|---|---|---|
| Idle | none | Free. Can walk, guard, or start any move. |
| GuardRaising | none | Guard held, hands coming up for `guard.raiseFrames` ticks. `IsBlocking` false. Can walk (slowed). |
| Guarding | none | Guard up. `IsBlocking` true. Can walk (slowed). |
| Jab | Startup, Active, Recovery | Lead hand punch. No walking. |
| Cross | Startup, Active, Recovery | Rear hand punch. No walking. |
| Slip | Startup, Active, Recovery | Head off the line. `IsEvading` true only in Active. No walking. |

### What each input does, per state
Order inside one tick: (1) move-timer advance and move end, (2) guard held/released, (3) fresh presses, (4) walking, (5) contact check.

| From state | Jab press | Cross press | Slip press | Guard held | Walking |
|---|---|---|---|---|---|
| Idle | start Jab | start Cross | start Slip | -> GuardRaising | yes, full speed |
| GuardRaising / Guarding | ignored | ignored | start Slip | stays; release -> Idle on that tick | yes, x `guard.moveSpeedMultiplier` |
| Jab Startup / Active | ignored | ignored | ignored | ignored until move ends | no |
| Jab Recovery | ignored | **cancel: start Cross** this tick (the one-two) | ignored | ignored until move ends | no |
| Cross (any phase) | ignored | ignored | ignored | ignored until move ends | no |
| Slip Startup / Active | ignored | ignored | ignored | ignored until move ends | no |
| Slip Recovery | **cancel: start Jab** | **cancel: start Cross** | ignored | ignored until move ends | no |

- The cancel targets come from `moves.<move>.cancelInto` in the data file; the shipped values are exactly the table above (jab -> cross; slip -> jab or cross; cross -> nothing). Only the Recovery phase can be cancelled. Startup and Active never are.
- **No input buffer.** A press that is ignored is dropped; it does not fire later. Mashing during a move does nothing (vision: button-mashing doesn't win).
- **Same-tick priority** when more than one fresh press arrives on one tick: Slip, then Jab, then Cross. Only the winner starts; the others are dropped.
- **Guard hold is level-based.** If guard is still held on the tick a move ends (or is held when nothing else starts from Idle), the fighter enters GuardRaising. Releasing guard from GuardRaising or Guarding returns to Idle on that tick. While guard is held, punches are ignored: to punch, the player must release guard. GAMEPLAY ABSTRACTION (see Boxing notes).
- A guard press and a punch press on the same tick from Idle: guard wins, the punch is ignored.
- A walk key held when a move starts stops walking for the move's duration; walking resumes automatically on the free tick if the key is still held.

### Movement and bounds
- Only the player moves, only along X. Each walking tick, `X` changes by `MoveAxis` x `movement.walkSpeedMps` / `tickRate` (x `guard.moveSpeedMultiplier` while GuardRaising or Guarding).
- Start position: `ring.playerX` from `data/balance/fighters.json`. Opponent position: `ring.opponentX`, fixed.
- Bounds, applied every tick after walking: `movement.minX` <= X <= opponentX - `movement.minSeparationM`. The player stops at the bound (no bounce, no push of the opponent). Loading fails with a clear exception if `minX` > `ring.playerX` or if `ring.playerX` > `ring.opponentX` - `minSeparationM`.
- Depth (Z) and height (Y) of the fighter root never change.

### Slip
- Player-facing: the boxer's upper body (head, neck, torso, arms) dips by `visual.slipHeadDropH` and leans sideways by `visual.slipLeanDeg` toward the camera side, so the side-on camera sees a clear drop and tilt. Feet stay planted. It returns to stance during Recovery.
- `IsEvading` is true only during Slip Active. Startup and Recovery are vulnerable: no evade, no block, no walking. The Recovery is the slip's cost; cancelling it into a punch is the reward (counter).
- Slip has no stamina cost in this task (stamina is out of scope). Nothing punches the player yet, so `IsEvading` and `IsBlocking` are exposed flags with no effect until opponent attacks exist.
- **Deferred behaviour (decided now, built later with opponent attacks):** `IsEvading` will evade **both** the jab and the cross (the two straight punches), with one slip key and one visual direction. It will **not** evade hooks when hooks are added (hooks need ducking or rotation, not a slip [S3]). Nothing in this task implements this; the flag is generic so no Core change is needed now. GAMEPLAY ABSTRACTION: one slip evades both straight punches instead of the boxer choosing the side that matches each punch, where the wrong side exposes the boxer to the other hand [S2] — one key and a side-on camera cannot show a left/right choice.

### Contact with the idle opponent (minimal)
- On each Active tick of a Jab or Cross, Core compares the gap (opponentX - player X) with the move's `reachM`. On the first Active tick where gap <= reach, the punch reports one `Contact` (Jab or Cross). A punch reports at most one contact. Out of range = no contact, no event.
- Contact is a rule on root positions, not mesh collision.
- The adapter shows `Contact: Jab` or `Contact: Cross` as a screen `Label` near the top centre for `feedback.contactTextFrames` ticks (a newer contact replaces the text and restarts the timer) and writes one `GD.Print` line per contact.
- **Deferred:** damage, health, scoring, opponent flinch or reaction, hit-stop, sound, blocking or slipping of the player's punches by the opponent, and any use of the boxer stats or build (reach and speed are the same for both boxers in this task).

### Idle opponent
- No AI. The opponent never moves, never punches, never changes state and never reacts to contact. It keeps the existing static guard pose (an optional gentle idle bob is allowed, as in character-select.md).
- The opponent is a fixed X value for Core; it has no `PlayerFighter` instance.

## Visuals (Godot adapter, game/)
Thin adapter in `game/scripts` reads input, calls Core, and poses the existing fighter parts from `State`, `Phase` and `PhaseFrame` each frame. Code-driven motion only (no AnimationPlayer resources, no external assets, no shaders).
- Stance: all fighters stand orthodox in M1. The lead hand is the glove on the lead-foot side (the side nearer the opponent); for orthodox that is `GloveL`. The developer confirms which built glove is the lead and documents it.
- Jab: lead `ForearmL`/`GloveL` move straight forward toward the opponent during Startup to `visual.punchExtendH`, hold through Active, return to guard during Recovery. The rear glove stays up.
- Cross: rear `GloveR` extends the same way, and the torso, shoulders and head turn by up to `visual.crossTorsoTurnDeg` toward the punch (the cross gets its power and extra reach from the rear-leg push and the hip and shoulder turn [S1][S3][S8]). The lead glove stays up.
- Guard: both gloves rise by `visual.guardGloveRaiseH` and move inward by `visual.guardGloveInwardW` to cover the face, interpolated over `guard.raiseFrames`; they drop back at once on release.
- Slip: as described under Slip. The look (upper body dips and leans, feet planted) is a readable visual choice, not a claim about how a real slip is built.
- Every move returns the parts exactly to the build's guard pose when the fighter is Idle (within 1 mm).
- Visual tuning: `visual.punchExtendH` is tuned so that a punch thrown at gap == reach visibly reaches the opponent's head or gloves, and a punch at gap > reach visibly falls short.

## Acceptance criteria
AC1 to AC16 are xUnit tests in `tests/Core.Tests` (tick-exact, using the shipped `player-moves.json` and `fighters.json` text). AC17 to AC24 are checked by the headless smoke scene where practical (the smoke may drive input with `Input.ActionPress` / `ActionRelease`) and by the playtest. "Ticks" means `Tick` calls.
- AC1. **Given** the shipped `data/balance/player-moves.json`, **when** parsed, **then** it has `tickRate`, moves `jab`, `cross`, `slip` with integer frame counts >= 1, positive `reachM` for jab and cross, `guard`, `movement` and `feedback` blocks; **and given** a copy with a 0 or missing frame count, a non-positive speed or reach, an unknown `cancelInto` name, or a bound that excludes `ring.playerX`, **when** parsed, **then** it throws and the message names the bad key.
- AC2. **Given** an Idle player, **when** Jab is pressed on tick 1 and no other input follows, **then** the state is Jab Startup for exactly `jab.startupFrames` ticks, Active for exactly `jab.activeFrames`, Recovery for exactly `jab.recoveryFrames`, and Idle on the next tick.
- AC3. **Given** an Idle player, **when** Cross is pressed, **then** the same timeline holds with the `cross` frame counts; **and** with the shipped data the cross's total ticks (startup + active + recovery) are greater than the jab's and at most 1.5 times the jab's (so the cross is a bit slower and heavier, and the one-two stays usable).
- AC4. **Given** an Idle player, **when** Slip is pressed, **then** the slip timeline follows the `slip` frame counts, and `IsEvading` is true on every Active tick and false on every Startup, Recovery and Idle tick.
- AC5. **Given** a Jab in Startup or Active, **when** Jab, Cross or Slip is pressed, **then** nothing changes, and after the jab ends the fighter is Idle (the press was not buffered).
- AC6. **Given** a Jab in Recovery, **when** Cross is pressed, **then** on that same tick the state is Cross Startup, frame 1 (the one-two cancel), and that cross uses the normal `cross` frame counts (no shortened startup after a jab); **and given** a jab pressed on tick 1 and Cross pressed on the first Recovery tick (tick 1 + `jab.startupFrames` + `jab.activeFrames`), **then** the fighter is Idle again exactly `cross` total ticks later, i.e. the cancel skips the rest of the jab's recovery; **and when** Jab or Slip is pressed instead, **then** it is ignored.
- AC7. **Given** a Cross in any phase, **when** Jab, Cross or Slip is pressed, **then** it is ignored and the cross runs to its end.
- AC8. **Given** a Slip in Recovery, **when** Jab (or Cross) is pressed, **then** on that same tick the state is Jab (or Cross) Startup, frame 1; **and given** a Slip in Startup or Active, **when** any punch is pressed, **then** it is ignored.
- AC9. **Given** an Idle player, **when** guard is held, **then** the state is GuardRaising with `IsBlocking` false for exactly `guard.raiseFrames` ticks, then Guarding with `IsBlocking` true for as long as it is held; **and when** guard is released, **then** on that tick the state is Idle and `IsBlocking` is false.
- AC10. **Given** Guarding, **when** Jab or Cross is pressed, **then** it is ignored and the state stays Guarding; **and when** Slip is pressed, **then** a Slip starts that tick, and if guard is still held when the slip ends the state becomes GuardRaising, then Guarding.
- AC11. **Given** Idle at `ring.playerX`, **when** forward is held for N ticks (no bound reached), **then** X increases by exactly N x `walkSpeedMps` / `tickRate` (within 1e-6); back decreases it the same way; both or neither held leaves X unchanged; while Guarding the change is multiplied by `guard.moveSpeedMultiplier`.
- AC12. **Given** a Jab, Cross or Slip in any phase, **when** forward or back is held, **then** X does not change; **and when** the move ends with the key still held, **then** walking resumes on the free tick.
- AC13. **Given** back held for 10 seconds of ticks, **then** X equals `movement.minX`; **given** forward held for 10 seconds of ticks, **then** X equals `ring.opponentX` - `movement.minSeparationM`; and X is never outside these bounds on any tick.
- AC14. **Given** a gap <= `jab.reachM`, **when** a Jab runs to its end, **then** exactly one Contact (Jab) is reported, on its first Active tick; **given** a gap between `jab.reachM` and `cross.reachM`, **then** a Jab reports no contact and a Cross reports exactly one Contact (Cross); **given** a gap > `cross.reachM`, **then** neither reports a contact.
- AC15. **Given** Idle, **when** Slip, Jab and Cross are all pressed on one tick, **then** Slip starts; **when** Jab and Cross are pressed on one tick, **then** Jab starts; **when** guard is held and Jab is pressed on the same tick from Idle, **then** the state is GuardRaising and no jab starts.
- AC16. **Given** a full fight sequence of any inputs, **when** ticked, **then** the opponent X used by Core never changes.
- AC17. **Given** Character Select step 2, **when** the player confirms the opponent, **then** the ring loads with the player on the left and the controls hint visible at the bottom, and the first D press moves the player toward the opponent with no delay or extra key.
- AC18. **Given** the project is built, **when** project.godot is inspected, **then** `fight_forward` (D), `fight_back` (A), `fight_jab` (J), `fight_cross` (K), `fight_guard` (S) and `fight_slip` (Space) exist with physical keycodes, none of them uses Escape, Enter, keypad Enter or any arrow key, and the existing actions are unchanged.
- AC19. **Given** the fight, **when** J is held down for 2 seconds, **then** exactly one jab is thrown (key repeat ignored); the same holds for K and Space.
- AC20. **Given** the fight (smoke), **when** a Jab reaches Active, **then** the lead glove is further toward the opponent than in Idle by at least half of `visual.punchExtendH` x fighter height, and the rear glove has not moved forward; for a Cross the rear glove extends and the lead does not; while Guarding both gloves are higher than in Idle; during Slip Active the `Head` is lower than in Idle; and back in Idle every posed part is within 1 mm of its built guard pose.
- AC21. **Given** the player in range, **when** a punch makes contact, **then** `Contact: Jab` or `Contact: Cross` appears near the top of the screen and disappears after `feedback.contactTextFrames` ticks, and the opponent does not move, change pose or react.
- AC22. **Given** the start position, **when** J is pressed without moving, **then** no contact text appears (the fighters start out of range); **and when** the player walks forward until stopped, **then** the player cannot pass the opponent and stops at the minimum separation.
- AC23. **Given** the fight in any move state, **when** Escape is pressed, **then** the menu shows (as in main-menu.md); **and when** New Game and both picks are made again, **then** the player starts at `ring.playerX`, Idle, with no leftover contact text.
- AC24. **Given** `Main.tscn` is launched directly (smoke, no select), **when** it runs, **then** the player is controllable with the default roster entry, a smoke-driven Jab completes and returns to Idle, and no error is logged.

## Balance values
All numbers live in `data/balance/player-moves.json` (plus the existing positions in `data/balance/fighters.json`), not in this spec or in code.
Pointer: `data/balance/player-moves.json` -> `tickRate`; `moves.jab|cross|slip.startupFrames|activeFrames|recoveryFrames|cancelInto`; `moves.jab|cross.reachM`; `guard.raiseFrames`, `guard.moveSpeedMultiplier`; `movement.walkSpeedMps`, `movement.minX`, `movement.minSeparationM`; `feedback.contactTextFrames`; `visual.*` (pose offsets, developer may tune about 30 percent for readability). `data/balance/fighters.json` -> `ring.playerX`, `ring.opponentX`.
Parsing and validation happen in Core (text in, exception on bad data); the adapter only reads the file. The file must be included in the exported build, as with fighters.json.

## Boxing notes
Reviewed by boxing-consultant on 2026-11-28 (work/10/boxing-review.md). Sources are in docs/boxing/sources.md.

### Real boxing this spec relies on
- The jab is the lead-hand straight punch and the cross is the rear-hand straight punch; in an orthodox stance the left hand and left foot lead, so `GloveL` is the lead [S1][S3].
- The cross has farther to travel than the jab and gets its power and extra reach from the rear-leg push and the hip and shoulder turn [S1][S3][S8]. The metre values for reach are tuning values, not measured (no source gives them).
- The jab then cross (the "one-two") is a basic combination taught as lead straight then rear straight, which justifies the jab -> cross cancel [S1][S2][S3]. Punches thrown in a combination are delivered faster than a single maximal punch [S4].
- A slip moves the head off the line of an incoming straight punch, taught against the jab and the rear straight, and is followed at once by a counter with both hands free, which justifies Slip Active = evade and Slip Recovery -> punch cancel [S2][S3].
- A high guard (hands up, chin down, elbows by the ribs) and a tighter cover with both forearms in front protect the head against straight punches [S1][S2][S3]. In real boxing only the punching hand leaves the guard while the other hand stays up and the punching hand returns along the same line [S1][S2].
- Real timing scale: throw to first contact about 0.11 to 0.14 s for jab and cross [S6]; a whole punch (out, contact, back) about 0.4 to 0.5 s [S5][S6]; contact itself about 0.02 to 0.03 s [S7]; defenders may have more time to evade than older estimates said [S4]. No allowed source gives real slip durations.

### GAMEPLAY ABSTRACTION lines
GAMEPLAY ABSTRACTION: jab and cross use fixed tick timings chosen for rhythm instead of measured durations (throw about 0.1 s, whole punch about 0.4 to 0.5 s [S5][S6]) — a readable startup/active/recovery beat; the jab total is deliberately quicker than real.
GAMEPLAY ABSTRACTION: the cross is a bit slower and heavier than the jab (total about 1.4 times) instead of nearly equal in speed [S4][S5][S6] — gives the cross a risk-versus-reward cost with only two punches; the gap is kept small because real speeds are close.
GAMEPLAY ABSTRACTION: slip timings are tuning values instead of measured durations (none exist in the allowed sources; a generous evade window fits [S4]) — a clear, learnable window.
GAMEPLAY ABSTRACTION: reach is a root-to-root distance (jab shorter than cross) instead of arm length, stance and step length [S1][S3] — one readable range rule.
GAMEPLAY ABSTRACTION: the boxer cannot step while punching or slipping instead of stepping with the punch [S9] — committed moves and simple controls.
GAMEPLAY ABSTRACTION: held guard is a separate committed state with no punching, instead of punching out of the guard with the free hand kept up [S1][S2] — gives guard a clear cost and keeps the four moves distinct. Both hands close together because guard is its own state.
GAMEPLAY ABSTRACTION: the idle pose is the stance guard and "Guard" is a tighter cover with a short raise instead of one continuous guard that is always up [S1][S3] — a visible state the player controls.
GAMEPLAY ABSTRACTION: one slip direction on one key instead of a side chosen against each punch [S2] — small key set and a side-on camera. Once opponent attacks exist the one slip will evade both jab and cross (see Slip).
GAMEPLAY ABSTRACTION: the only chains are jab -> cross and slip -> punch instead of the wider set such as the double jab, jab to the body and hooks [S1][S3] — M1 has four moves; combos come later.
GAMEPLAY ABSTRACTION: walking is a smooth glide along one axis instead of step-and-drag footwork and pivots [S1][S8] — roadmap M1 limits footwork; walk speed has no source.
GAMEPLAY ABSTRACTION: all boxers are orthodox with identical reach and timings instead of southpaws and varied builds [S3] — stats are display-only until combat tuning.
GAMEPLAY ABSTRACTION: a punch "contact" is a distance check on root positions with no target, damage or reaction instead of a glove landing on a head or body [S7] — the opponent is idle in this task.
GAMEPLAY ABSTRACTION: no input buffering and no cancel out of Startup or Active instead of a boxer adapting a punch mid-flight — readable, mash-proof rhythm (vision pillar).

### Boxing review outcome
The designer's decision on each of the consultant's suggestions. The data file is authoritative for the numbers; the table shows the change made.

| # | Suggestion | Decision | Reason |
|---|---|---|---|
| C1 | Narrow the jab/cross gap | **Accepted.** Cross startup 10 -> 8, recovery 16 -> 14 (active stays 4), so cross total 30 -> 26 ticks against the jab's 18 (ratio 1.67 -> 1.44). Jab recovery kept at 9 (the optional lift to 12 is **rejected**). | Real startups are nearly equal [S6], so the gap shrinks, but the client wants the cross to feel a bit heavier and the one-two stays usable. A snappy 18-tick jab keeps the rhythm quick, and A1 already marks it as quicker than real. AC3 now also caps the ratio at 1.5. |
| C2 | Shorter cross startup when it follows a jab | **Rejected for M1.** | The cancel already skips the rest of the jab's recovery, so the one-two is quicker without it. A second startup value would add a data key and a code branch for a small feel gain. Revisit in combat tuning. AC6 is unchanged apart from the timeline check. |
| C3 | Remove "because of hip and shoulder rotation" as the reason the cross is slower, and the "bend at the knees and waist" slip claim | **Accepted.** | Rotation is the source of the cross's power and reach [S1][S3], not a proven cause of lower speed; the slip look is a visual choice. Both removed from the claims above. |
| C4 | Guard wording: only the punching hand leaves the guard | **Accepted.** | Stated above with [S1][S2]; the closed-both-hands behaviour is an abstraction line. |
| Slip direction | Does one slip evade both jab and cross once opponents attack? | **Yes, deferred.** `IsEvading` will evade both straight punches and not hooks. Built with opponent attacks, not now. | A single key cannot choose a side; both punches are straights [S2][S3]. Marked as an abstraction under Slip. |

## Out of scope
- Stamina, health, damage, KO, knockdowns, round timer display or round end, points decision, scoring.
- Opponent AI or any opponent movement, punches, blocking, slipping, reaction or flinch. The opponent never attacks, so `IsBlocking` / `IsEvading` have no effect yet.
- Hooks, uppercuts, body shots, feints, clinches, lateral or depth movement, pivots, southpaw stance, switching sides.
- Use of boxer stats or build in moves (reach, speed, power).
- Hit-stop, camera shake, particles, sound, shaders, HUD beyond the contact text and controls hint.
- Gamepad, mouse controls in the fight, rebinding, pause menu, settings.
- Input buffering, combos beyond the two cancel rules above.

## Open questions
1. Guard key: S (left hand, next to the move keys) or L (right hand, next to the punch keys)? **Recommendation: S.** Guard and punches are exclusive, so the punching hand never needs to reach for guard, and the left hand already sits on the move keys.
2. Should the arrow keys Left/Right also move the boxer as an alternative to A/D? **Recommendation: no for M1.** The brief asked to avoid menu keys, a small key set reads better in the controls hint, and adding the alternate later is a one-line input-map change.
3. Is one slip direction acceptable for M1, or does the client want slip-left and slip-right (one more key, 8 total)? **Recommendation: one direction.** The side-on camera cannot show a left/right choice well and the opponent has no attacks yet; if kept, one slip will evade both jab and cross later (see Slip). Revisit when opponent AI arrives.

## Playtest checklist
Five things the client checks by hand at G3.
1. Pick both boxers: the ring appears with a controls hint at the bottom. Hold D: your boxer (left, "YOU") glides toward the opponent and stops a short distance in front of it without passing through. Hold A: it backs away and stops before leaving the floor.
2. From the starting distance press J: the lead glove shoots forward and comes back, and no "Contact" text appears. Walk close and press J, then K: each punch shows "Contact: Jab" / "Contact: Cross" at the top for under a second, and the gloves visibly reach the opponent, who does not react.
3. Press K by itself and then J by itself: the cross feels a bit slower and heavier (body turns) than the jab, but not sluggish. Press J then quickly K: the cross follows the jab smoothly as a one-two. Mash J and K fast: the boxer does not throw faster than the moves allow.
4. Hold S: both gloves come up to cover the face and the boxer walks more slowly with A/D; pressing J or K does nothing until you let go of S. Hold J for two seconds: only one jab is thrown.
5. Press Space: the boxer's upper body dips and leans, then comes back; press J or K right as it comes back and the punch fires straight out of the slip. Press Escape at any moment: you are back at the menu, and a new fight starts fresh.
