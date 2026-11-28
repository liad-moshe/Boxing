# Boxing review: task 12 (left and right jab and cross)

Reviewer: boxing-consultant. Date: 2026-11-28. Spec: docs/specs/player-controls.md (#12 amendment). Data: data/balance/player-moves.json.

## Evidence limits (read first)
- Sources S1, S2, S3 and a Boxing Canada Club Coach manual are PDFs. In this session the fetch tool returned raw PDF data, so I could **not** re-read them or search them for "southpaw", "double jab", "rear jab" or "lead straight". I cite S1-S3 only for claims already reviewed in #10 (jab = lead straight, cross = rear straight, one-two, only the punching hand leaves the guard).
- New source **S10** (peer-reviewed, 2026) covers lead/rear versus orthodox/southpaw and the jab/cross mechanics. It is the main answer to B1, B4 and B8.
- **No allowed source found** that names a rear-hand jab, a lead-hand power cross, a mirrored one-two, or a slip-side/counter pairing. I do not claim they do or do not exist in coaching practice. They stay GAMEPLAY ABSTRACTION. I did not find a source that makes them unsafe to keep either.

## Findings
| # | Spec item | Verdict | Note | Sources |
|---|---|---|---|---|
| 1 | B1 Jab and cross are defined by lead vs rear hand | OK | Confirmed. In both orthodox and southpaw the jab is the lead hand and the cross the rear hand. Orthodox leads with the left hand and foot; southpaw is the mirror (right lead). The words name the hand's role, not left or right. | [S10][S6][S1][S3] |
| 2 | B1 Spec text "jab is lead, cross is rear" (Boxing notes, first bullet) | OK | Add [S10] for the southpaw half. The old bullet only covers orthodox. | [S10] |
| 3 | B2 Right jab (rear-hand quick straight) | ABSTRACTION | No allowed source names a rear-hand jab. The game's right jab is a jab-speed straight from the rear hand. Keep it marked. | [S10][S1][S3] |
| 4 | B2 Should `rightJab` timing or reach differ from `leftJab`? | OK (keep equal for M1) | No source gives per-hand timings. Peer-reviewed data say fist acceleration did not differ between stances for any punch, and lead-vs-rear differences are in force, not speed. Equal numbers are defensible. Optional later tuning: a rear-hand punch has farther to travel than the lead [S1][S3], so a slightly longer startup or shorter reach for `rightJab` is plausible, but unmeasured; do not tune on a claim. | [S10][S7][S1][S3] |
| 5 | B3 Left cross (lead-hand rotating power straight) | ABSTRACTION | No allowed source names a lead-hand cross or "power jab". The cross is the rear-hand punch by definition, so a left cross from an orthodox stance is the game's invention. Keep marked. | [S10][S1][S3] |
| 6 | B3 Should `leftCross` timing or reach differ from `rightCross`? | OK (keep equal for M1), with note | Rear-hand cross shows higher force and fist acceleration than the lead jab [S7]; stance changes force but not fist acceleration [S10]. Nothing sourced supports a per-hand timing change. If the designer wants a gameplay difference later, make it damage or power (data-only), not ticks. | [S7][S10] |
| 7 | B4 Keep all boxers orthodox; right jab / left cross as abstractions | OK | Confirmed as acceptable. A boxer leads with the right hand by being a southpaw (mirror stance, right hand and foot forward) [S10]. In S10 the same boxers performed both stances, so switching is a real act, not a click. A later "switch stance" task is the realistic route. | [S10] |
| 8 | B4 Naming under a later stance feature | CHANGE (note for designer) | If stance is added later, "jab" and "cross" must follow lead and rear, not left and right. The left glove in a southpaw stance would throw the cross [S10]. The #12 names (LeftJab, RightCross) are glove-based, so a later task will need a rename or a stance-aware label. Not needed now. | [S10] |
| 9 | B4 Stance effects | ABSTRACTION | Real stance changes force by punch (jab higher in southpaw, cross higher in orthodox in one sample of right-handed boxers) and gives no universal advantage [S10]. The game uses no stance effects. Not a reason to change anything now. | [S10] |
| 10 | B5 Mirrored one-two (RightJab -> LeftCross) | ABSTRACTION | No allowed source teaches a rear-then-lead pair. The classic one-two is lead straight then rear straight [S1][S2][S3]. Keep the cancel, marked, because the client wants symmetry. It is data-only, so it can be removed from `rightJab.cancelInto` if playtest shows confusion. | [S1][S2][S3] |
| 11 | B6 No same-hand jab -> cross and no jab -> jab | CHANGE (wording) | The reason given ("only the punching hand leaves the guard and returns along the same line") does not forbid a same-hand follow-up: the same hand can throw again. The restriction is a scope and readability choice. State it as ABSTRACTION. The #10-reviewed line already notes real combinations include a double jab [S1][S3]. A same-hand double is a reasonable later task. | [S1][S2][S3] |
| 12 | B6 Other-hand cancel rule | OK | A cancel into the other hand matches only the punching hand leaving the guard while the other hand stays up [S1][S2]. | [S1][S2] |
| 13 | B7 Any of four punches may cancel a slip's Recovery | ABSTRACTION | A counter right after a defence is taught [S2][S3], and the slip side depends on the incoming punch [S2]. No allowed source I could verify ranks hand-and-side pairings, so I cannot call any pairing unrealistic. Keep all four; mark as abstraction; revisit in the opponent-attack spec, where side-to-punch pairing and a boxing review are already planned. | [S2][S3] |
| 14 | B8 Left jab with no torso turn | OK | The jab relies mainly on the front-leg push with slight torso involvement. | [S10] |
| 15 | B8 Right cross and left cross turn up to 25 deg | OK | The cross relies on hip and torso rotation driven from the rear leg [S10][S9]. The mirrored left cross turn is a visual choice (see 16). | [S10][S9] |
| 16 | B8 Left cross turns the left shoulder forward (mirror) | ABSTRACTION | No allowed source describes the body action of a lead-hand rotating straight. A mirror image is plausible geometry but unsourced: a visual choice only. | [S10] |
| 17 | B8 Right jab small turn (`rearJabTurnDeg` 10) | ABSTRACTION | The jab has slight torso involvement [S10], so a small turn reads plausible; a rear-hand jab itself is unsourced (see 3). Visual choice only. | [S10] |
| 18 | Boxing notes, bullet 2: cross power and reach "from hip and shoulder turn [S1][S3][S8]" | CHANGE | S8 does not say that. It describes the cross as forward body translation with a straight, piston-like arm and a modest trunk share (about 7 percent elite, 16 percent junior), and pelvis contribution only for the hook. Remove [S8] from the rotation claim; keep [S1][S3] and add [S10]. S8 stays valid for the rear-leg drive and forward travel. | [S8][S10] |
| 19 | Tick timings | OK | No per-hand change. Keep leftJab = rightJab = 6/3/9, leftCross = rightCross = 8/4/14. The #10 timing abstractions still apply. Real throw-to-contact is 0.11-0.14 s for both punches [S6]. | [S6] |
| 20 | Contact text and "hand changes only which glove moves" | OK | No boxing claim. | none |

## Recommendations to the designer
1. **Keep** all numbers in player-moves.json unchanged. Per-hand timing differences have no allowed source; equal ticks are supportable (finding 4, 6).
2. **Change** the spec wording in three places: add [S10] to the lead/rear bullet (2); drop [S8] from the rotation sentence and add [S10] (18); re-describe the same-hand restriction as a game choice, not a boxing rule (11).
3. **Keep and mark** as ABSTRACTION (add or tighten the lines below): rear-hand jab, lead-hand cross, mirrored one-two, mirrored torso turns, any-punch-after-slip.
4. **Do not** add southpaw or stance switching to this task. If planned later, name punches by lead and rear (8).
5. Tell the client that the right jab and left cross are game inventions, and that "jab" and "cross" in real boxing follow the hand's role. This is safe to explain, not a blocker.

## GAMEPLAY ABSTRACTION lines (proposed replacements and additions)
GAMEPLAY ABSTRACTION: the player can throw a jab-speed straight with the rear hand (right jab) and a rotating straight with the lead hand (left cross) from an orthodox stance instead of the jab being the lead hand's punch and the cross the rear hand's, in either stance [S10][S1][S3] — client request for a choice of hand on both punches; no allowed source names either punch.
GAMEPLAY ABSTRACTION: the left and right version of each punch share the same ticks and reach instead of lead and rear hands differing in travel and force [S1][S3][S7] — no allowed source gives per-hand timings, and fist acceleration did not differ by stance [S10]; per-hand tuning is data-only later.
GAMEPLAY ABSTRACTION: the stance never changes when the player uses the right glove first instead of switching to southpaw to lead with the right hand [S10] — stance switching is out of scope.
GAMEPLAY ABSTRACTION: the mirrored one-two (RightJab -> LeftCross) is allowed instead of only lead straight then rear straight [S1][S2][S3] — keeps the two hands symmetrical; removable by data.
GAMEPLAY ABSTRACTION: a jab cannot cancel into a same-hand cross or another jab instead of real combinations that repeat a hand, such as the double jab [S1][S3] — M1 has few moves; same-hand repeats come later.
GAMEPLAY ABSTRACTION: any of the four punches may cancel a slip's Recovery regardless of slip side instead of a counter chosen for the slip and the incoming punch [S2][S3] — no opponent attacks yet.
GAMEPLAY ABSTRACTION: the left cross and right jab torso turns are mirrored or scaled visual choices instead of described body actions [S10] — readability from the side-on camera.

## Open points
- Two checks would need a text-readable copy of S1-S3 or the Boxing Canada Club Coach manual (a coach credential for it is also unconfirmed): a coach-manual statement on southpaw, and one on the double jab. If the designer or client wants these cited as fact rather than as the #10 carry-over, a later session with a PDF text tool should re-read them.
