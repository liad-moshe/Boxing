using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Boxing.Core;
using Godot;

namespace Boxing.Game.Smoke;

/// <summary>
/// The scripted fight part of the smoke test (issue #10). Drives the fight keys with Input.ActionPress /
/// ActionRelease against the direct-launch ring (default roster entry) and checks AC17 to AC22 and AC24.
/// Each await resumes at the start of a physics frame, before nodes run their _PhysicsProcess, so a press made
/// there is seen by the fight controller on that same frame and the state read next frame includes its tick.
/// </summary>
public partial class Smoke
{
    private static readonly string[] PosedParts =
    {
        "Head", "Hair", "Neck", "EyeL", "EyeR", "Nose", "Mouth", "Torso", "ShoulderL", "ShoulderR",
        "UpperArmL", "UpperArmR", "ForearmL", "ForearmR", "GloveL", "GloveR",
    };

    private async void RunFightScript()
    {
        try
        {
            await FightScript();
            _finished = true;
            GD.Print($"SMOKE: PASS after {_frames} physics frames (fight script included)");
            GetTree().Quit(0);
        }
        catch (Exception ex)
        {
            Fail($"fight script: {ex.Message}");
        }
    }

    private async Task FightScript()
    {
        var main = _main ?? throw new InvalidOperationException("no Main");
        var fight = main.GetNodeOrNull<FightController>("Fight") ?? throw new InvalidOperationException("Main has no Fight controller");
        var fighter = fight.Fighter;
        var roster = GameSession.Roster;
        var moves = GameSession.Moves;
        var playerRoot = main.GetNode<Node3D>("PlayerFighter");
        var opponentRoot = main.GetNode<Node3D>("OpponentFighter");
        var def = roster.FindOrFirst(GameSession.PlayerId);
        var height = (float)(roster.BaseHeight * def.Look.HeightScale);

        // AC17 / AC24: controls are live at the ring's start, the hint is on screen, the player is Idle at ring.playerX.
        Expect(fight.HintLabel.Visible && fight.HintLabel.Text == FightController.HintText, "controls hint is not shown");
        Expect(fight.HintLabel.Text.Contains("J  jab", StringComparison.Ordinal), "hint does not list the jab key");
        Expect(fighter.State == FighterState.Idle && Math.Abs(fighter.X - roster.PlayerX) < 1e-9, "player does not start Idle at ring.playerX");
        CheckInputMap();

        var rest = SnapshotParts(playerRoot);
        var opponentRest = SnapshotParts(opponentRoot);
        var opponentRootRest = opponentRoot.Position;

        // AC22: a jab from the start position is out of range: no contact text at any point.
        await Tap(InputActions.FightJab);
        await WaitUntil(() => fighter.State == FighterState.Jab, 3, "jab to start");
        for (var i = 0; i < moves.Get(FighterState.Jab).TotalFrames + 2; i++)
        {
            Expect(!fight.ContactLabel.Visible, "contact text shown for an out-of-range jab");
            await NextFrame();
        }

        await WaitUntil(() => fighter.State == FighterState.Idle, 30, "idle after jab");
        ExpectPoseAtRest(playerRoot, rest, "after the jab");

        // AC19: A and D together do not move the boxer; arrow keys do nothing.
        var startX = fighter.X;
        Input.ActionPress(InputActions.FightForward);
        Input.ActionPress(InputActions.FightBack);
        await Frames(20);
        Input.ActionRelease(InputActions.FightForward);
        Input.ActionRelease(InputActions.FightBack);
        Expect(Math.Abs(fighter.X - startX) < 1e-9, "A and D together moved the boxer");
        foreach (var arrow in new[] { "ui_left", "ui_right", "ui_up", "ui_down", "menu_left", "menu_right" })
        {
            Input.ActionPress(arrow);
        }

        await Frames(10);
        foreach (var arrow in new[] { "ui_left", "ui_right", "ui_up", "ui_down", "menu_left", "menu_right" })
        {
            Input.ActionRelease(arrow);
        }

        Expect(Math.Abs(fighter.X - startX) < 1e-9, "an arrow key moved the boxer");

        // AC17: the first D press moves the boxer toward the opponent with no delay (within a few frames).
        Input.ActionPress(InputActions.FightForward);
        await Frames(6);
        Input.ActionRelease(InputActions.FightForward);
        Expect(fighter.X > startX, "D did not move the boxer forward");
        Expect(Math.Abs(playerRoot.Position.X - (float)fighter.X) < 1e-4f, "the player node does not follow Core X");
        Input.ActionPress(InputActions.FightBack);
        await WaitUntil(() => fighter.X <= startX + 1e-9, 60, "walking back to the start");
        Input.ActionRelease(InputActions.FightBack);
        await NextFrame();
        Expect(Math.Abs(fighter.X - startX) < 1e-9, "the player did not return to the start position");

        // AC19: holding a key for a long time throws exactly one move.
        await CheckHeldKeyThrowsOnce(InputActions.FightJab, FighterState.Jab, 120);
        await CheckHeldKeyThrowsOnce(InputActions.FightCross, FighterState.Cross, 40);
        await CheckHeldKeyThrowsOnce(InputActions.FightSlipLeft, FighterState.SlipLeft, 40);
        await CheckHeldKeyThrowsOnce(InputActions.FightSlipRight, FighterState.SlipRight, 40);

        // AC20: poses.
        var restGloveL = rest["GloveL"].Origin;
        var restGloveR = rest["GloveR"].Origin;
        var minExtend = 0.5f * (float)moves.Visual.PunchExtendH * height;

        await Tap(InputActions.FightJab);
        await WaitUntil(() => fighter.State == FighterState.Jab && fighter.Phase == MovePhase.Active, 12, "jab active");
        var leadMove = (playerRoot.GetNode<Node3D>("GloveL").Position - restGloveL).X;
        Expect(leadMove >= minExtend, $"jab: lead glove moved {leadMove:0.000} m, needs at least {minExtend:0.000}");
        Expect((playerRoot.GetNode<Node3D>("GloveR").Position - restGloveR).X <= 0.001f, "jab: the rear glove moved forward");
        GD.Print($"SMOKE: jab Active lead glove +{leadMove:0.000} m, front edge {GloveFront(playerRoot, "GloveL") - playerRoot.Position.X:0.000} m from root (reach {moves.Get(FighterState.Jab).ReachM} m)");
        ExpectReachesHead(playerRoot, opponentRoot, "GloveL", moves.Get(FighterState.Jab).ReachM, "jab");
        await WaitUntil(() => fighter.State == FighterState.Idle, 40, "idle after jab");
        await NextFrame();
        ExpectPoseAtRest(playerRoot, rest, "after the jab (pose)");

        await Tap(InputActions.FightCross);
        await WaitUntil(() => fighter.State == FighterState.Cross && fighter.Phase == MovePhase.Active, 14, "cross active");
        var rearMove = (playerRoot.GetNode<Node3D>("GloveR").Position - restGloveR).X;
        Expect(rearMove >= minExtend, $"cross: rear glove moved {rearMove:0.000} m, needs at least {minExtend:0.000}");
        Expect((playerRoot.GetNode<Node3D>("GloveL").Position - restGloveL).X <= 0.001f, "cross: the lead glove moved forward");
        GD.Print($"SMOKE: cross Active rear glove +{rearMove:0.000} m, front edge {GloveFront(playerRoot, "GloveR") - playerRoot.Position.X:0.000} m from root (reach {moves.Get(FighterState.Cross).ReachM} m)");
        ExpectReachesHead(playerRoot, opponentRoot, "GloveR", moves.Get(FighterState.Cross).ReachM, "cross");
        await WaitUntil(() => fighter.State == FighterState.Idle, 40, "idle after cross");
        await NextFrame();
        ExpectPoseAtRest(playerRoot, rest, "after the cross");

        Input.ActionPress(InputActions.FightGuard);
        await WaitUntil(() => fighter.State == FighterState.Guarding, 12, "guarding");
        foreach (var glove in new[] { "GloveL", "GloveR" })
        {
            Expect(playerRoot.GetNode<Node3D>(glove).Position.Y > rest[glove].Origin.Y + 0.005f, $"guard: {glove} is not higher than at rest");
        }

        Input.ActionRelease(InputActions.FightGuard);
        await WaitUntil(() => fighter.State == FighterState.Idle, 6, "idle after guard");
        await NextFrame();
        ExpectPoseAtRest(playerRoot, rest, "after the guard");

        var rootBefore = playerRoot.GlobalPosition;
        var turn = new Dictionary<FighterState, float>();
        foreach (var (action, state, side) in new[]
        {
            (InputActions.FightSlipLeft, FighterState.SlipLeft, -1f),
            (InputActions.FightSlipRight, FighterState.SlipRight, 1f),
        })
        {
            await Tap(action);
            await WaitUntil(() => fighter.State == state && fighter.Phase == MovePhase.Active, 12, "slip active");
            Expect(fighter.IsEvading, "slip: IsEvading is false in Active");
            var head = playerRoot.GetNode<Node3D>("Head").Position;
            var headRest = rest["Head"].Origin;
            Expect(head.Y < headRest.Y - 0.02f, $"{state}: the head did not dip");
            Expect((head.Z - headRest.Z) * side > 0.02f, $"{state}: the head did not lean to its side");
            Expect(playerRoot.GlobalPosition.IsEqualApprox(rootBefore), $"{state}: the root moved");
            foreach (var foot in new[] { "FootL", "FootR", "UpperLegL", "UpperLegR" })
            {
                Expect((playerRoot.GetNode<Node3D>(foot).Position - rest[foot].Origin).Length() < 0.001f, $"{state}: {foot} moved");
            }

            turn[state] = playerRoot.GetNode<Node3D>("ShoulderL").Position.X - playerRoot.GetNode<Node3D>("ShoulderR").Position.X;
            await WaitUntil(() => fighter.State == FighterState.Idle, 60, "idle after slip");
            await NextFrame();
            ExpectPoseAtRest(playerRoot, rest, $"after {state}");
        }

        Expect(turn[FighterState.SlipLeft] * turn[FighterState.SlipRight] < 0f, "the slips do not turn the shoulders in opposite directions");

        // AC21 / AC22: walk until stopped, never past the minimum separation.
        Input.ActionPress(InputActions.FightForward);
        await WaitUntil(() => fighter.X >= roster.OpponentX - moves.MinSeparationM - 1e-9, 300, "reaching the minimum separation");
        await Frames(30);
        Input.ActionRelease(InputActions.FightForward);
        Expect(Math.Abs(fighter.X - (roster.OpponentX - moves.MinSeparationM)) < 1e-9, "the player did not stop at the minimum separation");
        Expect(playerRoot.Position.X < opponentRoot.Position.X, "the player passed the opponent");

        await Tap(InputActions.FightJab);
        await WaitUntil(() => fight.ContactLabel.Visible, 12, "contact text for the jab");
        Expect(fight.ContactLabel.Text == "Contact: Jab", $"unexpected contact text '{fight.ContactLabel.Text}'");
        var visibleFrames = 0;
        while (fight.ContactLabel.Visible && visibleFrames < moves.ContactTextFrames + 10)
        {
            visibleFrames++;
            await NextFrame();
        }

        Expect(Math.Abs(visibleFrames - moves.ContactTextFrames) <= 2, $"contact text stayed {visibleFrames} frames, expected {moves.ContactTextFrames}");
        await WaitUntil(() => fighter.State == FighterState.Idle, 40, "idle after the contact jab");

        await Tap(InputActions.FightCross);
        await WaitUntil(() => fight.ContactLabel.Visible, 14, "contact text for the cross");
        Expect(fight.ContactLabel.Text == "Contact: Cross", $"unexpected contact text '{fight.ContactLabel.Text}'");
        await WaitUntil(() => fighter.State == FighterState.Idle, 40, "idle after the contact cross");

        GD.Print($"SMOKE: opponent glove front {opponentRoot.Position.X - GloveMinX(opponentRoot, "GloveL"):0.000} m (L) / {opponentRoot.Position.X - GloveMinX(opponentRoot, "GloveR"):0.000} m (R) in front of its root");

        // The idle opponent never reacts (AC21).
        Expect(opponentRoot.Position.IsEqualApprox(opponentRootRest), "the opponent moved");
        var opponentNow = SnapshotParts(opponentRoot);
        foreach (var (name, transform) in opponentRest)
        {
            Expect(opponentNow[name].IsEqualApprox(transform), $"the opponent's {name} changed pose");
        }
    }

    private void CheckInputMap()
    {
        var expected = new (StringName Action, Key Key)[]
        {
            (InputActions.FightForward, Key.D), (InputActions.FightBack, Key.A), (InputActions.FightJab, Key.J),
            (InputActions.FightCross, Key.K), (InputActions.FightGuard, Key.S), (InputActions.FightSlipLeft, Key.Q),
            (InputActions.FightSlipRight, Key.E),
        };
        var forbidden = new[] { Key.Escape, Key.Enter, Key.KpEnter, Key.Left, Key.Right, Key.Up, Key.Down, Key.Space };
        foreach (var (action, key) in expected)
        {
            Expect(InputMap.HasAction(action), $"{action} is not in the InputMap");
            var events = InputMap.ActionGetEvents(action);
            Expect(events.Count == 1, $"{action} must have exactly one event, has {events.Count}");
            var k = events[0] as InputEventKey ?? throw new InvalidOperationException($"{action} is not a key event");
            Expect(k.PhysicalKeycode == key && k.Keycode == Key.None, $"{action} is not bound by physical keycode {key}");
            Expect(Array.IndexOf(forbidden, k.PhysicalKeycode) < 0, $"{action} uses a reserved key");
        }

        Expect(PhysicalKeys("ui_back").SetEquals(new[] { Key.Escape }), "ui_back changed");
        Expect(PhysicalKeys("menu_up").SetEquals(new[] { Key.Up }), "menu_up changed");
        Expect(PhysicalKeys("menu_down").SetEquals(new[] { Key.Down }), "menu_down changed");
        Expect(PhysicalKeys("menu_confirm").SetEquals(new[] { Key.Enter, Key.KpEnter }), "menu_confirm changed");
    }

    private static HashSet<Key> PhysicalKeys(StringName action)
    {
        var keys = new HashSet<Key>();
        foreach (var e in InputMap.ActionGetEvents(action))
        {
            if (e is InputEventKey k)
            {
                keys.Add(k.PhysicalKeycode);
            }
        }

        return keys;
    }

    private async Task CheckHeldKeyThrowsOnce(StringName action, FighterState state, int heldFrames)
    {
        var fighter = _main!.GetNode<FightController>("Fight").Fighter;
        await WaitUntil(() => fighter.State == FighterState.Idle, 60, "idle before a held key");
        var starts = 0;
        Input.ActionPress(action);
        for (var i = 0; i < heldFrames; i++)
        {
            await NextFrame();
            if (fighter.State == state && fighter.Phase == MovePhase.Startup && fighter.PhaseFrame == 1)
            {
                starts++;
            }
        }

        Input.ActionRelease(action);
        Expect(starts == 1, $"holding {action} for {heldFrames} frames started {starts} moves, expected exactly 1");
        await WaitUntil(() => fighter.State == FighterState.Idle, 60, "idle after a held key");
    }

    private static Dictionary<string, Transform3D> SnapshotParts(Node3D root)
    {
        var map = new Dictionary<string, Transform3D>();
        foreach (var child in root.GetChildren())
        {
            if (child is Node3D node)
            {
                map[node.Name] = node.Transform;
            }
        }

        return map;
    }

    private static void ExpectPoseAtRest(Node3D root, Dictionary<string, Transform3D> rest, string when)
    {
        foreach (var name in PosedParts)
        {
            var now = root.GetNode<Node3D>(name).Transform;
            var home = rest[name];
            Expect((now.Origin - home.Origin).Length() < 0.001f, $"{name} is not back at its guard position {when}");
            Expect(now.Basis.IsEqualApprox(home.Basis), $"{name} is not back at its guard orientation {when}");
        }
    }

    /// <summary>At gap == reach the extended glove must reach the opponent's head front, without going past its root.</summary>
    private static void ExpectReachesHead(Node3D playerRoot, Node3D opponentRoot, string glove, double reach, string punch)
    {
        var head = opponentRoot.GetNode<MeshInstance3D>("Head");
        var headFront = opponentRoot.Position.X - (head.GlobalTransform * head.Mesh.GetAabb()).Position.X;
        var tip = GloveFront(playerRoot, glove) - playerRoot.Position.X;
        GD.Print($"SMOKE: {punch} tip {tip:0.000} m from root; at gap == reach ({reach} m) it ends {reach - tip:0.000} m from the opponent's root (head front {headFront:0.000} m)");
        Expect(tip >= reach - headFront - 0.03, $"{punch}: at gap == reach the glove falls short of the opponent's head");
        Expect(tip < reach, $"{punch}: at gap == reach the glove goes past the opponent's root");
    }

    private static float GloveMinX(Node3D root, string glove)
    {
        var node = root.GetNode<MeshInstance3D>(glove);
        return (node.GlobalTransform * node.Mesh.GetAabb()).Position.X;
    }

    private static float GloveFront(Node3D root, string glove)
    {
        var node = root.GetNode<MeshInstance3D>(glove);
        var box = node.GlobalTransform * node.Mesh.GetAabb();
        return box.End.X;
    }

    private static void Expect(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private async Task Tap(StringName action)
    {
        Input.ActionPress(action);
        await NextFrame();
        Input.ActionRelease(action);
    }

    private async Task Frames(int count)
    {
        for (var i = 0; i < count; i++)
        {
            await NextFrame();
        }
    }

    private async Task NextFrame()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
    }

    private async Task WaitUntil(Func<bool> condition, int maxFrames, string what)
    {
        for (var i = 0; i <= maxFrames; i++)
        {
            if (condition())
            {
                return;
            }

            await NextFrame();
        }

        throw new InvalidOperationException($"timed out waiting for {what}");
    }
}
