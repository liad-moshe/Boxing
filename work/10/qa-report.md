# QA report: task 10 player controls

Branch task/10-player-controls, spec docs/specs/player-controls.md (APPROVED). First QA cycle on this task (not a repeat FAIL).
No tests were added: every AC has automated coverage or is explicitly a playtest step. No production code was touched.

## Commands (docs/process/commands.md, in order)
| # | Command | Exit | Result |
|---|---|---|---|
| 1 | `dotnet build Boxing.sln -warnaserror` | 0 | 0 Warning(s), 0 Error(s) |
| 2 | `dotnet test tests/Core.Tests` | 0 | Passed 141, Failed 0, Skipped 0 |
| 3 | `godot --headless --path . --build-solutions --quit` | 0 | dotnet_build_project DONE; working tree stayed clean (no tab rewrite of Main.cs) |
| 4 | `godot --headless --path . res://game/scenes/smoke/Smoke.tscn` | 0 | `SMOKE: PASS after 743 physics frames (fight script included)`; 0 lines match `ERROR`, `SCRIPT ERROR`, `Unhandled exception` |

Smoke prints `Contact: Jab (gap 0.75 m)` and `Contact: Cross (gap 0.75 m)` (the controller's GD.Print per contact) and the reach checks (jab tip 0.975 m, cross tip 1.118 m from root, both reach the opponent head front at gap == reach).

## AC map
| AC | How verified | Result |
|---|---|---|
| AC1 | MoveSetTests: Shipped_Parses_WithAllMovesAndBlocks, Shipped_CancelTargets_AreTheSpecTable, BadMoveNumber/MissingFrameCount/MissingMove/UnknownCancelInto/BadBlockNumber/MissingBlock/BadTickRate, MinX_AbovePlayerX, PlayerX_TooCloseToOpponent (all assert the key is named) | PASS |
| AC2 | PlayerFighterTests Jab_RunsItsTimeline_ThenIsIdle (AssertTimeline, tick-exact, shipped JSON) | PASS |
| AC3 | Cross_RunsItsTimeline_ThenIsIdle, Cross_IsSlowerThanJab_ButWithinOnePointFiveTimes (26 vs 18 = 1.44) | PASS |
| AC4 | SlipLeft/SlipRight_RunsItsTimeline_ThenIsIdle, Slip_EvadesOnlyInActive_WithItsSide | PASS |
| AC5 | Jab_StartupAndActive_IgnoreEveryPress_AndNothingIsBuffered | PASS |
| AC6 | JabRecovery_CrossPress_CancelsIntoCrossStartup_OnSameTick, OneTwo_EndsExactlyCrossTotalTicksAfterTheCancel, JabRecovery_JabAndSlipPresses_AreIgnored, JabStartup_CrossPress_IsIgnored | PASS |
| AC7 | Cross_IgnoresEveryPressInEveryPhase_AndRunsToItsEnd | PASS |
| AC8 | SlipRecovery_PunchPress_CancelsIntoThatPunch_OnSameTick, SlipRecovery_AnySlipPress_IsIgnored_IncludingOtherSide, Slip_StartupAndActive_IgnoreEveryPress_AndKeepTheirSide | PASS |
| AC9 | Guard_RaisesForRaiseFrames_ThenGuardsWhileHeld_AndReleasesToIdleOnThatTick, Guard_ReleasedWhileRaising_ReturnsToIdleAtOnce | PASS |
| AC10 | Guarding_IgnoresJabAndCross, Guarding_SlipPress_StartsThatSlip_AndGuardResumesAfterIt, GuardHeldDuringAMove_IsIgnored_ThenGuardRaisesOnTheTickTheMoveEnds | PASS |
| AC11 | Walking_ForwardAndBack_ChangesXByExactlySpeedOverTickRatePerTick, Walking_BothOrNeitherKeyHeld_LeavesXUnchanged, Walking_WhileGuarding_IsSlowedByTheMultiplier_RaisingAndGuarding | PASS |
| AC12 | Moves_DoNotWalk_AndWalkingResumesOnTheFreeTick (theory over the 4 moves) | PASS |
| AC13 | Walking_BackForTenSeconds_StopsExactlyAtMinX_AndNeverLeavesTheBounds, Walking_ForwardForTenSeconds_StopsExactlyAtMinSeparation_AndNeverPassesTheOpponent | PASS |
| AC14 | Jab_InReach_ReportsExactlyOneContact_OnItsFirstActiveTick, Jab_ExactlyAtReach_Contacts, BetweenJabAndCrossReach_JabMisses_CrossContactsOnce, BeyondCrossReach_NeitherPunchContacts, Slips_NeverContact | PASS |
| AC15 | SameTick_SlipLeftBeatsEverything, SameTick_SlipRightBeatsPunches, SameTick_JabBeatsCross, SameTick_GuardWinsOverAPunch_FromIdle, SameTick_IllegalWinnerIsSkipped | PASS |
| AC16 | OpponentX_NeverChanges_ThroughAnyInputSequence (OpponentX is a get-only property) | PASS |
| AC17 | Smoke: hint label text and visibility, Idle at ring.playerX, first fight_forward press moves X and the node follows. Hint position at the bottom (BottomWide anchor in code) and the real D key: playtest 1 | PASS (smoke) + playtest |
| AC18 | Smoke CheckInputMap (7 actions, exactly one event, physical keycode, Keycode None, none on Escape/Enter/KpEnter/arrows/Space; ui_back, menu_up, menu_down, menu_confirm unchanged). Also read project.godot: physical_keycode 68 D, 65 A, 74 J, 75 K, 83 S, 81 Q, 69 E. menu_left/right are code-registered and untouched (InputActions.EnsureRegistered unchanged) | PASS |
| AC19 | Smoke CheckHeldKeyThrowsOnce for J (120 frames), K, Q, E: exactly 1 start each; A+D together: X unchanged; ui_left/right/up/down and menu_left/right pressed: X unchanged. Note: the arrow check drives the actions, not raw arrow keys; AC18 proves no fight action uses an arrow, so the result is equivalent. Real keys: playtest 1 and 4 | PASS (smoke) + playtest |
| AC20 | Smoke: lead glove +0.451 m (needs >= 0.5 x 0.24 x H) with rear glove not forward; cross mirror; guard gloves higher; slips: head lower, Z sign per side, shoulders opposite, root and feet unchanged; every posed part within 1 mm and basis equal after each move. Core side: PoseBlendTests | PASS |
| AC21 | Smoke: text `Contact: Jab` / `Contact: Cross`, visible for contactTextFrames (+-2); opponent root and every part transform unchanged. ContactBannerTests for the countdown and restart | PASS |
| AC22 | Smoke: out-of-range jab never shows contact text; walking stops at minX separation 0.75 m and never passes the opponent. Core: StartPosition_JabIsOutOfRange_NoContact | PASS |
| AC23 | Not in smoke (Escape handling in Main is unchanged; a new Main creates a new FightController, PlayerFighter and ContactBanner, so state is fresh by construction; ContactBannerTests New_IsHidden_WithNoText). Needs the real flow: playtest 5 | PLAYTEST ONLY |
| AC24 | The smoke itself is the direct launch of Main.tscn with the default roster: Jab completes and returns Idle, zero ERROR lines | PASS |

AC with no automated coverage: AC23 only (Escape and fresh restart). It is covered by playtest step 5. The physical keys and the on-screen hint look (AC17, AC19) are also checked by hand.

## API-drift scan (changed .cs in game/ and src/, plus tests)
Patterns: `Spatial`, `KinematicBody`, `.Translation`, `yield`, `Connect(`, `Godot.Object`, `OS.GetTicksMsec`, `Engine.EditorHint`, `[Remote]`, `[Puppet]`, `[Master]`, `.Instance()`, `VisualServer`, `Reference`, `pragma`, `SuppressMessage`. Result: no hits (the only match was `Camera3D`, a false positive of the `Camera` pattern). Compiler with warnings-as-errors found no obsolete API (CS0618/CS0612). Godot 4 idioms confirmed: `Input.IsActionJustPressed` in `_PhysicsProcess`, `Engine.PhysicsTicksPerSecond`, `SceneTree.SignalName.PhysicsFrame`, `GD.PushError`.

## Hard rules
- No external assets: the diff adds no image, model, font, audio or .import file. Added files are .cs, .cs.uid, .json, .md only.
- No mouse in the fight: FightController, FighterAnimator, Main have no mouse events. Labels use `MouseFilter.Ignore`. The only mouse code in the repo is the pre-existing Menu.cs and CharacterSelect.cs (unchanged by this task).
- No absolute or user-specific paths in committed files: grep of the added lines finds only `res://` paths and source URLs.
- Protected paths (.claude, .github, CLAUDE.md, docs/process, global.json, .gitignore, .gitattributes): untouched.
- Core has no Godot reference: no `using Godot` in src/, no Godot reference in the Core csproj.
- Opponent has no AI: no controller or Core instance for it; `OpponentX` is a fixed value; smoke asserts the opponent root and all parts are unchanged.
- No scope creep: no damage, stamina, health, score, round or timer code in the new files. No AnimationPlayer, shader or audio.
- Diff size: about 2.3k new lines (Core 570, adapters 330, smoke 340, tests 1000), above the ~400-line guide, reported by the developer; route was already full and G1-approved. Not a QA defect.

## Data and tuning spot check
- Frame counts and distances in Core all come from player-moves.json via `MoveSet.Parse`. The only literals in Core are state resets (0 or 1), the `1e-9` reach epsilon and the pose-blend 0/1 constants. Tests read the shipped JSON through TestData (the csproj links the file), so a changed number in the JSON changes the test target, not a hard-coded copy.
- Shipped numbers match the spec: jab 6/3/9 reach 1.10, cross 8/4/14 reach 1.25 (26 total, 1.44x jab), slips 3/15/12, guard raise 4 and speed x0.6, walk 1.6 m/s, minX -2.8, separation 0.75, contact text 45 ticks, tickRate 60 (equal to Godot's default; the controller checks it).
- `visual.punchExtendH` 0.30 -> 0.24 is a 20 percent reduction, within the spec's about 30 percent tolerance. Other visual values are unchanged. The smoke shows it still reaches the opponent head at gap == reach (jab ends 0.125 m from the opponent root vs head front 0.122 m; cross 0.132 vs 0.122, within the 0.03 m allowance).
- The cross glove travels `punchExtendH x H + (cross.reachM - jab.reachM)`, an adapter-side rule documented in the developer's notes. Fine under the spec (one tuning value for both punches).

## KB check
- Backticked paths in docs/kb all exist.
- docs/kb/input-map.md does not yet list the 7 new actions (fight_forward, fight_back, fight_jab, fight_cross, fight_guard, fight_slip_left, fight_slip_right). docs/kb was not changed on this branch. Per docs/process/workflow.md, kb-keeper (step 9) runs after QA, and the brief lists kb-keeper for this task. This is not a code defect, but the CI kb-check will fail until kb-keeper adds them. Producer: run kb-keeper before shipping, and give it the action table from project.godot, the handlers (`game/scripts/FightController.cs` + `FightController._PhysicsProcess`), and the new feature-map and ui-map entries (controls hint, contact label).

## Defects
None.

## Playtest checklist for the client (G3)
Keys are physical key positions on a QWERTY layout. The controls hint at the bottom of the screen shows them.
1. Start New Game, pick both boxers. The ring appears with the line `A / D  move    J  jab    K  cross    S  guard (hold)    Q / E  slip left / right    Esc  menu` at the bottom. Hold D: your boxer (left, "YOU") glides toward the opponent and stops about 0.75 m short without passing through. Hold A: it backs away and stops at the edge. Hold A and D together: no movement. Press the arrow keys: nothing happens.
2. At the start distance press J: the left (lead) glove shoots out and returns, and no "Contact" text appears. Hold D to close in, then press J and later K: "Contact: Jab" and "Contact: Cross" show at the top for under a second, the glove visibly reaches the opponent, and the opponent does not react.
3. Press K alone, then J alone: the cross feels a bit slower and heavier (body turns) than the jab, but not sluggish. Press J then quickly K: the cross follows the jab as a one-two. Mash J and K quickly: the boxer never throws faster than the moves allow.
4. Hold S: both gloves come up to cover the face and A / D walking is slower. While S is held J and K do nothing until you release. Release S: the guard drops at once. Hold J for 2 seconds: only one jab is thrown.
5. Press Q, then E: each dips the upper body and returns; Q leans away from the camera and E toward it, shoulders turn opposite ways, feet stay put. Press J or K as a slip comes back: the punch fires straight out of it. Pressing the other slip key mid-slip does nothing. Press Esc at any moment (mid-punch, mid-slip, while guarding): you are back at the menu. New Game and picking again starts a fresh fight with the boxer at the starting spot, Idle, with no leftover "Contact" text.

VERDICT: PASS
