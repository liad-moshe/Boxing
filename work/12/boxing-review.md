# Boxing review: task 12 (stance: orthodox and southpaw, jab = lead, cross = rear)

Note: the earlier 4-key review (left/right jab and cross) is superseded by the G2 rescope. Only its still-valid points are carried (marked "carried").

Reviewer: boxing-consultant. Date: 2026-11-28. Spec: docs/specs/player-controls.md (stance version, "Boxing notes"). Data: data/balance/player-moves.json. New registry row: S11.

## Evidence limits (read first)
- **S10** (peer-reviewed, 2026) is the main source: it defines orthodox and southpaw, jab = lead hand, cross = rear hand, and compares both stances in 30 trained right-handed boxers [S10].
- **S11** (new, peer-reviewed, 2014) gives the real-world picture of stance as a boxer's usual style: about 75 percent orthodox and 25 percent southpaw among 340 top professionals, and a matchup effect [S11].
- **S3 and the other coach PDFs (S1, S2):** the fetch tool returned raw PDF bytes again, so I could not search them for "southpaw". The registry says S3 was used for the orthodox stance; I have **no verified southpaw passage in S3**. I do not cite S3 for any southpaw claim.
- S10 and S11 were read at abstract / web-page level only (see registry notes). No full text was read for either.

## Findings
| # | Spec item | Verdict | Note | Sources |
|---|---|---|---|---|
| 1 | Q1 Orthodox = left hand and left foot lead; southpaw = mirror (right hand and foot lead) | OK | S10 states this definition. | [S10] |
| 2 | Q1 Same claim cited as [S10][S3] (spec lines 8, 57, 201) | CHANGE | S3 is verified only for the orthodox stance, not southpaw. Cite [S10] for the definition and for the southpaw half. Keep [S3] only on orthodox-only statements, or drop it from these three places. | [S10][S3] |
| 3 | Lead hand and lead foot "on the side nearer the opponent" | OK | Standard meaning of lead in S10's definition; the spec's own geometry. | [S10] |
| 4 | Q2 Jab = lead hand, cross = rear hand, in both stances (southpaw: right jab, left cross) | OK | S10 names the jab as the lead-hand and the cross as the rear-hand punch in either stance. The words follow the hand's role, not left or right. | [S10] |
| 5 | Q2 One-two in southpaw = right jab then left cross | OK, reword | The one-two is taught as lead straight then rear straight [S1][S2]; I could not confirm the coach manuals word it for southpaw. Southpaw right-then-left follows from the role definition [S10]. Write the rule by role ("lead straight, then rear straight") and cite S10 for the southpaw mapping, S1/S2 for the one-two itself. Do not claim S1/S2 say "southpaw". | [S10][S1][S2] |
| 6 | Q3 Southpaw cross torso turn as an exact mirror (left shoulder forward) | ABSTRACTION (as written) | S10 says the cross relies on hip and torso rotation from the rear leg, and defines southpaw as the mirror stance. That supports "rotation, rear side drives". It does not state which shoulder ends forward or how many degrees. Keep it as the spec's existing abstraction line. The 25 degrees is a tuning value. | [S10] |
| 7 | Q3 Same, for the orthodox cross (right shoulder forward) | OK (carried, visual choice) | Rotation from the rear leg is sourced [S10][S9]; the exact pose is a visual choice. | [S10][S9] |
| 8 | Southpaw build and guard poses as exact reflections | ABSTRACTION | The mirror stance is S10's own definition, so the layout is sound. A real southpaw's body action is not described as an exact reflection. The spec's line is right. | [S10] |
| 9 | Jab with no torso turn, both stances | OK | The jab relies mainly on the front-leg push with slight torso involvement, so "no turn" is a simplification of "slight". See finding 10. | [S10] |
| 10 | Jab no torso turn (visual) | ABSTRACTION | "Slight torso involvement" becomes none. Fine for readability; add a few words to the carried line if the designer wants it explicit. | [S10] |
| 11 | Q4 Stance chosen per boxer in Character Select, for both player and opponent | ABSTRACTION (fair) | Real boxers fight from one usual stance: southpaw is common among left-handers, orthodox among right-handers, and about 25 percent of top pros are southpaw [S11]. A free pre-fight pick for both sides is a game choice. The orthodox default is realistic, since orthodox is the majority [S11]. | [S11][S10] |
| 12 | Q4 Existing line "any boxer may be either stance, with no link to handedness or stats" | CHANGE (wording) | Add [S11] and say what real boxing does: a boxer usually has one trained stance tied to handedness. Suggested: "...instead of a boxer's own usual stance, which is often linked to handedness [S11]". | [S11] |
| 13 | Q5 "Trained boxers can throw the basic punches from both stances" | CHANGE | Too general. S10's sample is 30 trained, all right-handed boxers who each threw jab, cross, lead hook and rear hook in both stances, orthodox first then southpaw, not randomised [S10]. Say that, and that the authors suggest training both stances may add versatility [S10]. Do not say all boxers can. | [S10] |
| 14 | Stance changes force (jab higher in southpaw, cross higher in orthodox in that sample) but not fist acceleration | OK | Matches S10's reported results. Caveat: all right-handed, so the pattern may differ for left-handers; do not use it as a tuning basis. | [S10] |
| 15 | "Stance changes no number" (timings, reach, cancels, power) | ABSTRACTION | Fist acceleration did not differ by stance, so equal timings are supportable. Force did differ [S10], so equal power is the simplification. Existing line covers it. | [S10] |
| 16 | Same tick timings and reach for both stances | OK | No allowed source gives per-stance timings. | [S10] |
| 17 | Q6 Slips stay on the boxer's own left and right; slip-left is toward the lead side in orthodox and the rear side in southpaw | OK | Pure geometry, no boxing claim. Slip direction in real boxing depends on the incoming punch [S2]. No source supplied that links slip side to stance. Keep deferred to the opponent-attack spec and review it then; do not claim a pairing now. | [S2] |
| 18 | Q6 Slip-recovery cancels into jab or cross by role, both stances | OK | Counter after a defence is taught [S2][S3]; choosing jab (lead) or cross (rear) is by role, so it is stance-neutral. Any slip-side-to-counter pairing is deferred. | [S2][S3] |
| 19 | Q7 Orthodox-versus-southpaw mismatch not modelled | ABSTRACTION | Real mismatch exists: both stances won a higher share against orthodox opponents than against southpaws, though stance had no effect on overall win rate [S11]. No objection to leaving it out while the opponent is idle. Add one abstraction line so the omission is on record (below). | [S11] |
| 20 | Both boxers may pick either stance, same boxer with both stances | ABSTRACTION | Covered by finding 12. Same boxer in different stances is a game choice only. | [S11] |
| 21 | Character Select tags `(Orthodox)` / `(Southpaw)` and stance line text | OK | Naming only. Terms match S10 and S11. | [S10][S11] |
| 22 | Tick timings, reach, `data/balance/player-moves.json` values | OK (carried) | No change. Throw to contact is about 0.11 to 0.14 s [S6]. Existing #10 abstractions still apply. | [S6] |
| 23 | Carried: cross rotation is not cited to S8; same-hand double jab restriction worded as scope choice | OK (carried) | The spec already applies both corrections. | [S8][S10] |
| 24 | Spec "Boxing notes" header says the 4-key review is "partly superseded" | CHANGE (housekeeping) | After this review, say the stance version was reviewed on 2026-11-28 (work/12/boxing-review.md) and the 4-key review is superseded. | none |
| 25 | Answers still unverifiable | Not verified | Whether S3, S1 or S2 text discusses southpaw. Needs a text-readable copy of those PDFs. Nothing in the design depends on it once S3 is removed from the southpaw claims. | none |

## Answers to the 7 Boxing questions
1. **Citation of the stance definitions:** correct for [S10]. S3 is only verified for orthodox, so cite S10 alone for the southpaw half (findings 1, 2).
2. **Southpaw jab = right, cross = left, one-two right then left:** correct by role [S10]. Word the one-two rule by role; S1/S2 are not verified to say it for southpaw (4, 5).
3. **Mirrored cross torso turn:** acceptable only as an abstraction. S10 supports rotation from the rear leg, not the exact mirrored shoulder or angle (6, 8).
4. **Per-boxer stance in Character Select:** a fair pre-fight choice. Add that real boxers have one usual stance tied to handedness and about a quarter of top pros are southpaw [S11] (11, 12).
5. **"Trained boxers can throw from both stances":** narrow it to what S10 showed: 30 trained right-handed boxers did it in a lab protocol (13).
6. **Slip-left toward the rear side in southpaw:** no claim needed now; keep deferred (17).
7. **Mismatches unmodelled:** no objection; record one abstraction line (19).

## Recommendations to the designer
1. Replace [S10][S3] with [S10] on the southpaw definition and lead/rear claims (spec lines 8, 57, 201). Keep S3 for orthodox-only statements.
2. Reword Boxing-notes bullet "Trained boxers can throw..." to the S10-scoped version (13).
3. Add [S11] to the "any boxer may be either stance" abstraction and say a real boxer usually has one usual stance linked to handedness (12).
4. Add the mismatch abstraction line below.
5. Word the one-two by role in both places (5).
6. Keep every number in player-moves.json as it is. No timing, reach or cancel change is needed.
7. Optional: note in the cross-turn line that S10 does not state the shoulder or the angle (6).

## GAMEPLAY ABSTRACTION lines (proposed replacements and additions)
GAMEPLAY ABSTRACTION: any boxer may take either stance by a free pre-fight choice, with no link to handedness or stats, instead of a boxer's own usual stance, which is often linked to handedness and is about one in four among top professionals [S11] — client request for a free choice; stats are display-only.
GAMEPLAY ABSTRACTION: orthodox against southpaw has no special effect instead of a real mismatch in which both stances won more of their fights against orthodox opponents than against southpaws [S11] — the opponent is idle and stance changes no number; matchup effects can be data-only later.
GAMEPLAY ABSTRACTION: the southpaw build, punch poses and cross torso turn are exact reflections of the orthodox ones instead of a real southpaw's own body action [S10] — S10 defines southpaw as the mirror stance but does not describe the body action as an exact reflection; readability.
GAMEPLAY ABSTRACTION: the jab has no torso turn instead of slight torso involvement [S10] — readability from the side-on camera.
GAMEPLAY ABSTRACTION: stance changes no timing, reach or power instead of changing punch force by punch type [S10] — one readable rule set; stance effects can be data-only later.
GAMEPLAY ABSTRACTION: stance is fixed for the whole fight instead of a stance the boxer can change during a bout [S10] — simple controls; an in-fight switch is a later task.

## Open points
- **Unverified:** whether S1, S2 or S3 mention southpaw. Needs a text-readable copy. Not blocking.
- S10 and S11 were read at abstract / web-page level only. The statements used here are the abstract-level ones (definitions, sample, force and acceleration result, stance share, mismatch result).
- No allowed source was found for slip side versus stance. It stays deferred to the opponent-attack spec.
