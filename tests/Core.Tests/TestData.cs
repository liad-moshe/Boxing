using System;
using System.IO;
using System.Text.Json.Nodes;

namespace Boxing.Core.Tests;

/// <summary>Shipped data files, copied next to the test assembly, plus helpers to damage a copy.</summary>
internal static class TestData
{
    public static string MovesText() =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "player-moves.json"));

    public static FighterRoster Roster() => FighterRosterTests.Shipped();

    public static MoveSet Moves()
    {
        var roster = Roster();
        return MoveSet.Parse(MovesText(), roster.PlayerX, roster.OpponentX);
    }

    /// <summary>The shipped moves text with one edit applied to the parsed tree (path of keys, new value or null to remove).</summary>
    public static string Edited(Action<JsonNode> edit)
    {
        var root = JsonNode.Parse(MovesText())!;
        edit(root);
        return root.ToJsonString();
    }
}
