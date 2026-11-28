using Boxing.Core;
using Godot;

namespace Boxing.Game;

/// <summary>
/// Thin adapter for the playable fight: once per physics tick it reads the fight keys, calls
/// <see cref="PlayerFighter.Tick"/> in Boxing.Core, moves the player's root along X, poses the boxer and
/// shows the contact text and controls hint. All rules live in Boxing.Core. The opponent stays idle.
/// </summary>
public partial class FightController : Node
{
    public const string HintText = "A / D  move    J  jab    K  cross    S  guard (hold)    Q / E  slip left / right    Esc  menu";

    private MoveSet _moves = null!;
    private Stance _stance;
    private Node3D _playerRoot = null!;
    private FighterAnimator _animator = null!;
    private ContactBanner _banner = null!;
    private Label _contactLabel = null!;
    private Label _hintLabel = null!;

    /// <summary>The player's move state machine from Boxing.Core.</summary>
    public PlayerFighter Fighter { get; private set; } = null!;

    /// <summary>The "Contact: ..." label, visible while the banner is.</summary>
    public Label ContactLabel => _contactLabel;

    /// <summary>The controls hint label at the bottom of the screen.</summary>
    public Label HintLabel => _hintLabel;

    /// <summary>Call before adding the node to the tree.</summary>
    public void Setup(Node3D playerRoot, FighterDef player, FighterRoster roster, MoveSet moves, Stance stance)
    {
        _moves = moves;
        _stance = stance;
        _playerRoot = playerRoot;
        Fighter = new PlayerFighter(moves, roster.PlayerX, roster.OpponentX);
        _banner = new ContactBanner(moves.ContactTextFrames);
        _animator = new FighterAnimator(playerRoot, player, roster, moves, 1f, stance);
    }

    public override void _Ready()
    {
        if (Engine.PhysicsTicksPerSecond != _moves.TickRate)
        {
            GD.PushError($"Physics tick rate {Engine.PhysicsTicksPerSecond} must equal tickRate {_moves.TickRate} in player-moves.json");
        }

        var hud = new CanvasLayer { Name = "Hud" };
        AddChild(hud);

        _contactLabel = MakeLabel("ContactLabel", string.Empty, 40);
        _contactLabel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.TopWide, Control.LayoutPresetMode.Minsize, 24);
        _contactLabel.Visible = false;
        hud.AddChild(_contactLabel);

        _hintLabel = MakeLabel("ControlsHint", HintText, 20);
        _hintLabel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.BottomWide, Control.LayoutPresetMode.Minsize, 16);
        _hintLabel.GrowVertical = Control.GrowDirection.Begin;
        hud.AddChild(_hintLabel);
    }

    public override void _PhysicsProcess(double delta)
    {
        // Fresh presses only: IsActionJustPressed ignores key repeat and held keys.
        var input = new FighterInput(
            MoveAxis: (Input.IsActionPressed(InputActions.FightForward) ? 1 : 0) - (Input.IsActionPressed(InputActions.FightBack) ? 1 : 0),
            GuardHeld: Input.IsActionPressed(InputActions.FightGuard),
            JabPressed: Input.IsActionJustPressed(InputActions.FightJab),
            CrossPressed: Input.IsActionJustPressed(InputActions.FightCross),
            SlipLeftPressed: Input.IsActionJustPressed(InputActions.FightSlipLeft),
            SlipRightPressed: Input.IsActionJustPressed(InputActions.FightSlipRight));

        _banner.Tick();
        Fighter.Tick(input);
        if (Fighter.Contact != ContactKind.None)
        {
            _banner.Show(Fighter.Contact);
            GD.Print($"Contact: {Fighter.Contact} (gap {Fighter.OpponentX - Fighter.X:0.00} m)");
        }

        // Only X changes; the root's Y and Z stay where the ring put them.
        var position = _playerRoot.Position;
        position.X = (float)Fighter.X;
        _playerRoot.Position = position;

        _animator.Apply(PoseBlend.From(Fighter, _moves, _stance));

        _contactLabel.Text = _banner.Text;
        _contactLabel.Visible = _banner.IsVisible;
    }

    private static Label MakeLabel(string name, string text, int fontSize)
    {
        var label = new Label
        {
            Name = name,
            Text = text,
            HorizontalAlignment = HorizontalAlignment.Center,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        label.AddThemeFontSizeOverride("font_size", fontSize);
        label.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f));
        label.AddThemeColorOverride("font_outline_color", new Color(0f, 0f, 0f));
        label.AddThemeConstantOverride("outline_size", 8);
        return label;
    }
}
