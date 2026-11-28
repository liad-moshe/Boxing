using System;
using Boxing.Core;
using Godot;

namespace Boxing.Game;

/// <summary>
/// Poses the existing primitive boxer parts from a <see cref="PoseBlend"/> (guard, jab, cross, slips).
/// Code-driven only: no AnimationPlayer, no assets. The blend amounts come from Boxing.Core; the offsets come
/// from the data file's <see cref="VisualTuning"/>. At an all-zero blend every part is restored to the exact
/// pose it was built in. The lead glove is <c>GloveL</c> (orthodox: left foot and left hand lead).
/// </summary>
public sealed class FighterAnimator
{
    // Parts that move rigidly with the upper body when it dips, leans or turns.
    private static readonly string[] UpperBodyParts = { "Head", "Hair", "Neck", "EyeL", "EyeR", "Nose", "Mouth", "Torso" };

    /// <summary>The punching shoulder drives forward by this fraction of the glove's extension.</summary>
    private const float ShoulderShare = 0.25f;

    /// <summary>How far the elbow drops/moves with the glove in guard, as a fraction of the glove's move.</summary>
    private const float GuardElbowShare = 0.5f;

    private readonly MoveSet _moves;
    private readonly float _facing;
    private readonly float _height;
    private readonly float _width;
    private readonly float _pivotY;
    private readonly (Node3D Node, Transform3D Home)[] _upper;
    private readonly Arm _lead;
    private readonly Arm _rear;
    private readonly float _crossExtraReach;

    public FighterAnimator(Node3D root, FighterDef fighter, FighterRoster roster, MoveSet moves, float facing)
    {
        _moves = moves;
        _facing = facing;
        _height = (float)(roster.BaseHeight * fighter.Look.HeightScale);
        _width = (float)(roster.BaseWidth * fighter.Look.WidthScale);
        _pivotY = root.GetNode<Node3D>("Waistband").Position.Y;

        _upper = new (Node3D, Transform3D)[UpperBodyParts.Length];
        for (var i = 0; i < UpperBodyParts.Length; i++)
        {
            var node = root.GetNode<Node3D>(UpperBodyParts[i]);
            _upper[i] = (node, node.Transform);
        }

        // One punchExtendH serves both punches, so the cross's glove travels further by exactly its extra reach (metres).
        _crossExtraReach = (float)(moves.Get(FighterState.Cross).ReachM - moves.Get(FighterState.Jab).ReachM);
        _lead = new Arm(root, "L");
        _rear = new Arm(root, "R");
    }

    public void Apply(PoseBlend pose)
    {
        if (pose == default)
        {
            Restore();
            return;
        }

        var v = _moves.Visual;
        var fwd = new Vector3(_facing, 0f, 0f);

        // Upper-body transform: dip, then lean about the facing axis, then turn about the vertical (spine) axis.
        var slip = (float)pose.Slip;
        var slipSign = pose.SlipSide == EvadeSide.Right ? 1f : pose.SlipSide == EvadeSide.Left ? -1f : 0f;
        var yaw = Mathf.DegToRad(((float)v.CrossTorsoTurnDeg * (float)pose.RearExtend) + ((float)v.SlipTurnDeg * slip * slipSign));
        var roll = Mathf.DegToRad((float)v.SlipLeanDeg * slip * slipSign * _facing);
        var rotation = new Basis(Vector3.Right, roll) * new Basis(Vector3.Up, yaw);
        var pivot = new Vector3(0f, _pivotY, 0f);
        var dip = new Vector3(0f, -(float)v.SlipHeadDropH * _height * slip, 0f);
        var upper = new Transform3D(rotation, pivot + dip - (rotation * pivot));

        foreach (var (node, home) in _upper)
        {
            node.Transform = upper * home;
        }

        PoseArm(_lead, (float)pose.LeadExtend, 0f, (float)pose.Guard, fwd, upper);
        PoseArm(_rear, (float)pose.RearExtend, _crossExtraReach, (float)pose.Guard, fwd, upper);
    }

    private void Restore()
    {
        foreach (var (node, home) in _upper)
        {
            node.Transform = home;
        }

        _lead.Restore();
        _rear.Restore();
    }

    private void PoseArm(Arm arm, float extend, float extraReach, float guard, Vector3 fwd, Transform3D upper)
    {
        var v = _moves.Visual;

        // Guard: glove up and inward (toward the centre line) by blend; the elbow follows part of the way.
        var raise = new Vector3(0f, (float)v.GuardGloveRaiseH * _height * guard, 0f);
        var inward = new Vector3(0f, 0f, -Math.Sign(arm.Wrist.Z) * (float)v.GuardGloveInwardW * _width * guard);
        var guardMove = raise + inward;

        // Punch: glove straight toward the opponent; the shoulder drives forward a little; the arm straightens.
        var punch = fwd * (((float)v.PunchExtendH * _height) + extraReach) * extend;

        var shoulder = arm.Shoulder + (punch * ShoulderShare);
        var wrist = arm.Wrist + guardMove + punch;
        var elbow = arm.Elbow + (guardMove * GuardElbowShare);
        var straight = shoulder + ((wrist - shoulder) * (arm.UpperLength / (arm.UpperLength + arm.ForeLength)));
        elbow = elbow.Lerp(straight, extend);

        var foreDir = (wrist - elbow).Normalized();
        var glovePos = wrist + (foreDir * arm.GloveOffset);

        // The glove turns with the forearm's change of direction.
        var restDir = (arm.Wrist - arm.Elbow).Normalized();
        var swing = restDir.Cross(foreDir);
        var delta = swing.LengthSquared() < 1e-10f ? Basis.Identity : new Basis(swing.Normalized(), restDir.AngleTo(foreDir));

        arm.ShoulderNode.Transform = new Transform3D(upper.Basis * arm.ShoulderRest.Basis, upper * shoulder);
        arm.UpperNode.Transform = Limb(upper * shoulder, upper * elbow, arm.UpperLength);
        arm.ForeNode.Transform = Limb(upper * elbow, upper * wrist, arm.ForeLength);
        arm.GloveNode.Transform = new Transform3D(upper.Basis * delta * arm.GloveRest.Basis, upper * glovePos);
    }

    /// <summary>A cylinder limb from <paramref name="from"/> to <paramref name="to"/>, stretched along its own axis from its built length.</summary>
    private static Transform3D Limb(Vector3 from, Vector3 to, float restLength)
    {
        var dir = to - from;
        var length = dir.Length();
        var basis = FighterBuilder.Orient(dir / length) * Basis.FromScale(new Vector3(1f, length / restLength, 1f));
        return new Transform3D(basis, (from + to) * 0.5f);
    }

    /// <summary>One arm's built (rest) geometry, read back from the primitive limbs.</summary>
    private sealed class Arm
    {
        public Arm(Node3D root, string side)
        {
            ShoulderNode = root.GetNode<MeshInstance3D>("Shoulder" + side);
            UpperNode = root.GetNode<MeshInstance3D>("UpperArm" + side);
            ForeNode = root.GetNode<MeshInstance3D>("Forearm" + side);
            GloveNode = root.GetNode<MeshInstance3D>("Glove" + side);
            ShoulderRest = ShoulderNode.Transform;
            UpperRest = UpperNode.Transform;
            ForeRest = ForeNode.Transform;
            GloveRest = GloveNode.Transform;

            UpperLength = ((CylinderMesh)UpperNode.Mesh).Height;
            ForeLength = ((CylinderMesh)ForeNode.Mesh).Height;
            var upperHalf = UpperRest.Basis.Y.Normalized() * (UpperLength * 0.5f);
            var foreHalf = ForeRest.Basis.Y.Normalized() * (ForeLength * 0.5f);
            Shoulder = UpperRest.Origin - upperHalf;
            Elbow = UpperRest.Origin + upperHalf;
            Wrist = ForeRest.Origin + foreHalf;
            GloveOffset = (GloveRest.Origin - Wrist).Length();
        }

        public MeshInstance3D ShoulderNode { get; }

        public MeshInstance3D UpperNode { get; }

        public MeshInstance3D ForeNode { get; }

        public MeshInstance3D GloveNode { get; }

        public Transform3D ShoulderRest { get; }

        public Transform3D UpperRest { get; }

        public Transform3D ForeRest { get; }

        public Transform3D GloveRest { get; }

        public float UpperLength { get; }

        public float ForeLength { get; }

        public Vector3 Shoulder { get; }

        public Vector3 Elbow { get; }

        public Vector3 Wrist { get; }

        public float GloveOffset { get; }

        public void Restore()
        {
            ShoulderNode.Transform = ShoulderRest;
            UpperNode.Transform = UpperRest;
            ForeNode.Transform = ForeRest;
            GloveNode.Transform = GloveRest;
        }
    }
}
