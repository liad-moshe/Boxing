using Boxing.Core;
using Godot;

namespace Boxing.Game;

/// <summary>
/// Main menu, built entirely from code-only Controls. Selection logic is <see cref="MenuSelection"/>;
/// Escape is decided by <see cref="ScreenFlow"/>. This node only wires input to them.
/// </summary>
public partial class Menu : Control
{
    public const string RingScenePath = "res://game/scenes/Main.tscn";

    private static readonly StringName UpAction = "menu_up";
    private static readonly StringName DownAction = "menu_down";
    private static readonly StringName ConfirmAction = "menu_confirm";
    private static readonly StringName BackAction = "ui_back";

    private static readonly string[] Labels = { "New Game", "Exit" };

    private static readonly Color BackgroundColor = new(0.08f, 0.10f, 0.14f);
    private static readonly Color TitleColor = new(0.92f, 0.92f, 0.95f);
    private static readonly Color NormalFill = new(0.22f, 0.23f, 0.26f);
    private static readonly Color NormalText = new(0.82f, 0.82f, 0.85f);
    private static readonly Color SelectedFill = new(0.78f, 0.28f, 0.16f);
    private static readonly Color SelectedPressedFill = new(0.60f, 0.20f, 0.11f);
    private static readonly Color SelectedText = new(1f, 1f, 1f);
    private static readonly Color SelectedBorder = new(1f, 0.85f, 0.4f);
    private static readonly Vector2 ItemSize = new(280f, 56f);
    private const int ItemGap = 16;
    private const int TitleFontSize = 64;
    private const int ItemFontSize = 26;

    private readonly MenuSelection _selection = new(Labels.Length);
    private readonly Button[] _buttons = new Button[Labels.Length];
    private bool _leaving;

    public override void _Ready()
    {
        var background = new ColorRect { Name = "Background", Color = BackgroundColor };
        background.SetAnchorsPreset(LayoutPreset.FullRect);
        background.MouseFilter = MouseFilterEnum.Ignore;
        AddChild(background);

        var column = new VBoxContainer { Name = "Column", Alignment = BoxContainer.AlignmentMode.Center };
        column.SetAnchorsPreset(LayoutPreset.FullRect);
        column.AddThemeConstantOverride("separation", ItemGap);
        column.MouseFilter = MouseFilterEnum.Ignore;
        AddChild(column);

        var title = new Label
        {
            Name = "Title",
            Text = "Boxing",
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        title.AddThemeFontSizeOverride("font_size", TitleFontSize);
        title.AddThemeColorOverride("font_color", TitleColor);
        column.AddChild(title);

        for (var i = 0; i < Labels.Length; i++)
        {
            var index = i;
            var button = new Button
            {
                Name = Labels[i].Replace(" ", string.Empty),
                Text = Labels[i],
                CustomMinimumSize = ItemSize,
                SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
                FocusMode = FocusModeEnum.None, // our selection is the only highlight; no second focus state
            };
            button.AddThemeFontSizeOverride("font_size", ItemFontSize);
            button.MouseEntered += () => OnHover(index);
            button.Pressed += () => Activate(index);
            column.AddChild(button);
            _buttons[i] = button;
        }

        Refresh();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (_leaving)
        {
            return;
        }

        // IsActionPressed ignores key echo by default (allowEcho = false).
        if (@event.IsActionPressed(BackAction))
        {
            GetViewport().SetInputAsHandled();
            if (ScreenFlow.OnBack(Screen.Menu) == BackResult.QuitApp)
            {
                _leaving = true;
                GetTree().Quit();
            }
        }
        else if (@event.IsActionPressed(UpAction))
        {
            _selection.MoveUp();
            Refresh();
            GetViewport().SetInputAsHandled();
        }
        else if (@event.IsActionPressed(DownAction))
        {
            _selection.MoveDown();
            Refresh();
            GetViewport().SetInputAsHandled();
        }
        else if (@event.IsActionPressed(ConfirmAction))
        {
            GetViewport().SetInputAsHandled();
            Activate(_selection.SelectedIndex);
        }
    }

    private void OnHover(int index)
    {
        if (_leaving)
        {
            return;
        }

        _selection.Hover(index);
        Refresh();
    }

    private void Activate(int index)
    {
        if (_leaving)
        {
            return;
        }

        _selection.Hover(index);
        Refresh();
        _leaving = true;
        if (index == 0)
        {
            GetTree().ChangeSceneToFile(RingScenePath);
        }
        else
        {
            GetTree().Quit();
        }
    }

    private void Refresh()
    {
        for (var i = 0; i < _buttons.Length; i++)
        {
            ApplyStyle(_buttons[i], i == _selection.SelectedIndex);
        }
    }

    private static void ApplyStyle(Button button, bool selected)
    {
        var normal = MakeBox(selected ? SelectedFill : NormalFill, selected);
        var pressed = MakeBox(selected ? SelectedPressedFill : NormalFill, selected);
        button.AddThemeStyleboxOverride("normal", normal);
        button.AddThemeStyleboxOverride("hover", normal);
        button.AddThemeStyleboxOverride("pressed", pressed);
        button.AddThemeStyleboxOverride("focus", normal);
        var text = selected ? SelectedText : NormalText;
        foreach (var name in new[] { "font_color", "font_hover_color", "font_pressed_color", "font_focus_color" })
        {
            button.AddThemeColorOverride(name, text);
        }
    }

    private static StyleBoxFlat MakeBox(Color fill, bool bordered)
    {
        var box = new StyleBoxFlat { BgColor = fill };
        if (bordered)
        {
            box.SetBorderWidthAll(4);
            box.BorderColor = SelectedBorder;
        }

        return box;
    }
}
