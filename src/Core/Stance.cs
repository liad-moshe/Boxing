using System;

namespace Boxing.Core;

/// <summary>A boxer's stance: orthodox leads with the left hand and foot, southpaw is the mirror (right leads).</summary>
public enum Stance
{
    Orthodox,
    Southpaw,
}

/// <summary>A side of the boxer's own body (anatomical: left is always the boxer's left).</summary>
public enum BodySide
{
    Left,
    Right,
}

/// <summary>
/// Pure stance rules. The jab is the lead hand's punch and the cross the rear hand's in both stances; the fight
/// logic never needs the stance, only the pose code does (to pick the glove and mirror the turn).
/// </summary>
public static class StanceRules
{
    public static BodySide LeadSide(Stance stance) => stance == Stance.Orthodox ? BodySide.Left : BodySide.Right;

    public static BodySide RearSide(Stance stance) => stance == Stance.Orthodox ? BodySide.Right : BodySide.Left;

    /// <summary>The glove that throws <paramref name="move"/> (Jab: lead, Cross: rear).</summary>
    public static BodySide PunchSide(FighterState move, Stance stance) => move switch
    {
        FighterState.Jab => LeadSide(stance),
        FighterState.Cross => RearSide(stance),
        _ => throw new ArgumentException($"{move} is not a punch", nameof(move)),
    };

    /// <summary>+1 orthodox, -1 southpaw: multiplied onto every boxer-sideways offset and stance-dependent turn.</summary>
    public static int MirrorSign(Stance stance) => stance == Stance.Orthodox ? 1 : -1;

    public static Stance Toggle(Stance stance) => stance == Stance.Orthodox ? Stance.Southpaw : Stance.Orthodox;

    /// <summary>Parses the data names "orthodox" / "southpaw" (exact, lower case); null for anything else.</summary>
    public static Stance? Parse(string? text) => text switch
    {
        "orthodox" => Stance.Orthodox,
        "southpaw" => Stance.Southpaw,
        _ => null,
    };

    public static string DisplayName(Stance stance) => stance == Stance.Orthodox ? "Orthodox" : "Southpaw";
}
