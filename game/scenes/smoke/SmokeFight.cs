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
            // AC19 / AC20: the Character Select stance line (before the ring runs, so keys cannot disturb the fight).
            await SelectScript();

            // AC30 / AC24: the direct launch is orthodox for both fighters.
            await FightScript();
            await StanceScript(Stance.Orthodox, walkClose: false);

            // AC21 / AC25 / AC27: a southpaw player against an orthodox opponent, as Character Select would hand off.
            var first = _main ?? throw new InvalidOperationException("no Main");
            first.QueueFree();
            await Frames(3);
            GameSession.PlayerStance = Stance.Southpaw;
            GameSession.OpponentStance = Stance.Orthodox;
            var southpawMain = GD.Load<PackedScene>(MainScenePath).Instantiate();
            AddChild(southpawMain);
            _main = southpawMain;
            await Frames(3);
            await StanceScript(Stance.Southpaw, walkClose: true);
            GameSession.Clear();

            Expect(_turnSize.ContainsKey(Stance.Orthodox) && _turnSize.ContainsKey(Stance.Southpaw), "cross torso turn was not measured for both stances");
            Expect(
                Math.Abs(_turnSize[Stance.Orthodox] - _turnSize[Stance.Southpaw]) <= 0.05f * _turnSize[Stance.Orthodox],
                $"the southpaw cross turn ({_turnSize[Stance.Southpaw]:0.0000}) is not the same size as the orthodox one ({_turnSize[Stance.Orthodox]:0.0000})");
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

        ExpectGuardCovers(playerRoot, rest, height, moves, "guard");

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

    private readonly Dictionary<Stance, float> _turnSize = new();

    private async Task Send(string action, Func<Task> next)
    {
        Input.ParseInputEvent(new InputEventAction { Action = action, Pressed = true, Strength = 1f });
        await next();
        Input.ParseInputEvent(new InputEventAction { Action = action, Pressed = false });
        await next();
    }

    /// <summary>AC19 / AC20 where practical: toggling and confirming on a real CharacterSelect instance.</summary>
    private async Task SelectScript()
    {
        var select = GD.Load<PackedScene>(ScenePaths.CharacterSelect).Instantiate();
        AddChild(select);
        try
        {
            await Frames(2);
            var line = select.GetNode<Label>("Column/StanceLine");
            var header = select.GetNode<Label>("Column/Header");
            var you = select.GetNode<Label>("Column/YouLine");
            const string Orthodox = "Stance: Orthodox   (Up / Down to change)";
            const string Southpaw = "Stance: Southpaw   (Up / Down to change)";
            Expect(line.Text == Orthodox, $"step 1 stance line reads '{line.Text}'");
            Expect(line.FocusMode == Control.FocusModeEnum.None, "the stance line can take keyboard focus");

            await Send("menu_up", NextFrame);
            Expect(line.Text == Southpaw, $"Up did not give Southpaw ('{line.Text}')");
            await Send("menu_down", NextFrame);
            Expect(line.Text == Orthodox, $"Down did not give Orthodox ('{line.Text}')");
            await Send("menu_right", NextFrame);
            await Send("menu_left", NextFrame);
            Expect(line.Text == Orthodox, "moving the highlight changed the stance");

            line.EmitSignal(Control.SignalName.GuiInput, new InputEventMouseButton { Pressed = true, ButtonIndex = MouseButton.Left });
            await Frames(1);
            Expect(line.Text == Southpaw, "clicking the stance line did not toggle it");
            Expect(header.Text == "Choose your boxer", "clicking the stance line confirmed a card");

            await Send("menu_confirm", NextFrame);
            Expect(header.Text == "Choose your opponent", "Enter did not confirm the highlighted card");
            Expect(you.Text.StartsWith("You: ", StringComparison.Ordinal) && you.Text.EndsWith("(Southpaw)", StringComparison.Ordinal), $"step 2 'You' line reads '{you.Text}'");
            Expect(line.Text == Orthodox, $"step 2 stance line reads '{line.Text}', expected Orthodox");
        }
        finally
        {
            select.QueueFree();
            await Frames(2);
            GameSession.Clear();
        }
    }

    /// <summary>
    /// AC21 / AC24 / AC25 / AC26 / AC27 / AC28 on the ring's player in <paramref name="stance"/>: tag, feet, which glove
    /// throws what, the cross torso turn direction, the one-two, slip-recovery cancels, guard and contact.
    /// </summary>
    private async Task StanceScript(Stance stance, bool walkClose)
    {
        var main = _main ?? throw new InvalidOperationException("no Main");
        var fight = main.GetNode<FightController>("Fight");
        var fighter = fight.Fighter;
        var roster = GameSession.Roster;
        var moves = GameSession.Moves;
        var root = main.GetNode<Node3D>("PlayerFighter");
        var opponent = main.GetNode<Node3D>("OpponentFighter");
        var height = (float)(roster.BaseHeight * roster.FindOrFirst(GameSession.PlayerId).Look.HeightScale);
        var name = StanceRules.DisplayName(stance);
        var lead = StanceRules.LeadSide(stance) == BodySide.Left ? "GloveL" : "GloveR";
        var rear = lead == "GloveL" ? "GloveR" : "GloveL";
        var leadFoot = lead == "GloveL" ? "FootL" : "FootR";
        var rearFoot = lead == "GloveL" ? "FootR" : "FootL";
        var opponentStance = stance == Stance.Southpaw ? Stance.Orthodox : (GameSession.OpponentStance ?? moves.DefaultStance);

        // AC21 / AC30: tags and build.
        Expect(root.GetNode<Label3D>("Tag").Text == $"YOU ({name})", $"player tag reads '{root.GetNode<Label3D>("Tag").Text}'");
        Expect(opponent.GetNode<Label3D>("Tag").Text == $"CPU ({StanceRules.DisplayName(opponentStance)})", "opponent tag does not show its stance");
        Expect(root.GetNode<Node3D>(leadFoot).Position.X - root.GetNode<Node3D>(rearFoot).Position.X > 0.02f, $"{stance}: the lead foot is not further toward the opponent");
        var oppLead = StanceRules.LeadSide(opponentStance) == BodySide.Left ? "FootL" : "FootR";
        var oppRear = oppLead == "FootL" ? "FootR" : "FootL";
        Expect(opponent.GetNode<Node3D>(oppRear).Position.X - opponent.GetNode<Node3D>(oppLead).Position.X > 0.02f, "the opponent's lead foot is not further toward the player");

        await WaitUntil(() => fighter.State == FighterState.Idle, 60, "idle before the stance script");
        if (walkClose)
        {
            Input.ActionPress(InputActions.FightForward);
            await WaitUntil(() => fighter.X >= roster.OpponentX - moves.MinSeparationM - 1e-9, 300, "walking close");
            Input.ActionRelease(InputActions.FightForward);
            await Frames(2);
        }

        var rest = SnapshotParts(root);
        var restSpan = rest["EyeR"].Origin.X - rest["EyeL"].Origin.X;
        var minExtend = 0.5f * (float)moves.Visual.PunchExtendH * height;
        float GloveMove(string glove) => (root.GetNode<Node3D>(glove).Position - rest[glove].Origin).X;
        float Turn() => (root.GetNode<Node3D>("EyeR").Position.X - root.GetNode<Node3D>("EyeL").Position.X) - restSpan; // the eyes ride rigidly with the upper body, so only the torso turn moves them in X

        // AC24 / AC25: the jab is the lead glove with no torso turn, the cross the rear glove with the rear shoulder forward.
        await Tap(InputActions.FightJab);
        await WaitUntil(() => fighter.State == FighterState.Jab && fighter.Phase == MovePhase.Active, 12, "jab active");
        Expect(GloveMove(lead) >= minExtend, $"{stance} jab: {lead} moved {GloveMove(lead):0.000} m, needs {minExtend:0.000}");
        Expect(GloveMove(rear) <= 0.001f, $"{stance} jab: {rear} moved forward");
        Expect(Math.Abs(Turn()) < 0.002f, $"{stance} jab turned the torso ({Turn():0.0000})");
        await WaitUntil(() => fighter.State == FighterState.Idle, 40, "idle after jab");
        await NextFrame();
        ExpectPoseAtRest(root, rest, $"after the {stance} jab");

        await Tap(InputActions.FightCross);
        await WaitUntil(() => fighter.State == FighterState.Cross && fighter.Phase == MovePhase.Active, 14, "cross active");
        Expect(GloveMove(rear) >= minExtend, $"{stance} cross: {rear} moved {GloveMove(rear):0.000} m, needs {minExtend:0.000}");
        Expect(GloveMove(lead) <= 0.001f, $"{stance} cross: {lead} moved forward");
        var turn = Turn();
        Expect(stance == Stance.Orthodox ? turn > 0.01f : turn < -0.01f, $"{stance} cross: wrong torso turn direction ({turn:0.0000})");
        _turnSize[stance] = Math.Abs(turn);
        await WaitUntil(() => fighter.State == FighterState.Idle, 40, "idle after cross");
        await NextFrame();
        ExpectPoseAtRest(root, rest, $"after the {stance} cross");

        // AC26: the one-two. The cross starts on the first jab Recovery tick; the jab glove is the lead glove.
        await Tap(InputActions.FightJab);
        await WaitUntil(() => fighter.State == FighterState.Jab && fighter.Phase == MovePhase.Recovery, 20, "jab recovery");
        await Tap(InputActions.FightCross);
        await WaitUntil(() => fighter.State == FighterState.Cross, 3, "cross out of the jab");
        await WaitUntil(() => fighter.State == FighterState.Idle, 60, "idle after the one-two");
        await NextFrame();
        ExpectPoseAtRest(root, rest, $"after the {stance} one-two");

        // AC27: slips, then a punch out of slip Recovery uses the stance's correct glove; guard raises both gloves.
        foreach (var (action, state, side) in new[]
        {
            (InputActions.FightSlipLeft, FighterState.SlipLeft, -1f),
            (InputActions.FightSlipRight, FighterState.SlipRight, 1f),
        })
        {
            await Tap(action);
            await WaitUntil(() => fighter.State == state && fighter.Phase == MovePhase.Active, 12, "slip active");
            var head = root.GetNode<Node3D>("Head").Position;
            Expect(head.Y < rest["Head"].Origin.Y - 0.02f, $"{stance} {state}: the head did not dip");
            Expect((head.Z - rest["Head"].Origin.Z) * side > 0.02f, $"{stance} {state}: the head did not lean to its own side");
            await WaitUntil(() => fighter.State == state && fighter.Phase == MovePhase.Recovery, 30, "slip recovery");
            await Tap(side < 0 ? InputActions.FightJab : InputActions.FightCross);
            var punch = side < 0 ? FighterState.Jab : FighterState.Cross;
            await WaitUntil(() => fighter.State == punch && fighter.Phase == MovePhase.Active, 14, "punch out of slip recovery");
            var expectedGlove = side < 0 ? lead : rear;
            var otherGlove = side < 0 ? rear : lead;
            Expect(GloveMove(expectedGlove) >= minExtend && GloveMove(otherGlove) <= 0.001f, $"{stance}: wrong glove after {state}");
            await WaitUntil(() => fighter.State == FighterState.Idle, 60, "idle after slip counter");
            await NextFrame();
            ExpectPoseAtRest(root, rest, $"after {stance} {state} and its counter");
        }

        Input.ActionPress(InputActions.FightGuard);
        await WaitUntil(() => fighter.State == FighterState.Guarding, 12, "guarding");
        foreach (var glove in new[] { "GloveL", "GloveR" })
        {
            Expect(root.GetNode<Node3D>(glove).Position.Y > rest[glove].Origin.Y + 0.005f, $"{stance} guard: {glove} is not higher than at rest");
        }

        ExpectGuardCovers(root, rest, height, moves, $"{stance} guard");

        Input.ActionRelease(InputActions.FightGuard);
        await WaitUntil(() => fighter.State == FighterState.Idle, 6, "idle after guard");
        await NextFrame();
        ExpectPoseAtRest(root, rest, $"after the {stance} guard");

        // AC28: contact text matches the punch whatever the stance (the ring has the player at minimum separation here).
        await WaitUntil(() => !fight.ContactLabel.Visible, 120, "earlier contact text to clear");
        await Tap(InputActions.FightJab);
        await WaitUntil(() => fight.ContactLabel.Visible, 12, "contact text for the jab");
        Expect(fight.ContactLabel.Text == "Contact: Jab", $"{stance}: unexpected contact text '{fight.ContactLabel.Text}'");
        await WaitUntil(() => fighter.State == FighterState.Idle && !fight.ContactLabel.Visible, 120, "idle after the contact jab");
        await Tap(InputActions.FightCross);
        await WaitUntil(() => fight.ContactLabel.Visible, 14, "contact text for the cross");
        Expect(fight.ContactLabel.Text == "Contact: Cross", $"{stance}: unexpected contact text '{fight.ContactLabel.Text}'");
        await WaitUntil(() => fighter.State == FighterState.Idle && !fight.ContactLabel.Visible, 120, "idle after the contact cross");
        GD.Print($"SMOKE: {stance} player ok (cross torso turn {_turnSize[stance]:0.0000} m of eye span)");
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

    /// <summary>
    /// AC31 / AC33 on a Guarding player (faces +X): each glove is at least 0.8 x guardGloveRaiseH x H higher than at rest, its centre is
    /// between the chin and the top of the head and 0.02 H to 0.20 H ahead of the head's front, and no limb stretches more than 15 percent.
    /// </summary>
    private static void ExpectGuardCovers(Node3D root, Dictionary<string, Transform3D> rest, float height, MoveSet moves, string when)
    {
        var head = root.GetNode<MeshInstance3D>("Head");
        var headRy = ((SphereMesh)head.Mesh).Radius * head.Scale.Y;
        var chin = head.Position.Y - headRy;
        var top = head.Position.Y + headRy;
        var front = head.Position.X + ((SphereMesh)head.Mesh).Radius;
        foreach (var side in new[] { "L", "R" })
        {
            var glove = root.GetNode<Node3D>("Glove" + side);
            var rise = glove.Position.Y - rest["Glove" + side].Origin.Y;
            Expect(rise >= 0.8f * (float)moves.Visual.GuardGloveRaiseH * height - 0.001f, $"{when}: Glove{side} rose only {rise:0.000} m");
            Expect(glove.Position.Y >= chin && glove.Position.Y <= top, $"{when}: Glove{side} height {glove.Position.Y:0.000} is outside the head span {chin:0.000}..{top:0.000}");
            var ahead = glove.Position.X - front;
            Expect(ahead >= 0.02f * height - 0.001f && ahead <= 0.20f * height + 0.001f, $"{when}: Glove{side} is {ahead:0.000} m ahead of the head front (height {height:0.00})");
            Expect(Math.Sign(glove.Position.Z) == Math.Sign(rest["Glove" + side].Origin.Z), $"{when}: Glove{side} crossed the centre line ({glove.Position.Z:0.000} vs rest {rest["Glove" + side].Origin.Z:0.000})");
            foreach (var limb in new[] { "UpperArm", "Forearm" })
            {
                var node = root.GetNode<Node3D>(limb + side);
                var stretch = node.Transform.Basis.Y.Length() / rest[limb + side].Basis.Y.Length();
                Expect(Math.Abs(stretch - 1f) <= 0.15f, $"{when}: {limb}{side} length changed by {stretch:0.00}x");
            }
        }
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
