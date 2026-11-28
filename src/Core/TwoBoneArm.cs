using System;

namespace Boxing.Core;

/// <summary>A plain 3D vector for pure pose geometry (no engine types in Core).</summary>
public readonly record struct Vec3(double X, double Y, double Z)
{
    public static Vec3 operator +(Vec3 a, Vec3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

    public static Vec3 operator -(Vec3 a, Vec3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    public static Vec3 operator *(Vec3 a, double k) => new(a.X * k, a.Y * k, a.Z * k);

    public double Dot(Vec3 o) => (X * o.X) + (Y * o.Y) + (Z * o.Z);

    public double Length => Math.Sqrt(Dot(this));
}

/// <summary>Two-bone arm solve: keeps both limb lengths fixed so a pulled-up glove never stretches an arm.</summary>
public static class TwoBoneArm
{
    /// <summary>
    /// Returns the elbow (and the wrist, pulled back along the shoulder line only if it was out of reach) for fixed
    /// <paramref name="upperLen"/> and <paramref name="foreLen"/>. Of the elbow positions that fit, picks the one closest to
    /// <paramref name="hint"/> (where the pose data would like the elbow).
    /// </summary>
    public static (Vec3 Elbow, Vec3 Wrist) Solve(Vec3 shoulder, Vec3 wrist, double upperLen, double foreLen, Vec3 hint)
    {
        var toWrist = wrist - shoulder;
        var d = toWrist.Length;
        var min = Math.Abs(upperLen - foreLen) + 1e-6;
        var max = upperLen + foreLen - 1e-6;
        var u = d < 1e-9 ? new Vec3(0, 1, 0) : toWrist * (1.0 / d);
        var clamped = Math.Clamp(d, min, max);
        var along = ((upperLen * upperLen) - (foreLen * foreLen) + (clamped * clamped)) / (2 * clamped);
        var radius = Math.Sqrt(Math.Max(0, (upperLen * upperLen) - (along * along)));
        var centre = shoulder + (u * along);

        var offset = hint - centre;
        var perp = offset - (u * offset.Dot(u));
        var perpLen = perp.Length;
        var dir = perpLen < 1e-9 ? new Vec3(0, -1, 0) - (u * -u.Y) : perp * (1.0 / perpLen);
        var elbow = centre + (dir * radius);
        return (elbow, shoulder + (u * clamped));
    }
}
