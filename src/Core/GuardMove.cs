namespace Boxing.Core;

/// <summary>
/// A guard offset in the boxer's frame (metres): Forward toward the opponent (negative = pulled back), Up vertical,
/// Inward toward the body centre line (always positive magnitude; the side decides the sign, see <see cref="GuardMove.Sideways"/>).
/// </summary>
public readonly record struct GuardOffset(double Forward, double Up, double Inward);

/// <summary>Pure geometry of how far a guard blend moves each glove and elbow from the Idle stance guard.</summary>
public static class GuardMove
{
    /// <summary>Glove and elbow offsets at <paramref name="guard"/> blend 0..1. Height and width are the fighter's metres.</summary>
    public static (GuardOffset Glove, GuardOffset Elbow) Offsets(VisualTuning visual, double height, double width, double guard)
    {
        var glove = new GuardOffset(
            -visual.GuardGloveBackH * height * guard,
            visual.GuardGloveRaiseH * height * guard,
            visual.GuardGloveInwardW * width * guard);
        var elbow = new GuardOffset(0, glove.Up * visual.GuardElbowRaiseShare, glove.Inward * visual.GuardElbowInwardShare);
        return (glove, elbow);
    }

    /// <summary>
    /// Boxer-sideways offset (right is positive) of an inward move for the arm on <paramref name="side"/>: toward the centre line,
    /// so the left arm moves right (+) and the right arm moves left (-). It follows the anatomical side, so a southpaw mirrors for free.
    /// </summary>
    public static double Sideways(GuardOffset offset, BodySide side) => side == BodySide.Left ? offset.Inward : -offset.Inward;
}
