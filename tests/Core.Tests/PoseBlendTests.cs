using Xunit;

namespace Boxing.Core.Tests;

public class PoseBlendTests
{
    private static readonly MoveSet Moves = TestData.Moves();
    private static readonly FighterRoster Roster = TestData.Roster();

    private static PlayerFighter NewFighter() => new(Moves, Roster.PlayerX, Roster.OpponentX);

    [Fact]
    public void Idle_IsAllZero()
    {
        var pose = PoseBlend.From(NewFighter(), Moves);

        Assert.Equal(0, pose.LeadExtend);
        Assert.Equal(0, pose.RearExtend);
        Assert.Equal(0, pose.Guard);
        Assert.Equal(0, pose.Slip);
        Assert.Equal(EvadeSide.None, pose.SlipSide);
    }

    [Fact]
    public void Jab_ReachesFullExtendByTheEndOfStartup_HoldsInActive_AndFallsBackToZeroAtIdle()
    {
        var f = NewFighter();
        var def = Moves.Get(FighterState.Jab);
        f.Tick(new FighterInput(JabPressed: true));
        var previous = 0.0;
        for (var i = 1; i <= def.StartupFrames; i++)
        {
            var pose = PoseBlend.From(f, Moves);
            Assert.True(pose.LeadExtend > previous);
            Assert.Equal(0, pose.RearExtend);
            previous = pose.LeadExtend;
            f.Tick(default);
        }

        Assert.Equal(1.0, previous, precision: 9);
        for (var i = 1; i <= def.ActiveFrames; i++)
        {
            Assert.Equal(1.0, PoseBlend.From(f, Moves).LeadExtend);
            f.Tick(default);
        }

        for (var i = 1; i <= def.RecoveryFrames; i++)
        {
            var value = PoseBlend.From(f, Moves).LeadExtend;
            Assert.InRange(value, 0.0001, previous - 0.0001);
            previous = value;
            f.Tick(default);
        }

        Assert.Equal(0, PoseBlend.From(f, Moves).LeadExtend);
    }

    [Fact]
    public void Cross_DrivesTheRearHandOnly()
    {
        var f = NewFighter();
        f.Tick(new FighterInput(CrossPressed: true));
        for (var i = 0; i < Moves.Get(FighterState.Cross).StartupFrames; i++)
        {
            f.Tick(default);
        }

        var pose = PoseBlend.From(f, Moves);

        Assert.Equal(1.0, pose.RearExtend);
        Assert.Equal(0, pose.LeadExtend);
    }

    [Fact]
    public void Guard_RaisesOverRaiseFrames_ThenHolds_AndDropsAtOnceOnRelease()
    {
        var f = NewFighter();
        var raise = Moves.GuardRaiseFrames;
        var hold = new FighterInput(GuardHeld: true);
        for (var i = 1; i <= raise; i++)
        {
            f.Tick(hold);
            Assert.Equal(i / (double)raise, PoseBlend.From(f, Moves).Guard, precision: 9);
        }

        f.Tick(hold);
        Assert.Equal(1.0, PoseBlend.From(f, Moves).Guard);

        f.Tick(default);
        Assert.Equal(0, PoseBlend.From(f, Moves).Guard);
    }

    [Theory]
    [InlineData(FighterState.SlipLeft, EvadeSide.Left)]
    [InlineData(FighterState.SlipRight, EvadeSide.Right)]
    public void Slip_DipsToFull_OnItsSide_AndReturnsToZero(FighterState slip, EvadeSide side)
    {
        var f = NewFighter();
        var def = Moves.Get(slip);
        f.Tick(new FighterInput(SlipLeftPressed: slip == FighterState.SlipLeft, SlipRightPressed: slip == FighterState.SlipRight));
        var peak = 0.0;
        for (var i = 1; i <= def.TotalFrames; i++)
        {
            var pose = PoseBlend.From(f, Moves);
            Assert.Equal(side, pose.SlipSide);
            Assert.Equal(0, pose.LeadExtend);
            Assert.InRange(pose.Slip, 0.0001, 1.0);
            peak = System.Math.Max(peak, pose.Slip);
            if (i > def.StartupFrames && i <= def.StartupFrames + def.ActiveFrames)
            {
                Assert.Equal(1.0, pose.Slip);
            }

            f.Tick(default);
        }

        Assert.Equal(1.0, peak);
        var idle = PoseBlend.From(f, Moves);
        Assert.Equal(0, idle.Slip);
        Assert.Equal(EvadeSide.None, idle.SlipSide);
    }
}
