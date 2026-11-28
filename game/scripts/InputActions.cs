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

    // Fight keys (issue #10). Defined in project.godot [input] by physical keycode: D, A, J, K, S, Q, E.
    public static readonly StringName FightForward = "fight_forward";
    public static readonly StringName FightBack = "fight_back";
    public static readonly StringName FightJab = "fight_jab";
    public static readonly StringName FightCross = "fight_cross";
    public static readonly StringName FightGuard = "fight_guard";
    public static readonly StringName FightSlipLeft = "fight_slip_left";
    public static readonly StringName FightSlipRight = "fight_slip_right";

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
