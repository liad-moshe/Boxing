using System;
using Xunit;

namespace Boxing.Core.Tests;

public class GuardPoseTests
{
    [Fact]
    public void Solve_PutsGloveAtTargetHeight()
    {
        var pose = GuardPose.Solve(0.27, 0.25, 0.08, 100, -0.05);
        Assert.Equal(-0.05, pose.Glove.Up, 3);
    }

    [Fact]
    public void Solve_ForearmPointsUpAndForward_ElbowBelowShoulder()
    {
        var pose = GuardPose.Solve(0.27, 0.25, 0.08, 100, -0.05);
        Assert.True(pose.Wrist.Up > pose.Elbow.Up);
        Assert.True(pose.Wrist.Fwd > pose.Elbow.Fwd);
        Assert.True(pose.Elbow.Up < 0);
    }

    [Fact]
    public void Solve_KeepsSegmentLengths()
    {
        var pose = GuardPose.Solve(0.27, 0.25, 0.08, 100, -0.05);
        Assert.Equal(0.27, Math.Sqrt((pose.Elbow.Fwd * pose.Elbow.Fwd) + (pose.Elbow.Up * pose.Elbow.Up)), 6);
        var df = pose.Wrist.Fwd - pose.Elbow.Fwd;
        var du = pose.Wrist.Up - pose.Elbow.Up;
        Assert.Equal(0.25, Math.Sqrt((df * df) + (du * du)), 6);
    }

    [Fact]
    public void Solve_UnreachableTarget_Clamps()
    {
        var pose = GuardPose.Solve(0.27, 0.25, 0.08, 100, 5);
        Assert.InRange(pose.UpperArmAngleDeg, 0, 85);
    }

    [Theory]
    [InlineData("buzz")]
    [InlineData("short")]
    public void Hair_KnownStyles(string style) => Assert.True(HairStyles.CapScale(style) > 1);

    [Fact]
    public void Hair_ShortIsThickerThanBuzz() =>
        Assert.True(HairStyles.CapScale("short") > HairStyles.CapScale("buzz"));

    [Fact]
    public void Hair_UnknownStyle_Throws() =>
        Assert.Throws<RosterException>(() => HairStyles.CapScale("mohawk"));
}
