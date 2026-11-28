# Plan 12 item 2: guard pose (developer)
- [ ] 1. MoveSet parses guardGloveBackH, guardElbowRaiseShare, guardElbowInwardShare (>=0) and guardGloveRaiseH >= 0.08; error names key (AC33 parse) -- MoveSetTests
- [ ] 2. Core GuardMove: glove/elbow offsets from VisualTuning, H, W, blend (AC31 raise >= 0.8x, AC32 monotonic/zero at 0, AC33 elbow shares, AC34 mirror) -- GuardMoveTests
- [ ] 3. FighterAnimator uses GuardMove (removes GuardElbowShare constant)
- [ ] 4. Smoke: guard height within head span, ahead of head, limb length within 15%, both stances
- [ ] 5. Four checks, commit
