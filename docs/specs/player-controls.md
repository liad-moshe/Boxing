# Spec: Player controls (playable fight, idle opponent)

Status: READY FOR G2 (issue #12 amendment: stance; the issue #10 version was APPROVED by the client at G2, 2026-11-28)   Issue: #12 (amends #10)

## What changed for issue #12
The client rejected the earlier 4-key draft. This version keeps the two punch keys of #10 and adds **stance**.
- **Keys unchanged:** J = jab, K = cross, exactly as on main. No new fight key, no renamed action.
- **Jab = lead hand, cross = rear hand**, as in real boxing [S10]. Which glove that is depends on the boxer's stance: **orthodox** (left hand and left foot lead) or **southpaw** (the mirror: right hand and right foot lead) [S10].
- **Stance is chosen per boxer in Character Select**, in each of the two steps, with Up / Down (the existing `menu_up` / `menu_down` actions, so no new key) or a mouse click on the stance line. Default: `stance.default` (orthodox). See "Stance choice (Character Select)" and the amendment in docs/specs/character-select.md.
- **Southpaw is a mirror:** mirrored build, mirrored punch animation and mirrored cross torso turn. **Same tick timings, reach, cancels and priority** in both stances.
- Walking, guard, slips (boxer-relative sides), the one-two cancel, slip-recovery cancels, no input buffer and the contact rule are **unchanged from #10**.
- Data: `moves.jab` / `moves.cross` as on main; one new block `stance.default`. No `visual.rearJabTurnDeg`.
- **No in-fight stance switch** (later task).

## Feedback round 1 (issue #12 playtest): guard pose
Client: "Covering barely moves the hands; not obvious the boxer is covering his face." Only the guard **pose values** change (see Visuals > Guard, AC31 to AC34, `visual.guard*` in the data file). No change to guard logic, timings, walk slow-down or punch lockout.

## Goal
When the player confirms both boxers (and their stances) in Character Select, the ring becomes a fight scene: the player's boxer walks forward and back, throws a jab with the lead hand and a cross with the rear hand, holds a guard and slips to either side, all from the keyboard. The opponent stands idle in its chosen stance. This sets the timing rules (startup, active, recovery, cancels) that stamina, damage and AI will build on later.

## Player story
As a player, I want to pick whether each boxer fights orthodox or southpaw, and then step in and out of range, jab, cross, guard and slip with a handful of keys, so that I can feel the rhythm of the basic boxing moves from either stance before the opponent fights back.

## Scene flow (how the fight is entered)
- Flow: Menu -> New Game -> Select step 1 (boxer + stance) -> Select step 2 (opponent + stance) -> confirm -> `game/scenes/Main.tscn`. The ring scene **is** the fight scene; no new scene file. The player fighter (left, "YOU") gets the controls; the opponent (right, "CPU") stays idle.
- Controls are live on the first physics tick of the ring. No countdown, intro or "Fight!" banner (vision pillar 3).
- Enter, the arrow keys (including Up / Down used for stance) are not fight keys, so a key still held from the select screen does nothing in the fight.
- Escape (`ui_back`) behaves exactly as today: back to the menu, picks **and stances** cleared. It works in any move state. New Game again starts a fresh fight with the player at `ring.playerX` in Idle.
- Direct launch of `Main.tscn` (smoke test): both fighters fall back to the first roster entry **and to `stance.default`**; the player is controllable.
- The invisible round clock keeps running unchanged.
- Controls hint at the bottom of the fight screen: unchanged from #10 (A / D move, J jab, K cross, S guard (hold), Q / E slip L / R, Esc menu). It names punches, not gloves, so it is the same in both stances.
- Each fighter's floating `Tag` shows its stance: `YOU (Orthodox)` / `YOU (Southpaw)` and `CPU (Orthodox)` / `CPU (Southpaw)`.

## Controls
### Fight (ring) — unchanged from #10
All bound by **physical keycode** in the `[input]` section of project.godot, one key event each.

| Action | Key (physical) | Type | Notes |
|---|---|---|---|
| fight_forward | D | held | Walk toward the opponent (+X for the player on the left). |
| fight_back | A | held | Walk away from the opponent (-X). Forward and back together cancel out. |
| fight_jab | J | press | Jab with the **lead** glove: `GloveL` in orthodox, `GloveR` in southpaw. Fresh press only; key repeat and holding never throw a second punch. |
| fight_cross | K | press | Cross with the **rear** glove: `GloveR` in orthodox, `GloveL` in southpaw. Fresh press only. |
| fight_guard | S | **hold** | Guard up while held, down on release. |
| fight_slip_left | Q | press | Slip to the **boxer's own** left, in either stance. Fresh press only. |
| fight_slip_right | E | press | Slip to the **boxer's own** right, in either stance. Fresh press only. |
| ui_back | Escape | press | Existing, unchanged. |

### Character Select — stance (new behaviour on existing actions)
| Action | Key | Notes |
|---|---|---|
| menu_up | Up arrow (physical) | Existing action (project.godot). **New use on this screen:** toggle the stance of the boxer being picked in the current step. Fresh press only. |
| menu_down | Down arrow (physical) | Existing action. Same as Up (two options, so either key flips orthodox <-> southpaw). Fresh press only. |
| (mouse) | Left click on the stance line | Toggles the stance. Control mouse signal, no InputMap action. |

**No clash with docs/kb/input-map.md.** No action is added, removed or re-bound; project.godot `[input]` is unchanged. `menu_up` / `menu_down` were listed as "do nothing on the Character Select screen"; they now toggle stance there (kb-keeper updates that note and the handler column: `CharacterSelect._UnhandledInput`). They still do the same as before on the main menu. In the fight, no action uses an arrow key, Enter, Escape or Space. W, L, U, I, O, Space stay free for later moves.

## Stance (what it means in this game)
- Two values: **Orthodox** and **Southpaw**. A stance is fixed for the whole fight; it is set before the fight in Character Select.
- **Lead and rear.** The lead hand and lead foot are on the side nearer the opponent. Orthodox: lead = left (`GloveL`, `FootL`), rear = right. Southpaw: lead = right (`GloveR`, `FootR`), rear = left [S10].
- **Jab is the lead hand's punch and cross is the rear hand's punch in both stances** [S10]. J always throws the jab and K always throws the cross; only the glove that moves changes.
- **Part names stay anatomical.** `GloveL` is always the boxer's left glove, in either stance. Code and tests that need "the lead glove" ask a pure Core helper (below), not a hard-coded name.
- **Boxer-relative sides do not change with stance.** Q is the boxer's own left and E the boxer's own right in both stances (client decision 4 of #10). In orthodox, slip-left is toward the lead-hand side; in southpaw it is toward the rear-hand side. This is just geometry; which side clears which punch is still deferred.
- **Stance changes no number.** Timings, reach, cancels, same-tick priority, guard, walking speed and bounds are identical in both stances.

## Stance choice (Character Select)
Full rules here; docs/specs/character-select.md carries a short amendment pointing to this section.
- **One stance line per step**, centred under the two cards: `Stance: Orthodox   (Up / Down to change)` or `Stance: Southpaw   (Up / Down to change)`. It is a code-built control with the same style as the cards, default font. It applies to the boxer being picked in **this** step (step 1: the player; step 2: the opponent), whichever card is highlighted. Moving the highlight with Left / Right or the mouse does not change the stance.
- **Toggle:** a fresh press of Up or Down, or a left click on the stance line, flips Orthodox <-> Southpaw. Key repeat (echo) is ignored, so a held Up toggles once. The stance line must **not** take keyboard focus (so Enter still confirms the highlighted card and never toggles), and a click on it does not confirm a card. Toggles are ignored while a screen change is in progress.
- **Defaults:** step 1 starts on `stance.default`. Each time step 2 is entered from step 1, the opponent stance starts on `stance.default`.
- **Confirm** (Enter or click on a card) stores the pick **and** the stance of that step.
- Step 2's "You:" line shows the player's stance: `You: Mateo Rivera (Southpaw)`.
- **Escape:** step 2 -> step 1 keeps the player's pick and the player's stance (the stance line shows it), and discards the opponent pick and stance. Step 1 -> menu discards everything. Ring -> menu discards everything; New Game starts fresh with defaults.
- Same boxer for both picks with different stances is allowed, with no warning.
- **Pure logic (src/Core):** a `Stance` value (Orthodox, Southpaw) parsed from the data strings `"orthodox"` / `"southpaw"`. `CharacterSelectFlow` gains `PlayerStance`, `OpponentStance` and `ToggleStance()` (toggles the stance of the current step; rejected or no change in `Done`, documented like `Confirm`). Its constructor takes the default stance. `Back()` from `PickOpponent` resets `OpponentStance` to the default and keeps `PlayerStance`.
- **Hand-off:** `GameSession` gains `PlayerStance` and `OpponentStance` next to the ids; cleared with them; `Main._Ready` falls back to `stance.default` when unset.

## Fight model (pure logic, src/Core)
All rules below live in `src/Core` with no Godot reference and are covered by xUnit tests. Types as on main: `MoveSet` (parsed from the JSON text, validated, now also exposing the default stance), `FighterInput` (one tick: `MoveAxis` -1/0/+1, `GuardHeld`, `JabPressed`, `CrossPressed`, `SlipLeftPressed`, `SlipRightPressed`; `Pressed` flags true only on the tick of a fresh press), and `PlayerFighter` with `Tick(FighterInput)` exposing `State`, `Phase`, `PhaseFrame`, `X`, `IsBlocking`, `IsEvading`, `EvadeSide` and a per-tick `Contact` (none, Jab or Cross).

**`PlayerFighter` does not need the stance** (no rule in this section depends on it). Stance is used only to choose which glove a pose moves. New pure helper (name is the developer's call), e.g. `StanceRules`:
- `LeadSide(stance)`: Left for Orthodox, Right for Southpaw; `RearSide(stance)` is the other.
- `PunchSide(move, stance)`: jab -> lead side, cross -> rear side.
- `MirrorSign(stance)`: +1 for Orthodox, -1 for Southpaw, multiplied onto every boxer-sideways offset and every stance-dependent turn angle (see Visuals).

### Time
- Ticks at `tickRate`; Core never uses floating-point seconds for timing. The adapter calls `Tick` once per `_PhysicsProcess`; physics tick rate must equal `tickRate` (`GD.PushError` if not).
- **Timeline rule:** a move that starts on tick N occupies ticks N to N + startup + active + recovery - 1 (Startup, then Active, then Recovery). The next tick is free and its input is processed normally.

### States (unchanged from #10)
| State | Phases | Meaning |
|---|---|---|
| Idle | none | Free. Can walk, guard, or start any move. |
| GuardRaising | none | Guard held, hands coming up for `guard.raiseFrames` ticks. `IsBlocking` false. Can walk (slowed). |
| Guarding | none | Guard up. `IsBlocking` true. Can walk (slowed). |
| Jab | Startup, Active, Recovery | Lead-hand straight. No walking. |
| Cross | Startup, Active, Recovery | Rear-hand straight. No walking. |
| SlipLeft | Startup, Active, Recovery | `IsEvading` true only in Active, `EvadeSide` Left. No walking. |
| SlipRight | Startup, Active, Recovery | `IsEvading` true only in Active, `EvadeSide` Right. No walking. |

### What each input does, per state (unchanged from #10)
Order inside one tick: (1) move-timer advance and move end, (2) guard held/released, (3) fresh presses, (4) walking, (5) contact check.

| From state | Jab (J) | Cross (K) | Slip press (Q or E) | Guard held | Walking |
|---|---|---|---|---|---|
| Idle | start Jab | start Cross | start that slip | -> GuardRaising | yes, full speed |
| GuardRaising / Guarding | ignored | ignored | start that slip | stays; release -> Idle on that tick | yes, x `guard.moveSpeedMultiplier` |
| Jab Startup / Active | ignored | ignored | ignored | ignored until move ends | no |
| Jab Recovery | ignored | **cancel: start Cross** (one-two) | ignored | ignored until move ends | no |
| Cross, any phase | ignored | ignored | ignored | ignored until move ends | no |
| Slip Startup / Active | ignored | ignored | ignored (including the other side) | ignored until move ends | no |
| Slip Recovery | **cancel: start Jab** | **cancel: start Cross** | ignored (no slip-to-slip) | ignored until move ends | no |

- Cancel targets come from `moves.<move>.cancelInto`; shipped values are exactly the table (jab -> cross; slips -> jab, cross; cross -> nothing). Only Recovery can be cancelled.
- **The one-two is the jab (lead hand) then the cross (rear hand) in both stances.** The rule is by role, so it is the same in both. Stance changes nothing in the rule or the ticks; it changes only which glove the pose moves.
- **Slip recovery cancels the same in both stances**, into the jab (lead hand) or the cross (rear hand), whichever slip side was used.
- No jab -> jab (no double jab). This is a scope and readability choice, not a boxing rule (see abstraction lines).
- **No input buffer.** An ignored press is dropped.
- **Same-tick priority:** drop presses the current state does not allow, then start only the first allowed of SlipLeft, SlipRight, Jab, Cross.
- **Guard hold is level-based.** Held on the tick a move ends -> GuardRaising. Release -> Idle that tick. While guard is held, jab and cross are ignored. A guard and a punch on the same tick from Idle: guard wins.
- A walk key held when a move starts stops walking for the move; walking resumes on the free tick if still held.

### Movement and bounds (unchanged from #10)
- Only the player moves, only along X: `MoveAxis` x `movement.walkSpeedMps` / `tickRate` per walking tick (x `guard.moveSpeedMultiplier` while GuardRaising or Guarding).
- Start `ring.playerX`; opponent fixed at `ring.opponentX` (fighters.json).
- Bounds every tick after walking: `movement.minX` <= X <= opponentX - `movement.minSeparationM`. Load fails with a clear exception if `minX` > `ring.playerX` or `ring.playerX` > `ring.opponentX` - `minSeparationM`.
- Root Y and Z never change.

### Slip (unchanged from #10)
- Sides are the **boxer's own** in either stance. For the player (facing +X, camera on the +Z side) the boxer's right is toward the camera and the left away from it. Slip direction in real boxing depends on the incoming punch [S2], so the side is a player choice that names a body side. Slips do not move the root.
- Look (both sides, both stances): upper body dips by `visual.slipHeadDropH`, leans toward the chosen side by `visual.slipLeanDeg`, shoulders turn by `visual.slipTurnDeg` in opposite directions for left and right; feet planted; return to stance during Recovery. Slip offsets follow the chosen side and are **not** mirrored by stance; they are applied on top of the boxer's (possibly mirrored) guard pose.
- `IsEvading` only in Active, with `EvadeSide`. No stamina cost. No effect yet (nothing punches the player).
- Deferred: which slip side clears which incoming punch (opponent-attack spec, with a boxing review [S2][S3]); slips will not evade hooks [S3].

### Contact with the idle opponent (unchanged from #10)
- On each Active tick of a punch, if (opponentX - player X) <= that move's `reachM`, the first such tick reports one `Contact` (Jab or Cross). At most one per punch. Root positions, not mesh collision.
- The adapter shows `Contact: Jab` or `Contact: Cross` near the top centre for `feedback.contactTextFrames` ticks (a newer contact replaces it and restarts the timer) and one `GD.Print` line per contact. The text does not name the glove.
- Deferred: damage, health, scoring, opponent reaction, hit-stop, sound, any stance effect.

### Idle opponent
Unchanged: no AI, never moves, punches or reacts. It is built in its chosen stance (mirrored build if southpaw) and keeps that static guard pose (optional idle bob). Core sees it only as a fixed X.

## Visuals (Godot adapter, game/)
Thin adapter in `game/scripts` reads input, calls Core and poses the fighter parts from `State`, `Phase`, `PhaseFrame` **and the fighter's stance**. Code-driven motion only (no AnimationPlayer resources, no external assets, no shaders).
- **Mirror rule.** The southpaw build and every southpaw punch/guard pose are the orthodox ones reflected across the boxer's own vertical facing plane (the plane through the root containing the facing direction and up). In practice: every boxer-sideways offset is multiplied by `MirrorSign`, every stance-dependent turn angle is multiplied by `MirrorSign`, and the roles of each L/R part pair swap. The developer applies this in the pure pose code (`GuardPose.Solve`, `PoseBlend.From` or equivalent in src/Core), not by scaling a node by -1 (a negative scale flips face winding and lighting).
- **Build (both fighters).** Orthodox as today: `FootL` is the lead foot, forward by `body.stanceLeadFootForwardW`, and `GloveL` takes the lead-glove place of the guard. Southpaw: `FootR` leads by the same amount and `GloveR` takes the lead-glove place; any torso/head turn of the guard stance flips sign. Face cues still face the opponent; the player still faces +X and the opponent -X.
- **Jab:** the lead glove and its forearm move straight toward the opponent during Startup to `visual.punchExtendH`, hold through Active, return during Recovery. No torso turn. The rear glove stays up.
- **Cross:** the rear glove and its forearm extend the same way, and torso, shoulders and head turn by up to `visual.crossTorsoTurnDeg` bringing the **rear** shoulder forward (right shoulder in orthodox, left in southpaw: same size, opposite direction). Feet stay planted. The lead glove stays up.
- **Guard (playtest fix, issue #12 feedback round 1):** the old pose moved the gloves up only 0.04 of height, so Guarding looked like Idle. Guard is now a **clearly different, high pose** read from the side-on camera: over `guard.raiseFrames` ticks (blend 0 -> 1, same curve as today) both gloves rise to about eye level (a tuning value, not a coaching claim: real manuals teach the lead hand at about eye level [S3] or about shoulder height [S1], and the rear hand near the chin [S3] or collarbone [S1]), are pulled back so they sit just in front of the face (not an arm's length out), and close inward; the elbows follow only a little (they stay low and tucked at the ribs, not flared up). Drop at once on release (as today). Logic, timings, walking slow-down and the punch lockout are unchanged; only the pose values change. Values in `visual.guardGloveRaiseH`, `guardGloveInwardW`, `guardGloveBackH`, `guardElbowRaiseShare`, `guardElbowInwardShare`; the last three are new keys that replace the animator's hard-coded elbow-share constant (developer: add them to `MoveSet`/`VisualTuning` parsing, non-negative, and expose them to `PoseBlend` consumers). The guard blend only ever **adds** to the Idle stance pose, so it also blends in and out cleanly, and a slip or punch in progress sits on top of it as today. Southpaw is the mirror (the same numbers, boxer-sideways offsets x `MirrorSign`, lead/rear glove roles swapped). Reference geometry in fractions of fighter height H, at default build: Idle glove centre about 0.84 H high and about 0.22 H ahead of the root; head front surface 0.065 H ahead; head spans about 0.86 H (chin) to 1.015 H (top). Guarding target: glove centre about 0.95 H high and about 0.14 H ahead of the root.
- **Slip:** unchanged (see Slip).
- **Timing curves are identical** in both stances (same tick, same fraction of travel).
- Every move returns all parts to the boxer's own guard pose (orthodox or southpaw) in Idle, within 1 mm.
- Visual tuning: the developer may tune `visual.*` by about 30 percent so a punch at gap == its reach visibly reaches the opponent and at gap > reach visibly falls short, in both stances, and records the final values in the data file.

## Acceptance criteria
AC1 to AC18 are xUnit tests in `tests/Core.Tests` (tick-exact, using the shipped `player-moves.json` and `fighters.json` text). AC19 to AC30 are checked by the headless smoke scene where practical (it may drive input with `Input.ActionPress` / `ActionRelease`, and set `GameSession` stances directly) and by the playtest. "A slip" in a test means each of SlipLeft and SlipRight run separately. "In both stances" means the test runs once with Orthodox and once with Southpaw.

**Move rules (as #10, data keys `jab` / `cross`)**
- AC1. **Given** the shipped `player-moves.json`, **when** parsed, **then** it has `tickRate`, `stance.default` equal to `"orthodox"`, moves `jab`, `cross`, `slipLeft`, `slipRight` with integer frame counts >= 1, positive `reachM` for jab and cross, `guard`, `movement` and `feedback` blocks; **and given** a copy with a 0 or missing frame count, a non-positive speed or reach, a missing move, an unknown `cancelInto` name (including `leftJab`, `rightCross` or any other #12-draft name), a missing `stance.default` or a value other than `orthodox` / `southpaw`, or a bound that excludes `ring.playerX`, **when** parsed, **then** it throws and the message names the bad key.
- AC2. **Given** an Idle player, **when** Jab is pressed on tick 1 and nothing else follows, **then** the state is Jab Startup for exactly `jab.startupFrames` ticks, Active for `activeFrames`, Recovery for `recoveryFrames`, and Idle on the next tick.
- AC3. **Given** an Idle player, **when** Cross is pressed, **then** the same timeline holds with `cross` frame counts; **and** with the shipped data the cross total is greater than the jab total and at most 1.5 times it.
- AC4. **Given** an Idle player, **when** SlipLeft (or SlipRight) is pressed, **then** the timeline follows its frame counts, `IsEvading` is true on every Active tick and false otherwise, and `EvadeSide` is Left (or Right) on every Active tick and none otherwise.
- AC5. **Given** a Jab in Startup or Active, **when** Jab, Cross or either slip is pressed, **then** nothing changes, and after the jab ends the fighter is Idle (not buffered).
- AC6. **Given** a Jab in Recovery, **when** Cross is pressed, **then** on that tick the state is Cross Startup, frame 1, with normal `cross` frame counts; **and given** Jab on tick 1 and Cross on the first Recovery tick (tick 1 + `startupFrames` + `activeFrames`), **then** the fighter is Idle exactly the cross total later; **and when** Jab or either slip is pressed in Jab Recovery instead, **then** it is ignored and the jab runs to its end.
- AC7. **Given** a Cross in any phase, **when** Jab, Cross or either slip is pressed, **then** it is ignored and the cross runs to its end.
- AC8. **Given** a slip in Recovery, **when** Jab (or Cross) is pressed, **then** on that tick the state is that punch's Startup, frame 1 (tested for both slip sides); **when** either slip is pressed in that Recovery, **then** it is ignored; **given** a slip in Startup or Active, **when** any punch or slip is pressed, **then** it is ignored and the slip keeps its side.
- AC9. **Given** Idle, **when** guard is held, **then** GuardRaising with `IsBlocking` false for exactly `guard.raiseFrames` ticks, then Guarding with `IsBlocking` true while held; **when** released, **then** Idle that tick with `IsBlocking` false.
- AC10. **Given** Guarding, **when** Jab or Cross is pressed, **then** it is ignored and the state stays Guarding; **when** a slip is pressed, **then** it starts that tick, and if guard is still held when it ends the state becomes GuardRaising, then Guarding.
- AC11. **Given** Idle at `ring.playerX`, **when** forward is held N ticks (no bound reached), **then** X increases by exactly N x `walkSpeedMps` / `tickRate` (within 1e-6); back decreases it the same way; both or neither leaves X unchanged; while Guarding the change is multiplied by `guard.moveSpeedMultiplier`.
- AC12. **Given** any punch or slip in any phase, **when** forward or back is held, **then** X does not change; **when** the move ends with the key held, **then** walking resumes on the free tick.
- AC13. **Given** back held 10 seconds of ticks, **then** X equals `movement.minX`; forward held 10 seconds, **then** X equals `ring.opponentX` - `movement.minSeparationM`; X is never outside these bounds.
- AC14. **Given** gap <= jab reach, **when** a Jab runs, **then** exactly one Contact Jab on its first Active tick; **given** jab reach < gap <= cross reach, **then** Jab reports none and Cross reports exactly one Contact Cross; **given** gap > cross reach, **then** neither reports.
- AC15. **Given** Idle, **when** SlipLeft, SlipRight, Jab and Cross are pressed on one tick, **then** SlipLeft starts; SlipRight + Jab + Cross -> SlipRight; Jab + Cross -> Jab; **given** Jab Recovery, **when** Jab and Cross on one tick, **then** Cross starts; **when** guard is held and a punch pressed on the same tick from Idle, **then** GuardRaising and no punch.
- AC16. **Given** any input sequence, **when** ticked, **then** the opponent X used by Core never changes.

**Stance (pure logic)**
- AC17. **Given** the stance helper, **when** queried, **then** Orthodox gives lead Left, rear Right, jab -> Left, cross -> Right, `MirrorSign` +1; Southpaw gives lead Right, rear Left, jab -> Right, cross -> Left, `MirrorSign` -1; **and given** the pure pose code (`GuardPose.Solve` / `PoseBlend.From` or equivalent) for the same build, **when** evaluated for every tick of Idle, Jab, Cross, GuardRaising and Guarding in both stances, **then** each southpaw part offset equals the orthodox offset of its L/R partner with the boxer-sideways component negated (within 1 mm) and each stance turn angle is negated (within 0.01 deg); **and** for SlipLeft and SlipRight the slip-specific offsets (dip, lean, slip turn) are identical in both stances.
- AC18. **Given** a new `CharacterSelectFlow` with default Orthodox, **then** `PlayerStance` and `OpponentStance` are Orthodox; **when** `ToggleStance()` in `PickPlayer`, **then** `PlayerStance` becomes Southpaw and a second toggle returns it to Orthodox, and `OpponentStance` is unchanged; **when** the player confirms and the step is `PickOpponent`, **then** `OpponentStance` is the default and `ToggleStance()` changes only `OpponentStance`; **when** `Back()` from `PickOpponent`, **then** `PlayerStance` is kept and `OpponentStance` is back to the default; **when** `ToggleStance()` is called in `Done`, **then** it is rejected or does nothing (documented) and both stances are unchanged; **and** Jab/Cross timelines from AC2 and AC3 are the same whatever stance was chosen (`PlayerFighter` takes no stance).

**Guard pose (issue #12 feedback, smoke scene or pose unit tests; "H" = that fighter's height, "head" = the `Head` part; both stances, for the player and the opponent's static pose where noted)**
- AC31. **Given** a fighter in Idle, **when** guard is held until the state is Guarding (blend 1), **then** each of `GloveL` and `GloveR` centres is at least 0.8 x `visual.guardGloveRaiseH` x H higher than in Idle, **and** each glove centre height is between the head's chin height and top-of-head height (the glove covers the face, not the neck or the sky), **and** each glove centre is ahead of the head's front surface (toward the opponent) by at least 0.02 H and by at most 0.20 H.
- AC32. **Given** a fighter in Idle, **when** guard is held, **then** the glove heights rise monotonically over exactly `guard.raiseFrames` ticks of GuardRaising and stay constant while Guarding (within 1 mm); **when** guard is released, **then** the next tick's pose is the Idle pose within 1 mm (no leftover); **and** neither glove, for either stance, ever changes sides of the body centre line (its boxer-sideways coordinate keeps its Idle sign) during raising, guarding or release.
- AC33. **Given** the shipped `player-moves.json`, **when** parsed, **then** `visual.guardGloveRaiseH` is at least 0.08, `guardGloveBackH`, `guardElbowRaiseShare` and `guardElbowInwardShare` exist and are >= 0 (a copy with one missing or negative throws and the message names the key); **and given** Guarding in each stance, **then** each elbow is lower than its glove centre, closer to the body centre line than its shoulder is, and each limb's displayed length is within 15 percent of its Idle length (no visible stretch).
- AC34. **Given** the same Guarding pose evaluated for Orthodox and Southpaw, **when** compared, **then** each southpaw part offset equals the orthodox offset of its L/R partner with the boxer-sideways component negated (within 1 mm), including the new guard offsets (extends AC17 to the new keys).

**Select screen and ring (smoke where practical, playtest)**
- AC19. **Given** Character Select step 1, **when** it shows, **then** a stance line under the cards reads `Stance: Orthodox   (Up / Down to change)`; **when** Up is pressed, **then** it reads Southpaw; **when** Down is pressed, **then** Orthodox again; **when** Up is held about 2 seconds, **then** it changes exactly once; **when** the stance line is clicked, **then** it toggles and no card is confirmed; **and when** Left / Right move the highlight, **then** the stance does not change.
- AC20. **Given** step 1 with Southpaw chosen, **when** Enter confirms Ghost, **then** step 2 shows `You: Mateo Rivera (Southpaw)` and its own stance line reads Orthodox; **when** Escape is pressed in step 2, **then** step 1 shows Ghost highlighted and the stance line reads Southpaw.
- AC21. **Given** player Ghost Southpaw and opponent Anvil Orthodox are confirmed, **when** the ring loads, **then** the Tags read `YOU (Southpaw)` and `CPU (Orthodox)`; the player's `FootR` is further toward the opponent than its `FootL`; the opponent's `FootL` is further toward the player than its `FootR`; both still face each other with face cues toward each other, and all fighter-build checks of character-select.md (parts, colours, feet on the floor) still pass.
- AC22. **Given** the project is built, **when** project.godot is inspected, **then** the `[input]` section is unchanged from main: `fight_forward` (D), `fight_back` (A), `fight_jab` (J), `fight_cross` (K), `fight_guard` (S), `fight_slip_left` (Q), `fight_slip_right` (E), with physical keycodes, one key each, none on Escape, Enter, keypad Enter, an arrow key or Space; no `fight_jab_left` / `fight_cross_right` or other #12-draft action exists; menu actions are unchanged.
- AC23. **Given** the fight in either stance, **when** J (or K, Q, E) is held 2 seconds, **then** exactly one move starts; **when** A and D are held together, **then** no movement; **when** an arrow key is pressed, **then** no movement and no stance change.
- AC24. **Given** an orthodox player (smoke), **when** Jab reaches Active, **then** `GloveL` is further toward the opponent than in Idle by at least half of `visual.punchExtendH` x fighter height, `GloveR` has not moved forward and the torso is not turned; **when** Cross reaches Active, **then** `GloveR` has moved forward the same way, `GloveL` has not, and the torso is turned with the right shoulder forward.
- AC25. **Given** a southpaw player (smoke), **when** Jab reaches Active, **then** `GloveR` moves forward as in AC24 and `GloveL` does not, with no torso turn; **when** Cross reaches Active, **then** `GloveL` moves forward and `GloveR` does not, and the torso turn has the same size as the orthodox cross's and the opposite direction (left shoulder forward).
- AC26. **Given** either stance, **when** Jab is pressed on tick 1 and Cross on the first Jab Recovery tick, **then** the cross starts that tick (one-two) and the fighter is Idle after the same total ticks as in orthodox; the glove that leads the combo is the lead glove of that stance.
- AC27. **Given** either stance (smoke), **when** SlipLeft and SlipRight reach Active, **then** `Head` is lower than in Idle, the SlipRight head is nearer the camera (greater Z) and the SlipLeft head further from it (for the player), the shoulders turn in opposite directions, root X and Z are unchanged; **and when** Jab or Cross is pressed in slip Recovery, **then** that punch starts with the stance's correct glove; **and** while Guarding both gloves are higher than in Idle; **and** back in Idle every posed part is within 1 mm of that stance's guard pose.
- AC28. **Given** the player in range, **when** a punch makes contact, **then** `Contact: Jab` or `Contact: Cross` (matching the punch, whatever the stance) appears near the top and disappears after `feedback.contactTextFrames` ticks; the opponent does not move or react. **Given** the start position, **when** J or K is pressed, **then** no contact text appears; walking forward stops at the minimum separation.
- AC29. **Given** the fight in any move state, **when** Escape is pressed, **then** the menu shows; **when** New Game is chosen again, **then** step 1's stance line reads `stance.default` (Orthodox), and after picks the player starts at `ring.playerX`, Idle, with no leftover contact text.
- AC30. **Given** `Main.tscn` is launched directly (smoke, no select), **when** it runs, **then** both fighters use the first roster entry and `stance.default` (Tags read `(Orthodox)`), a smoke-driven Jab and Cross each complete and return to Idle, and no error is logged.

## Balance values
All numbers live in `data/balance/player-moves.json` (plus positions and body proportions in `data/balance/fighters.json`), not in this spec or in code.
Pointer: `data/balance/player-moves.json` -> `tickRate`; `stance.default` (new); `moves.jab|cross|slipLeft|slipRight.startupFrames|activeFrames|recoveryFrames|cancelInto`; `moves.jab|cross.reachM`; `guard.raiseFrames`, `guard.moveSpeedMultiplier`; `movement.walkSpeedMps`, `movement.minX`, `movement.minSeparationM`; `feedback.contactTextFrames`; `visual.punchExtendH`, `visual.crossTorsoTurnDeg`, `visual.guardGloveRaiseH`, `visual.guardGloveInwardW`, `visual.guardGloveBackH`, `visual.guardElbowRaiseShare`, `visual.guardElbowInwardShare` (the last three are new, issue #12 feedback), `visual.slip*`. `data/balance/fighters.json` -> `ring.playerX`, `ring.opponentX`, `body.stanceLeadFootForwardW`, `body.stanceFootSpreadW`.
Changes in #12 against main: only the `stance` block was added (and comments updated). `moves.jab` / `moves.cross`, their numbers and all `cancelInto` lists are as on main. There is no per-stance number: a southpaw uses the same values mirrored. A later per-stance difference (for example power) would be data-only.
Parsing and validation in Core; the adapter only reads the file, which must be in the exported build.

## Boxing notes
#10 notes reviewed by boxing-consultant on 2026-11-28 (work/10/boxing-review.md). The stance version of #12 was reviewed by boxing-consultant on 2026-11-28 (work/12/boxing-review.md); the earlier 4-key review is superseded. Sources: docs/boxing/sources.md.

### Real boxing this spec relies on
- Orthodox means the left hand and left foot lead; southpaw is the mirror, with the right hand and right foot leading [S10].
- The jab is the lead-hand punch and the cross the rear-hand punch, in either stance; the names follow the hand's role, not left or right [S10]. So a southpaw jabs with the right glove and crosses with the left [S10].
- The cross has farther to travel than the jab and gets its power from the rear-leg push and hip and torso rotation [S1][S3][S10]; the jab relies mainly on the front-leg push with slight torso involvement [S10]. Reach values are tuning values, not measured.
- The one-two is taught as lead straight then rear straight, which justifies the jab -> cross cancel [S1][S2][S3]. For a southpaw that is the right-hand jab then the left-hand cross, by the role definition [S10] (the coach manuals are not claimed to word it for southpaw). Combination punches are delivered faster than a single maximal punch [S4].
- Only the punching hand leaves the guard while the other stays up, and the punching hand returns along the same line [S1][S2].
- A slip moves the head off the line of an incoming straight punch and is followed at once by a counter [S2][S3]; the slip side depends on the incoming punch [S2].
- A guard with the hands up and the elbows tucked in to protect the ribs protects the head and body against straight punches [S1][S3]. Real guards put the lead hand at about eye level [S3] or about shoulder height [S1] and the rear hand near the chin [S3] or collarbone [S1]; neither manual gives a forearm angle or a "forearms cover the face" instruction. "Forearms covering" is therefore only the game's picture of "hands up protect the head", not a coaching fact. The exact glove heights, distances and elbow positions are tuning values, not claims.
- In S10, 30 trained, all right-handed boxers each threw the jab, cross, lead hook and rear hook from both stances (orthodox first, then southpaw, not randomised); the authors suggest training both stances may add versatility [S10]. This does not show that every boxer can. In that sample stance changed punch force (jab higher in southpaw, cross higher in orthodox) but not fist acceleration [S10]; all were right-handed, so this is not a tuning basis.
- Real boxers usually fight from one usual stance, often linked to handedness; about 75 percent of 340 top professionals were orthodox and 25 percent southpaw [S11].
- Real timing scale: throw to first contact about 0.11 to 0.14 s for jab and cross [S6]; a whole punch about 0.4 to 0.5 s [S5][S6]; contact about 0.02 to 0.03 s [S7].

### GAMEPLAY ABSTRACTION lines
New in #12 (stance):
GAMEPLAY ABSTRACTION: stance is fixed for the whole fight instead of a stance the boxer can change during a bout [S10] — simple controls; an in-fight switch is a later task.
GAMEPLAY ABSTRACTION: stance changes no timing, reach or power instead of changing punch force by punch type [S10] — one readable rule set; stance effects can be data-only later.
GAMEPLAY ABSTRACTION: the southpaw build, punch poses and cross torso turn are exact reflections of the orthodox ones instead of a real southpaw's own body action [S10] — S10 defines southpaw as the mirror stance but does not describe the body action as an exact reflection, nor state which shoulder ends forward or the turn angle (the angle is a tuning value); readability.
GAMEPLAY ABSTRACTION: any boxer may take either stance by a free pre-fight choice, with no link to handedness or stats, instead of a boxer's own usual stance, which is often linked to handedness and is about one in four among top professionals [S11] — client request for a free choice; stats are display-only.
GAMEPLAY ABSTRACTION: orthodox against southpaw has no special effect instead of a real mismatch in which both stances won more of their fights against orthodox opponents than against southpaws [S11] — the opponent is idle and stance changes no number; matchup effects can be data-only later.
GAMEPLAY ABSTRACTION: the jab has no torso turn instead of slight torso involvement [S10] — readability from the side-on camera.

Carried from #10 (reviewed):
GAMEPLAY ABSTRACTION: jab and cross use fixed tick timings chosen for rhythm instead of measured durations (throw about 0.1 s, whole punch about 0.4 to 0.5 s [S5][S6]) — a readable startup/active/recovery beat; the jab total is deliberately quicker than real.
GAMEPLAY ABSTRACTION: the cross is a bit slower and heavier than the jab (total about 1.4 times) instead of nearly equal in speed [S4][S5][S6] — gives the cross a risk-versus-reward cost; the gap is small because real speeds are close.
GAMEPLAY ABSTRACTION: slip timings are tuning values instead of measured durations (none in the allowed sources; a generous evade window fits [S4]) — a clear, learnable window. Both sides share timings.
GAMEPLAY ABSTRACTION: reach is a root-to-root distance (jab shorter than cross) instead of arm length, stance and step length [S1][S3] — one readable range rule.
GAMEPLAY ABSTRACTION: the boxer cannot step while punching or slipping instead of stepping with the punch [S9] — committed moves and simple controls.
GAMEPLAY ABSTRACTION: held guard is a separate committed state with no punching instead of punching out of the guard with the free hand kept up [S1][S2] — gives guard a clear cost.
GAMEPLAY ABSTRACTION: the idle pose is the stance guard and "Guard" is a tighter, much higher cover with a short raise instead of one continuous guard [S1][S3] — a visible state the player controls, readable from the side-on camera.
GAMEPLAY ABSTRACTION: both gloves raised to eye level in front of the face, with a visibly higher Guard than the Idle stance, instead of a lead hand at about eye level or shoulder height and a rear hand near chin or collarbone held constantly, with elbows tucked [S1][S3] — the player must read the held Guard state from a side-on camera. The raised Guard pose is exaggerated for readability.
GAMEPLAY ABSTRACTION: the slip side is a free key choice and the head moves toward or away from the camera with an added shoulder turn, instead of a lateral head move chosen in answer to the incoming punch [S2] — the camera only sees depth, and the opponent has no attacks yet.
GAMEPLAY ABSTRACTION: a slip is a committed move with its own startup, evade window and recovery, with no step, instead of a head movement not tied to fixed frame counts [S2] — readable timing.
GAMEPLAY ABSTRACTION: the only chains are jab -> cross and slip -> punch, with no double jab, instead of a wider set such as the double jab, body jab and hooks [S1][S3] — M1 has few moves; same-hand repeats and combos come later. (The ban on a same-hand repeat is a scope choice, not a boxing rule.)
GAMEPLAY ABSTRACTION: walking is a smooth glide along one axis instead of step-and-drag footwork and pivots [S1][S8] — roadmap M1 limits footwork.
GAMEPLAY ABSTRACTION: both stances share identical reach and timings for every build instead of varied builds and styles [S3] — stats are display-only until combat tuning.
GAMEPLAY ABSTRACTION: a punch "contact" is a distance check on root positions with no target, damage or reaction instead of a glove landing [S7] — the opponent is idle.
GAMEPLAY ABSTRACTION: no input buffering and no cancel out of Startup or Active instead of a boxer adapting a punch mid-flight — readable, mash-proof rhythm (vision pillar).

### Boxing review outcome (#10)
Kept for the record; decisions on work/10/boxing-review.md.

| # | Suggestion | Decision | Reason |
|---|---|---|---|
| C1 | Narrow the jab/cross gap | **Accepted.** Cross 26 ticks against the jab's 18 (ratio 1.44). | Real startups are nearly equal [S6]; AC3 caps the ratio at 1.5. |
| C2 | Shorter cross startup when it follows a jab | **Rejected for M1.** | The cancel already skips the rest of the jab's recovery. |
| C3 | Remove rotation as the reason the cross is slower | **Accepted.** | Rotation is the source of power [S1][S3], not a proven cause of lower speed. |
| C4 | Only the punching hand leaves the guard | **Accepted.** | [S1][S2]. |
| Slip direction | One slip for both punches? | **Superseded** by two slips; side-to-punch mapping deferred. | [S2] |

### Carried from the #12 4-key review (still valid after the rescope)
- Jab and cross follow lead and rear, not left and right, in either stance [S10] (work/12/boxing-review.md finding 1, 8). This rescope follows that recommendation.
- [S8] is not cited for cross rotation (finding 18).
- The no-double-jab rule is worded as a scope choice (finding 11).
- Findings on right jab, left cross, mirrored one-two and four-punch slip cancels no longer apply (those moves were removed).

### Boxing review outcome (#12)
Reviewed 2026-11-28 (work/12/boxing-review.md). Applied:
- Southpaw definition and lead/rear roles cite [S10] alone; [S3] is verified only for orthodox, so it is not cited for any southpaw claim.
- The one-two is worded by role (jab then cross); the southpaw mapping rests on [S10], not on the coach manuals.
- The "both stances" claim is scoped to S10's sample (30 trained right-handed boxers) and "any boxer may be either stance" now cites [S11].
- Abstraction lines added or reworded: mirrored southpaw build and cross turn, free stance choice, no matchup effect, no jab torso turn.
- Slip side versus stance: no claim now, deferred to the opponent-attack spec.
- Not verified: whether S1, S2 or S3 text mentions southpaw (needs a text-readable copy). Nothing in this design depends on it. S10 and S11 were read at abstract level only.

## Out of scope
- **In-fight stance switch** (a later task), and any switch during Character Select other than the toggle described.
- Any stance effect on timing, reach, power, damage, stamina, accuracy or AI.
- Per-boxer default stance in the roster, handedness, saving the last stance choice.
- Rear-hand jab, lead-hand cross, mirrored one-two, separate left/right punch keys (the rejected 4-key design).
- Stamina, health, damage, KO, knockdowns, round timer display or round end, scoring.
- Opponent AI or any opponent movement, punches, blocking, slipping, reaction.
- Hooks, uppercuts, body shots, feints, double jab or other same-hand repeats, clinches, lateral or depth root movement, pivots, footwork changes for southpaw beyond the mirrored stance.
- Deciding which slip side evades which punch.
- Use of boxer stats or build in moves.
- Hit-stop, camera shake, particles, sound, shaders, HUD beyond the contact text, controls hint, stance line and stance in the Tag.
- Gamepad, mouse in the fight, rebinding, pause, settings, new InputMap actions.
- Input buffering; combos beyond the cancel rules above.
- Any code or tests (this is a spec).

## Client decisions
Made at G2 of #10 (2026-11-28), still in force:
1. Guard stays on S (hold).
2. Two slips, Q and E; sides are the boxer's own left and right.
3. Walking is A / D only; arrow keys are not fight keys.
4. Boxer-relative sides are kept (Q = boxer's left, E = boxer's right) in both stances.

Made at G2 of #12 (2026-11-28):
5. Two punch keys only: J = jab (lead hand), K = cross (rear hand). The 4-key design is dropped.
6. Stance per boxer, chosen in Character Select (orthodox or southpaw); southpaw is mirrored; no in-fight switch.

## Client decisions needed (G2 of #12)
1. **Stance control:** proposed Up / Down (existing actions, no new key) plus a click on the stance line. Alternative: Tab as a dedicated stance key (one new code-registered action). Recommendation: Up / Down, because Left / Right already move between cards, so Up / Down reads as "change the other setting", and it adds no key.
2. **Default stance:** both steps start on Orthodox (`stance.default`). Alternative: a per-boxer default in the roster (for example Ghost starts southpaw). Not proposed, to keep stance a pure player choice.

Deferred, not a client decision now: which slip side beats which punch (opponent-attack spec).

## Playtest checklist
Five things the client checks by hand at G3.
1. Click New Game: under the two cards a line reads "Stance: Orthodox (Up / Down to change)". Press Up: it says Southpaw; press Down: Orthodox; clicking the line also flips it, and Left / Right still only move the card highlight. Pick Southpaw and confirm Ghost: step 2 shows "You: Mateo Rivera (Southpaw)" and its own stance line starts on Orthodox.
2. Leave the opponent on Orthodox and confirm Bruno: in the ring the tags read "YOU (Southpaw)" and "CPU (Orthodox)", and the two boxers stand as mirror images in their footing: your right foot is forward, the opponent's left foot is forward.
3. As the southpaw, walk close with D and press J: the glove nearer the camera (your right) shoots out and "Contact: Jab" shows. Press K: the other glove (your left) shoots out, the body turns with the left shoulder coming forward, it feels a bit slower and heavier, and "Contact: Cross" shows. From the starting distance neither shows any "Contact" text.
4. Press J then quickly K: the cross follows the jab smoothly as a one-two. Mash J and K: the boxer never punches faster than the moves allow, and holding a key throws only one punch. Hold S: both gloves clearly lift to your face and tuck in front of it with the elbows down (it must be obvious from the side that you are covering up, and the same in either stance), walking slows, and J and K do nothing until you let go. Press Q or E: the upper body dips to that side (E toward the camera), and J or K pressed right as it comes back fires out of it.
5. Press Escape, start a New Game and keep both boxers on Orthodox: everything works as before, J now throws the left glove (further from the camera) and K the right glove with the right shoulder turning, with the same rhythm as the southpaw fight. Escape in step 2 brings back step 1 with your stance kept.
