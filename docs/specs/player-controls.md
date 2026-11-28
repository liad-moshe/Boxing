# Spec: Player controls (playable fight, idle opponent)

Status: DRAFT (issue #12 amendment; the issue #10 version was APPROVED by the client at G2, 2026-11-28)   Issue: #12 (amends #10)

## What changed for issue #12
Jab and cross are each split into a **left-hand** and a **right-hand** version, so the player has four punch keys instead of two. Everything else (walking, guard, slips, timings, no input buffer, contact rule, idle opponent) is unchanged from the approved #10 version, apart from the rules that name the punches. Summary of the deltas:
- Actions `fight_jab` (J) and `fight_cross` (K) are **replaced** by `fight_jab_left` (J), `fight_jab_right` (K), `fight_cross_left` (U), `fight_cross_right` (I). **K changes meaning** (it was the cross, it is now the right jab). See Client decisions needed.
- States `Jab` / `Cross` become `LeftJab`, `RightJab`, `LeftCross`, `RightCross`; the `Contact` result and the contact text name the hand.
- One-two cancel: a jab's Recovery cancels only into the **other hand's** cross (left jab -> right cross, right jab -> left cross). Slip Recovery cancels into any of the four punches.
- Data: `moves.jab|cross` become `moves.leftJab|rightJab|leftCross|rightCross` (same numbers, mirrored); one new visual key `visual.rearJabTurnDeg`.
- Stance stays orthodox for every boxer. A right jab and a left cross are therefore an off-hand jab and a lead-hand cross; whether that is realistic is a **Boxing question** below.

## Goal
When the player confirms both boxers in Character Select, the ring becomes a fight scene: the player's boxer walks forward and back and throws a jab or a cross with either glove, holds a guard and slips to either side, all from the keyboard. The opponent stands idle. This sets the timing rules (startup, active, recovery, cancels) that stamina, damage and AI will build on later, now with a choice of hand for each straight punch.

## Player story
As a player, I want to choose which glove throws my jab or my cross, and to step in and out of range, guard and slip left or right with a handful of keys, so that I can feel the rhythm of the basic boxing moves and build simple two-handed combinations before the opponent fights back.

## Scene flow (how the fight is entered)
- Unchanged flow: Menu -> New Game -> Select step 1 -> Select step 2 -> confirm -> `game/scenes/Main.tscn`. The ring scene **is** the fight scene; no new scene file is required. The player fighter (left, "YOU") gets the controls; the opponent (right, "CPU") stays idle.
- Controls are live on the first physics tick of the ring. There is no countdown, intro or "Fight!" banner (vision pillar 3: launch to first punch in seconds).
- The confirm key (Enter) is not a fight key, so a held Enter from the select screen does nothing in the fight. The arrow keys are not fight keys, so an arrow still held from the select screen does nothing in the fight.
- Escape (`ui_back`) behaves exactly as today: back to the menu, picks cleared. It works in any move state. New Game again starts a fresh fight with the player back at `ring.playerX` in Idle.
- Direct launch of `Main.tscn` (smoke test) still works: both fighters fall back to the first roster entry and the player is controllable.
- The existing invisible round clock keeps running unchanged; nothing happens when it reaches zero (round logic is out of scope).
- A one-line controls hint is shown at the bottom of the screen in the fight (code-built `Label`, default font): `A / D  move    J / K  jab L / R    U / I  cross L / R    S  guard (hold)    Q / E  slip L / R    Esc  menu`. The labels name the key **positions** on a QWERTY layout (see Controls). If the line is wider than the window at the default resolution, the developer may split it into two lines; the content stays the same.

## Controls
Nine fight actions, all bound by **physical keycode** (key position, so AZERTY and other layouts get the same hand shape). They are in the `[input]` section of project.godot (not code-registered), so the kb-check covers them. Each action has exactly one key event.

| Action | Key (physical) | Type | Notes |
|---|---|---|---|
| fight_forward | D | held | Unchanged. Walk toward the opponent (+X for the player on the left). |
| fight_back | A | held | Unchanged. Walk away from the opponent (-X). Forward and back together cancel out (no movement). |
| fight_jab_left | J | press | **New (replaces `fight_jab`, same key).** Jab with the boxer's left glove (`GloveL`, the lead hand in orthodox). Fresh press only; key repeat (echo) and holding never throw a second punch. |
| fight_jab_right | K | press | **New (K was `fight_cross`).** Jab with the boxer's right glove (`GloveR`, the rear hand in orthodox). Fresh press only. |
| fight_cross_left | U | press | **New.** Cross with the boxer's left glove (`GloveL`). Fresh press only. |
| fight_cross_right | I | press | **New.** Cross with the boxer's right glove (`GloveR`, the rear hand in orthodox: the #10 cross). Fresh press only. |
| fight_guard | S | **hold** | Unchanged. Guard is up while held, down on release. |
| fight_slip_left | Q | press | Unchanged. Slip to the boxer's left. Fresh press only. |
| fight_slip_right | E | press | Unchanged. Slip to the boxer's right. Fresh press only. |
| ui_back | Escape | press | Existing, unchanged. |

`fight_jab` and `fight_cross` are **removed** from project.godot (no alias is kept, so no key can fire two actions).

**Key count: 9 fight keys + Escape = 10 keys** (A, D, S, Q, E, J, K, U, I, Esc). Justification:
- **Left and right always mean the boxer's own hands**, the same rule as the slips (client decision 4 of #10). The player faces +X and the camera sits on the +Z side, so the right glove is the one **nearer the camera** and the left glove is the one further from it. They are not screen directions.
- **A 2 x 2 punch grid under the right hand.** Column = glove: the left column (J, U) throws with the left glove, the right column (K, I) with the right glove, so the key's side matches the glove's side. Row = punch: the home row (J, K) is the jab, the quick punch thrown most, where the fingers rest; the row above (U, I) is the cross, the heavier punch, a short reach up. The right index finger covers J and U, the middle finger K and I; the hand never leaves home position.
- **The orthodox one-two is J then I** (left jab, right cross: a short diagonal roll up and right). The mirrored one-two is K then U.
- The left hand stays on Q, E, A, S, D exactly as in #10 (move, guard, slips). Two hands, no stretching, no mouse.
- Four punch keys is the smallest set that gives every hand-and-punch pair its own key. A "hand modifier" key (for example Shift + J for the right hand) was rejected: chords are slower, harder to read in a playtest, and break the fresh-press rule when the modifier is pressed a tick late.

**No clash with docs/kb/input-map.md.** The existing keys are Escape, Up, Down, Enter, keypad Enter (project.godot), Left, Right (code-registered `menu_left` / `menu_right`, Character Select only), and the #10 fight keys D, A, J, K, S, Q, E. J and K are re-assigned to new actions (the old actions are removed); U and I were free. No fight action uses an arrow key, Enter, keypad Enter, Escape or Space. Menu action definitions are not changed. W, L, O, Space, Up and Down stay free for later moves. `game/scripts/InputActions.cs` holds the fight action names and must be updated to the new names (it does not register them).

## Fight model (pure logic, src/Core)
All rules below live in `src/Core` with no Godot reference and are covered by xUnit tests. Suggested types (names are the developer's call): `MoveSet` (parsed from the JSON text, validated), `FighterInput` (one tick of input: `MoveAxis` -1/0/+1, `GuardHeld`, `LeftJabPressed`, `RightJabPressed`, `LeftCrossPressed`, `RightCrossPressed`, `SlipLeftPressed`, `SlipRightPressed`, where the six `Pressed` flags are true only on the tick of a fresh press), and `PlayerFighter` with `Tick(FighterInput)` exposing `State`, `Phase`, `PhaseFrame`, `X`, `IsBlocking`, `IsEvading`, `EvadeSide` (none, Left or Right; non-none only while `IsEvading`) and a per-tick `Contact` result (none, LeftJab, RightJab, LeftCross or RightCross). The adapter builds `MoveAxis` as (`fight_forward` held ? 1 : 0) - (`fight_back` held ? 1 : 0).

If the developer prefers to keep a `Jab` / `Cross` state plus a separate `Hand` property, that is allowed, as long as tests can observe both the punch type and the hand on every tick and the data keys below are used. The rest of this spec uses the four-state names.

### Time
- The simulation runs in whole ticks at `tickRate` from the data file. Core never uses floating-point seconds for move timing. The Godot adapter calls `Tick` exactly once per `_PhysicsProcess`, and the project's physics tick rate must equal `tickRate` (Godot's default; the adapter logs an error with `GD.PushError` if they differ).
- **Timeline rule:** a move that starts on tick N occupies ticks N to N + startup + active + recovery - 1: first its Startup ticks, then its Active ticks, then its Recovery ticks. On the next tick the fighter is free and that tick's input is processed normally.

### States
| State | Phases | Meaning |
|---|---|---|
| Idle | none | Free. Can walk, guard, or start any move. |
| GuardRaising | none | Guard held, hands coming up for `guard.raiseFrames` ticks. `IsBlocking` false. Can walk (slowed). |
| Guarding | none | Guard up. `IsBlocking` true. Can walk (slowed). |
| LeftJab | Startup, Active, Recovery | Jab with the left glove. No walking. |
| RightJab | Startup, Active, Recovery | Jab with the right glove. No walking. |
| LeftCross | Startup, Active, Recovery | Cross with the left glove. No walking. |
| RightCross | Startup, Active, Recovery | Cross with the right glove. No walking. |
| SlipLeft | Startup, Active, Recovery | Unchanged. `IsEvading` true only in Active, with `EvadeSide` Left. No walking. |
| SlipRight | Startup, Active, Recovery | Unchanged. `IsEvading` true only in Active, with `EvadeSide` Right. No walking. |

"A jab" below means LeftJab or RightJab; "a cross" means LeftCross or RightCross; "a punch" means any of the four. **The other hand's cross** of LeftJab is RightCross, and of RightJab is LeftCross.

### What each input does, per state
Order inside one tick: (1) move-timer advance and move end, (2) guard held/released, (3) fresh presses, (4) walking, (5) contact check.

"Slip press" means either slip key (Q or E); both rows of behaviour are identical, only the side differs.

| From state | Left jab (J) | Right jab (K) | Left cross (U) | Right cross (I) | Slip press (L or R) | Guard held | Walking |
|---|---|---|---|---|---|---|---|
| Idle | start LeftJab | start RightJab | start LeftCross | start RightCross | start that slip | -> GuardRaising | yes, full speed |
| GuardRaising / Guarding | ignored | ignored | ignored | ignored | start that slip | stays; release -> Idle on that tick | yes, x `guard.moveSpeedMultiplier` |
| Any jab, Startup / Active | ignored | ignored | ignored | ignored | ignored | ignored until move ends | no |
| LeftJab Recovery | ignored | ignored | ignored | **cancel: start RightCross** (one-two) | ignored | ignored until move ends | no |
| RightJab Recovery | ignored | ignored | **cancel: start LeftCross** (mirrored one-two) | ignored | ignored | ignored until move ends | no |
| Any cross, any phase | ignored | ignored | ignored | ignored | ignored | ignored until move ends | no |
| Slip Startup / Active | ignored | ignored | ignored | ignored | ignored (including the other side) | ignored until move ends | no |
| Slip Recovery | **cancel: start LeftJab** | **cancel: start RightJab** | **cancel: start LeftCross** | **cancel: start RightCross** | ignored (no slip-to-slip chain) | ignored until move ends | no |

- The cancel targets come from `moves.<move>.cancelInto` in the data file; the shipped values are exactly the table above (leftJab -> rightCross; rightJab -> leftCross; slipLeft and slipRight -> any of the four punches; crosses -> nothing). Only the Recovery phase can be cancelled. Startup and Active never are.
- **No same-hand jab -> cross and no jab -> jab.** A left jab does not cancel into a left cross, and no jab cancels into a jab (no double jab, as in #10). In the one-two only the other hand fires while the punching hand comes back (see Boxing notes).
- **No input buffer.** A press that is ignored is dropped; it does not fire later. Mashing during a move does nothing (vision: button-mashing doesn't win).
- **Same-tick priority** when more than one fresh press arrives on one tick: first the presses that the current state does not allow (the "ignored" cells above) are dropped; then, of the allowed presses, only the first in this order starts: SlipLeft, SlipRight, LeftJab, RightJab, LeftCross, RightCross. The others are dropped. This is the #10 rule (allowed first, then fixed order) with the four punches in place of two. Examples: J and I on one tick from Idle -> LeftJab starts (the cross is dropped, not queued); U and I on one tick in LeftJab Recovery -> RightCross starts (LeftCross is not allowed there); K and J on one tick from Idle -> LeftJab starts.
- **Pressing both hands "at once" never throws both.** There is no two-glove punch; a near-simultaneous press is either one tick (priority above) or two ticks (the second press is handled by the state the first one started, usually ignored).
- **Guard hold is level-based.** If guard is still held on the tick a move ends (or is held when nothing else starts from Idle), the fighter enters GuardRaising. Releasing guard from GuardRaising or Guarding returns to Idle on that tick. While guard is held, all four punches are ignored: to punch, the player must release guard. GAMEPLAY ABSTRACTION (see Boxing notes).
- A guard press and any punch press on the same tick from Idle: guard wins, the punch is ignored.
- A walk key held when a move starts stops walking for the move's duration; walking resumes automatically on the free tick if the key is still held.

### Movement and bounds
Unchanged from #10.
- Only the player moves, only along X. Each walking tick, `X` changes by `MoveAxis` x `movement.walkSpeedMps` / `tickRate` (x `guard.moveSpeedMultiplier` while GuardRaising or Guarding).
- Start position: `ring.playerX` from `data/balance/fighters.json`. Opponent position: `ring.opponentX`, fixed.
- Bounds, applied every tick after walking: `movement.minX` <= X <= opponentX - `movement.minSeparationM`. The player stops at the bound (no bounce, no push of the opponent). Loading fails with a clear exception if `minX` > `ring.playerX` or if `ring.playerX` > `ring.opponentX` - `minSeparationM`.
- Depth (Z) and height (Y) of the fighter root never change.

### Slip (two sides)
Unchanged from #10, except that a slip's Recovery now cancels into any of the four punches.
- **What left and right mean.** The sides are the **boxer's own**: slip-left moves the head to the boxer's left (the lead-hand side for an orthodox boxer), slip-right to the boxer's right (the rear-hand side). They are **not** screen directions. The boxer's right is the side **toward the camera** and the boxer's left is **away from the camera**. Slip direction in real boxing depends on the incoming punch [S2], so the side must be a player choice that names a body side. The slips do **not** move the boxer along X.
- **Player-facing look (both sides share the same timing and the same dip).** The upper body dips by `visual.slipHeadDropH`, leans sideways toward the chosen side by `visual.slipLeanDeg`, and the shoulders turn about the vertical axis by `visual.slipTurnDeg`, in opposite directions for left and right. Feet stay planted. The pose returns to stance during Recovery. The look is a readable visual choice, not a claim about how a real slip is built.
- The two slips have the same data shape and identical shipped timings (`moves.slipLeft`, `moves.slipRight`).
- `IsEvading` is true only during a slip's Active phase, and `EvadeSide` tells which side. Startup and Recovery are vulnerable. The Recovery is the slip's cost; cancelling it into a punch is the reward (counter).
- Slip has no stamina cost in this task. Nothing punches the player yet, so `IsEvading`, `EvadeSide` and `IsBlocking` have no effect until opponent attacks exist.
- **Deferred behaviour (built later with opponent attacks):** which slip side clears which incoming straight punch is **not decided here** (opponent-attack spec, with a boxing-consultant review [S2][S3]). Slips will not evade hooks when hooks are added [S3]. GAMEPLAY ABSTRACTION: the player picks the slip side with a key, and in this task the choice has no effect on any punch because there are none, instead of the side being the answer to a punch that is already coming [S2] — the opponent is idle.

### Contact with the idle opponent (minimal)
- On each Active tick of a punch, Core compares the gap (opponentX - player X) with that move's `reachM`. On the first Active tick where gap <= reach, the punch reports one `Contact` naming the move (LeftJab, RightJab, LeftCross or RightCross). A punch reports at most one contact. Out of range = no contact, no event.
- With the shipped data both jabs share `jab` reach and both crosses share `cross` reach (the values are mirrored), so the hand never changes whether a punch lands in this task.
- Contact is a rule on root positions, not mesh collision.
- The adapter shows `Contact: Left Jab`, `Contact: Right Jab`, `Contact: Left Cross` or `Contact: Right Cross` as a screen `Label` near the top centre for `feedback.contactTextFrames` ticks (a newer contact replaces the text and restarts the timer) and writes one `GD.Print` line per contact.
- **Deferred:** damage, health, scoring, opponent flinch or reaction, hit-stop, sound, blocking or slipping of the player's punches by the opponent, any difference in effect between the hands, and any use of the boxer stats or build.

### Idle opponent
Unchanged. No AI. The opponent never moves, never punches, never changes state and never reacts to contact. It keeps the existing static guard pose (an optional gentle idle bob is allowed). The opponent is a fixed X value for Core; it has no `PlayerFighter` instance.

## Visuals (Godot adapter, game/)
Thin adapter in `game/scripts` reads input, calls Core, and poses the existing fighter parts from `State`, `Phase` and `PhaseFrame` each frame. Code-driven motion only (no AnimationPlayer resources, no external assets, no shaders).
- Stance: all fighters stand orthodox. The lead hand is `GloveL` (the glove on the lead-foot side, nearer the opponent); the rear hand is `GloveR`, nearer the camera. The stance does **not** change when the player throws with the right glove first or with the left glove's cross.
- **Mirroring rule.** Each punch moves only its own glove and forearm (left: `ForearmL`/`GloveL`; right: the right forearm and `GloveR`); the other glove stays up in guard. The left and right versions of one punch use the same timing curve (startup out, hold through Active, return during Recovery) and the same `visual.punchExtendH`. Any torso turn of a left-glove punch is the same size as the right-glove version and in the opposite direction (the left shoulder comes forward instead of the right).
- LeftJab: the left glove moves straight forward toward the opponent during Startup to `visual.punchExtendH`, holds through Active, returns to guard during Recovery. No torso turn. (This is the #10 jab, unchanged.)
- RightJab: the right glove extends the same way; the shoulders turn by `visual.rearJabTurnDeg` toward the punch (right shoulder forward), so the rear glove, which starts further back, still visibly reaches at jab range. The turn is clearly smaller than a cross's so the right jab and the right cross look different.
- RightCross: the right glove extends, and the torso, shoulders and head turn by up to `visual.crossTorsoTurnDeg` toward the punch (the #10 cross, unchanged).
- LeftCross: the left glove extends, and the torso, shoulders and head turn by `visual.crossTorsoTurnDeg` in the opposite direction (left shoulder forward). The feet stay planted and the stance stays orthodox.
- Guard: unchanged. Both gloves rise by `visual.guardGloveRaiseH` and move inward by `visual.guardGloveInwardW` over `guard.raiseFrames`; they drop back at once on release.
- Slip: unchanged (see Slip).
- Every move returns the parts exactly to the build's guard pose when the fighter is Idle (within 1 mm).
- Visual tuning: `visual.punchExtendH` and `visual.rearJabTurnDeg` are tuned so that **each** of the four punches thrown at gap == its reach visibly reaches the opponent's head or gloves, and at gap > its reach visibly falls short. The developer may tune `visual.*` by about 30 percent for readability and records the final values in the data file.

## Acceptance criteria
AC1 to AC16 are xUnit tests in `tests/Core.Tests` (tick-exact, using the shipped `player-moves.json` and `fighters.json` text). AC17 to AC24 are checked by the headless smoke scene where practical (the smoke may drive input with `Input.ActionPress` / `ActionRelease`) and by the playtest. "Ticks" means `Tick` calls. "A slip" in a test means each of SlipLeft and SlipRight run separately; "a jab" means each of LeftJab and RightJab run separately; "a cross" means each of LeftCross and RightCross run separately, each with its own data entry's frame counts.
- AC1. **Given** the shipped `data/balance/player-moves.json`, **when** parsed, **then** it has `tickRate`, moves `leftJab`, `rightJab`, `leftCross`, `rightCross`, `slipLeft`, `slipRight` with integer frame counts >= 1, positive `reachM` for the four punches, `guard`, `movement` and `feedback` blocks; **and given** a copy with a 0 or missing frame count, a non-positive speed or reach, a missing punch entry, an unknown `cancelInto` name (including the old names `jab` or `cross`), or a bound that excludes `ring.playerX`, **when** parsed, **then** it throws and the message names the bad key.
- AC2. **Given** an Idle player, **when** LeftJab (or RightJab) is pressed on tick 1 and no other input follows, **then** the state is that jab's Startup for exactly its `startupFrames` ticks, Active for exactly `activeFrames`, Recovery for exactly `recoveryFrames`, and Idle on the next tick; the state names the correct hand on every tick.
- AC3. **Given** an Idle player, **when** LeftCross (or RightCross) is pressed, **then** the same timeline holds with that cross's frame counts; **and** with the shipped data each cross's total ticks are greater than each jab's total and at most 1.5 times it; **and** `leftJab` and `rightJab` have identical frame counts and reach, as do `leftCross` and `rightCross` (mirrored).
- AC4. **Given** an Idle player, **when** SlipLeft (or SlipRight) is pressed, **then** the timeline follows the `slipLeft` (or `slipRight`) frame counts, `IsEvading` is true on every Active tick and false on every other tick, and `EvadeSide` is Left (or Right) on every Active tick and none on every other tick.
- AC5. **Given** a jab in Startup or Active, **when** any of the four punches or either slip is pressed, **then** nothing changes, and after the jab ends the fighter is Idle (the press was not buffered).
- AC6. **Given** a LeftJab in Recovery, **when** RightCross is pressed, **then** on that same tick the state is RightCross Startup, frame 1, using the normal `rightCross` frame counts; **given** a RightJab in Recovery, **when** LeftCross is pressed, **then** on that tick the state is LeftCross Startup, frame 1; **and given** a jab pressed on tick 1 and the other hand's cross pressed on the first Recovery tick (tick 1 + `startupFrames` + `activeFrames`), **then** the fighter is Idle again exactly that cross's total ticks later; **and when**, in a jab's Recovery, the same hand's cross, either jab or either slip is pressed instead, **then** it is ignored and the jab runs to its end.
- AC7. **Given** a cross (either hand) in any phase, **when** any of the four punches or either slip is pressed, **then** it is ignored and the cross runs to its end.
- AC8. **Given** a SlipLeft or SlipRight in Recovery, **when** any one of the four punches is pressed, **then** on that same tick the state is that punch's Startup, frame 1 (tested once per punch per slip side); **and when** either slip key is pressed in that Recovery, **then** it is ignored; **and given** a slip in Startup or Active, **when** any punch or either slip key is pressed, **then** it is ignored and the slip keeps its side.
- AC9. **Given** an Idle player, **when** guard is held, **then** the state is GuardRaising with `IsBlocking` false for exactly `guard.raiseFrames` ticks, then Guarding with `IsBlocking` true for as long as it is held; **and when** guard is released, **then** on that tick the state is Idle and `IsBlocking` is false.
- AC10. **Given** Guarding, **when** any of the four punches is pressed, **then** it is ignored and the state stays Guarding; **and when** SlipLeft (or SlipRight) is pressed, **then** that slip starts that tick, and if guard is still held when the slip ends the state becomes GuardRaising, then Guarding.
- AC11. **Given** Idle at `ring.playerX`, **when** forward is held for N ticks (no bound reached), **then** X increases by exactly N x `walkSpeedMps` / `tickRate` (within 1e-6); back decreases it the same way; both or neither held leaves X unchanged; while Guarding the change is multiplied by `guard.moveSpeedMultiplier`.
- AC12. **Given** any of the four punches or a slip in any phase, **when** forward or back is held, **then** X does not change; **and when** the move ends with the key still held, **then** walking resumes on the free tick.
- AC13. **Given** back held for 10 seconds of ticks, **then** X equals `movement.minX`; **given** forward held for 10 seconds of ticks, **then** X equals `ring.opponentX` - `movement.minSeparationM`; and X is never outside these bounds on any tick.
- AC14. **Given** a gap <= the jab reach, **when** a LeftJab (or RightJab) runs to its end, **then** exactly one Contact naming that jab and hand is reported, on its first Active tick; **given** a gap between the jab reach and the cross reach, **then** neither jab reports a contact and a LeftCross (or RightCross) reports exactly one Contact naming that cross and hand; **given** a gap > the cross reach, **then** none of the four reports a contact.
- AC15. **Given** Idle, **when** SlipLeft, SlipRight and all four punches are pressed on one tick, **then** SlipLeft starts; **when** SlipRight and all four punches, **then** SlipRight starts; **when** all four punches, **then** LeftJab starts; **when** RightJab, LeftCross and RightCross, **then** RightJab starts; **when** LeftCross and RightCross, **then** LeftCross starts; **given** a LeftJab in Recovery, **when** LeftCross and RightCross are pressed on one tick, **then** RightCross starts (the not-allowed press is dropped before the order applies); **when** guard is held and any punch is pressed on the same tick from Idle, **then** the state is GuardRaising and no punch starts.
- AC16. **Given** a full fight sequence of any inputs, **when** ticked, **then** the opponent X used by Core never changes.
- AC17. **Given** Character Select step 2, **when** the player confirms the opponent, **then** the ring loads with the player on the left and the controls hint (naming J / K jab, U / I cross) visible at the bottom, and the first D press moves the player toward the opponent with no delay or extra key.
- AC18. **Given** the project is built, **when** project.godot is inspected, **then** `fight_forward` (D), `fight_back` (A), `fight_jab_left` (J), `fight_jab_right` (K), `fight_cross_left` (U), `fight_cross_right` (I), `fight_guard` (S), `fight_slip_left` (Q) and `fight_slip_right` (E) exist with physical keycodes, one key each; `fight_jab` and `fight_cross` do not exist; no two actions share a key; none of them uses Escape, Enter, keypad Enter, any arrow key or Space; and the menu actions (including code-registered `menu_left` / `menu_right`) are unchanged.
- AC19. **Given** the fight, **when** J is held down for 2 seconds, **then** exactly one punch is thrown (key repeat ignored); the same holds for K, U, I, Q and E. **And when** A and D are held together, **then** the boxer does not move; **and when** an arrow key is pressed in the fight, **then** the boxer does not move.
- AC20. **Given** the fight (smoke), **when** each punch reaches Active, **then** its own glove is further toward the opponent than in Idle by at least half of `visual.punchExtendH` x fighter height and the other glove has not moved forward (LeftJab and LeftCross move `GloveL`; RightJab and RightCross move `GloveR`); during RightCross Active and LeftCross Active the torso is turned by the same amount in opposite directions; during LeftJab Active the torso is not turned; during RightJab Active the torso turn is non-zero and smaller than during RightCross Active; while Guarding both gloves are higher than in Idle; during SlipLeft and SlipRight Active the `Head` is lower than in Idle, the SlipRight head is nearer the camera (greater Z) and the SlipLeft head further from it, the shoulders are turned in opposite directions, and the fighter root X and Z are unchanged; and back in Idle every posed part is within 1 mm of its built guard pose.
- AC21. **Given** the player in range, **when** a punch makes contact, **then** `Contact: Left Jab`, `Contact: Right Jab`, `Contact: Left Cross` or `Contact: Right Cross` (matching the punch thrown) appears near the top of the screen and disappears after `feedback.contactTextFrames` ticks, and the opponent does not move, change pose or react.
- AC22. **Given** the start position, **when** J, K, U or I is pressed without moving, **then** no contact text appears (the fighters start out of range); **and when** the player walks forward until stopped, **then** the player cannot pass the opponent and stops at the minimum separation.
- AC23. **Given** the fight in any move state, **when** Escape is pressed, **then** the menu shows (as in main-menu.md); **and when** New Game and both picks are made again, **then** the player starts at `ring.playerX`, Idle, with no leftover contact text.
- AC24. **Given** `Main.tscn` is launched directly (smoke, no select), **when** it runs, **then** the player is controllable with the default roster entry, a smoke-driven LeftJab and a smoke-driven RightCross each complete and return to Idle, and no error is logged.

## Balance values
All numbers live in `data/balance/player-moves.json` (plus the existing positions in `data/balance/fighters.json`), not in this spec or in code.
Pointer: `data/balance/player-moves.json` -> `tickRate`; `moves.leftJab|rightJab|leftCross|rightCross|slipLeft|slipRight.startupFrames|activeFrames|recoveryFrames|cancelInto`; `moves.leftJab|rightJab|leftCross|rightCross.reachM`; `guard.raiseFrames`, `guard.moveSpeedMultiplier`; `movement.walkSpeedMps`, `movement.minX`, `movement.minSeparationM`; `feedback.contactTextFrames`; `visual.punchExtendH`, `visual.crossTorsoTurnDeg`, `visual.rearJabTurnDeg` (new), `visual.guard*`, `visual.slip*` (developer may tune about 30 percent for readability). `data/balance/fighters.json` -> `ring.playerX`, `ring.opponentX`.
Changes in #12: `moves.jab` and `moves.cross` were renamed and duplicated into the four hand entries with the **same numbers** (left and right of one punch type are mirrored); `cancelInto` lists use the new names; `visual.rearJabTurnDeg` was added. If the boxing review asks for different numbers per hand (for example a slower or shorter off-hand jab), only the data file changes; the code and these rules stay the same.
Parsing and validation happen in Core (text in, exception on bad data); the adapter only reads the file. The file must be included in the exported build, as with fighters.json.

## Boxing notes
The #10 notes were reviewed by boxing-consultant on 2026-11-28 (work/10/boxing-review.md). The #12 additions below are **not yet reviewed**; see Boxing questions. Sources are in docs/boxing/sources.md.

### Real boxing this spec relies on
- The jab is the lead-hand straight punch and the cross is the rear-hand straight punch; in an orthodox stance the left hand and left foot lead, so `GloveL` is the lead [S1][S3]. This is why LeftJab and RightCross are the "natural" orthodox pair and the #10 moves map onto them unchanged.
- The cross has farther to travel than the jab and gets its power and extra reach from the rear-leg push and the hip and shoulder turn [S1][S3][S8]. The metre values for reach are tuning values, not measured.
- The jab then cross (the "one-two") is a basic combination taught as lead straight then rear straight, which justifies the LeftJab -> RightCross cancel [S1][S2][S3]. Punches thrown in a combination are delivered faster than a single maximal punch [S4].
- In real boxing only the punching hand leaves the guard while the other hand stays up and the punching hand returns along the same line [S1][S2]. This is why a jab cancels into the **other** hand's cross and not the same hand's.
- A slip moves the head off the line of an incoming straight punch and is followed at once by a counter with both hands free, which justifies Slip Recovery -> punch cancel [S2][S3]. The side of the slip depends on the incoming punch [S2].
- A high guard and a tighter cover with both forearms in front protect the head against straight punches [S1][S2][S3].
- Real timing scale: throw to first contact about 0.11 to 0.14 s for jab and cross [S6]; a whole punch about 0.4 to 0.5 s [S5][S6]; contact about 0.02 to 0.03 s [S7]. No allowed source gives real slip durations.

### GAMEPLAY ABSTRACTION lines
New in #12 (provisional until the boxing review):
GAMEPLAY ABSTRACTION: from an orthodox stance the player can throw a jab and a cross with either glove (a right jab and a left cross included), instead of the jab being the lead-hand punch and the cross the rear-hand punch [S1][S3] — client request for a choice of hand on both straight punches; whether a rear-hand jab or a lead-hand cross has a real-boxing counterpart is Boxing question B2/B3.
GAMEPLAY ABSTRACTION: the right jab and the left cross use the same timings and reach as their mirrored partner, instead of the lead and rear hand differing in travel distance and power [S1][S3][S7] — mirrored moves are easier to learn and the client asked for the same timings; per-hand tuning is data-only if the review asks for it.
GAMEPLAY ABSTRACTION: the stance never changes when the player leads with the right glove, instead of a boxer switching to southpaw (right foot and hand forward) to lead with the right [S3] — stance switching is out of scope; see Boxing question B4.
GAMEPLAY ABSTRACTION: the mirrored one-two (RightJab -> LeftCross) is allowed as a cancel from an orthodox stance, instead of only the lead-then-rear one-two [S1][S2][S3] — keeps the two hands symmetrical; pending Boxing question B5.

Carried from #10 (reviewed):
GAMEPLAY ABSTRACTION: jab and cross use fixed tick timings chosen for rhythm instead of measured durations (throw about 0.1 s, whole punch about 0.4 to 0.5 s [S5][S6]) — a readable startup/active/recovery beat; the jab total is deliberately quicker than real.
GAMEPLAY ABSTRACTION: the cross is a bit slower and heavier than the jab (total about 1.4 times) instead of nearly equal in speed [S4][S5][S6] — gives the cross a risk-versus-reward cost; the gap is kept small because real speeds are close.
GAMEPLAY ABSTRACTION: slip timings are tuning values instead of measured durations (none exist in the allowed sources; a generous evade window fits [S4]) — a clear, learnable window. Slip-left and slip-right share the same timings.
GAMEPLAY ABSTRACTION: reach is a root-to-root distance (jab shorter than cross) instead of arm length, stance and step length [S1][S3] — one readable range rule.
GAMEPLAY ABSTRACTION: the boxer cannot step while punching or slipping instead of stepping with the punch [S9] — committed moves and simple controls.
GAMEPLAY ABSTRACTION: held guard is a separate committed state with no punching, instead of punching out of the guard with the free hand kept up [S1][S2] — gives guard a clear cost and keeps the moves distinct.
GAMEPLAY ABSTRACTION: the idle pose is the stance guard and "Guard" is a tighter cover with a short raise instead of one continuous guard that is always up [S1][S3] — a visible state the player controls.
GAMEPLAY ABSTRACTION: the slip side is a free key choice and the head moves toward or away from the camera with an added shoulder turn, instead of a lateral head move chosen in answer to the incoming punch [S2] — the camera only sees depth, and the opponent has no attacks yet.
GAMEPLAY ABSTRACTION: a slip is a committed move with its own startup, evade window and recovery, with no step or lateral root movement, instead of a head movement not tied to fixed frame counts [S2] — readable timing and simple controls.
GAMEPLAY ABSTRACTION: the only chains are jab -> other-hand cross and slip -> punch instead of the wider set such as the double jab, jab to the body and hooks [S1][S3] — M1 has few moves; combos come later.
GAMEPLAY ABSTRACTION: walking is a smooth glide along one axis instead of step-and-drag footwork and pivots [S1][S8] — roadmap M1 limits footwork.
GAMEPLAY ABSTRACTION: all boxers are orthodox with identical reach and timings instead of southpaws and varied builds [S3] — stats are display-only until combat tuning.
GAMEPLAY ABSTRACTION: a punch "contact" is a distance check on root positions with no target, damage or reaction instead of a glove landing on a head or body [S7] — the opponent is idle in this task.
GAMEPLAY ABSTRACTION: no input buffering and no cancel out of Startup or Active instead of a boxer adapting a punch mid-flight — readable, mash-proof rhythm (vision pillar).

### Boxing review outcome (#10)
Kept for the record; decisions taken on work/10/boxing-review.md.

| # | Suggestion | Decision | Reason |
|---|---|---|---|
| C1 | Narrow the jab/cross gap | **Accepted.** Cross 26 ticks against the jab's 18 (ratio 1.44). | Real startups are nearly equal [S6]; the cross still feels a bit heavier. AC3 caps the ratio at 1.5. |
| C2 | Shorter cross startup when it follows a jab | **Rejected for M1.** | The cancel already skips the rest of the jab's recovery. |
| C3 | Remove rotation as the reason the cross is slower, and the slip "bend" claim | **Accepted.** | Rotation is the source of power and reach [S1][S3], not a proven cause of lower speed. |
| C4 | Only the punching hand leaves the guard | **Accepted.** | Stated with [S1][S2]; closed-both-hands guard is an abstraction line. |
| Slip direction | One slip for both punches? | **Superseded** by two slips; side-to-punch mapping deferred. | [S2] |

## Boxing questions
For boxing-consultant (work/12/boxing-review.md). Each answer is either a citation from docs/boxing/sources.md (or a new row the consultant adds), or a confirmation that the item stays a GAMEPLAY ABSTRACTION. Nothing below blocks the key layout; it can only change data values, the cancel list, or the abstraction wording.
- **B1. Which hand throws which punch.** Confirm that in real boxing "jab" and "cross" are defined by **lead vs rear hand**, not by left vs right, so that an orthodox boxer's jab is the left hand and a southpaw's jab is the right hand [S1][S3 cover orthodox; is there a source for southpaw?].
- **B2. Off-hand (rear-hand) jab.** Is a quick, non-rotating straight with the **rear** hand a recognised punch in coaching material (for example a "rear jab" or a short rear straight), or is it purely a GAMEPLAY ABSTRACTION? If it exists, is it slower, shorter in reach or less powerful than the lead jab, and should `rightJab` timing or `reachM` differ from `leftJab`?
- **B3. Lead-hand cross.** Is a rotating power straight with the **lead** hand (a "lead straight" or "power jab") a recognised punch, or a GAMEPLAY ABSTRACTION? If it exists, how does it differ from the rear cross in reach, power and travel, and should `leftCross` timing or `reachM` differ from `rightCross`?
- **B4. Orthodox vs southpaw.** Given B1, the realistic way to lead with the right hand is a southpaw stance or a stance switch. Confirm the spec's choice to keep every boxer orthodox and mark the right jab / left cross as abstractions is acceptable, or recommend a later "switch stance" move instead (it would be a separate task; this spec does not add it).
- **B5. Mirrored one-two.** From an orthodox stance, is a rear-hand jab followed by a lead-hand cross (RightJab -> LeftCross) taught or used at all? If not, should the cancel be removed from `rightJab.cancelInto` (data-only change) or stay as an abstraction for symmetry?
- **B6. Same-hand follow-ups.** The spec forbids a jab cancelling into the same hand's cross and any jab -> jab. Confirm this matches "only the punching hand leaves the guard and returns along the same line" [S1][S2], or say whether a same-hand double (for example the double jab) should be allowed in a later task.
- **B7. Counter after a slip.** The spec lets any of the four punches cancel a slip's Recovery. Is any hand / punch pairing after a slip to a given side unrealistic enough that it should be marked or restricted later (when the opponent attacks)? Not built now; for the opponent-attack spec.
- **B8. Visual torso turn.** A left cross turns the left shoulder forward (mirror of the right cross), and a right jab gets a small turn so the rear glove reaches. Confirm these read as plausible, or mark them as visual choices only.

## Out of scope
- Stamina, health, damage, KO, knockdowns, round timer display or round end, points decision, scoring.
- Opponent AI or any opponent movement, punches, blocking, slipping, reaction or flinch.
- Any difference in effect between the left and right hand (damage, power, accuracy). In this task the hand changes only which glove moves and the contact text.
- Southpaw stance, switching stance, choosing a stance in Character Select.
- Hooks, uppercuts, body shots, feints, double jab or other same-hand repeats, two-glove punches, clinches, lateral or depth movement of the boxer's root, pivots.
- Deciding which slip side evades which punch (deferred to the opponent-attack spec).
- Use of boxer stats or build in moves.
- Hit-stop, camera shake, particles, sound, shaders, HUD beyond the contact text and controls hint.
- Gamepad, mouse controls in the fight, rebinding, pause menu, settings, keeping the old `fight_jab` / `fight_cross` names as aliases.
- Input buffering, combos beyond the cancel rules above.
- Any code or tests (this is a spec; the developer and QA do those).

## Client decisions
Made by the client at G2 of #10 (2026-11-28) and still in force:
1. **Guard stays on S** (hold).
2. **Two slip moves, slip-left and slip-right.** Keys Q and E; sides are the boxer's own left and right.
3. **Walking is A / D only.** The arrow keys are not used for any fight action.
4. **Boxer-relative sides are kept.** Q = the boxer's left = away from the camera, E = the boxer's right = toward the camera. This spec applies the same rule to the gloves.

### Client decisions needed (G2 of #12)
1. **Punch keys.** Proposed: J = left jab, K = right jab, U = left cross, I = right cross (columns = glove, rows = jab on the home row and cross above). This keeps J as the left jab but **moves the right cross from K to I**, so the old J-then-K one-two becomes J-then-I. **Alternative A:** crosses on the home row and jabs above (U = left jab, I = right jab, J = left cross, K = right cross): keeps K as the right cross but moves the left jab from J to U. **Alternative B:** keep J = left jab and K = right cross exactly, and put the two new punches on L = right jab and H = left cross: no muscle-memory change, but the keys no longer match the glove side or the punch type. **Recommendation:** the proposed grid, because it is the only layout where both "which side" and "which punch" can be read off the key position.

## Open questions
1. **Which slip side beats which punch** is still deferred (opponent-attack spec, with a boxing review).
2. **Per-hand tuning** (B2, B3): if the boxing review says the off-hand jab or the lead-hand cross should be slower or shorter, the designer will update only `data/balance/player-moves.json` and AC3's "mirrored" clause before development.

## Playtest checklist
Five things the client checks by hand at G3.
1. Pick both boxers: the ring appears with a controls hint at the bottom naming J / K for jabs and U / I for crosses. Hold D: your boxer (left, "YOU") glides toward the opponent and stops a short distance in front of it. Hold A: it backs away. Holding A and D together does not move it, and the arrow keys do nothing.
2. Walk close and press J, then K, then U, then I, waiting for each to finish: J and U move the glove further from the camera, K and I the glove nearer the camera, and the top text says "Contact: Left Jab", "Contact: Right Jab", "Contact: Left Cross", "Contact: Right Cross" in that order. From the starting distance none of the four shows any "Contact" text.
3. Compare K (right jab) with I (right cross), and J (left jab) with U (left cross): each cross turns the body clearly more and feels a bit slower and heavier than the jab of the same glove; the left cross turns the body the opposite way to the right cross. Only the punching glove moves; the other glove stays up.
4. Press J then quickly I: the right cross follows the left jab smoothly as a one-two; K then quickly U does the same with the hands swapped. Press J then quickly U (same glove), or J then K: the second punch does not fire. Mash all four punch keys: the boxer never throws faster than the moves allow, and holding any punch key throws only one punch.
5. Hold S: both gloves come up, the boxer walks more slowly, and none of J, K, U, I does anything until you let go. Press Q or E: the upper body dips and leans to that side; press any of J, K, U or I right as the slip comes back and that punch fires out of it. Press Escape at any moment: you are back at the menu, and a new fight starts fresh.
