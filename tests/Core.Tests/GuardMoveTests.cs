using Xunit;

namespace Boxing.Core.Tests;

public class GuardMoveTests
{
    private static readonly VisualTuning V = TestData.Moves().Visual;
    private const double H = 1.8;
    private const double W = 0.5;

    [Fact]
    public void AtBlendZero_NothingMoves()
    {
        var (glove, elbow) = GuardMove.Offsets(V, H, W, 0);

        Assert.Equal(new GuardOffset(0, 0, 0), glove);
        Assert.Equal(new GuardOffset(0, 0, 0), elbow);
    }

    [Fact]
    public void FullGuard_RaisesGloveAtLeast80PercentOfTheData_PullsBack_AndCloses()
    {
        var (glove, _) = GuardMove.Offsets(V, H, W, 1);

        Assert.True(glove.Up >= 0.8 * V.GuardGloveRaiseH * H);
        Assert.Equal(V.GuardGloveRaiseH * H, glove.Up, 9);
        Assert.Equal(-V.GuardGloveBackH * H, glove.Forward, 9);
        Assert.Equal(V.GuardGloveInwardW * W, glove.Inward, 9);
    }

    [Fact]
    public void Elbow_FollowsBySharesOfTheGloveMove_AndDoesNotGoBack()
    {
        var (glove, elbow) = GuardMove.Offsets(V, H, W, 1);

        Assert.Equal(glove.Up * V.GuardElbowRaiseShare, elbow.Up, 9);
        Assert.Equal(glove.Inward * V.GuardElbowInwardShare, elbow.Inward, 9);
        Assert.Equal(0, elbow.Forward);
        Assert.True(elbow.Up < glove.Up || glove.Up == 0);
    }

    [Fact]
    public void Raise_IsMonotonicInTheBlend()
    {
        var previous = 0.0;
        for (var i = 1; i <= 10; i++)
        {
            var (glove, _) = GuardMove.Offsets(V, H, W, i / 10.0);
            Assert.True(glove.Up > previous);
            previous = glove.Up;
        }
    }

    [Fact]
    public void Sideways_InwardMovesTowardTheCentreLine_ForBothSides_AndMirrors()
    {
        var (glove, _) = GuardMove.Offsets(V, H, W, 1);

        // Boxer-sideways coordinate of each glove: left is negative, right positive (boxer's own frame).
        var left = GuardMove.Sideways(glove, BodySide.Left);
        var right = GuardMove.Sideways(glove, BodySide.Right);

        Assert.True(left > 0);
        Assert.True(right < 0);
        Assert.Equal(left, -right, 9);
    }
}
