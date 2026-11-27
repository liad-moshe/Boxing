using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;

namespace Boxing.Core;

/// <summary>Colours (hex strings) and build scales of a fighter's primitive body.</summary>
public sealed record FighterLook(
    string SkinColor,
    string HairColor,
    string HairStyle,
    string TrunksColor,
    string WaistbandColor,
    string GloveColor,
    double HeightScale,
    double WidthScale,
    double HeadScale,
    double NeckScale,
    double ShoulderScale,
    double ArmLengthScale,
    double ArmThicknessScale,
    double LegThicknessScale);

/// <summary>Shared human proportions and pose values (fractions of height H or width W), from the data file.</summary>
public sealed class FighterBody
{
    /// <summary>Numeric keys that must be present in the "body" object.</summary>
    public static readonly IReadOnlyList<string> RequiredNumbers = new[]
    {
        "headRadiusH", "headStretchY", "neckRadiusW", "neckHeightH", "torsoWidthW", "torsoDepthW", "torsoHeightH",
        "shoulderRadiusW", "shoulderSpanW", "upperArmLengthH", "forearmLengthH", "armRadiusW", "gloveRadiusH",
        "gloveStretch", "shortsHeightH", "waistbandHeightH", "upperLegLengthH", "lowerLegLengthH", "legRadiusW",
        "footLengthH", "footHeightH", "eyeRadiusH", "noseRadiusH", "guardElbowBendDeg", "guardGloveHeightH",
        "stanceFootSpreadW", "stanceLeadFootForwardW",
    };

    private readonly IReadOnlyDictionary<string, double> _values;

    internal FighterBody(IReadOnlyDictionary<string, double> values, string eyeColor, string mouthColor)
    {
        _values = values;
        EyeColor = eyeColor;
        MouthColor = mouthColor;
    }

    public string EyeColor { get; }

    public string MouthColor { get; }

    /// <summary>Numeric body value by data key (see <see cref="RequiredNumbers"/>).</summary>
    public double this[string key] => _values[key];
}

public sealed record FighterDef(
    string Id,
    string Name,
    string Nickname,
    string Archetype,
    IReadOnlyDictionary<string, int> Stats,
    FighterLook Look)
{
    /// <summary>Name with nickname, e.g. Bruno "The Anvil" Kessler.</summary>
    public string DisplayName
    {
        get
        {
            if (string.IsNullOrEmpty(Nickname))
            {
                return Name;
            }

            var space = Name.IndexOf(' ', StringComparison.Ordinal);
            return space < 0
                ? $"{Name} \"{Nickname}\""
                : $"{Name[..space]} \"{Nickname}\" {Name[(space + 1)..]}";
        }
    }

    public int StatTotal
    {
        get
        {
            var total = 0;
            foreach (var value in Stats.Values)
            {
                total += value;
            }

            return total;
        }
    }
}

/// <summary>Thrown when roster data is invalid; the message names the problem.</summary>
public sealed class RosterException : Exception
{
    public RosterException(string message)
        : base(message)
    {
    }
}

/// <summary>Roster parsed from fighters.json text (file reading is the Godot adapter's job).</summary>
public sealed class FighterRoster
{
    private FighterRoster(
        int statMax,
        IReadOnlyList<string> statOrder,
        IReadOnlyDictionary<string, string> statLabels,
        IReadOnlyList<FighterDef> fighters,
        double playerX,
        double opponentX,
        double baseHeight,
        double baseWidth,
        string defaultHighlightId,
        FighterBody body)
    {
        Body = body;
        StatMax = statMax;
        StatOrder = statOrder;
        StatLabels = statLabels;
        Fighters = fighters;
        PlayerX = playerX;
        OpponentX = opponentX;
        BaseHeight = baseHeight;
        BaseWidth = baseWidth;
        DefaultHighlightId = defaultHighlightId;
    }

    public int StatMax { get; }

    public IReadOnlyList<string> StatOrder { get; }

    public IReadOnlyDictionary<string, string> StatLabels { get; }

    public IReadOnlyList<FighterDef> Fighters { get; }

    public double PlayerX { get; }

    public double OpponentX { get; }

    public double BaseHeight { get; }

    public double BaseWidth { get; }

    public string DefaultHighlightId { get; }

    public FighterBody Body { get; }

    /// <summary>Index of the default highlight (first card if the configured id is unknown).</summary>
    public int DefaultHighlightIndex => Math.Max(0, IndexOf(DefaultHighlightId));

    public int IndexOf(string id)
    {
        for (var i = 0; i < Fighters.Count; i++)
        {
            if (Fighters[i].Id == id)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>Fighter with this id, or the first roster entry when the id is null or unknown.</summary>
    public FighterDef FindOrFirst(string? id)
    {
        var index = id is null ? -1 : IndexOf(id);
        return Fighters[Math.Max(0, index)];
    }

    public static FighterRoster Parse(string json)
    {
        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(json);
        }
        catch (JsonException ex)
        {
            throw new RosterException($"fighters data is not valid JSON: {ex.Message}");
        }

        using (doc)
        {
            var root = doc.RootElement;
            var statMax = Require(root, "statMax", "root").GetInt32();
            if (statMax < 1)
            {
                throw new RosterException($"statMax must be at least 1, got {statMax}");
            }

            var statOrder = new List<string>();
            foreach (var s in Require(root, "statOrder", "root").EnumerateArray())
            {
                statOrder.Add(s.GetString() ?? throw new RosterException("statOrder contains a non-string entry"));
            }

            if (statOrder.Count == 0)
            {
                throw new RosterException("statOrder is empty");
            }

            var labels = new Dictionary<string, string>();
            foreach (var stat in statOrder)
            {
                labels[stat] = stat;
            }

            if (root.TryGetProperty("statLabels", out var labelsEl))
            {
                foreach (var p in labelsEl.EnumerateObject())
                {
                    labels[p.Name] = p.Value.GetString() ?? p.Name;
                }
            }

            var fighters = new List<FighterDef>();
            var ids = new HashSet<string>();
            foreach (var f in Require(root, "fighters", "root").EnumerateArray())
            {
                var id = GetString(f, "id", "fighter");
                if (!ids.Add(id))
                {
                    throw new RosterException($"duplicate fighter id '{id}'");
                }

                var statsEl = Require(f, "stats", id);
                var stats = new Dictionary<string, int>();
                foreach (var stat in statOrder)
                {
                    if (!statsEl.TryGetProperty(stat, out var v) || v.ValueKind != JsonValueKind.Number)
                    {
                        throw new RosterException($"fighter '{id}' is missing stat '{stat}'");
                    }

                    var value = v.GetInt32();
                    if (value < 1 || value > statMax)
                    {
                        throw new RosterException($"fighter '{id}' stat '{stat}' is {value}, must be 1 to {statMax}");
                    }

                    stats[stat] = value;
                }

                var lookEl = Require(f, "look", id);
                var look = new FighterLook(
                    GetColor(lookEl, "skinColor", id),
                    GetColor(lookEl, "hairColor", id),
                    GetHairStyle(lookEl, id),
                    GetColor(lookEl, "trunksColor", id),
                    GetColor(lookEl, "waistbandColor", id),
                    GetColor(lookEl, "gloveColor", id),
                    GetScale(lookEl, "heightScale", id),
                    GetScale(lookEl, "widthScale", id),
                    GetScale(lookEl, "headScale", id),
                    GetScale(lookEl, "neckScale", id),
                    GetScale(lookEl, "shoulderScale", id),
                    GetScale(lookEl, "armLengthScale", id),
                    GetScale(lookEl, "armThicknessScale", id),
                    GetScale(lookEl, "legThicknessScale", id));
                fighters.Add(new FighterDef(
                    id,
                    GetString(f, "name", id),
                    f.TryGetProperty("nickname", out var nick) ? nick.GetString() ?? string.Empty : string.Empty,
                    GetString(f, "archetype", id),
                    stats,
                    look));
            }

            if (fighters.Count == 0)
            {
                throw new RosterException("fighter list is empty");
            }

            var bodyEl = Require(root, "body", "root");
            var bodyValues = new Dictionary<string, double>();
            foreach (var key in FighterBody.RequiredNumbers)
            {
                var el = Require(bodyEl, key, "body");
                if (el.ValueKind != JsonValueKind.Number)
                {
                    throw new RosterException($"'{key}' in body is not a number");
                }

                bodyValues[key] = el.GetDouble();
            }

            var body = new FighterBody(bodyValues, GetColor(bodyEl, "eyeColor", "body"), GetColor(bodyEl, "mouthColor", "body"));

            var ring = Require(root, "ring", "root");
            var defaultId = fighters[0].Id;
            if (root.TryGetProperty("ui", out var ui) && ui.TryGetProperty("defaultHighlightId", out var d))
            {
                defaultId = d.GetString() ?? defaultId;
            }

            return new FighterRoster(
                statMax,
                statOrder,
                labels,
                fighters,
                Require(ring, "playerX", "ring").GetDouble(),
                Require(ring, "opponentX", "ring").GetDouble(),
                Require(ring, "baseHeight", "ring").GetDouble(),
                Require(ring, "baseWidth", "ring").GetDouble(),
                defaultId,
                body);
        }
    }

    /// <summary>Parses "#rrggbb" into 0..1 components.</summary>
    public static (double R, double G, double B) ParseHexColor(string hex)
    {
        var s = hex.TrimStart('#');
        if (s.Length != 6)
        {
            throw new RosterException($"colour '{hex}' must look like #rrggbb");
        }

        double Part(int i) => int.Parse(s.AsSpan(i, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255.0;
        return (Part(0), Part(2), Part(4));
    }

    private static JsonElement Require(JsonElement parent, string name, string where)
    {
        if (parent.ValueKind != JsonValueKind.Object || !parent.TryGetProperty(name, out var value))
        {
            throw new RosterException($"missing '{name}' in {where}");
        }

        return value;
    }

    private static string GetColor(JsonElement parent, string name, string where)
    {
        var value = GetString(parent, name, where);
        try
        {
            ParseHexColor(value);
        }
        catch (Exception ex) when (ex is RosterException or FormatException)
        {
            throw new RosterException($"'{name}' in {where} is not a valid colour: '{value}' (use #rrggbb)");
        }

        return value;
    }

    private static string GetHairStyle(JsonElement parent, string where)
    {
        var style = GetString(parent, "hairStyle", where);
        HairStyles.CapScale(style);
        return style;
    }

    private static double GetScale(JsonElement parent, string name, string where)
    {
        var el = Require(parent, name, where);
        if (el.ValueKind != JsonValueKind.Number || el.GetDouble() <= 0)
        {
            throw new RosterException($"'{name}' in {where} must be a number greater than 0");
        }

        return el.GetDouble();
    }

    private static string GetString(JsonElement parent, string name, string where) =>
        Require(parent, name, where).GetString() ?? throw new RosterException($"'{name}' in {where} is not a string");
}
