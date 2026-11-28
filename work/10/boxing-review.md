# Boxing review: task 10 (player controls, spec docs/specs/player-controls.md)

Reviewer: boxing-consultant. Date: 2026-11-28. Sources: registry docs/boxing/sources.md, rows S1 to S9.

Verdict words: REALISTIC (matches real boxing), ABSTRACTION-OK (the game simplifies on purpose; fine),
SUGGEST-CHANGE (a specific change is proposed). These map to the registry words OK / ABSTRACTION / CHANGE.
Nothing here edits the spec or the balance file; the game-designer owns both.

Source limits, stated plainly:
- Tier 1 rulebooks were not needed: none of the six claims is about rules or scoring.
- The coach manuals (S1 to S3) say nothing about slip duration, and no peer-reviewed slip timing was found.
  The often-quoted "100 to 300 ms defensive window" is only a second-hand citation in a 2024 to 2026
  paper, so it is NOT used here.
- S2 and S3 are federation coach-education documents with no named individual credentials per page
  (credentials are stated in the registry rows). They are good for technique, not for numbers.
- Page numbers below are approximate (the PDFs were read as converted text).

## Findings on the six claims

| # | Spec item (Needs boxing-consultant) | Verdict | Note | Sources |
|---|---|---|---|---|
| 1 | Jab = lead hand, cross = rear hand, orthodox lead is the left | REALISTIC | Orthodox puts the left foot and left hand forward; the jab is the front-hand straight and the cross is the rear-hand straight. Spec's "GloveL is the lead" is right. | [S1][S3] |
| 2a | Cross reaches further | REALISTIC (direction), ABSTRACTION-OK (the numbers) | The rear hand has farther to travel than the jab, because it starts from the back of a sideways stance. Its power comes from a rear-leg push and hip and shoulder turn. No source gives a metre figure, so jab 1.10 m and cross 1.25 m are tuning values only. | [S1][S3][S8] |
| 2b | Cross is slower to start and recover than the jab | SUGGEST-CHANGE | The evidence does not support "clearly slower". Jab and cross both reach the target in about 0.10 to 0.14 s in one bag study, and the cross total was not longer there. In another study the jab had the shortest delivery of six punches, but the abstract does not give the cross figure. Real difference is small and mixed. Keep the cross as a heavier move for balance, but call it an abstraction and narrow the gap (see change C1). | [S4][S5][S6][S9] |
| 3 | Jab then cross (one-two) is fundamental, justifies jab -> cross cancel | REALISTIC | Coach material teaches the one-two as a quick jab, a pivot, then the rear straight, and lists lead-straight-then-rear-straight as a basic combination and drill. Research shows punches thrown in a combination have shorter delivery times than a single maximal punch, which supports the cancel feeling quicker than a standalone cross. | [S1][S2][S3][S4] |
| 4a | Slip moves head off the line of a straight punch | REALISTIC | The head moves to the side of the incoming punch. Coaches teach it against the jab and the straight rear punch. | [S2][S3] |
| 4b | "by bending at the knees and waist" | ABSTRACTION-OK (source pending for slip itself) | Allowed sources describe knee bend and waist turn for ducking, rotation and the bob-and-weave, and not for the slip by name. Do not state knee and waist as a fact about the slip; the visual (upper body dips and leans, feet planted) is a reasonable look. | [S3] |
| 4c | Slip leaves the boxer set to counter; Slip Recovery -> Jab/Cross cancel | REALISTIC | Federation material teaches "defend, then counter right away", with a lead or rear straight thrown as the head moves away, and "slip and counter with both hands free". | [S2][S3] |
| 4d | Slip Active (evade) lasts 15 ticks, total 0.5 s | ABSTRACTION-OK | No allowed source gives slip durations. One study concludes defenders have more time to evade a punch than older estimates said, which fits a generous window, but gives no number in its abstract. | [S4] |
| 5a | A high guard with both gloves by the face protects the head | REALISTIC | The standard guard keeps hands up, chin down, elbows by the ribs. A tighter cover (both forearms and fists in front, chin down) is taught against straight punches. | [S1][S2][S3] |
| 5b | Punching opens the guard / punches cannot be thrown from held guard | ABSTRACTION-OK, wording change | In real boxing only the punching arm leaves the guard; the other hand stays up and the punching hand returns on the same line. So the opening is one-sided and brief, not a full guard drop. The game's "no punching while guard is held" is a deliberate simplification. | [S1][S2] |
| 6 | Real-world timing scale | See table below | Throw to contact about 0.10 to 0.14 s; fist out and back about 0.4 to 0.5 s; game jab startup matches, game jab total is quicker than real. | [S5][S6][S7] |

## Timing comparison (claim 6)

| Quantity | Real (source) | Game (player-moves.json) | Reading |
|---|---|---|---|
| Throw, start to first contact | jab about 0.12 s, cross about 0.11 s (experienced boxers at a bag, sensor glove) [S6] | jab startup 6 ticks = 0.10 s; cross startup 10 ticks = 0.17 s | Jab matches. Cross startup is about 50 percent longer than measured (abstraction, see C1). |
| Fist out, contact and back (whole punch) | about 0.40 to 0.52 s (jab 0.52, cross 0.40, experienced) [S6]; jab delivery about 0.4 s in another amateur sample [S5] | jab 18 ticks = 0.30 s; cross 30 ticks = 0.50 s | Cross total is in the real range. Jab total is shorter than real (snappier; abstraction). |
| Contact on the target | about 0.02 to 0.03 s, cross slightly longer than jab [S7] | Active 3 and 4 ticks = 0.05 and 0.07 s | Game "Active" is a rule window, not a measured contact. Abstraction. |
| Jab vs cross speed at contact | both slower than hooks (about 8 to 9 m/s for straights) [S4][S8] | not modelled | n/a |

Caution on S5: its abstract prints the jab delivery as "405 ± 0.15 ms", an SD that looks like a unit slip. Use only "about 0.4 s, shortest of six punch types".
The S6 numbers are for a hit on a bag with a sensor, so "total" includes the contact and the return; they are
not the same measurement as S5.

## Suggested changes (game terms, for the game-designer to decide)

- **C1 (claim 2b). Narrow the jab/cross gap.** Real startup is nearly equal. Suggested: cross startup 7 or 8 ticks
  (not 10), cross recovery about 12 to 14 (not 16), so cross total is about 24 to 26 ticks against the jab's
  18 (ratio about 1.4, down from 1.67). Optional: lift jab recovery from 9 to 12 so the jab total is about 0.35 s,
  closer to real. If the designer wants the current feel, keep the values and just rewrite the abstraction line
  (see A2); nothing in AC2 or AC3 breaks either way, since AC3 only needs cross total > jab total.
- **C2 (claim 3, optional).** Because combination punches are delivered faster than single ones [S4], the cancelled
  cross after a jab could start with a shorter startup than a standalone cross (for example 10 -> 7 ticks). This
  is a feel option, not a realism requirement. Changing it affects AC6, so only do it if the designer wants it.
- **C3 (spec wording, claim 2b and 4b).** Remove "because of hip and shoulder rotation" as the reason the cross is
  slower, and remove "bending at the knees and waist" as a stated slip fact. Hip and shoulder rotation is the
  source of the cross's power and extra reach [S1][S3], not a proven cause of lower speed.
- **C4 (guard wording, claim 5b).** The current line "punching opens the guard" should read: in real boxing the
  punching hand leaves the guard while the other hand stays up. The game closes both hands because guard is a
  separate, committed state.

## Notes for later tasks (no action now)

- Slip direction matters in real boxing: slipping to the wrong side of a straight punch exposes the boxer to the
  other hand, and the correct side differs for a jab and for a rear straight [S2]. When opponent attacks arrive,
  one slip direction will evade only one of them. Expect the designer to decide whether the one-direction slip
  evades both jab and cross, or only one, and to mark it as an abstraction.
- Slip answers straight punches. Federation material teaches ducking and rotation (knee bend) for hooks [S3], so a
  slip should not evade hooks when hooks are added.
- Real boxers step with the jab ("stepping in" is a coach-reported theme) [S9]; the game's "no walking while
  punching" is the abstraction below.
- A real double jab exists as a basic combination [S1]. The game cannot chain jab -> jab (spec out of scope), see A8.

## GAMEPLAY ABSTRACTION lines (all listed, for the spec's Boxing notes)

- A1. GAMEPLAY ABSTRACTION: the jab and cross have fixed tick timings chosen for rhythm instead of measured durations (throw about 0.1 s, whole punch about 0.4 to 0.5 s [S5][S6]) — readable startup/active/recovery beats; the jab total is deliberately quicker than real.
- A2. GAMEPLAY ABSTRACTION: the cross is clearly slower than the jab instead of nearly equal in speed [S4][S5][S6] — gives the cross a risk-versus-reward cost with only two punches.
- A3. GAMEPLAY ABSTRACTION: reach is a root-to-root distance with jab 1.10 m and cross 1.25 m instead of arm length, stance and step length [S1][S3] — a single readable range rule; the real cross does reach further [S1].
- A4. GAMEPLAY ABSTRACTION: the boxer cannot walk while punching or slipping instead of stepping with the punch [S9] — committed moves, simple controls.
- A5. GAMEPLAY ABSTRACTION: held guard is a separate state with no punching instead of punching out of the guard with the free hand kept up [S1][S2] — gives guard a clear cost and keeps four distinct moves.
- A6. GAMEPLAY ABSTRACTION: the idle pose is the stance guard and "Guard" is a tighter cover with a 4-tick raise instead of one continuous guard that is always up [S1][S3] — a visible state the player controls.
- A7. GAMEPLAY ABSTRACTION: one slip direction on one key instead of a side chosen against each punch [S2] — small key set and a side-on camera.
- A8. GAMEPLAY ABSTRACTION: the only chains are jab -> cross and slip -> punch instead of the wider set (double jab, jab to body, hooks) [S1][S3] — M1 has four moves; combos come later.
- A9. GAMEPLAY ABSTRACTION: walking is a smooth glide along one axis instead of step-and-drag footwork and pivots (the cross uses a rear-leg push and pivot [S1][S8]) — roadmap M1 limits footwork; walk speed 1.6 m/s has no source.
- A10. GAMEPLAY ABSTRACTION: all boxers are orthodox with identical reach and timings instead of southpaws and varied builds [S3] — stats are display-only until combat tuning.
- A11. GAMEPLAY ABSTRACTION: a punch "contact" is a distance check on root positions with no target, damage or reaction instead of a glove landing on a head or body [S7] — opponent is idle in this task.
- A12. GAMEPLAY ABSTRACTION: no input buffering and no cancel out of Startup or Active instead of real boxers adapting a punch mid-flight — readable, mash-proof rhythm (vision pillar).

## Count

REALISTIC / OK: 6 findings (1, 2a direction, 3, 4a, 4c, 5a). ABSTRACTION-OK: 4 findings (2a numbers, 4b, 4d, 5b).
SUGGEST-CHANGE: 1 finding (2b), with 4 proposed changes (C1 to C4). Abstraction lines: 12.
Claim 6 delivered as a comparison table.

QUESTIONS (for the client): none.
ESCALATE: game-designer: decide C1/C2 (jab/cross gap and the cancel startup) and whether the single slip direction
evades both jab and cross once opponent attacks exist.
