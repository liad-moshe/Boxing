using System;

namespace Boxing.Core;

/// <summary>Single selected index shared by mouse hover and keyboard. Never "no selection".</summary>
public sealed class MenuSelection
{
    private readonly int _itemCount;

    public MenuSelection(int itemCount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(itemCount, 1);
        _itemCount = itemCount;
    }

    public int SelectedIndex { get; private set; }

    public void MoveUp() => SelectedIndex = (SelectedIndex + _itemCount - 1) % _itemCount;

    public void MoveDown() => SelectedIndex = (SelectedIndex + 1) % _itemCount;

    /// <summary>Selects <paramref name="index"/>; out-of-range values are ignored.</summary>
    public void Hover(int index)
    {
        if (index >= 0 && index < _itemCount)
        {
            SelectedIndex = index;
        }
    }
}
