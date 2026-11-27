---
name: godot-csharp-api
description: Godot 4.7.2 C# engine-correctness rules - how to verify any engine API against the exact-version GodotSharp.xml or the version-pinned docs, the Godot 3 to 4 rename table, C# signal/event patterns for 4.x, and the compile-a-probe fallback. Use before writing or reviewing any code that touches the Godot API.
user-invocable: false
---

# Godot 4.7.2 C# — engine correctness

## The rules (environment-spec.md §8)
1. **The compiler is the oracle.** Directory.Build.props sets `TreatWarningsAsErrors=true` and
   `Nullable=enable`, and makes CS0618/CS0612 (obsolete API) errors. Never suppress a warning, never add
   `#pragma warning disable`, never edit the props files.
2. **Verify before you use.** For any engine API you are not certain of, check the exact-version XML docs
   or the 4.7 docs (below). Don't rely on memory: much of the Godot C# code online is Godot 3.
3. **Never use Godot 3 APIs** (table below). CI and QA grep for them.
4. **Architecture:** pure game logic goes in `src/Core` (Boxing.Core, NO `using Godot`), unit-tested
   with xUnit. Godot nodes in `game/` are thin adapters: they read input, call Core, and move nodes.

## Where the truth is
**Exact-version API docs (preferred):**
- Path: `~/.nuget/packages/godotsharp/4.7.2/lib/net8.0/GodotSharp.xml`. It appears after the first
  `dotnet build`; on Windows `~` is `%USERPROFILE%`.
- Search it with the Grep tool or `grep -o 'name="M:Godot.Node3D.LookAt[^"]*"' <path>`.
- Member ID prefixes:
  - `T:` type: `T:Godot.CharacterBody3D`
  - `P:` property: `P:Godot.Node3D.Position`
  - `M:` method, with its parameter types: `M:Godot.SceneTree.Quit(System.Int32)`
  - `E:` event
  - `F:` field or enum value: `F:Godot.Key.Escape`
- Members are inherited, so search the declaring class: `AlbedoColor` lives on `BaseMaterial3D`, not on
  `StandardMaterial3D`.
- No hit means the API doesn't exist in 4.7.2. Do not use it.

**Version-pinned web docs:** `https://docs.godotengine.org/en/4.7/`. Never `/stable` or `/latest`.
- Class reference: `https://docs.godotengine.org/en/4.7/classes/class_<lowercasename>.html`, for example
  `class_characterbody3d.html`. The pages use snake_case; C# uses PascalCase (`move_and_slide` →
  `MoveAndSlide`).
- C# specifics: `https://docs.godotengine.org/en/4.7/tutorials/scripting/c_sharp/`.

## Godot 3 → Godot 4 renames (never write the left column)
| Godot 3 | Godot 4.7 C# |
|---|---|
| `Spatial` | `Node3D` |
| `KinematicBody` / `RigidBody` / `StaticBody` / `Area` | `CharacterBody3D` / `RigidBody3D` / `StaticBody3D` / `Area3D` |
| `Camera`, `MeshInstance`, `DirectionalLight`, `Position3D` | `Camera3D`, `MeshInstance3D`, `DirectionalLight3D`, `Marker3D` |
| `Sprite`, `Position2D` | `Sprite2D`, `Marker2D` |
| `.Translation` | `.Position` (and `GlobalPosition`) |
| `Godot.Object` | `GodotObject` |
| `Reference` | `RefCounted` |
| `VisualServer` | `RenderingServer` |
| `yield(obj, "signal")` | `await ToSignal(obj, Class.SignalName.X)` |
| `Connect("sig", this, "Method")` | C# event `obj.Sig += Method;` or `Connect(Class.SignalName.Sig, Callable.From(Method))` |
| `OS.GetTicksMsec()` | `Time.GetTicksMsec()` |
| `Engine.EditorHint` | `Engine.IsEditorHint()` |
| `[Remote]`, `[Puppet]`, `[Master]` | `[Rpc(...)]` |
| `packedScene.Instance()` | `packedScene.Instantiate()` / `Instantiate<T>()` |
| `_Process(float delta)` / `_PhysicsProcess(float delta)` | `_Process(double delta)` / `_PhysicsProcess(double delta)` |
| `MoveAndSlide(velocity, up)` | set `Velocity`, then call `MoveAndSlide()` (no arguments) |
| `Tween` node added to the tree | `var tween = CreateTween();` |
| `export` keyword / `[Export]` on fields only | `[Export]` on properties or fields; the class must be `partial` |

## C# patterns for 4.x
- Every class deriving from a Godot type is **`partial`**, because source generators fill in the rest:
  `public partial class Boxer : CharacterBody3D`.
- **Custom signal:**
  ```csharp
  [Signal] public delegate void PunchLandedEventHandler(int damage);   // name MUST end in EventHandler
  EmitSignal(SignalName.PunchLanded, 12);                               // raises event PunchLanded
  ```
- **Subscribe** with C# events: `boxer.PunchLanded += OnPunchLanded;`. Unsubscribe with `-=` in
  `_ExitTree()` when the publisher outlives you. Built-in signals are events too:
  `button.Pressed += OnStart;`.
- **Await a signal:**
  `await ToSignal(GetTree().CreateTimer(0.5), SceneTreeTimer.SignalName.Timeout);`
- **Input actions:** `@event.IsActionPressed("ui_quit_game")` in `_UnhandledInput`, or
  `Input.IsActionPressed(...)` when polling. Cache action names as `static readonly StringName`.
- Use node paths sparingly. Prefer building nodes in code or `[Export]` references.
- Engine types use `float`/`Vector3`. Godot frame `delta` is `double`. Keep Core math in `double`, and
  convert at the adapter boundary.

## When unsure: write a 5-line probe and compile it
1. Create `game/_probe/Probe.cs`:
   ```csharp
   using Godot;
   public partial class Probe : Node3D
   {
       public override void _Ready() { var b = new CharacterBody3D(); b.Velocity = Vector3.Zero; b.MoveAndSlide(); }
   }
   ```
2. `dotnet build Boxing.sln -warnaserror`. If it compiles with no CS0618/CS0612, the API exists and is not
   obsolete.
3. **Delete `game/_probe/`** before committing. Probes are never committed.
