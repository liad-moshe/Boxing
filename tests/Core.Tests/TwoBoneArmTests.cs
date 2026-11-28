using Xunit;

namespace Boxing.Core.Tests;

public class TwoBoneArmTests
{
    private static readonly Vec3 Shoulder = new(0, 1.4, 0.2);

    [Fact]
    public void Elbow_KeepsBothLimbLengths()
    {
        var wrist = new Vec3(0.15, 1.55, 0.05);

        var (elbow, w) = TwoBoneArm.Solve(Shoulder, wrist, 0.3, 0.3, new Vec3(0.1, 1.2, 0.2));

        Assert.Equal(0.3, (elbow - Shoulder).Length, 6);
        Assert.Equal(0.3, (w - elbow).Length, 6);
        Assert.True((wrist - w).Length < 1e-9);
    }

    [Fact]
    public void Elbow_PicksTheSideNearestTheHint_AndRestPoseIsAFixedPoint()
    {
        var elbow = new Vec3(0.1, 1.15, 0.2);
        var wrist = new Vec3(0.3, 1.38, 0.1);
        var upper = (elbow - Shoulder).Length;
        var fore = (wrist - elbow).Length;

        var (solved, _) = TwoBoneArm.Solve(Shoulder, wrist, upper, fore, elbow);

        Assert.True((solved - elbow).Length < 1e-6);
    }

    [Fact]
    public void OutOfReachWrist_IsPulledBackToFullReach()
    {
        var (elbow, w) = TwoBoneArm.Solve(Shoulder, new Vec3(2, 1.4, 0.2), 0.3, 0.3, new Vec3(0.1, 1.2, 0.2));

        Assert.True((w - Shoulder).Length <= 0.6);
        Assert.Equal(0.3, (elbow - Shoulder).Length, 6);
    }
}
