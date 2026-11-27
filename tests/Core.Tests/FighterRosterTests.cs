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
        + "\"ui\":{\"defaultHighlightId\":\"a\"}}";

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

    private static string Fighter(string id, string stats) =>
        "{\"id\":\"" + id + "\",\"name\":\"N M\",\"nickname\":\"Nick\",\"archetype\":\"X\",\"stats\":{" + stats + "},"
        + "\"look\":{\"skinColor\":\"#ffffff\",\"trunksColor\":\"#ffffff\",\"gloveColor\":\"#ffffff\","
        + "\"heightScale\":1,\"widthScale\":1}}";

    private static string Json(params string[] fighters) =>
        Template.Replace("FIGHTERS", string.Join(",", fighters), StringComparison.Ordinal);
}
