using Xunit;

namespace Boxing.Core.Tests;

public class ContactBannerTests
{
    [Fact]
    public void New_IsHidden_WithNoText()
    {
        var banner = new ContactBanner(45);

        Assert.False(banner.IsVisible);
        Assert.Equal(string.Empty, banner.Text);
    }

    [Theory]
    [InlineData(ContactKind.Jab, "Contact: Jab")]
    [InlineData(ContactKind.Cross, "Contact: Cross")]
    public void Show_DisplaysTheTextForExactlyTheConfiguredTicks(ContactKind kind, string text)
    {
        var banner = new ContactBanner(45);

        banner.Show(kind);
        for (var tick = 1; tick < 45; tick++)
        {
            Assert.True(banner.IsVisible);
            Assert.Equal(text, banner.Text);
            banner.Tick();
        }

        Assert.True(banner.IsVisible);
        banner.Tick();
        Assert.False(banner.IsVisible);
        Assert.Equal(string.Empty, banner.Text);
    }

    [Fact]
    public void NewerContact_ReplacesTheTextAndRestartsTheTimer()
    {
        var banner = new ContactBanner(10);
        banner.Show(ContactKind.Jab);
        for (var i = 0; i < 6; i++)
        {
            banner.Tick();
        }

        banner.Show(ContactKind.Cross);

        Assert.Equal("Contact: Cross", banner.Text);
        for (var i = 0; i < 9; i++)
        {
            banner.Tick();
        }

        Assert.True(banner.IsVisible);
        banner.Tick();
        Assert.False(banner.IsVisible);
    }

    [Fact]
    public void ShowNone_DoesNothing()
    {
        var banner = new ContactBanner(10);

        banner.Show(ContactKind.None);

        Assert.False(banner.IsVisible);
    }

    [Fact]
    public void Tick_WhenHidden_StaysHidden()
    {
        var banner = new ContactBanner(3);

        banner.Tick();
        banner.Tick();

        Assert.False(banner.IsVisible);
    }
}
