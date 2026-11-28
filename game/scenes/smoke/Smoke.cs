using System;
using System.Collections.Generic;
using Boxing.Core;
using Godot;

namespace Boxing.Game.Smoke;

/// <summary>
/// Headless smoke test: instantiates Main.tscn, runs ~120 physics frames, checks Main is still in the tree,
/// then plays a scripted fight with the fight keys (SmokeFight.cs).
/// Exit code 0 = pass, 1 = fail. Run: godot --headless --path . res://game/scenes/smoke/Smoke.tscn
/// </summary>
public partial class Smoke : Node
{
    private const string MainScenePath = ScenePaths.Ring;
    private const int FramesToRun = 120;

    private Node? _main;
    private int _frames;
    private bool _finished;
    private bool _scriptStarted;

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
            if (_frames < FramesToRun || _scriptStarted)
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

            var lookError = CheckLook(_main);
            if (lookError is not null)
            {
                Fail(lookError);
                return;
            }

            // The look checks passed; now play the fight (SmokeFight.cs), which prints PASS and quits on success.
            _scriptStarted = true;
            RunFightScript();
        }
        catch (Exception ex)
        {
            Fail($"exception during frame {_frames}: {ex}");
        }
    }

    private static readonly string[] RequiredParts =
    {
        "Head", "Hair", "Neck", "EyeL", "EyeR", "Nose", "Mouth", "Torso", "ShoulderL", "ShoulderR",
        "UpperArmL", "UpperArmR", "ForearmL", "ForearmR", "GloveL", "GloveR", "Shorts", "Waistband",
        "UpperLegL", "UpperLegR", "LowerLegL", "LowerLegR", "FootL", "FootR", "Tag",
    };

    /// <summary>Checks AC24 to AC28 on the ring's fighters and on a built Anvil/Ghost pair. Returns an error or null.</summary>
    private string? CheckLook(Node main)
    {
        var roster = GameSession.Roster;
        var player = main.GetNode<Node3D>("PlayerFighter");
        var opponent = main.GetNode<Node3D>("OpponentFighter");
        var first = roster.Fighters[0];
        foreach (var (root, label, facing) in new[] { (player, "PlayerFighter", 1f), (opponent, "OpponentFighter", -1f) })
        {
            var error = CheckParts(root, label) ?? CheckColours(root, label, first) ?? CheckPose(root, label, facing);
            if (error is not null)
            {
                return error;
            }
        }

        if (player.GlobalPosition.X > opponent.GlobalPosition.X)
        {
            return "player fighter is not on the -X side";
        }

        // AC26: compare the two shipped builds side by side.
        var anvilDef = roster.FindOrFirst("anvil");
        var ghostDef = roster.FindOrFirst("ghost");
        var anvil = FighterBuilder.Build("A", anvilDef, roster, "A", 1f, Stance.Orthodox);
        var ghost = FighterBuilder.Build("G", ghostDef, roster, "G", -1f, Stance.Orthodox);
        var holder = new Node3D { Name = "LookCheck" };
        AddChild(holder);
        holder.AddChild(anvil);
        holder.AddChild(ghost);
        try
        {
            foreach (var (root, def, facing) in new[] { (anvil, anvilDef, 1f), (ghost, ghostDef, -1f) })
            {
                var error = CheckParts(root, def.Id) ?? CheckColours(root, def.Id, def) ?? CheckPose(root, def.Id, facing);
                if (error is not null)
                {
                    return error;
                }
            }

            if (!(TotalHeight(ghost) > TotalHeight(anvil)))
            {
                return "Ghost is not taller than Anvil";
            }

            if (!(Span(anvil, "ShoulderL", "ShoulderR") > Span(ghost, "ShoulderL", "ShoulderR")))
            {
                return "Anvil shoulders are not wider than Ghost's";
            }

            if (!(Reach(ghost) > Reach(anvil)))
            {
                return "Ghost's arm reach is not longer than Anvil's";
            }
        }
        finally
        {
            holder.QueueFree();
        }

        return null;
    }

    private static string? CheckParts(Node3D root, string label)
    {
        foreach (var name in RequiredParts)
        {
            if (root.GetNodeOrNull<Node>(name) is null)
            {
                return $"{label} is missing part {name}";
            }
        }

        return null;
    }

    private static string? CheckColours(Node3D root, string label, FighterDef def)
    {
        var expected = new Dictionary<string, string>
        {
            ["Shorts"] = def.Look.TrunksColor,
            ["GloveL"] = def.Look.GloveColor,
            ["GloveR"] = def.Look.GloveColor,
            ["Hair"] = def.Look.HairColor,
            ["Head"] = def.Look.SkinColor,
            ["Waistband"] = def.Look.WaistbandColor,
        };
        foreach (var (name, hex) in expected)
        {
            var mesh = root.GetNode<MeshInstance3D>(name).Mesh as PrimitiveMesh;
            var color = (mesh?.Material as StandardMaterial3D)?.AlbedoColor;
            var (r, g, b) = FighterRoster.ParseHexColor(hex);
            if (color is null || Math.Abs(color.Value.R - r) > 0.01 || Math.Abs(color.Value.G - g) > 0.01 || Math.Abs(color.Value.B - b) > 0.01)
            {
                return $"{label} {name} colour does not match {hex}";
            }
        }

        return null;
    }

    private static string? CheckPose(Node3D root, string label, float facing)
    {
        foreach (var foot in new[] { "FootL", "FootR" })
        {
            var low = WorldBox(root.GetNode<MeshInstance3D>(foot)).Position.Y;
            if (Math.Abs(low) > 0.01f)
            {
                return $"{label} {foot} lowest point is {low} m from the floor";
            }
        }

        var head = Center(root, "Head");
        var torso = Center(root, "Torso");
        if (!(head.Y > torso.Y))
        {
            return $"{label} head is not above the torso";
        }

        var shortsTop = WorldBox(root.GetNode<MeshInstance3D>("Shorts")).End.Y;
        foreach (var side in new[] { "L", "R" })
        {
            var glove = Center(root, "Glove" + side);
            if (!(glove.Y > shortsTop))
            {
                return $"{label} Glove{side} is not above the shorts";
            }

            if (!((glove.X - torso.X) * facing > 0f))
            {
                return $"{label} Glove{side} is not in front of the torso";
            }

            var gloveMesh = (SphereMesh)root.GetNode<MeshInstance3D>("Glove" + side).Mesh;
            var forearm = (CylinderMesh)root.GetNode<MeshInstance3D>("Forearm" + side).Mesh;
            if (gloveMesh.Radius < 1.5f * Math.Max(forearm.TopRadius, forearm.BottomRadius))
            {
                return $"{label} Glove{side} is under 1.5x the forearm radius";
            }
        }

        // AC28: face cues on the facing side of the head.
        foreach (var cue in new[] { "EyeL", "EyeR", "Nose", "Mouth" })
        {
            if (!((Center(root, cue).X - head.X) * facing > 0f))
            {
                return $"{label} {cue} is not on the side facing the opponent";
            }
        }

        return null;
    }

    private static Aabb WorldBox(MeshInstance3D node) => node.GlobalTransform * node.Mesh.GetAabb();

    private static Vector3 Center(Node3D root, string name)
    {
        var box = WorldBox(root.GetNode<MeshInstance3D>(name));
        return box.Position + (box.Size * 0.5f);
    }

    private static float TotalHeight(Node3D root)
    {
        var low = float.MaxValue;
        var high = float.MinValue;
        foreach (var child in root.GetChildren())
        {
            if (child is MeshInstance3D mesh)
            {
                var box = WorldBox(mesh);
                low = Math.Min(low, box.Position.Y);
                high = Math.Max(high, box.End.Y);
            }
        }

        return high - low;
    }

    private static float Span(Node3D root, string a, string b) => Center(root, a).DistanceTo(Center(root, b));

    private static float Reach(Node3D root) => Center(root, "GloveL").DistanceTo(Center(root, "ShoulderL"));

    private void Fail(string reason)
    {
        _finished = true;
        GD.PrintErr($"SMOKE: FAIL - {reason}");
        GetTree().Quit(1);
    }
}
