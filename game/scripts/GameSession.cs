using Boxing.Core;
using Godot;

namespace Boxing.Game;

/// <summary>Static hand-off of the select screen's choices to the ring, plus the loaded roster.</summary>
public static class GameSession
{
    public const string RosterPath = "res://data/balance/fighters.json";

    private static FighterRoster? _roster;

    public static string? PlayerId { get; set; }

    public static string? OpponentId { get; set; }

    /// <summary>The roster, loaded once from the data file (works in the editor and in exports).</summary>
    public static FighterRoster Roster => _roster ??= FighterRoster.Parse(FileAccess.GetFileAsString(RosterPath));

    public static void Clear()
    {
        PlayerId = null;
        OpponentId = null;
    }
}
