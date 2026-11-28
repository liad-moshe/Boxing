using System;
using Xunit;

namespace Boxing.Core.Tests;

/// <summary>
/// QA acceptance test for AC33 and AC31 on the full-guard arm, using the shipped data and the same arithmetic as
/// FighterBuilder (build) and FighterAnimator (guard pose): elbow lower than the glove, elbow nearer the centre line
/// than its shoulder, limb lengths kept, glove still on its own side, glove covers the face.
/// </summary>
public class GuardArmGeometryTests
{
    [Fact]
    public void FullGuard_ElbowTuckedLowAndInboard_LimbsKeepLength_GloveCoversFace_ForEveryFighterAndSide()
    {
        var roster = TestData.Roster();
        var visual = TestData.Moves().Visual;
        var b = roster.Body;

        foreach (var fighter in roster.Fighters)
        {
            var look = fighter.Look;
            var h = roster.BaseHeight * look.HeightScale;
            var w = roster.BaseWidth * look.WidthScale;
            var hipY = (b["footHeightH"] * h) + (b["lowerLegLengthH"] * h) + (b["upperLegLengthH"] * h);
            var torsoTop = hipY + (b["shortsHeightH"] * h * 0.5) - (b["waistbandHeightH"] * h) + (b["torsoHeightH"] * h);
            var shoulderR = b["shoulderRadiusW"] * w;
            var shoulderY = torsoTop - (shoulderR * 0.6);
            var shoulderHalf = b["shoulderSpanW"] * w * look.ShoulderScale * 0.5;
            var upperLen = b["upperArmLengthH"] * h * look.ArmLengthScale;
            var foreLen = b["forearmLengthH"] * h * look.ArmLengthScale;
            var gloveR = b["gloveRadiusH"] * h;
            var gloveOffset = gloveR * b["gloveStretch"] * 0.6;
            var pose = GuardPose.Solve(upperLen, foreLen, gloveOffset, b["guardElbowBendDeg"], (b["guardGloveHeightH"] * h) - shoulderY);

            var headRx = b["headRadiusH"] * h * look.HeadScale;
            var headRy = headRx * b["headStretchY"];
            var headY = torsoTop + (b["neckHeightH"] * h) + (headRy * 0.8);
            var front = headRx;

            foreach (var zSign in new[] { -1.0, 1.0 })
            {
                var shoulder = new Vec3(0, shoulderY, zSign * shoulderHalf);
                var restElbow = shoulder + new Vec3(pose.Elbow.Fwd, pose.Elbow.Up, -zSign * shoulderHalf * 0.2);
                var restWrist = new Vec3(pose.Wrist.Fwd, shoulderY + pose.Wrist.Up, zSign * Math.Max(gloveR * 1.1, shoulderHalf * 0.2));
                var restDir = (restWrist - restElbow) * (1.0 / (restWrist - restElbow).Length);
                var restGlove = restWrist + (restDir * gloveOffset);

                var (g, e) = GuardMove.Offsets(visual, h, w, 1);
                var inward = -Math.Sign(restWrist.Z);
                var wrist = restWrist + new Vec3(g.Forward, g.Up, inward * g.Inward);
                var hint = restElbow + new Vec3(e.Forward, e.Up, inward * e.Inward);
                var (elbow, solvedWrist) = TwoBoneArm.Solve(shoulder, wrist, upperLen, foreLen, hint);
                var foreDir = (solvedWrist - elbow) * (1.0 / (solvedWrist - elbow).Length);
                var glove = solvedWrist + (foreDir * gloveOffset);
                var who = $"{fighter.DisplayName} z{zSign}";

                Assert.True(elbow.Y < glove.Y, $"{who}: elbow {elbow.Y:0.000} not lower than glove {glove.Y:0.000}");
                Assert.True(Math.Abs(elbow.Z) < Math.Abs(shoulder.Z), $"{who}: elbow z {elbow.Z:0.000} not inboard of shoulder z {shoulder.Z:0.000}");
                Assert.InRange((elbow - shoulder).Length / upperLen, 0.85, 1.15);
                Assert.InRange((solvedWrist - elbow).Length / foreLen, 0.85, 1.15);
                Assert.Equal(Math.Sign(restGlove.Z), Math.Sign(glove.Z));
                Assert.True(glove.Y - restGlove.Y >= 0.8 * visual.GuardGloveRaiseH * h - 0.001, $"{who}: glove rise {glove.Y - restGlove.Y:0.000}");
                Assert.InRange(glove.Y, headY - headRy, headY + headRy);
                Assert.InRange(glove.X - front, (0.02 * h) - 0.001, (0.20 * h) + 0.001);
            }
        }
    }
}
