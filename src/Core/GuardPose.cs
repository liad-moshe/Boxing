using System;

namespace Boxing.Core;

/// <summary>A point in the arm's plane, relative to the shoulder: Fwd toward the opponent, Up vertical (metres).</summary>
public readonly record struct PlanePoint(double Fwd, double Up);

/// <summary>Elbow, wrist and glove-centre positions of one guard-pose arm.</summary>
public sealed record ArmPose(double UpperArmAngleDeg, PlanePoint Elbow, PlanePoint Wrist, PlanePoint Glove);

/// <summary>Pure geometry for the static boxing guard stance (GAMEPLAY ABSTRACTION: a fixed ready pose).</summary>
public static class GuardPose
{
    /// <summary>
    /// Solves the arm in its plane. The upper arm hangs down and forward at an angle from vertical; the elbow
    /// is flexed by <paramref name="bendDeg"/> so the forearm points up and forward. The angle is chosen so the
    /// glove centre sits at <paramref name="gloveUpFromShoulder"/> (negative = below the shoulder), clamped to 0..85 degrees.
    /// </summary>
    public static ArmPose Solve(double upperLen, double foreLen, double gloveOffset, double bendDeg, double gloveUpFromShoulder)
    {
        var lo = 0.0;
        var hi = 85.0;
        for (var i = 0; i < 40; i++)
        {
            var mid = (lo + hi) / 2;
            if (At(mid, upperLen, foreLen, gloveOffset, bendDeg).Glove.Up < gloveUpFromShoulder)
            {
                lo = mid;
            }
            else
            {
                hi = mid;
            }
        }

        return At((lo + hi) / 2, upperLen, foreLen, gloveOffset, bendDeg);
    }

    private static ArmPose At(double angleDeg, double upperLen, double foreLen, double gloveOffset, double bendDeg)
    {
        var a = angleDeg * Math.PI / 180;
        var forearmAngle = (angleDeg + bendDeg - 90) * Math.PI / 180;
        var elbow = new PlanePoint(upperLen * Math.Sin(a), -upperLen * Math.Cos(a));
        var dir = new PlanePoint(Math.Cos(forearmAngle), Math.Sin(forearmAngle));
        var wrist = new PlanePoint(elbow.Fwd + (foreLen * dir.Fwd), elbow.Up + (foreLen * dir.Up));
        var glove = new PlanePoint(wrist.Fwd + (gloveOffset * dir.Fwd), wrist.Up + (gloveOffset * dir.Up));
        return new ArmPose(angleDeg, elbow, wrist, glove);
    }
}

/// <summary>Maps a data-file hair style name to how far the hair cap stands off the scalp (1 = skull).</summary>
public static class HairStyles
{
    public static double CapScale(string style) => style switch
    {
        "buzz" => 1.04,
        "short" => 1.1,
        _ => throw new RosterException($"unknown hairStyle '{style}' (use buzz or short)"),
    };
}
