using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Boxing.Core.Tests;

public class FighterRosterTests
{
    private const string Template =
        "{\"statMax\":10,\"statOrder\":[\"power\",\"speed\"],\"fighters\":[FIGHTERS],"
        + "\"ring\":{\"playerX\":-1,\"opponentX\":1,\"baseHeight\":1.8,\"baseWidth\":0.5},"
        + "\"body\":BODY,"
        + "\"ui\":{\"defaultHighlightId\":\"a\"}}";

    private static string BodyJson(string? skipKey = null)
    {
        var parts = FighterBody.RequiredNumbers.Where(k => k != skipKey).Select(k => $"\"{k}\":1.0").ToList();
        parts.Add("\"eyeColor\":\"#111111\"");
        parts.Add("\"mouthColor\":\"#772222\"");
        return "{" + string.Join(",", parts) + "}";
    }

    internal static FighterRoster Shipped() =>
        FighterRoster.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "fighters.json")));

    [Fact]
    public void Shipped_HasAnvilThenGhost_WithAllStatsInRange()
    {
        var roster = Shipped();
        Assert.Equal(new[] { "anvil", "ghost" }, roster.Fighters.Select(f => f.Id).ToArray());
        foreach (var f in roster.Fighters)
        {
            Assert.Equal(roster.StatOrder.Count, f.Stats.Count);
            Assert.All(f.Stats.Values, v => Assert.InRange(v, 1, roster.StatMax));
        }
    }

    [Fact]
    public void Shipped_StatTotalsEqual()
    {
        var roster = Shipped();
        Assert.Equal(roster.Fighters[0].StatTotal, roster.Fighters[1].StatTotal);
    }

    [Fact]
    public void Shipped_NeitherBoxerDominates()
    {
        var roster = Shipped();
        var a = roster.Fighters[0];
        var b = roster.Fighters[1];
        Assert.Contains(roster.StatOrder, s => a.Stats[s] > b.Stats[s]);
        Assert.Contains(roster.StatOrder, s => b.Stats[s] > a.Stats[s]);
    }

    [Fact]
    public void Shipped_DefaultsAndDisplayName()
    {
        var roster = Shipped();
        Assert.Equal(0, roster.DefaultHighlightIndex);
        Assert.Equal("Bruno \"The Anvil\" Kessler", roster.Fighters[0].DisplayName);
        Assert.Equal("Power", roster.StatLabels["power"]);
        Assert.Equal("anvil", roster.FindOrFirst(null).Id);
        Assert.Equal("ghost", roster.FindOrFirst("ghost").Id);
    }

    [Fact]
    public void Valid_Loads() =>
        Assert.Single(FighterRoster.Parse(Json(Fighter("a", "\"power\":5,\"speed\":5"))).Fighters);

    [Fact]
    public void MissingStat_Throws()
    {
        var ex = Assert.Throws<RosterException>(() => FighterRoster.Parse(Json(Fighter("a", "\"power\":5"))));
        Assert.Contains("speed", ex.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public void StatOutOfRange_Throws(int value)
    {
        var ex = Assert.Throws<RosterException>(
            () => FighterRoster.Parse(Json(Fighter("a", $"\"power\":{value},\"speed\":5"))));
        Assert.Contains("power", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void DuplicateIds_Throws()
    {
        var f = Fighter("a", "\"power\":5,\"speed\":5");
        var ex = Assert.Throws<RosterException>(() => FighterRoster.Parse(Json(f, f)));
        Assert.Contains("duplicate", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void EmptyList_Throws()
    {
        var ex = Assert.Throws<RosterException>(() => FighterRoster.Parse(Json()));
        Assert.Contains("empty", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ParseHexColor_Works()
    {
        var (r, g, b) = FighterRoster.ParseHexColor("#ff0000");
        Assert.Equal(1.0, r);
        Assert.Equal(0.0, g);
        Assert.Equal(0.0, b);
    }

    private static string Fighter(string id, string stats, string? lookOverride = null) =>
        "{\"id\":\"" + id + "\",\"name\":\"N M\",\"nickname\":\"Nick\",\"archetype\":\"X\",\"stats\":{" + stats + "},"
        + "\"look\":" + (lookOverride ?? Look()) + "}";

    private static string Look(string skin = "#ffffff", string headScale = "1") =>
        "{\"skinColor\":\"" + skin + "\",\"hairColor\":\"#000000\",\"hairStyle\":\"buzz\",\"trunksColor\":\"#ffffff\","
        + "\"waistbandColor\":\"#ffffff\",\"gloveColor\":\"#ffffff\",\"heightScale\":1,\"widthScale\":1,"
        + "\"headScale\":" + headScale + ",\"neckScale\":1,\"shoulderScale\":1,\"armLengthScale\":1,"
        + "\"armThicknessScale\":1,\"legThicknessScale\":1}";

    private static string Json(params string[] fighters) =>
        Template.Replace("FIGHTERS", string.Join(",", fighters), StringComparison.Ordinal)
            .Replace("BODY", BodyJson(), StringComparison.Ordinal);

    [Fact]
    public void Shipped_LookHasHairWaistbandAndBuildScales()
    {
        var roster = Shipped();
        foreach (var f in roster.Fighters)
        {
            Assert.Matches("^#[0-9a-fA-F]{6}$", f.Look.HairColor);
            Assert.Matches("^#[0-9a-fA-F]{6}$", f.Look.WaistbandColor);
            Assert.False(string.IsNullOrEmpty(f.Look.HairStyle));
            foreach (var scale in new[]
            {
                f.Look.HeightScale, f.Look.WidthScale, f.Look.HeadScale, f.Look.NeckScale,
                f.Look.ShoulderScale, f.Look.ArmLengthScale, f.Look.ArmThicknessScale, f.Look.LegThicknessScale,
            })
            {
                Assert.True(scale > 0);
            }
        }

        Assert.Equal("buzz", roster.Fighters[0].Look.HairStyle);
        Assert.Equal("short", roster.Fighters[1].Look.HairStyle);
    }

    [Fact]
    public void Shipped_BodyIsPresent()
    {
        var body = Shipped().Body;
        foreach (var key in FighterBody.RequiredNumbers)
        {
            Assert.True(body[key] > 0 || key == "guardElbowBendDeg", key);
        }

        Assert.Matches("^#[0-9a-fA-F]{6}$", body.EyeColor);
        Assert.Matches("^#[0-9a-fA-F]{6}$", body.MouthColor);
    }

    [Fact]
    public void BadLookColour_Throws()
    {
        var ex = Assert.Throws<RosterException>(() => FighterRoster.Parse(
            Json(Fighter("a", "\"power\":5,\"speed\":5", Look(skin: "banana")))));
        Assert.Contains("skinColor", ex.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    public void NonPositiveScale_Throws(string value)
    {
        var ex = Assert.Throws<RosterException>(() => FighterRoster.Parse(
            Json(Fighter("a", "\"power\":5,\"speed\":5", Look(headScale: value)))));
        Assert.Contains("headScale", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MissingBodyKey_Throws()
    {
        var json = Template.Replace("FIGHTERS", Fighter("a", "\"power\":5,\"speed\":5"), StringComparison.Ordinal)
            .Replace("BODY", BodyJson("headRadiusH"), StringComparison.Ordinal);
        var ex = Assert.Throws<RosterException>(() => FighterRoster.Parse(json));
        Assert.Contains("headRadiusH", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MissingBody_Throws()
    {
        var json = Template.Replace("FIGHTERS", Fighter("a", "\"power\":5,\"speed\":5"), StringComparison.Ordinal)
            .Replace("\"body\":BODY,", string.Empty, StringComparison.Ordinal);
        var ex = Assert.Throws<RosterException>(() => FighterRoster.Parse(json));
        Assert.Contains("body", ex.Message, StringComparison.Ordinal);
    }
}
