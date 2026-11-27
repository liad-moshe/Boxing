using System;
using Godot;

namespace Boxing.Game.Smoke;

/// <summary>
/// Headless smoke test: instantiates Main.tscn, runs ~120 physics frames, checks Main is still in the tree.
/// Exit code 0 = pass, 1 = fail. Run: godot --headless --path . res://game/scenes/smoke/Smoke.tscn
/// </summary>
public partial class Smoke : Node
{
    private const string MainScenePath = ScenePaths.Ring;
    private const int FramesToRun = 120;

    private Node? _main;
    private int _frames;
    private bool _finished;

    public override void _Ready()
    {
        try
        {
            var scene = GD.Load<PackedScene>(MainScenePath);
            if (scene is null)
            {
                Fail($"could not load {MainScenePath}");
                return;
            }

            var menuScene = GD.Load<PackedScene>(ScenePaths.Menu);
            if (menuScene is null)
            {
                Fail("could not load Menu.tscn");
                return;
            }

            var menu = menuScene.Instantiate();
            AddChild(menu);
            if (menu.GetNodeOrNull<Button>("Column/NewGame") is null || menu.GetNodeOrNull<Button>("Column/Exit") is null)
            {
                Fail("Menu is missing its New Game / Exit buttons");
                return;
            }

            menu.QueueFree();

            var selectScene = GD.Load<PackedScene>(ScenePaths.CharacterSelect);
            if (selectScene is null)
            {
                Fail("could not load CharacterSelect.tscn");
                return;
            }

            var select = selectScene.Instantiate();
            AddChild(select);
            if (select.GetNodeOrNull<Node>("Column/Cards/Card_anvil") is null || select.GetNodeOrNull<Node>("Column/Cards/Card_ghost") is null)
            {
                Fail("CharacterSelect is missing its two cards");
                return;
            }

            select.QueueFree();

            _main = scene.Instantiate();
            AddChild(_main);
            GD.Print($"SMOKE: instantiated {MainScenePath}, running {FramesToRun} physics frames");
        }
        catch (Exception ex)
        {
            Fail($"exception while instantiating Main: {ex}");
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_finished)
        {
            return;
        }

        try
        {
            _frames++;
            if (_frames < FramesToRun)
            {
                return;
            }

            if (_main is null || !IsInstanceValid(_main) || !_main.IsInsideTree())
            {
                Fail("Main is no longer in the scene tree");
                return;
            }

            if (_main.GetNodeOrNull<Node3D>("PlayerFighter") is null || _main.GetNodeOrNull<Node3D>("OpponentFighter") is null)
            {
                Fail("Main is missing its two fighters");
                return;
            }

            if (_main is Main main && main.Clock.RemainingSeconds >= Main.RoundSeconds)
            {
                Fail("RoundClock did not advance");
                return;
            }

            _finished = true;
            GD.Print($"SMOKE: PASS after {_frames} physics frames");
            GetTree().Quit(0);
        }
        catch (Exception ex)
        {
            Fail($"exception during frame {_frames}: {ex}");
        }
    }

    private void Fail(string reason)
    {
        _finished = true;
        GD.PrintErr($"SMOKE: FAIL - {reason}");
        GetTree().Quit(1);
    }
}
