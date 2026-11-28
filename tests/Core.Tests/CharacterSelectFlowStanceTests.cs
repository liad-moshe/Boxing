using Xunit;

namespace Boxing.Core.Tests;

public class CharacterSelectFlowStanceTests
{
    private static CharacterSelectFlow New() => new(FighterRosterTests.Shipped());

    [Fact]
    public void New_BothStancesStartOrthodox()
    {
        var flow = New();
        Assert.Equal(Stance.Orthodox, flow.PlayerStance);
        Assert.Equal(Stance.Orthodox, flow.OpponentStance);
    }

    [Fact]
    public void Constructor_TakesTheDefaultStance()
    {
        var flow = new CharacterSelectFlow(FighterRosterTests.Shipped(), Stance.Southpaw);
        Assert.Equal(Stance.Southpaw, flow.PlayerStance);
        Assert.Equal(Stance.Southpaw, flow.OpponentStance);
        flow.ToggleStance();
        flow.Confirm("ghost");
        Assert.Equal(Stance.Southpaw, flow.OpponentStance);
        Assert.Equal(Stance.Orthodox, flow.PlayerStance);
    }

    [Fact]
    public void Toggle_InPickPlayer_FlipsOnlyThePlayerStance_AndBack()
    {
        var flow = New();
        Assert.True(flow.ToggleStance());
        Assert.Equal(Stance.Southpaw, flow.PlayerStance);
        Assert.Equal(Stance.Orthodox, flow.OpponentStance);
        Assert.True(flow.ToggleStance());
        Assert.Equal(Stance.Orthodox, flow.PlayerStance);
    }

    [Fact]
    public void Confirm_KeepsThePlayerStance_OpponentStartsOnDefault_ToggleChangesOnlyOpponent()
    {
        var flow = New();
        flow.ToggleStance();
        flow.Confirm("ghost");
        Assert.Equal(Stance.Southpaw, flow.PlayerStance);
        Assert.Equal(Stance.Orthodox, flow.OpponentStance);

        Assert.True(flow.ToggleStance());
        Assert.Equal(Stance.Southpaw, flow.PlayerStance);
        Assert.Equal(Stance.Southpaw, flow.OpponentStance);
    }

    [Fact]
    public void Back_FromOpponentStep_KeepsPlayerStance_ResetsOpponentStance()
    {
        var flow = New();
        flow.ToggleStance();
        flow.Confirm("ghost");
        flow.ToggleStance();

        Assert.Equal(SelectBackResult.GoToPlayerStep, flow.Back());

        Assert.Equal(Stance.Southpaw, flow.PlayerStance);
        Assert.Equal(Stance.Orthodox, flow.OpponentStance);
        Assert.Equal("ghost", flow.PlayerId);
    }

    [Fact]
    public void Back_FromPlayerStep_ResetsBothToDefault()
    {
        var flow = New();
        flow.ToggleStance();
        Assert.Equal(SelectBackResult.GoToMenu, flow.Back());
        Assert.Equal(Stance.Orthodox, flow.PlayerStance);
        Assert.Equal(Stance.Orthodox, flow.OpponentStance);
    }

    [Fact]
    public void Toggle_InDone_IsRejected_BothStancesUnchanged()
    {
        var flow = New();
        flow.ToggleStance();
        flow.Confirm("ghost");
        flow.Confirm("anvil");

        Assert.False(flow.ToggleStance());

        Assert.Equal(Stance.Southpaw, flow.PlayerStance);
        Assert.Equal(Stance.Orthodox, flow.OpponentStance);
    }
}
