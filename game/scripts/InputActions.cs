using Godot;

namespace Boxing.Game;

/// <summary>
/// Makes sure the select-screen actions exist. They belong in project.godot (see work/5/governance-proposal.md);
/// until they are there this registers them at runtime. No-op once project.godot defines them.
/// </summary>
public static class InputActions
{
    public static readonly StringName Left = "menu_left";
    public static readonly StringName Right = "menu_right";

    public static void EnsureRegistered()
    {
        Ensure(Left, Key.Left);
        Ensure(Right, Key.Right);
    }

    private static void Ensure(StringName action, Key key)
    {
        if (InputMap.HasAction(action))
        {
            return;
        }

        InputMap.AddAction(action);
        InputMap.ActionAddEvent(action, new InputEventKey { PhysicalKeycode = key });
    }
}
