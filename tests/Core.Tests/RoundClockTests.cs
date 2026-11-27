using Boxing.Core;
using Xunit;

namespace Boxing.Core.Tests;

public class RoundClockTests
{
    [Fact]
    public void NewClock_HasFullTime_AndIsNotOver()
    {
        var clock = new RoundClock(180);

        Assert.Equal(180, clock.RemainingSeconds);
        Assert.False(clock.IsOver);
    }

    [Fact]
    public void Tick_ReducesRemainingTime()
    {
        var clock = new RoundClock(180);

        clock.Tick(30.5);

        Assert.Equal(149.5, clock.RemainingSeconds, precision: 6);
        Assert.False(clock.IsOver);
    }

    [Fact]
    public void Tick_PastZero_ClampsToZero_AndIsOver()
    {
        var clock = new RoundClock(3);

        clock.Tick(2);
        clock.Tick(5);

        Assert.Equal(0, clock.RemainingSeconds);
        Assert.True(clock.IsOver);
    }
}
