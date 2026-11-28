using System;
using Boxing.Core;
using Godot;

namespace Boxing.Game;

/// <summary>
/// Builds a standing, guard-pose human boxer from primitives (no external assets). Every proportion and
/// colour comes from data/balance/fighters.json (<see cref="FighterBody"/> and <see cref="FighterLook"/>).
/// Coordinates: X toward the opponent is <c>facing</c>; the boxer's right side is +Z * facing, left is -Z * facing.
/// </summary>
public static class FighterBuilder
{
    /// <param name="facing">+1 faces +X, -1 faces -X.</param>
    /// <param name="stance">Orthodox: the left foot leads. Southpaw: the right foot leads (the guard itself is left/right symmetric).</param>
    public static Node3D Build(string nodeName, FighterDef fighter, FighterRoster roster, string tag, float facing, Stance stance)
    {
        var look = fighter.Look;
        var b = roster.Body;
        var h = (float)(roster.BaseHeight * look.HeightScale);
        var w = (float)(roster.BaseWidth * look.WidthScale);
        float H(string key) => (float)b[key] * h;
        float W(string key) => (float)b[key] * w;

        var skin = ToColor(look.SkinColor);
        var root = new Node3D { Name = nodeName };

        // Vertical layout, bottom to top.
        var footH = H("footHeightH");
        var legRadius = W("legRadiusW") * (float)look.LegThicknessScale;
        var lowerLen = H("lowerLegLengthH");
        var upperLen = H("upperLegLengthH");
        var hipY = footH + lowerLen + upperLen;
        var shortsH = H("shortsHeightH");
        var shortsTop = hipY + (shortsH * 0.5f);
        var bandH = H("waistbandHeightH");
        var torsoH = H("torsoHeightH");
        var torsoBottom = shortsTop - bandH;
        var torsoTop = torsoBottom + torsoH;
        var neckH = H("neckHeightH");
        var neckR = W("neckRadiusW") * (float)look.NeckScale;
        var headRx = H("headRadiusH") * (float)look.HeadScale;
        var headStretch = (float)b["headStretchY"];
        var headRy = headRx * headStretch;
        var headY = torsoTop + neckH + (headRy * 0.8f);

        // Legs and feet: lead (left) foot forward, feet apart.
        var hipHalf = W("torsoWidthW") * 0.25f;
        var footHalf = W("stanceFootSpreadW") * 0.5f;
        var footLen = H("footLengthH");
        for (var side = -1; side <= 1; side += 2)
        {
            var suffix = side < 0 ? "L" : "R";
            var zSign = side * facing;
            var leadForward = (side < 0 ? BodySide.Left : BodySide.Right) == StanceRules.LeadSide(stance) ? W("stanceLeadFootForwardW") : 0f;
            var hip = new Vector3(0f, hipY, zSign * hipHalf);
            var ankle = new Vector3(facing * leadForward, footH, zSign * footHalf);
            var knee = ((hip + ankle) * 0.5f) + new Vector3(facing * h * 0.03f, 0f, 0f);
            root.AddChild(Limb("UpperLeg" + suffix, hip, knee, legRadius, legRadius * 0.85f, skin));
            root.AddChild(Limb("LowerLeg" + suffix, knee, ankle, legRadius * 0.85f, legRadius * 0.7f, skin));
            root.AddChild(Part(
                "Foot" + suffix,
                new BoxMesh { Size = new Vector3(footLen, footH, legRadius * 2.2f) },
                skin,
                new Vector3(ankle.X + (facing * footLen * 0.25f), footH * 0.5f, ankle.Z)));
        }

        // Shorts and waistband.
        var hipWidth = W("torsoWidthW") * 0.95f;
        var shortsDepth = W("torsoDepthW") * 1.15f;
        root.AddChild(Part("Shorts", new BoxMesh { Size = new Vector3(shortsDepth, shortsH, hipWidth) }, ToColor(look.TrunksColor), new Vector3(0f, hipY, 0f)));
        root.AddChild(Part("Waistband", new BoxMesh { Size = new Vector3(shortsDepth + 0.01f, bandH, hipWidth + 0.01f) }, ToColor(look.WaistbandColor), new Vector3(0f, shortsTop - (bandH * 0.5f), 0f)));

        // Torso: tapered (wide shoulders, narrower waist), a cylinder scaled to an ellipse.
        var shoulderHalf = W("shoulderSpanW") * (float)look.ShoulderScale * 0.5f;
        var waistHalf = Math.Min(shoulderHalf, W("torsoWidthW") * 0.5f * 0.8f);
        var torso = Part(
            "Torso",
            new CylinderMesh { TopRadius = 1f, BottomRadius = waistHalf / shoulderHalf, Height = torsoH, RadialSegments = 24 },
            skin,
            new Vector3(0f, torsoBottom + (torsoH * 0.5f), 0f));
        torso.Scale = new Vector3(W("torsoDepthW") * 0.5f, 1f, shoulderHalf);
        root.AddChild(torso);

        // Neck and head.
        root.AddChild(Part("Neck", new CylinderMesh { TopRadius = neckR, BottomRadius = neckR * 1.1f, Height = neckH * 1.6f }, skin, new Vector3(0f, torsoTop + (neckH * 0.5f), 0f)));
        var head = Part("Head", new SphereMesh { Radius = headRx, Height = headRx * 2f }, skin, new Vector3(0f, headY, 0f));
        head.Scale = new Vector3(1f, headStretch, 1f);
        root.AddChild(head);

        var cap = (float)HairStyles.CapScale(look.HairStyle);
        var hair = Part("Hair", new SphereMesh { Radius = headRx * cap, Height = headRx * cap, IsHemisphere = true }, ToColor(look.HairColor), new Vector3(0f, headY + (headRy * 0.2f), 0f));
        hair.Scale = new Vector3(1f, headStretch, 1f);
        root.AddChild(hair);

        // Face cues on the side facing the opponent.
        var eyeR = H("eyeRadiusH");
        var eyeColor = ToColor(roster.Body.EyeColor);
        const float EyeZ = 0.35f;
        const float EyeUp = 0.05f;
        var eyeX = headRx * MathF.Sqrt(1f - (EyeZ * EyeZ) - (EyeUp * EyeUp));
        for (var side = -1; side <= 1; side += 2)
        {
            root.AddChild(Part(
                side < 0 ? "EyeL" : "EyeR",
                new SphereMesh { Radius = eyeR, Height = eyeR * 2f },
                eyeColor,
                new Vector3(facing * eyeX, headY + (headRy * EyeUp), side * facing * headRx * EyeZ)));
        }

        var noseR = H("noseRadiusH");
        root.AddChild(Part("Nose", new SphereMesh { Radius = noseR, Height = noseR * 2f }, skin, new Vector3(facing * ((headRx * 0.97f) + (noseR * 0.5f)), headY - (headRy * 0.2f), 0f)));
        root.AddChild(Part("Mouth", new BoxMesh { Size = new Vector3(noseR * 0.6f, noseR * 0.5f, headRx * 0.7f) }, ToColor(roster.Body.MouthColor), new Vector3(facing * headRx * 0.86f, headY - (headRy * 0.5f), 0f)));

        // Shoulders and arms in guard.
        var shoulderR = W("shoulderRadiusW");
        var shoulderY = torsoTop - (shoulderR * 0.6f);
        var armLenScale = (float)look.ArmLengthScale;
        var upperArmLen = H("upperArmLengthH") * armLenScale;
        var foreLen = H("forearmLengthH") * armLenScale;
        var armR = W("armRadiusW") * (float)look.ArmThicknessScale;
        var gloveR = H("gloveRadiusH");
        var gloveStretch = (float)b["gloveStretch"];
        var gloveOffset = gloveR * gloveStretch * 0.6;
        var gloveColor = ToColor(look.GloveColor);
        var pose = GuardPose.Solve(upperArmLen, foreLen, gloveOffset, b["guardElbowBendDeg"], (b["guardGloveHeightH"] * h) - shoulderY);

        for (var side = -1; side <= 1; side += 2)
        {
            var suffix = side < 0 ? "L" : "R";
            var zSign = side * facing;
            var shoulder = new Vector3(0f, shoulderY, zSign * shoulderHalf);
            root.AddChild(Part("Shoulder" + suffix, new SphereMesh { Radius = shoulderR, Height = shoulderR * 2f }, skin, shoulder));

            // Tuck the arm inward so the gloves end up in front of the face rather than out at the shoulders.
            var elbow = shoulder + new Vector3(facing * (float)pose.Elbow.Fwd, (float)pose.Elbow.Up, -zSign * shoulderHalf * 0.2f);
            var wristZ = zSign * Math.Max(gloveR * 1.1f, shoulderHalf * 0.2f);
            var wrist = new Vector3(shoulder.X + (facing * (float)pose.Wrist.Fwd), shoulderY + (float)pose.Wrist.Up, wristZ);
            var gloveDir = (wrist - elbow).Normalized();
            var glovePos = wrist + (gloveDir * (float)gloveOffset);

            root.AddChild(Limb("UpperArm" + suffix, shoulder, elbow, armR, armR * 0.9f, skin));
            root.AddChild(Limb("Forearm" + suffix, elbow, wrist, armR * 0.9f, armR * 0.75f, skin));
            var glove = Part("Glove" + suffix, new SphereMesh { Radius = gloveR, Height = gloveR * 2f }, gloveColor, Vector3.Zero);
            glove.Transform = new Transform3D(Orient(gloveDir).Scaled(new Vector3(1f, gloveStretch, 1f)), glovePos);
            root.AddChild(glove);
        }

        root.AddChild(new Label3D
        {
            Name = "Tag",
            Text = tag,
            FontSize = 96,
            PixelSize = 0.006f,
            Billboard = BaseMaterial3D.BillboardModeEnum.Enabled,
            NoDepthTest = true,
            Position = new Vector3(0f, headY + headRy + 0.25f, 0f),
        });
        return root;
    }

    /// <summary>A tapered cylinder from <paramref name="from"/> (radius rFrom) to <paramref name="to"/> (radius rTo).</summary>
    private static MeshInstance3D Limb(string name, Vector3 from, Vector3 to, float rFrom, float rTo, Color color)
    {
        var dir = to - from;
        var node = Part(name, new CylinderMesh { TopRadius = rTo, BottomRadius = rFrom, Height = dir.Length(), RadialSegments = 16 }, color, Vector3.Zero);
        node.Transform = new Transform3D(Orient(dir.Normalized()), (from + to) * 0.5f);
        return node;
    }

    /// <summary>Basis whose Y axis points along <paramref name="dir"/> (unit length).</summary>
    internal static Basis Orient(Vector3 dir)
    {
        var axis = Vector3.Up.Cross(dir);
        if (axis.LengthSquared() < 1e-8f)
        {
            return dir.Y >= 0f ? Basis.Identity : new Basis(Vector3.Right, Mathf.Pi);
        }

        return new Basis(axis.Normalized(), Vector3.Up.AngleTo(dir));
    }

    private static MeshInstance3D Part(string name, PrimitiveMesh mesh, Color color, Vector3 position)
    {
        mesh.Material = new StandardMaterial3D { AlbedoColor = color };
        return new MeshInstance3D { Name = name, Mesh = mesh, Position = position };
    }

    private static Color ToColor(string hex)
    {
        var (r, g, b) = FighterRoster.ParseHexColor(hex);
        return new Color((float)r, (float)g, (float)b);
    }
}
