# Boxing answer: which punches can follow a slip? (task 12, item 1)

Question (client): after a slip left or right the game allows an immediate jab or cross. In real boxing, can a
boxer punch toward the other side of the slip? Should the punch or glove depend on slip side and stance?

No new registry rows were needed: everything below comes from [S2] and [S3], re-read this session through a text
conversion of the publisher PDFs (page numbers approximate). [S1] (beginner manual) was only partly readable
(text cut off before its counter-punching section) and gives nothing on slips.

## Short answer
Yes. Real boxers can throw either hand out of a slip. The coaching material does not forbid or require a glove
for a given slip side. It does show that the side of the slip changes which counters are taught and how safe
they are. So the game's "any slip into jab or cross" is a simplification, not an error.

## What is verified (Tier 3 only)
1. Slip direction depends on the incoming punch and the danger of the other hand. [S2] (Common Errors, about
   p.38) uses a right-handed opponent's left jab as the example: slip to the right, because slipping left
   exposes the boxer to the opponent's right hand [S2]. The bullet does not state the defender's own stance;
   for an orthodox defender, right is the rear-foot side (my reading of the manual's foot terms, not stated
   directly).
2. The manual teaches a slip toward the FRONT-foot side, inside the opponent's jab, followed by a lead-hand
   hook to the head [S2] (section 3.16, about pp.59-60). The same slip toward the front-foot side against the
   jab leads to: a lead-hand hook to the body then the head (Double Hook, about pp.47-48); a rear-hand uppercut
   to the body then a lead-hand hook to the head (about pp.52-53); and a lead hook, rear uppercut, lead hook
   chain (about pp.53-54) [S2]. In these counters the first or last punch is the hand on the same side as the
   slip, with a rear-hand body uppercut as the alternative. None of them is a jab or a cross.
3. A slip toward the REAR-foot side exists as a taught skill against a straight power (rear-hand) punch to
   the head, listed as an intermediate-level skill [S2] (about p.107), and a boxer who draws a straight power
   punch is told to be ready to slip to the front or the rear foot side with both hands free to counter [S2]
   (about p.70). The manual's worked counter to the rear straight punch is a lead-hand parry followed by a
   straight power punch, not a slip counter [S2] (section 3.16, about pp.57-58). I found no written counter
   for a rear-side slip.
4. Counters are by any punch. AIBA's coaches manual says every basic defence should be followed by a counter
   with a basic punch, and that any punch can be a counter [S3] (about pp.41, 56, 76-77). It does NOT name
   which hand answers which defence [S3].
5. The AIBA manual uses no "inside" or "outside" and never the word "slip". Its defences (ducks, rotations,
   sway, parry) are described as toward the boxer's lead side or rear side, so for a southpaw they simply
   reverse [S3] (about pp.57-68). Neither manual gives separate southpaw slip instructions [S2][S3].
6. In both manuals the straight punches are the lead hand (jab) and the rear hand (cross), the same roles in
   either stance [S3]; the stance study defines them the same way [S10].

## What is NOT verified (no allowed source found)
- Which glove is "right" after an outside (rear-side) slip, such as a cross over the jab, or after an inside
  slip, such as a jab or cross straight from the slip. Widely repeated in blogs and forums, which are not
  sources. Not stated as fact.
- Any claim that a southpaw slips or counters differently from a mirror image of an orthodox boxer. Only
  the manual's lead/rear wording supports "mirror" [S3].
- Any timing for a counter after a slip. The registry only has punch durations [S5][S6], not slip-to-counter
  times.
- The teaching sequence for beginners. [S1]'s counter-punching section could not be read.

## Reading for the client's "other side" question
- Slip to the lead-hand side (inside the jab, in orthodox: left): the taught counters are the lead hook and
  the rear uppercut [S2]. A cross from there is not taught in these manuals; the jab is not either.
- Slip to the rear-hand side (outside, in orthodox: right): taught as a skill [S2], with no counter named.
- So "punch toward the other side of the slip" is physically possible for a straight punch (the straight
  punches go forward, not sideways), but the manuals favour punches that continue the slip's turn (hooks,
  uppercuts). This is a teaching preference, not a rule. It does not make a jab or cross illegal or wrong.

## Recommendation for the designer
Keep as GAMEPLAY ABSTRACTION in M1; do not restrict pairings now. Reasons: no allowed source forbids a
slip-to-jab or slip-to-cross, there is no opponent attack to make the side matter, and a restriction would
need sources we do not have. Revisit when the opponent-attack spec decides which slip side evades which punch
(already deferred in the spec). At that point, new restrictions could be:
- A: a slip must match the incoming punch's side (data: [S2] item 1), mirrored for southpaw by boxer-relative
  sides. This is a defence rule, not a counter rule.
- B: if hooks come later, offer a lead hook and rear body uppercut as slip counters toward the lead side [S2].
Both would be new issues with a boxing review. Stance should not change any of this beyond the mirror the
spec already applies.

GAMEPLAY ABSTRACTION: both slip sides can cancel into the jab or the cross instead of mostly continuing with the
lead hook or rear uppercut after a front-foot-side slip [S2] — M1 has only two punches and a simple, learnable rhythm.
GAMEPLAY ABSTRACTION: the same cancel is allowed after either side with no link to the incoming punch, instead of
choosing the slip side by the opponent's punch and exposed hand [S2] — no opponent attacks yet.
GAMEPLAY ABSTRACTION: orthodox and southpaw cancel identically (slip then lead or rear straight) instead of
written stance-specific guidance, which the manuals do not give [S2][S3] — keeps the spec's mirror rule.

## Suggested spec wording (for the designer; spec not edited)
"A boxer can throw either hand out of a slip; the manuals teach the slip's turn into lead hooks and rear
uppercuts more than straights [S2]. M1 allows jab and cross from both slips as an abstraction."
