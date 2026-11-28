using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Boxing.Core;

/// <summary>What the player's fighter is doing. Jab, Cross, SlipLeft and SlipRight are the timed moves.</summary>
public enum FighterState
{
    Idle,
    GuardRaising,
    Guarding,
    Jab,
    Cross,
    SlipLeft,
    SlipRight,
}

/// <summary>Where a timed move is in its timeline. None for Idle and the guard states.</summary>
public enum MovePhase
{
    None,
    Startup,
    Active,
    Recovery,
}

/// <summary>Which side the head moved to during a slip (the boxer's own left or right).</summary>
public enum EvadeSide
{
    None,
    Left,
    Right,
}

/// <summary>A punch that reached the idle opponent on this tick.</summary>
public enum ContactKind
{
    None,
    Jab,
    Cross,
}

/// <summary>One timed move from the data file. All durations are whole ticks.</summary>
public sealed record MoveDef(
    int StartupFrames,
    int ActiveFrames,
    int RecoveryFrames,
    double ReachM,
    IReadOnlyList<FighterState> CancelInto)
{
    public int TotalFrames => StartupFrames + ActiveFrames + RecoveryFrames;
}

/// <summary>Pose offsets for the code-driven animation (fractions of fighter height H or width W, and degrees).</summary>
public sealed record VisualTuning(
    double PunchExtendH,
    double CrossTorsoTurnDeg,
    double GuardGloveRaiseH,
    double GuardGloveInwardW,
    double SlipHeadDropH,
    double SlipLeanDeg,
    double SlipTurnDeg);

/// <summary>Thrown when player-moves data is invalid; the message names the bad key.</summary>
public sealed class MoveDataException : Exception
{
    public MoveDataException(string message)
        : base(message)
    {
    }
}

/// <summary>The player's move timings, guard, movement and feedback values, parsed from player-moves.json text.</summary>
public sealed class MoveSet
{
    private static readonly FighterState[] MoveStates =
    {
        FighterState.Jab, FighterState.Cross, FighterState.SlipLeft, FighterState.SlipRight,
    };

    private readonly Dictionary<FighterState, MoveDef> _moves;

    private MoveSet(
        int tickRate,
        Dictionary<FighterState, MoveDef> moves,
        int guardRaiseFrames,
        double guardMoveSpeedMultiplier,
        double walkSpeedMps,
        double minX,
        double minSeparationM,
        int contactTextFrames,
        VisualTuning visual)
    {
        TickRate = tickRate;
        _moves = moves;
        GuardRaiseFrames = guardRaiseFrames;
        GuardMoveSpeedMultiplier = guardMoveSpeedMultiplier;
        WalkSpeedMps = walkSpeedMps;
        MinX = minX;
        MinSeparationM = minSeparationM;
        ContactTextFrames = contactTextFrames;
        Visual = visual;
    }

    public int TickRate { get; }

    public int GuardRaiseFrames { get; }

    public double GuardMoveSpeedMultiplier { get; }

    public double WalkSpeedMps { get; }

    public double MinX { get; }

    public double MinSeparationM { get; }

    public int ContactTextFrames { get; }

    public VisualTuning Visual { get; }

    /// <summary>True for Jab, Cross, SlipLeft and SlipRight.</summary>
    public static bool IsTimedMove(FighterState state) =>
        state is FighterState.Jab or FighterState.Cross or FighterState.SlipLeft or FighterState.SlipRight;

    /// <summary>The timed move for Jab, Cross, SlipLeft or SlipRight.</summary>
    public MoveDef Get(FighterState state) =>
        _moves.TryGetValue(state, out var def) ? def : throw new ArgumentException($"{state} is not a timed move", nameof(state));

    /// <summary>Parses and validates the data. <paramref name="playerX"/> and <paramref name="opponentX"/> come from fighters.json (ring).</summary>
    public static MoveSet Parse(string json, double playerX, double opponentX)
    {
        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(json);
        }
        catch (JsonException ex)
        {
            throw new MoveDataException($"player-moves data is not valid JSON: {ex.Message}");
        }

        using (doc)
        {
            var root = doc.RootElement;
            var tickRate = WholeNumber(root, "tickRate", "root", 1);

            var movesEl = Require(root, "moves", "root");
            var moves = new Dictionary<FighterState, MoveDef>();
            foreach (var state in MoveStates)
            {
                moves[state] = ParseMove(movesEl, state);
            }

            var guard = Require(root, "guard", "root");
            var raise = WholeNumber(guard, "raiseFrames", "guard", 1);
            var multiplier = Positive(guard, "moveSpeedMultiplier", "guard");

            var movement = Require(root, "movement", "root");
            var walk = Positive(movement, "walkSpeedMps", "movement");
            var minSeparation = Positive(movement, "minSeparationM", "movement");
            var minX = Number(movement, "minX", "movement");

            var feedback = Require(root, "feedback", "root");
            var textFrames = WholeNumber(feedback, "contactTextFrames", "feedback", 1);

            var visualEl = Require(root, "visual", "root");
            var visual = new VisualTuning(
                NonNegative(visualEl, "punchExtendH", "visual"),
                NonNegative(visualEl, "crossTorsoTurnDeg", "visual"),
                NonNegative(visualEl, "guardGloveRaiseH", "visual"),
                NonNegative(visualEl, "guardGloveInwardW", "visual"),
                NonNegative(visualEl, "slipHeadDropH", "visual"),
                NonNegative(visualEl, "slipLeanDeg", "visual"),
                NonNegative(visualEl, "slipTurnDeg", "visual"));

            if (minX > playerX)
            {
                throw new MoveDataException($"movement.minX ({minX}) is above ring.playerX ({playerX}); the player must start inside the bounds");
            }

            if (playerX > opponentX - minSeparation)
            {
                throw new MoveDataException(
                    $"ring.playerX ({playerX}) is closer than movement.minSeparationM ({minSeparation}) to ring.opponentX ({opponentX})");
            }

            return new MoveSet(tickRate, moves, raise, multiplier, walk, minX, minSeparation, textFrames, visual);
        }
    }

    private static MoveDef ParseMove(JsonElement movesEl, FighterState state)
    {
        var text = state.ToString();
        var name = char.ToLowerInvariant(text[0]) + text[1..];
        var where = "moves." + name;
        var el = Require(movesEl, name, "moves");
        var startup = WholeNumber(el, "startupFrames", where, 1);
        var active = WholeNumber(el, "activeFrames", where, 1);
        var recovery = WholeNumber(el, "recoveryFrames", where, 1);
        var isPunch = state is FighterState.Jab or FighterState.Cross;
        var reach = isPunch ? Positive(el, "reachM", where) : 0.0;

        var cancels = new List<FighterState>();
        if (el.TryGetProperty("cancelInto", out var list))
        {
            if (list.ValueKind != JsonValueKind.Array)
            {
                throw new MoveDataException($"{where}.cancelInto must be a list of move names");
            }

            foreach (var item in list.EnumerateArray())
            {
                var target = item.ValueKind == JsonValueKind.String ? item.GetString() : null;
                var found = false;
                foreach (var candidate in MoveStates)
                {
                    if (string.Equals(candidate.ToString(), target, StringComparison.OrdinalIgnoreCase))
                    {
                        cancels.Add(candidate);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    throw new MoveDataException($"{where}.cancelInto has an unknown move name '{target ?? item.ToString()}'");
                }
            }
        }

        return new MoveDef(startup, active, recovery, reach, cancels);
    }

    private static JsonElement Require(JsonElement parent, string name, string where)
    {
        if (parent.ValueKind != JsonValueKind.Object || !parent.TryGetProperty(name, out var value))
        {
            throw new MoveDataException($"missing '{name}' in {where}");
        }

        return value;
    }

    private static double Number(JsonElement parent, string name, string where)
    {
        var el = Require(parent, name, where);
        if (el.ValueKind != JsonValueKind.Number || !el.TryGetDouble(out var value) || double.IsNaN(value) || double.IsInfinity(value))
        {
            throw new MoveDataException($"{where}.{name} must be a number");
        }

        return value;
    }

    private static double Positive(JsonElement parent, string name, string where)
    {
        var value = Number(parent, name, where);
        return value > 0 ? value : throw new MoveDataException($"{where}.{name} must be greater than 0, got {value}");
    }

    private static double NonNegative(JsonElement parent, string name, string where)
    {
        var value = Number(parent, name, where);
        return value >= 0 ? value : throw new MoveDataException($"{where}.{name} must be 0 or more, got {value}");
    }

    private static int WholeNumber(JsonElement parent, string name, string where, int min)
    {
        var el = Require(parent, name, where);
        if (el.ValueKind != JsonValueKind.Number || !el.TryGetInt32(out var value) || value < min)
        {
            throw new MoveDataException($"{where}.{name} must be a whole number of at least {min}, got {el}");
        }

        return value;
    }
}
