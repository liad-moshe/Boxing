using System;
using System.Text.Json.Nodes;
using Xunit;

namespace Boxing.Core.Tests;

public class MoveSetTests
{
    private static MoveSet Parse(string text)
    {
        var roster = TestData.Roster();
        return MoveSet.Parse(text, roster.PlayerX, roster.OpponentX);
    }

    [Fact]
    public void Shipped_Parses_WithAllMovesAndBlocks()
    {
        var moves = TestData.Moves();

        Assert.Equal(60, moves.TickRate);
        foreach (var state in new[] { FighterState.Jab, FighterState.Cross, FighterState.SlipLeft, FighterState.SlipRight })
        {
            var def = moves.Get(state);
            Assert.True(def.StartupFrames >= 1);
            Assert.True(def.ActiveFrames >= 1);
            Assert.True(def.RecoveryFrames >= 1);
        }

        Assert.True(moves.Get(FighterState.Jab).ReachM > 0);
        Assert.True(moves.Get(FighterState.Cross).ReachM > 0);
        Assert.True(moves.GuardRaiseFrames >= 1);
        Assert.True(moves.WalkSpeedMps > 0);
        Assert.True(moves.ContactTextFrames >= 1);
    }

    [Fact]
    public void Shipped_CancelTargets_AreTheSpecTable()
    {
        var moves = TestData.Moves();

        Assert.Equal(new[] { FighterState.Cross }, moves.Get(FighterState.Jab).CancelInto);
        Assert.Empty(moves.Get(FighterState.Cross).CancelInto);
        Assert.Equal(new[] { FighterState.Jab, FighterState.Cross }, moves.Get(FighterState.SlipLeft).CancelInto);
        Assert.Equal(new[] { FighterState.Jab, FighterState.Cross }, moves.Get(FighterState.SlipRight).CancelInto);
    }

    [Theory]
    [InlineData("jab", "startupFrames", 0)]
    [InlineData("cross", "activeFrames", -2)]
    [InlineData("slipLeft", "recoveryFrames", 0)]
    [InlineData("slipRight", "startupFrames", 1.5)]
    [InlineData("jab", "reachM", 0)]
    [InlineData("cross", "reachM", -1)]
    public void BadMoveNumber_Throws_NamingTheKey(string move, string key, double bad)
    {
        var text = TestData.Edited(r => r["moves"]![move]![key] = bad);

        var ex = Assert.Throws<MoveDataException>(() => Parse(text));

        Assert.Contains(key, ex.Message);
        Assert.Contains(move, ex.Message);
    }

    [Theory]
    [InlineData("startupFrames")]
    [InlineData("activeFrames")]
    [InlineData("recoveryFrames")]
    public void MissingFrameCount_Throws_NamingTheKey(string key)
    {
        var text = TestData.Edited(r => ((JsonObject)r["moves"]!["jab"]!).Remove(key));

        var ex = Assert.Throws<MoveDataException>(() => Parse(text));

        Assert.Contains(key, ex.Message);
    }

    [Fact]
    public void MissingMove_Throws_NamingTheMove()
    {
        var text = TestData.Edited(r => ((JsonObject)r["moves"]!).Remove("slipRight"));

        var ex = Assert.Throws<MoveDataException>(() => Parse(text));

        Assert.Contains("slipRight", ex.Message);
    }

    [Fact]
    public void UnknownCancelInto_Throws_NamingTheKey()
    {
        var text = TestData.Edited(r => r["moves"]!["jab"]!["cancelInto"] = new JsonArray("hook"));

        var ex = Assert.Throws<MoveDataException>(() => Parse(text));

        Assert.Contains("cancelInto", ex.Message);
        Assert.Contains("hook", ex.Message);
    }

    [Theory]
    [InlineData("movement", "walkSpeedMps", 0)]
    [InlineData("movement", "walkSpeedMps", -1)]
    [InlineData("movement", "minSeparationM", 0)]
    [InlineData("guard", "raiseFrames", 0)]
    [InlineData("guard", "moveSpeedMultiplier", 0)]
    [InlineData("feedback", "contactTextFrames", 0)]
    public void BadBlockNumber_Throws_NamingTheKey(string block, string key, double bad)
    {
        var text = TestData.Edited(r => r[block]![key] = bad);

        var ex = Assert.Throws<MoveDataException>(() => Parse(text));

        Assert.Contains(key, ex.Message);
    }

    [Theory]
    [InlineData("guard")]
    [InlineData("movement")]
    [InlineData("feedback")]
    [InlineData("visual")]
    public void MissingBlock_Throws_NamingTheBlock(string block)
    {
        var text = TestData.Edited(r => ((JsonObject)r).Remove(block));

        var ex = Assert.Throws<MoveDataException>(() => Parse(text));

        Assert.Contains(block, ex.Message);
    }

    [Fact]
    public void BadTickRate_Throws()
    {
        var text = TestData.Edited(r => r["tickRate"] = 0);

        var ex = Assert.Throws<MoveDataException>(() => Parse(text));

        Assert.Contains("tickRate", ex.Message);
    }

    [Fact]
    public void MinX_AbovePlayerX_Throws_NamingTheKey()
    {
        var roster = TestData.Roster();
        var text = TestData.Edited(r => r["movement"]!["minX"] = roster.PlayerX + 0.5);

        var ex = Assert.Throws<MoveDataException>(() => Parse(text));

        Assert.Contains("minX", ex.Message);
    }

    [Fact]
    public void PlayerX_TooCloseToOpponent_Throws_NamingTheKey()
    {
        var text = TestData.MovesText();

        var ex = Assert.Throws<MoveDataException>(() => MoveSet.Parse(text, 1.0, 1.5));

        Assert.Contains("minSeparationM", ex.Message);
    }

    [Fact]
    public void NotJson_Throws()
    {
        Assert.Throws<MoveDataException>(() => Parse("{ nope"));
    }

    [Fact]
    public void VisualNumbers_AreRead()
    {
        var visual = TestData.Moves().Visual;

        Assert.True(visual.PunchExtendH > 0);
        Assert.True(visual.SlipTurnDeg > 0);
        Assert.True(visual.GuardGloveInwardW > 0);
    }

    [Fact]
    public void MissingVisualKey_Throws_NamingTheKey()
    {
        var text = TestData.Edited(r => ((JsonObject)r["visual"]!).Remove("slipLeanDeg"));

        var ex = Assert.Throws<MoveDataException>(() => Parse(text));

        Assert.Contains("slipLeanDeg", ex.Message);
    }
}
