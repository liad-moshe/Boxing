using Xunit;

namespace Boxing.Core.Tests;

public class ScreenFlowTests
{
    [Fact]
    public void OnBack_FromMenu_QuitsApp()
    {
        Assert.Equal(BackResult.QuitApp, ScreenFlow.OnBack(Screen.Menu));
    }

    [Fact]
    public void OnBack_FromCharacterSelect_GoesToMenu_NeverQuits()
    {
        Assert.Equal(BackResult.GoToMenu, ScreenFlow.OnBack(Screen.CharacterSelect));
    }

    [Fact]
    public void OnBack_FromRing_GoesToMenu_NeverQuits()
    {
        var result = ScreenFlow.OnBack(Screen.Ring);
        Assert.Equal(BackResult.GoToMenu, result);
        Assert.NotEqual(BackResult.QuitApp, result);
    }
}
