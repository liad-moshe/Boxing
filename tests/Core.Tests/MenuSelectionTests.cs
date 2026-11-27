using System;
using Xunit;

namespace Boxing.Core.Tests;

public class MenuSelectionTests
{
    [Fact]
    public void New_StartsAtZero()
    {
        Assert.Equal(0, new MenuSelection(2).SelectedIndex);
    }

    [Fact]
    public void MoveDown_FromFirst_GoesToSecond()
    {
        var s = new MenuSelection(2);
        s.MoveDown();
        Assert.Equal(1, s.SelectedIndex);
    }

    [Fact]
    public void MoveDown_FromLast_WrapsToFirst()
    {
        var s = new MenuSelection(2);
        s.MoveDown();
        s.MoveDown();
        Assert.Equal(0, s.SelectedIndex);
    }

    [Fact]
    public void MoveUp_FromFirst_WrapsToLast()
    {
        var s = new MenuSelection(2);
        s.MoveUp();
        Assert.Equal(1, s.SelectedIndex);
    }

    [Fact]
    public void Hover_SetsIndex_AndMoveDownContinuesFromIt()
    {
        var s = new MenuSelection(2);
        s.Hover(1);
        Assert.Equal(1, s.SelectedIndex);
        s.MoveDown();
        Assert.Equal(0, s.SelectedIndex);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void Hover_OutOfRange_IsIgnored(int index)
    {
        var s = new MenuSelection(2);
        s.Hover(1);
        s.Hover(index);
        Assert.Equal(1, s.SelectedIndex);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void New_WithFewerThanOneItem_Throws(int count)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new MenuSelection(count));
    }
}
