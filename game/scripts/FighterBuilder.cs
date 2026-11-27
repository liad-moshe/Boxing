using Boxing.Core;
using Godot;

namespace Boxing.Game;

/// <summary>Builds a standing, guard-pose fighter from primitives (no external assets).</summary>
public static class FighterBuilder
{
    /// <param name="facing">+1 faces +X, -1 faces -X.</param>
    public static Node3D Build(string nodeName, FighterDef fighter, FighterRoster roster, string tag, float facing)
    {
        var height = (float)(roster.BaseHeight * fighter.Look.HeightScale);
        var width = (float)(roster.BaseWidth * fighter.Look.WidthScale);
        var skin = ToColor(fighter.Look.SkinColor);
        var trunks = ToColor(fighter.Look.TrunksColor);
        var glove = ToColor(fighter.Look.GloveColor);

        var root = new Node3D { Name = nodeName };

        // Legs (skin), from the floor up to the trunks.
        for (var side = -1; side <= 1; side += 2)
        {
            root.AddChild(Part("Leg" + side, new CylinderMesh
            {
                TopRadius = width * 0.16f,
                BottomRadius = width * 0.14f,
                Height = height * 0.42f,
            }, skin, new Vector3(0f, height * 0.21f, side * width * 0.22f)));
        }

        root.AddChild(Part("Trunks", new BoxMesh { Size = new Vector3(width * 0.7f, height * 0.16f, width * 1.0f) }, trunks, new Vector3(0f, height * 0.46f, 0f)));
        root.AddChild(Part("Torso", new CapsuleMesh { Radius = width * 0.48f, Height = height * 0.42f }, skin, new Vector3(0f, height * 0.69f, 0f)));
        root.AddChild(Part("Head", new SphereMesh { Radius = height * 0.07f, Height = height * 0.14f }, skin, new Vector3(0f, height * 0.9f, 0f)));

        // Gloves raised in front of the chin.
        for (var side = -1; side <= 1; side += 2)
        {
            root.AddChild(Part("Glove" + side, new SphereMesh { Radius = height * 0.055f, Height = height * 0.11f }, glove,
                new Vector3(facing * width * 0.75f, height * 0.8f, side * width * 0.3f)));
        }

        var label = new Label3D
        {
            Name = "Tag",
            Text = tag,
            FontSize = 96,
            PixelSize = 0.006f,
            Billboard = BaseMaterial3D.BillboardModeEnum.Enabled,
            NoDepthTest = true,
            Position = new Vector3(0f, height + 0.25f, 0f),
        };
        root.AddChild(label);
        return root;
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
