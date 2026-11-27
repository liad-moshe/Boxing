# Plan: humanoid look (issue #5 playtest fix)
- [x] 1. Tests (AC29): shipped look has hair/waistband/8 scales, body present; bad colour, non-positive scale, missing body key throw naming the problem. Test: FighterRosterTests
- [x] 2. Core: extend FighterLook, add FighterBody, parse and validate in FighterRoster. Test: dotnet test
- [x] 3. Rewrite FighterBuilder with the named parts (AC24, AC25, AC27, AC28 geometry). Test: build
- [x] 4. Smoke checks AC24-AC28 in Smoke.cs. Test: godot smoke
- [x] 5. Run all four checks from docs/process/commands.md
