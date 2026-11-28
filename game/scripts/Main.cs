using Boxing.Core;
using Godot;

namespace Boxing.Game;

/// <summary>
/// The ring and fight scene. Builds a primitive ring floor, a side-on camera and a light entirely from
/// code (no external assets), runs a <see cref="RoundClock"/> from Boxing.Core, hosts the playable fight
/// (<see cref="FightController"/>) and returns to the menu on ui_back.
/// </summary>
public partial class Main : Node3D
{
    public const double RoundSeconds = 180;

    private static readonly StringName BackAction = "ui_back";

    private bool _leaving;

    /// <summary>The round clock from Boxing.Core; proves the game assembly references pure logic.</summary>
    public RoundClock Clock { get; } = new(RoundSeconds);

    public override void _Ready()
    {
        var ringFloor = new MeshInstance3D
        {
            Name = "RingFloor",
            Mesh = new BoxMesh
            {
                Size = new Vector3(6f, 0.2f, 6f),
                Material = new StandardMaterial3D { AlbedoColor = new Color(0.78f, 0.74f, 0.66f) },
            },
            Position = new Vector3(0f, -0.1f, 0f),
        };
        AddChild(ringFloor);

        var light = new DirectionalLight3D
        {
            Name = "Sun",
            RotationDegrees = new Vector3(-50f, -30f, 0f),
            ShadowEnabled = true,
        };
        AddChild(light);

        // Side-on camera, like a classic fighting game (client decision, docs/vision.md).
        var camera = new Camera3D { Name = "Camera", Fov = 50f, Current = true };
        AddChild(camera);
        camera.LookAtFromPosition(new Vector3(0f, 1.6f, 7f), new Vector3(0f, 0.8f, 0f), Vector3.Up);

        // Picks come from character select; a direct launch (smoke test) falls back to the first roster entry.
        var roster = GameSession.Roster;
        var player = roster.FindOrFirst(GameSession.PlayerId);
        var opponent = roster.FindOrFirst(GameSession.OpponentId);
        var moves = GameSession.Moves;
        var playerStance = GameSession.PlayerStance ?? moves.DefaultStance;
        var opponentStance = GameSession.OpponentStance ?? moves.DefaultStance;
        AddChild(FighterBuilder.Build("PlayerFighter", player, roster, $"YOU ({StanceRules.DisplayName(playerStance)})", 1f, playerStance));
        AddChild(FighterBuilder.Build("OpponentFighter", opponent, roster, $"CPU ({StanceRules.DisplayName(opponentStance)})", -1f, opponentStance));
        var playerRoot = GetNode<Node3D>("PlayerFighter");
        playerRoot.Position = new Vector3((float)roster.PlayerX, 0f, 0f);
        GetNode<Node3D>("OpponentFighter").Position = new Vector3((float)roster.OpponentX, 0f, 0f);

        // The ring is the fight: the player's boxer is controllable from the first physics tick; the opponent stays idle.
        var fight = new FightController { Name = "Fight" };
        fight.Setup(playerRoot, player, roster, moves, playerStance);
        AddChild(fight);
    }

    public override void _Process(double delta)
    {
        Clock.Tick(delta);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        // IsActionPressed ignores key echo by default, so a held Escape fires once.
        if (_leaving || !@event.IsActionPressed(BackAction))
        {
            return;
        }

        GetViewport().SetInputAsHandled();
        _leaving = true;
        // From the ring ScreenFlow only ever answers GoToMenu (Escape never quits from here).
        if (ScreenFlow.OnBack(Screen.Ring) == BackResult.GoToMenu)
        {
            GameSession.Clear();
            GetTree().ChangeSceneToFile(ScenePaths.Menu);
        }
    }
}
