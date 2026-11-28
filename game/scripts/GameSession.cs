using Boxing.Core;
using Godot;

namespace Boxing.Game;

/// <summary>Static hand-off of the select screen's choices to the ring, plus the loaded roster.</summary>
public static class GameSession
{
    public const string RosterPath = "res://data/balance/fighters.json";

    public const string MovesPath = "res://data/balance/player-moves.json";

    private static FighterRoster? _roster;
    private static MoveSet? _moves;

    public static string? PlayerId { get; set; }

    public static string? OpponentId { get; set; }

    /// <summary>The roster, loaded once from the data file (works in the editor and in exports).</summary>
    public static FighterRoster Roster => _roster ??= FighterRoster.Parse(FileAccess.GetFileAsString(RosterPath));

    /// <summary>The player's move data, loaded once; validated against the ring positions of the roster.</summary>
    public static MoveSet Moves => _moves ??= MoveSet.Parse(FileAccess.GetFileAsString(MovesPath), Roster.PlayerX, Roster.OpponentX);

    public static void Clear()
    {
        PlayerId = null;
        OpponentId = null;
    }
}
