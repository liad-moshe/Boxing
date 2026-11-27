using Xunit;

namespace Boxing.Core.Tests;

public class CharacterSelectFlowTests
{
    private static CharacterSelectFlow New() => new(FighterRosterTests.Shipped());

    [Fact]
    public void New_StartsAtPickPlayer_NoIds()
    {
        var flow = New();
        Assert.Equal(SelectStep.PickPlayer, flow.Step);
        Assert.Null(flow.PlayerId);
        Assert.Null(flow.OpponentId);
    }

    [Fact]
    public void ConfirmPlayer_AdvancesToOpponent()
    {
        var flow = New();
        Assert.True(flow.Confirm("ghost"));
        Assert.Equal("ghost", flow.PlayerId);
        Assert.Equal(SelectStep.PickOpponent, flow.Step);
    }

    [Fact]
    public void SameBoxerAllowed_ReachesDone()
    {
        var flow = New();
        flow.Confirm("ghost");
        Assert.True(flow.Confirm("ghost"));
        Assert.Equal("ghost", flow.OpponentId);
        Assert.Equal(SelectStep.Done, flow.Step);
    }

    [Fact]
    public void Back_FromPlayerStep_GoesToMenu() => Assert.Equal(SelectBackResult.GoToMenu, New().Back());

    [Fact]
    public void Back_FromOpponentStep_ReturnsToPlayerStep_ClearsOpponent()
    {
        var flow = New();
        flow.Confirm("anvil");
        Assert.Equal(SelectBackResult.GoToPlayerStep, flow.Back());
        Assert.Equal(SelectStep.PickPlayer, flow.Step);
        Assert.Null(flow.OpponentId);
        Assert.Equal("anvil", flow.PlayerId);
    }

    [Fact]
    public void Confirm_UnknownId_RejectedStateUnchanged()
    {
        var flow = New();
        Assert.False(flow.Confirm("nobody"));
        Assert.Equal(SelectStep.PickPlayer, flow.Step);
        Assert.Null(flow.PlayerId);
    }

    [Fact]
    public void Confirm_AfterDone_RejectedStateUnchanged()
    {
        var flow = New();
        flow.Confirm("anvil");
        flow.Confirm("ghost");
        Assert.False(flow.Confirm("anvil"));
        Assert.Equal("anvil", flow.PlayerId);
        Assert.Equal("ghost", flow.OpponentId);
        Assert.Equal(SelectStep.Done, flow.Step);
    }
}
