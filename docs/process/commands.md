# Commands — build, test, smoke, export

The exact commands every role and CI use. Run them from the repo root in Git Bash.
`godot` must be the 4.7.2 .NET build on PATH (SETUP.md).
This file is protected: agents never edit it.

## The four checks (in this order)
| # | Command | Pass when |
|---|---|---|
| 1 | `dotnet build Boxing.sln -warnaserror` | exit code 0, "0 Warning(s)", "0 Error(s)" |
| 2 | `dotnet test tests/Core.Tests` | exit code 0, "Failed: 0" |
| 3 | `godot --headless --path . --build-solutions --quit` | exit code 0 |
| 4 | `godot --headless --path . res://game/scenes/smoke/Smoke.tscn` | exit code 0, the output contains `SMOKE: PASS`, and none of `ERROR`, `SCRIPT ERROR`, `Unhandled exception` |

What each one proves:
1. **build:** all three projects compile. Warnings are errors, and obsolete APIs (CS0618/CS0612) fail.
2. **test:** the pure-logic unit tests in Boxing.Core pass (xUnit). No Godot needed.
3. **build-solutions:** Godot's own editor build of the C# project works, and it is also the first-run
   import of a fresh clone. Run it before #4 on a fresh clone.
4. **smoke:** the real engine loads the game assembly, instantiates Main.tscn and runs about 120 physics
   frames. Smoke.cs quits with 1 on any failure.

One-liner for the smoke grep check:
```bash
out=$(godot --headless --path . res://game/scenes/smoke/Smoke.tscn 2>&1); code=$?; echo "$out"
[ $code -eq 0 ] && grep -q 'SMOKE: PASS' <<<"$out" && ! grep -E -q 'ERROR|SCRIPT ERROR|Unhandled exception' <<<"$out"
```

## Import only
`godot --headless --import --path .` imports resources and generates `*.uid` files for new scripts. Commit the
`*.uid` files, never `.godot/`.

## Export (Windows)
```bash
mkdir -p build/windows
godot --headless --path . --export-release "Windows Desktop" build/windows/Boxing.exe
```
This needs the Godot 4.7.2 .NET export templates (SETUP.md). The preset is in export_presets.cfg. Never
launch the exported game from an agent.

## Known noise (do not "fix")
- Commands 3 and the import print one engine-internal error from Godot's own editor plugin:
  `ERROR: Unable to start the timer because it's not inside the scene tree` with a backtrace in
  `GodotTools.HotReloadAssemblyWatcher.RestartTimer`. It comes from the headless editor, not from our code,
  and the exit code is still 0. Only the smoke output (#4) is grepped for `ERROR`.
- The headless editor commands (3 and import) open the main scene's script and save it on exit. If the
  machine's Godot editor setting *Text Editor → Behavior → Indent → Type* is **Tabs**, this rewrites
  `game/scripts/Main.cs` with tabs. The repo uses 4 spaces (Godot 4.7 C# style guide). Set the editor
  to **Spaces** (SETUP.md), or restore the file with `git checkout -- game/scripts/Main.cs`.
