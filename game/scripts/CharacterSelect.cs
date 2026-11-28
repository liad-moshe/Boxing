using Boxing.Core;
using Godot;

namespace Boxing.Game;

/// <summary>
/// Character select screen (two steps), built from code-only Controls. State lives in
/// <see cref="CharacterSelectFlow"/>; the highlight in <see cref="MenuSelection"/>. Stats are display-only.
/// </summary>
public partial class CharacterSelect : Control
{
    private const ulong ClickCooldownMs = 350;

    private static readonly StringName ConfirmAction = "menu_confirm";
    private static readonly StringName BackAction = "ui_back";
    private static readonly StringName UpAction = "menu_up";
    private static readonly StringName DownAction = "menu_down";

    private static readonly Color BackgroundColor = new(0.08f, 0.10f, 0.14f);
    private static readonly Color HeaderColor = new(0.92f, 0.92f, 0.95f);
    private static readonly Color DimText = new(0.72f, 0.74f, 0.80f);
    private static readonly Color NormalFill = new(0.16f, 0.17f, 0.21f);
    private static readonly Color SelectedFill = new(0.30f, 0.17f, 0.13f);
    private static readonly Color SelectedBorder = new(1f, 0.85f, 0.4f);
    private static readonly Color NormalBorder = new(0.30f, 0.32f, 0.38f);
    private static readonly Color BarBack = new(0.07f, 0.08f, 0.10f);
    private static readonly Color BarFill = new(0.85f, 0.45f, 0.18f);
    private static readonly Color YouTagColor = new(1f, 0.85f, 0.4f);

    private readonly FighterRoster _roster = GameSession.Roster;
    private readonly CharacterSelectFlow _flow;
    private readonly MenuSelection _selection;
    private PanelContainer[] _cards = System.Array.Empty<PanelContainer>();
    private Label[] _youTags = System.Array.Empty<Label>();
    private Label _header = null!;
    private Label _youLine = null!;
    private Label _stanceLine = null!;
    private bool _leaving;
    private bool _mouseMoved;
    private ulong _lastStepMs;

    public CharacterSelect()
    {
        _flow = new CharacterSelectFlow(_roster, GameSession.Moves.DefaultStance);
        _selection = new MenuSelection(_roster.Fighters.Count);
    }

    public override void _Ready()
    {
        InputActions.EnsureRegistered();
        GameSession.Clear();

        var background = new ColorRect { Name = "Background", Color = BackgroundColor };
        background.SetAnchorsPreset(LayoutPreset.FullRect);
        background.MouseFilter = MouseFilterEnum.Ignore;
        AddChild(background);

        var column = new VBoxContainer { Name = "Column", Alignment = BoxContainer.AlignmentMode.Center };
        column.SetAnchorsPreset(LayoutPreset.FullRect);
        column.AddThemeConstantOverride("separation", 14);
        column.MouseFilter = MouseFilterEnum.Ignore;
        AddChild(column);

        _header = MakeLabel("Header", string.Empty, 40, HeaderColor);
        column.AddChild(_header);
        _youLine = MakeLabel("YouLine", string.Empty, 22, YouTagColor);
        column.AddChild(_youLine);

        var row = new HBoxContainer { Name = "Cards", Alignment = BoxContainer.AlignmentMode.Center };
        row.AddThemeConstantOverride("separation", 32);
        row.MouseFilter = MouseFilterEnum.Ignore;
        column.AddChild(row);

        var count = _roster.Fighters.Count;
        _cards = new PanelContainer[count];
        _youTags = new Label[count];
        for (var i = 0; i < count; i++)
        {
            _cards[i] = BuildCard(i, _roster.Fighters[i], out _youTags[i]);
            row.AddChild(_cards[i]);
        }

        // One stance line for the boxer picked in the current step. A Label never takes keyboard focus, so Enter
        // still confirms the highlighted card; it only listens to mouse clicks.
        _stanceLine = MakeLabel("StanceLine", string.Empty, 24, HeaderColor);
        _stanceLine.MouseFilter = MouseFilterEnum.Stop;
        _stanceLine.FocusMode = FocusModeEnum.None;
        _stanceLine.GuiInput += OnStanceLineInput;
        column.AddChild(_stanceLine);

        column.AddChild(MakeLabel("Hint", "Left / Right: choose    Enter or click: confirm    Esc: back", 18, DimText));

        _selection.Hover(_roster.DefaultHighlightIndex);
        Refresh();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (_leaving)
        {
            return;
        }

        if (@event.IsActionPressed(BackAction))
        {
            GetViewport().SetInputAsHandled();
            GoBack();
        }
        else if (@event.IsActionPressed(InputActions.Left))
        {
            _selection.MoveUp();
            Refresh();
            GetViewport().SetInputAsHandled();
        }
        else if (@event.IsActionPressed(InputActions.Right))
        {
            _selection.MoveDown();
            Refresh();
            GetViewport().SetInputAsHandled();
        }
        else if (@event.IsActionPressed(UpAction) || @event.IsActionPressed(DownAction))
        {
            // Fresh press only (echo ignored), so a held Up or Down toggles once.
            ToggleStance();
            GetViewport().SetInputAsHandled();
        }
        else if (@event.IsActionPressed(ConfirmAction))
        {
            // Echo is ignored by IsActionPressed, so a held Enter confirms once.
            GetViewport().SetInputAsHandled();
            Confirm(_selection.SelectedIndex);
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion)
        {
            _mouseMoved = true;
        }
    }

    private PanelContainer BuildCard(int index, FighterDef fighter, out Label youTag)
    {
        var card = new PanelContainer
        {
            Name = "Card_" + fighter.Id,
            CustomMinimumSize = new Vector2(340f, 0f),
            MouseFilter = MouseFilterEnum.Stop,
        };
        card.MouseEntered += () => OnHover(index);
        card.GuiInput += e => OnCardInput(index, e);

        var margin = new MarginContainer { MouseFilter = MouseFilterEnum.Ignore };
        foreach (var side in new[] { "margin_left", "margin_right", "margin_top", "margin_bottom" })
        {
            margin.AddThemeConstantOverride(side, 18);
        }

        card.AddChild(margin);

        var box = new VBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
        box.AddThemeConstantOverride("separation", 8);
        margin.AddChild(box);

        youTag = MakeLabel("YouTag", "YOU", 22, YouTagColor);
        youTag.HorizontalAlignment = HorizontalAlignment.Left;
        box.AddChild(youTag);

        var name = MakeLabel("Name", fighter.DisplayName, 24, HeaderColor);
        name.HorizontalAlignment = HorizontalAlignment.Left;
        box.AddChild(name);
        var archetype = MakeLabel("Archetype", fighter.Archetype, 20, DimText);
        archetype.HorizontalAlignment = HorizontalAlignment.Left;
        box.AddChild(archetype);

        foreach (var stat in _roster.StatOrder)
        {
            box.AddChild(BuildStatRow(stat, fighter.Stats[stat]));
        }

        return card;
    }

    private Control BuildStatRow(string stat, int value)
    {
        var row = new HBoxContainer { Name = "Stat_" + stat, MouseFilter = MouseFilterEnum.Ignore };
        row.AddThemeConstantOverride("separation", 10);

        var label = MakeLabel("Label", _roster.StatLabels[stat], 18, HeaderColor);
        label.HorizontalAlignment = HorizontalAlignment.Left;
        label.CustomMinimumSize = new Vector2(90f, 0f);
        row.AddChild(label);

        var bar = new Control
        {
            Name = "Bar",
            CustomMinimumSize = new Vector2(170f, 16f),
            SizeFlagsVertical = SizeFlags.ShrinkCenter,
            MouseFilter = MouseFilterEnum.Ignore,
        };
        var back = new ColorRect { Name = "Back", Color = BarBack, MouseFilter = MouseFilterEnum.Ignore };
        back.SetAnchorsPreset(LayoutPreset.FullRect);
        bar.AddChild(back);
        var fill = new ColorRect { Name = "Fill", Color = BarFill, MouseFilter = MouseFilterEnum.Ignore };
        fill.SetAnchorsPreset(LayoutPreset.FullRect);
        fill.AnchorRight = (float)value / _roster.StatMax;
        bar.AddChild(fill);
        row.AddChild(bar);

        var number = MakeLabel("Value", value.ToString(System.Globalization.CultureInfo.InvariantCulture), 18, HeaderColor);
        number.HorizontalAlignment = HorizontalAlignment.Left;
        row.AddChild(number);
        return row;
    }

    private static Label MakeLabel(string name, string text, int size, Color color)
    {
        var label = new Label
        {
            Name = name,
            Text = text,
            HorizontalAlignment = HorizontalAlignment.Center,
            MouseFilter = MouseFilterEnum.Ignore,
        };
        label.AddThemeFontSizeOverride("font_size", size);
        label.AddThemeColorOverride("font_color", color);
        return label;
    }

    private void OnHover(int index)
    {
        // Ignore synthetic MouseEntered (cursor resting over a card after a screen change).
        if (_leaving || !_mouseMoved)
        {
            return;
        }

        _selection.Hover(index);
        Refresh();
    }

    private void OnCardInput(int index, InputEvent @event)
    {
        if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } click)
        {
            // A double click must advance only one step.
            if (click.DoubleClick || Time.GetTicksMsec() - _lastStepMs < ClickCooldownMs)
            {
                return;
            }

            Confirm(index);
        }
    }

    private void OnStanceLineInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left, DoubleClick: false })
        {
            ToggleStance();
        }
    }

    private void ToggleStance()
    {
        if (_leaving || !_flow.ToggleStance())
        {
            return;
        }

        Refresh();
    }

    private void Confirm(int index)
    {
        if (_leaving)
        {
            return;
        }

        _selection.Hover(index);
        if (!_flow.Confirm(_roster.Fighters[index].Id))
        {
            return;
        }

        _lastStepMs = Time.GetTicksMsec();
        if (_flow.Step == SelectStep.Done)
        {
            _leaving = true;
            GameSession.PlayerId = _flow.PlayerId;
            GameSession.OpponentId = _flow.OpponentId;
            GameSession.PlayerStance = _flow.PlayerStance;
            GameSession.OpponentStance = _flow.OpponentStance;
            GetTree().ChangeSceneToFile(ScenePaths.Ring);
            return;
        }

        // Step 2 highlight starts on the player's own pick.
        Refresh();
    }

    private void GoBack()
    {
        var result = _flow.Back();
        if (result == SelectBackResult.GoToMenu)
        {
            _leaving = true;
            GameSession.Clear();
            GetTree().ChangeSceneToFile(ScenePaths.Menu);
        }
        else if (result == SelectBackResult.GoToPlayerStep)
        {
            _lastStepMs = Time.GetTicksMsec();
            _selection.Hover(_roster.IndexOf(_flow.PlayerId ?? string.Empty));
            Refresh();
        }
    }

    private void Refresh()
    {
        var pickingOpponent = _flow.Step == SelectStep.PickOpponent;
        _header.Text = pickingOpponent ? "Choose your opponent" : "Choose your boxer";
        _youLine.Visible = pickingOpponent;
        var playerIndex = _flow.PlayerId is null ? -1 : _roster.IndexOf(_flow.PlayerId);
        _youLine.Text = pickingOpponent && playerIndex >= 0 ? $"You: {_roster.Fighters[playerIndex].Name} ({StanceRules.DisplayName(_flow.PlayerStance)})" : string.Empty;
        var shown = pickingOpponent ? _flow.OpponentStance : _flow.PlayerStance;
        _stanceLine.Text = $"Stance: {StanceRules.DisplayName(shown)}   (Up / Down to change)";

        for (var i = 0; i < _cards.Length; i++)
        {
            var selected = i == _selection.SelectedIndex;
            var box = new StyleBoxFlat { BgColor = selected ? SelectedFill : NormalFill, BorderColor = selected ? SelectedBorder : NormalBorder };
            box.SetBorderWidthAll(selected ? 5 : 2);
            _cards[i].AddThemeStyleboxOverride("panel", box);
            _youTags[i].Visible = pickingOpponent && _roster.Fighters[i].Id == _flow.PlayerId;
        }
    }
}
