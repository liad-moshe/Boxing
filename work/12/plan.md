# Plan: #12 stance (orthodox / southpaw), developer

Each item: failing test first, then code, then `dotnet test tests/Core.Tests` green.

- [x] 1. `Stance` + `StanceRules` (LeadSide/RearSide/PunchSide/MirrorSign/Parse/DisplayName) in src/Core. Test: AC17 helper part.
- [x] 2. `MoveSet.DefaultStance` from `stance.default`; bad/missing value throws naming `stance.default`. Test: AC1 (stance), shipped = orthodox.
- [x] 3. `PoseBlend.From(fighter, moves, stance)`: Left/Right extend, signed TorsoTurn; southpaw = mirrored; slip/guard identical. Update PoseBlendTests. Test: AC17 pose part, every tick of Idle/Jab/Cross/GuardRaising/Guarding/slips.
- [x] 4. Timelines identical whatever the stance (PlayerFighter has no stance input). Test: AC18 last clause, AC26 (one-two ticks).
- [x] 5. `CharacterSelectFlow` stances: PlayerStance/OpponentStance/ToggleStance/Back/Done, ctor default. Test: AC18.
- [x] 6. Adapters: GameSession stances; Main fallback + Tag text; FighterBuilder lead foot by stance; FighterAnimator by anatomical side + signed turn + cross extra reach on the cross hand; FightController passes stance.
- [x] 7. CharacterSelect: stance line (no focus, click toggles, Up/Down toggles via menu_up/menu_down, "You:" line shows stance), hand-off to GameSession.
- [x] 8. Smoke: select-screen stance line (AC19/20 where practical), ring orthodox (AC24, AC30) and southpaw (AC21, AC25, AC27) passes.
- [x] 9. Run all four checks; commit.
