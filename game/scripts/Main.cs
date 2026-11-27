using Boxing.Core;
using Godot;

namespace Boxing.Game;

/// <summary>
/// Pipeline-proof main scene. Builds a primitive ring floor, a side-on camera and a light entirely from
/// code (no external assets), runs a <see cref="RoundClock"/> from Boxing.Core, and returns to the menu on ui_back.
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
            GetTree().ChangeSceneToFile(ScenePaths.Menu);
        }
    }
}
