# Spec: Player controls (playable fight, idle opponent)

Status: DRAFT   Issue: #10

## Goal
When the player confirms both boxers in Character Select, the ring becomes a fight scene: the player's boxer walks forward and back and throws a jab, a cross, a held guard and a slip to either side, all from the keyboard. The opponent stands idle. This is the first time the game is played rather than looked at, and it sets the timing rules (startup, active, recovery, cancels) that stamina, damage and AI will build on later.

## Player story
As a player, I want to step in and out of range and throw a jab, a cross, a guard and a slip left or right with a handful of keys, so that I can feel the rhythm of the basic boxing moves before the opponent fights back.

## Scene flow (how the fight is entered)
- Unchanged flow: Menu -> New Game -> Select step 1 -> Select step 2 -> confirm -> `game/scenes/Main.tscn`. The ring scene **is** the fight scene; no new scene file is required. The player fighter (left, "YOU") gets the controls; the opponent (right, "CPU") stays idle.
- Controls are live on the first physics tick of the ring. There is no countdown, intro or "Fight!" banner (vision pillar 3: launch to first punch in seconds).
- The confirm key (Enter) is not a fight key, so a held Enter from the select screen does nothing in the fight. The Left and Right arrows are fight keys (move), so an arrow still held from the select screen walks the boxer as soon as the fight starts; this is accepted (the player lets go).
- Escape (`ui_back`) behaves exactly as today: back to the menu, picks cleared. It works in any move state. New Game again starts a fresh fight with the player back at `ring.playerX` in Idle.
- Direct launch of `Main.tscn` (smoke test) still works: both fighters fall back to the first roster entry and the player is controllable.
- The existing invisible round clock keeps running unchanged; nothing happens when it reaches zero (round logic is out of scope).
- A one-line controls hint is shown at the bottom of the screen in the fight (code-built `Label`, default font): `A / D or Left / Right  move    J  jab    K  cross    S  guard (hold)    Q / E  slip left / right    Esc  menu`. The labels name the key **positions** on a QWERTY layout (see Controls).

## Controls
Seven new actions, all bound by **physical keycode** (key position, so AZERTY and other layouts get the same hand shape; on AZERTY the "A" position is Q and the "Q" position is A). They are added to the `[input]` section of project.godot (not code-registered), so the kb-check covers them. `fight_forward` and `fight_back` each have **two** key events (a letter and an arrow); the other five have one.

| Action | Key (physical) | Type | Notes |
|---|---|---|---|
| fight_forward | D **and** Right arrow | held | Walk toward the opponent (+X for the player on the left). Either key does it; both together are the same as one. |
| fight_back | A **and** Left arrow | held | Walk away from the opponent (-X). Forward and back together cancel out (no movement), whichever keys are used. |
| fight_jab | J | press | Lead-hand straight punch. Fresh press only; key repeat (echo) and holding never throw a second jab. |
| fight_cross | K | press | Rear-hand straight punch. Fresh press only. |
| fight_guard | S | **hold** | Guard is up while held, down on release. Hold was chosen over toggle so guard state always matches the finger, and so there is no "forgot my guard was on" state. |
| fight_slip_left | Q | press | Slip to the boxer's left (head off the line to the lead-hand side). Fresh press only. See Slip. |
| fight_slip_right | E | press | Slip to the boxer's right (head off the line to the rear-hand side). Fresh press only. See Slip. |
| ui_back | Escape | press | Existing, unchanged. |

**Key count: 9 fight keys + Escape = 10 keys** (A, D, Left, Right, J, K, S, Q, E, Esc), or 7 if the arrows are ignored. Justification:
- Movement 2 actions (4 keys): the side-on camera shows one axis (toward / away from the opponent), so up/down movement would add keys without readable value. No lateral footwork in M1 (roadmap). The arrows are the client's requested second way to walk; they add keys but no new behaviour.
- Offence 2 (jab, cross) and defence 3 (guard, slip left, slip right): the four M1 moves, with slip split in two by client decision. Guard is a hold, so it needs no "off" key.
- Slip keys Q and E: they sit directly above A and D, so the left hand's ring and middle fingers reach them with a short move up from A and D while the hand stays on its home position, Q is physically left of E (matches left / right), and neither key is in the right hand's J / K / arrow area. The right hand stays on J / K for punches. Space is no longer used.
- Hand layout (A / D walking): left hand on Q, E, A, S, D (move, guard, slip), right hand on J, K (punches). Two hands, no stretching, no mouse. Players who walk with the arrows move the right hand off J / K; see Open questions.

**No clash with docs/kb/input-map.md.** The existing keys are Escape, Up, Down, Enter, keypad Enter (project.godot) and Left, Right (code-registered `menu_left` / `menu_right`, used only on the Character Select screen). The Left and Right arrows are now shared by two **different actions on different screens**: `menu_left` / `menu_right` in Character Select and `fight_back` / `fight_forward` in the fight. The two screens are never active together, the fight does not read `menu_*`, and Character Select does not read `fight_*`, so there is no clash. Existing action definitions are not changed. Q, E and S collide with nothing. W, L, Space, Up and Down stay free for later moves.

## Fight model (pure logic, src/Core)
All rules below live in `src/Core` with no Godot reference and are covered by xUnit tests. Suggested types (names are the developer's call): `MoveSet` (parsed from the JSON text, validated), `FighterInput` (one tick of input: `MoveAxis` -1/0/+1, `GuardHeld`, `JabPressed`, `CrossPressed`, `SlipLeftPressed`, `SlipRightPressed`, where the four `Pressed` flags are true only on the tick of a fresh press), and `PlayerFighter` with `Tick(FighterInput)` exposing `State`, `Phase`, `PhaseFrame`, `X`, `IsBlocking`, `IsEvading`, `EvadeSide` (none, Left or Right; non-none only while `IsEvading`) and a per-tick `Contact` result (none, Jab or Cross). The adapter builds `MoveAxis` as (`fight_forward` held ? 1 : 0) - (`fight_back` held ? 1 : 0), so the letter and arrow keys are indistinguishable to Core.

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
| SlipLeft | Startup, Active, Recovery | Head off the line to the boxer's left. `IsEvading` true only in Active, with `EvadeSide` Left. No walking. |
| SlipRight | Startup, Active, Recovery | Head off the line to the boxer's right. `IsEvading` true only in Active, with `EvadeSide` Right. No walking. |

### What each input does, per state
Order inside one tick: (1) move-timer advance and move end, (2) guard held/released, (3) fresh presses, (4) walking, (5) contact check.

"Slip press" in the table means either slip key (Q or E); both rows of behaviour are identical, only the side differs.

| From state | Jab press | Cross press | Slip press (L or R) | Guard held | Walking |
|---|---|---|---|---|---|
| Idle | start Jab | start Cross | start that SlipLeft / SlipRight | -> GuardRaising | yes, full speed |
| GuardRaising / Guarding | ignored | ignored | start that slip | stays; release -> Idle on that tick | yes, x `guard.moveSpeedMultiplier` |
| Jab Startup / Active | ignored | ignored | ignored | ignored until move ends | no |
| Jab Recovery | ignored | **cancel: start Cross** this tick (the one-two) | ignored | ignored until move ends | no |
| Cross (any phase) | ignored | ignored | ignored | ignored until move ends | no |
| SlipLeft / SlipRight Startup / Active | ignored | ignored | ignored (including the other side) | ignored until move ends | no |
| SlipLeft / SlipRight Recovery | **cancel: start Jab** | **cancel: start Cross** | ignored (no slip-to-slip chain, including the other side) | ignored until move ends | no |

- The cancel targets come from `moves.<move>.cancelInto` in the data file; the shipped values are exactly the table above (jab -> cross; slipLeft and slipRight -> jab or cross; cross -> nothing). Only the Recovery phase can be cancelled. Startup and Active never are.
- **No input buffer.** A press that is ignored is dropped; it does not fire later. Mashing during a move does nothing (vision: button-mashing doesn't win).
- **Same-tick priority** when more than one fresh press arrives on one tick: SlipLeft, then SlipRight, then Jab, then Cross. Only the winner starts; the others are dropped. A fixed order (left slip first) keeps the rule simple and never silently throws away every input; pressing both slip keys on one tick is a rare double-tap and slip-left winning is accepted.
- **Guard hold is level-based.** If guard is still held on the tick a move ends (or is held when nothing else starts from Idle), the fighter enters GuardRaising. Releasing guard from GuardRaising or Guarding returns to Idle on that tick. While guard is held, punches are ignored: to punch, the player must release guard. GAMEPLAY ABSTRACTION (see Boxing notes).
- A guard press and a punch press on the same tick from Idle: guard wins, the punch is ignored.
- A walk key held when a move starts stops walking for the move's duration; walking resumes automatically on the free tick if the key is still held.

### Movement and bounds
- Only the player moves, only along X. Each walking tick, `X` changes by `MoveAxis` x `movement.walkSpeedMps` / `tickRate` (x `guard.moveSpeedMultiplier` while GuardRaising or Guarding).
- Start position: `ring.playerX` from `data/balance/fighters.json`. Opponent position: `ring.opponentX`, fixed.
- Bounds, applied every tick after walking: `movement.minX` <= X <= opponentX - `movement.minSeparationM`. The player stops at the bound (no bounce, no push of the opponent). Loading fails with a clear exception if `minX` > `ring.playerX` or if `ring.playerX` > `ring.opponentX` - `minSeparationM`.
- Depth (Z) and height (Y) of the fighter root never change.

### Slip (two sides)
- **What left and right mean.** The sides are the **boxer's own**: slip-left moves the head to the boxer's left (the lead-hand side for an orthodox boxer), slip-right to the boxer's right (the rear-hand side). They are **not** screen directions. The player faces +X and the camera sits on the +Z side, so the boxer's right is the side **toward the camera** and the boxer's left is **away from the camera**. Slip direction in real boxing depends on the incoming punch [S2], so the side must be a player choice that names a body side, not a fixed "back" or "forward" step. The slips do **not** move the boxer along X; the root stays where it is and nothing else about position changes.
- **Player-facing look (both sides share the same timing and the same dip).** The upper body (head, neck, torso, arms) dips by `visual.slipHeadDropH`, leans sideways toward the chosen side by `visual.slipLeanDeg` (rolling about the facing axis, so the head moves toward the camera for slip-right and away for slip-left), and the shoulders turn about the vertical axis by `visual.slipTurnDeg`, in opposite directions for left and right. The turn exists only so a side-on camera can tell the two sides apart: the sideways lean alone is a small, depth-only change in this view. Feet stay planted. The pose returns to stance during Recovery. The look is a readable visual choice, not a claim about how a real slip is built.
- The two slips have the same data shape and the shipped timings are identical (`moves.slipLeft`, `moves.slipRight`); they are separate entries so they can later be tuned apart (for example an inside versus outside slip) without a code change.
- `IsEvading` is true only during a slip's Active phase, and `EvadeSide` tells which side (Left for SlipLeft, Right for SlipRight). Startup and Recovery are vulnerable: no evade, no block, no walking. The Recovery is the slip's cost; cancelling it into a punch is the reward (counter).
- Slip has no stamina cost in this task (stamina is out of scope). Nothing punches the player yet, so `IsEvading`, `EvadeSide` and `IsBlocking` are exposed state with no effect until opponent attacks exist.
- **Deferred behaviour (built later with opponent attacks):** which slip side clears which incoming straight punch (jab or cross) is **not decided here**. The opponent-attack spec will define it, with a boxing-consultant review of the real-boxing side [S2][S3]. The wrong side is expected to leave the boxer open to a punch, as in real boxing where the side depends on the punch [S2]. Slips will not evade hooks when hooks are added (hooks need ducking or rotation, not a slip [S3]). Nothing in this task implements this; Core already exposes `EvadeSide`, so no Core change is needed for it. GAMEPLAY ABSTRACTION: the player picks the slip side with a key, and in this task the choice has no effect on any punch because there are none, instead of the side being the answer to a punch that is already coming [S2] — the opponent is idle.

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
- Slip: as described under Slip. SlipRight leans the head toward the camera (+Z), SlipLeft away from it (-Z); both dip and turn the shoulders (opposite directions). Feet planted. The look is a readable visual choice, not a claim about how a real slip is built. The player root's Z never changes; only the upper-body parts are posed.
- Every move returns the parts exactly to the build's guard pose when the fighter is Idle (within 1 mm).
- Visual tuning: `visual.punchExtendH` is tuned so that a punch thrown at gap == reach visibly reaches the opponent's head or gloves, and a punch at gap > reach visibly falls short.

## Acceptance criteria
AC1 to AC16 are xUnit tests in `tests/Core.Tests` (tick-exact, using the shipped `player-moves.json` and `fighters.json` text). AC17 to AC24 are checked by the headless smoke scene where practical (the smoke may drive input with `Input.ActionPress` / `ActionRelease`) and by the playtest. "Ticks" means `Tick` calls. "A slip" in a test means each of SlipLeft and SlipRight run separately, with that side's own frame counts.
- AC1. **Given** the shipped `data/balance/player-moves.json`, **when** parsed, **then** it has `tickRate`, moves `jab`, `cross`, `slipLeft`, `slipRight` with integer frame counts >= 1, positive `reachM` for jab and cross, `guard`, `movement` and `feedback` blocks; **and given** a copy with a 0 or missing frame count, a non-positive speed or reach, an unknown `cancelInto` name, or a bound that excludes `ring.playerX`, **when** parsed, **then** it throws and the message names the bad key.
- AC2. **Given** an Idle player, **when** Jab is pressed on tick 1 and no other input follows, **then** the state is Jab Startup for exactly `jab.startupFrames` ticks, Active for exactly `jab.activeFrames`, Recovery for exactly `jab.recoveryFrames`, and Idle on the next tick.
- AC3. **Given** an Idle player, **when** Cross is pressed, **then** the same timeline holds with the `cross` frame counts; **and** with the shipped data the cross's total ticks (startup + active + recovery) are greater than the jab's and at most 1.5 times the jab's (so the cross is a bit slower and heavier, and the one-two stays usable).
- AC4. **Given** an Idle player, **when** SlipLeft (or SlipRight) is pressed, **then** the timeline follows the `slipLeft` (or `slipRight`) frame counts, `IsEvading` is true on every Active tick and false on every Startup, Recovery and Idle tick, and `EvadeSide` is Left (or Right) on every Active tick and none on every other tick.
- AC5. **Given** a Jab in Startup or Active, **when** Jab, Cross, SlipLeft or SlipRight is pressed, **then** nothing changes, and after the jab ends the fighter is Idle (the press was not buffered).
- AC6. **Given** a Jab in Recovery, **when** Cross is pressed, **then** on that same tick the state is Cross Startup, frame 1 (the one-two cancel), and that cross uses the normal `cross` frame counts (no shortened startup after a jab); **and given** a jab pressed on tick 1 and Cross pressed on the first Recovery tick (tick 1 + `jab.startupFrames` + `jab.activeFrames`), **then** the fighter is Idle again exactly `cross` total ticks later, i.e. the cancel skips the rest of the jab's recovery; **and when** Jab, SlipLeft or SlipRight is pressed instead, **then** it is ignored.
- AC7. **Given** a Cross in any phase, **when** Jab, Cross, SlipLeft or SlipRight is pressed, **then** it is ignored and the cross runs to its end.
- AC8. **Given** a SlipLeft or SlipRight in Recovery, **when** Jab (or Cross) is pressed, **then** on that same tick the state is Jab (or Cross) Startup, frame 1; **and when** either slip key (including the other side) is pressed in that Recovery, **then** it is ignored; **and given** a slip in Startup or Active, **when** any punch or either slip key is pressed, **then** it is ignored and the slip keeps its side.
- AC9. **Given** an Idle player, **when** guard is held, **then** the state is GuardRaising with `IsBlocking` false for exactly `guard.raiseFrames` ticks, then Guarding with `IsBlocking` true for as long as it is held; **and when** guard is released, **then** on that tick the state is Idle and `IsBlocking` is false.
- AC10. **Given** Guarding, **when** Jab or Cross is pressed, **then** it is ignored and the state stays Guarding; **and when** SlipLeft (or SlipRight) is pressed, **then** that slip starts that tick, and if guard is still held when the slip ends the state becomes GuardRaising, then Guarding.
- AC11. **Given** Idle at `ring.playerX`, **when** forward is held for N ticks (no bound reached), **then** X increases by exactly N x `walkSpeedMps` / `tickRate` (within 1e-6); back decreases it the same way; both or neither held leaves X unchanged; while Guarding the change is multiplied by `guard.moveSpeedMultiplier`.
- AC12. **Given** a Jab, Cross, SlipLeft or SlipRight in any phase, **when** forward or back is held, **then** X does not change; **and when** the move ends with the key still held, **then** walking resumes on the free tick.
- AC13. **Given** back held for 10 seconds of ticks, **then** X equals `movement.minX`; **given** forward held for 10 seconds of ticks, **then** X equals `ring.opponentX` - `movement.minSeparationM`; and X is never outside these bounds on any tick.
- AC14. **Given** a gap <= `jab.reachM`, **when** a Jab runs to its end, **then** exactly one Contact (Jab) is reported, on its first Active tick; **given** a gap between `jab.reachM` and `cross.reachM`, **then** a Jab reports no contact and a Cross reports exactly one Contact (Cross); **given** a gap > `cross.reachM`, **then** neither reports a contact.
- AC15. **Given** Idle, **when** SlipLeft, SlipRight, Jab and Cross are all pressed on one tick, **then** SlipLeft starts; **when** SlipRight, Jab and Cross are pressed on one tick, **then** SlipRight starts; **when** Jab and Cross are pressed on one tick, **then** Jab starts; **when** guard is held and Jab is pressed on the same tick from Idle, **then** the state is GuardRaising and no jab starts.
- AC16. **Given** a full fight sequence of any inputs, **when** ticked, **then** the opponent X used by Core never changes.
- AC17. **Given** Character Select step 2, **when** the player confirms the opponent, **then** the ring loads with the player on the left and the controls hint visible at the bottom, and the first D press **or** the first Right-arrow press moves the player toward the opponent with no delay or extra key (the smoke drives `fight_forward`; the playtest uses both physical keys).
- AC18. **Given** the project is built, **when** project.godot is inspected, **then** `fight_forward` (D and Right arrow), `fight_back` (A and Left arrow), `fight_jab` (J), `fight_cross` (K), `fight_guard` (S), `fight_slip_left` (Q) and `fight_slip_right` (E) exist with physical keycodes; none of them uses Escape, Enter, keypad Enter, Up, Down or Space; the only arrow keys used are Left and Right, and only by `fight_back` and `fight_forward`; no fight action is shared between two keys of different meaning; and the existing actions (including code-registered `menu_left` / `menu_right`) are unchanged.
- AC19. **Given** the fight, **when** J is held down for 2 seconds, **then** exactly one jab is thrown (key repeat ignored); the same holds for K, Q and E. **And when** A and Left (or D and Right) are held together, **then** the boxer walks at the normal speed, not double; **and when** D and Left (or A and Right) are held together, **then** the boxer does not move.
- AC20. **Given** the fight (smoke), **when** a Jab reaches Active, **then** the lead glove is further toward the opponent than in Idle by at least half of `visual.punchExtendH` x fighter height, and the rear glove has not moved forward; for a Cross the rear glove extends and the lead does not; while Guarding both gloves are higher than in Idle; during SlipLeft Active and SlipRight Active the `Head` is lower than in Idle, the SlipRight head is nearer the camera (greater Z) than in Idle and the SlipLeft head is further from it (smaller Z), the shoulders are turned in opposite directions for the two sides, and the fighter root X and Z are unchanged; and back in Idle every posed part is within 1 mm of its built guard pose.
- AC21. **Given** the player in range, **when** a punch makes contact, **then** `Contact: Jab` or `Contact: Cross` appears near the top of the screen and disappears after `feedback.contactTextFrames` ticks, and the opponent does not move, change pose or react.
- AC22. **Given** the start position, **when** J is pressed without moving, **then** no contact text appears (the fighters start out of range); **and when** the player walks forward until stopped, **then** the player cannot pass the opponent and stops at the minimum separation.
- AC23. **Given** the fight in any move state, **when** Escape is pressed, **then** the menu shows (as in main-menu.md); **and when** New Game and both picks are made again, **then** the player starts at `ring.playerX`, Idle, with no leftover contact text.
- AC24. **Given** `Main.tscn` is launched directly (smoke, no select), **when** it runs, **then** the player is controllable with the default roster entry, a smoke-driven Jab completes and returns to Idle, and no error is logged.

## Balance values
All numbers live in `data/balance/player-moves.json` (plus the existing positions in `data/balance/fighters.json`), not in this spec or in code.
Pointer: `data/balance/player-moves.json` -> `tickRate`; `moves.jab|cross|slipLeft|slipRight.startupFrames|activeFrames|recoveryFrames|cancelInto`; `moves.jab|cross.reachM`; `guard.raiseFrames`, `guard.moveSpeedMultiplier`; `movement.walkSpeedMps`, `movement.minX`, `movement.minSeparationM`; `feedback.contactTextFrames`; `visual.*` (pose offsets, developer may tune about 30 percent for readability). `data/balance/fighters.json` -> `ring.playerX`, `ring.opponentX`.
Parsing and validation happen in Core (text in, exception on bad data); the adapter only reads the file. The file must be included in the exported build, as with fighters.json.

## Boxing notes
Reviewed by boxing-consultant on 2026-11-28 (work/10/boxing-review.md). Sources are in docs/boxing/sources.md.

### Real boxing this spec relies on
- The jab is the lead-hand straight punch and the cross is the rear-hand straight punch; in an orthodox stance the left hand and left foot lead, so `GloveL` is the lead [S1][S3].
- The cross has farther to travel than the jab and gets its power and extra reach from the rear-leg push and the hip and shoulder turn [S1][S3][S8]. The metre values for reach are tuning values, not measured (no source gives them).
- The jab then cross (the "one-two") is a basic combination taught as lead straight then rear straight, which justifies the jab -> cross cancel [S1][S2][S3]. Punches thrown in a combination are delivered faster than a single maximal punch [S4].
- A slip moves the head off the line of an incoming straight punch, taught against the jab and the rear straight, and is followed at once by a counter with both hands free, which justifies Slip Active = evade and Slip Recovery -> punch cancel [S2][S3]. The side of the slip depends on the incoming punch [S2], which is why the game has a slip to each side.
- A high guard (hands up, chin down, elbows by the ribs) and a tighter cover with both forearms in front protect the head against straight punches [S1][S2][S3]. In real boxing only the punching hand leaves the guard while the other hand stays up and the punching hand returns along the same line [S1][S2].
- Real timing scale: throw to first contact about 0.11 to 0.14 s for jab and cross [S6]; a whole punch (out, contact, back) about 0.4 to 0.5 s [S5][S6]; contact itself about 0.02 to 0.03 s [S7]; defenders may have more time to evade than older estimates said [S4]. No allowed source gives real slip durations.

### GAMEPLAY ABSTRACTION lines
GAMEPLAY ABSTRACTION: jab and cross use fixed tick timings chosen for rhythm instead of measured durations (throw about 0.1 s, whole punch about 0.4 to 0.5 s [S5][S6]) — a readable startup/active/recovery beat; the jab total is deliberately quicker than real.
GAMEPLAY ABSTRACTION: the cross is a bit slower and heavier than the jab (total about 1.4 times) instead of nearly equal in speed [S4][S5][S6] — gives the cross a risk-versus-reward cost with only two punches; the gap is kept small because real speeds are close.
GAMEPLAY ABSTRACTION: slip timings are tuning values instead of measured durations (none exist in the allowed sources; a generous evade window fits [S4]) — a clear, learnable window. Slip-left and slip-right share the same timings.
GAMEPLAY ABSTRACTION: reach is a root-to-root distance (jab shorter than cross) instead of arm length, stance and step length [S1][S3] — one readable range rule.
GAMEPLAY ABSTRACTION: the boxer cannot step while punching or slipping instead of stepping with the punch [S9] — committed moves and simple controls.
GAMEPLAY ABSTRACTION: held guard is a separate committed state with no punching, instead of punching out of the guard with the free hand kept up [S1][S2] — gives guard a clear cost and keeps the four moves distinct. Both hands close together because guard is its own state.
GAMEPLAY ABSTRACTION: the idle pose is the stance guard and "Guard" is a tighter cover with a short raise instead of one continuous guard that is always up [S1][S3] — a visible state the player controls.
GAMEPLAY ABSTRACTION: the slip side is a free key choice and the head moves toward or away from the camera with an added shoulder turn so the side reads from the side-on camera, instead of a lateral head move chosen in answer to the incoming punch [S2] — the camera only sees depth, and the opponent has no attacks yet; the side-to-punch rule is deferred (see Slip).
GAMEPLAY ABSTRACTION: a slip is a committed move with its own startup, evade window and recovery, with no step or lateral root movement, instead of a head movement that is not tied to fixed frame counts [S2] — readable timing and simple controls.
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
| Slip direction | Does one slip evade both jab and cross once opponents attack? | **Superseded by the client decision to have two slips.** The side-to-punch mapping is deferred to the opponent-attack spec and will need a boxing review. | The review's concern (one side cannot answer both punches [S2]) is addressed by giving the player both sides. Marked as an abstraction under Slip. |

## Out of scope
- Stamina, health, damage, KO, knockdowns, round timer display or round end, points decision, scoring.
- Opponent AI or any opponent movement, punches, blocking, slipping, reaction or flinch. The opponent never attacks, so `IsBlocking` / `IsEvading` have no effect yet.
- Hooks, uppercuts, body shots, feints, clinches, lateral or depth movement of the boxer's root (the slips only pose the upper body), pivots, southpaw stance, switching sides.
- Deciding which slip side evades which punch (deferred to the opponent-attack spec).
- Use of boxer stats or build in moves (reach, speed, power).
- Hit-stop, camera shake, particles, sound, shaders, HUD beyond the contact text and controls hint.
- Gamepad, mouse controls in the fight, rebinding, pause menu, settings.
- Input buffering, combos beyond the two cancel rules above.

## Client decisions
Made by the client at G2 (2026-11-28) and built into this spec:
1. **Guard stays on S** (hold).
2. **The Left and Right arrows also move the boxer**, in addition to A / D; both keys of each pair are bound to the same action (`fight_back`, `fight_forward`).
3. **Two slip moves, slip-left and slip-right**, instead of one. The designer chose the keys (Q and E) and the meaning of the two sides (the boxer's own left and right).

## Open questions
1. **Punching while walking on the arrows.** With the arrows the right hand walks and J / K (right hand) are out of reach, so an arrows-only player can guard and slip (left hand: S, Q, E) but cannot punch without letting go. A / D walking has no such problem. **Recommendation: leave as is for M1** (it is exactly what the client asked for) and see at G3 which way the client plays; if arrows are meant for real use, add left-hand punch alternates (one more key each for jab and cross, a one-line input-map change per action) in the next task.
2. **Are boxer-relative slip sides right?** Q = the boxer's left = away from the camera, E = the boxer's right = toward the camera, so on screen Q and E are not "screen-left / screen-right" and the player may need a moment to learn them. **Recommendation: keep boxer-relative.** It matches real boxing language [S2] and the side will matter once punches come in; the screen-relative alternative (Q = back, E = toward the opponent) would turn the slips into a lean-back and a lean-forward, which is not a slip. Revisit after the G3 playtest.
3. **Which slip side beats which punch** is deferred (see Slip, Out of scope). Not needed to build this task. **Recommendation:** decide in the opponent-attack spec with a boxing-consultant review.

## Playtest checklist
Five things the client checks by hand at G3.
1. Pick both boxers: the ring appears with a controls hint at the bottom. Hold D (or the Right arrow): your boxer (left, "YOU") glides toward the opponent and stops a short distance in front of it without passing through. Hold A (or the Left arrow): it backs away and stops before leaving the floor. Both key pairs behave the same, and holding D and Left together does not move it.
2. From the starting distance press J: the lead glove shoots forward and comes back, and no "Contact" text appears. Walk close and press J, then K: each punch shows "Contact: Jab" / "Contact: Cross" at the top for under a second, and the gloves visibly reach the opponent, who does not react.
3. Press K by itself and then J by itself: the cross feels a bit slower and heavier (body turns) than the jab, but not sluggish. Press J then quickly K: the cross follows the jab smoothly as a one-two. Mash J and K fast: the boxer does not throw faster than the moves allow.
4. Hold S: both gloves come up to cover the face and the boxer walks more slowly with A/D; pressing J or K does nothing until you let go of S. Hold J for two seconds: only one jab is thrown.
5. Press Q, then E: each dips the upper body and comes back, Q leaning away from the camera and E toward it, with the shoulders turning opposite ways so you can tell them apart; the feet stay put. Press J or K right as a slip comes back and the punch fires straight out of it; pressing the other slip key mid-slip does nothing. Press Escape at any moment: you are back at the menu, and a new fight starts fresh.
