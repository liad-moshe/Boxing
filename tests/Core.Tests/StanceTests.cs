using System;
using System.Text.Json.Nodes;
using Xunit;

namespace Boxing.Core.Tests;

public class StanceTests
{
    private static readonly MoveSet Moves = TestData.Moves();
    private static readonly FighterRoster Roster = TestData.Roster();

    public static TheoryData<Stance> BothStances => new() { Stance.Orthodox, Stance.Southpaw };

    private static PlayerFighter NewFighter() => new(Moves, Roster.PlayerX, Roster.OpponentX);

    private static MoveSet Parse(string text) => MoveSet.Parse(text, Roster.PlayerX, Roster.OpponentX);

    [Fact]
    public void Orthodox_LeadsLeft_JabLeft_CrossRight_MirrorPlusOne()
    {
        Assert.Equal(BodySide.Left, StanceRules.LeadSide(Stance.Orthodox));
        Assert.Equal(BodySide.Right, StanceRules.RearSide(Stance.Orthodox));
        Assert.Equal(BodySide.Left, StanceRules.PunchSide(FighterState.Jab, Stance.Orthodox));
        Assert.Equal(BodySide.Right, StanceRules.PunchSide(FighterState.Cross, Stance.Orthodox));
        Assert.Equal(1, StanceRules.MirrorSign(Stance.Orthodox));
    }

    [Fact]
    public void Southpaw_LeadsRight_JabRight_CrossLeft_MirrorMinusOne()
    {
        Assert.Equal(BodySide.Right, StanceRules.LeadSide(Stance.Southpaw));
        Assert.Equal(BodySide.Left, StanceRules.RearSide(Stance.Southpaw));
        Assert.Equal(BodySide.Right, StanceRules.PunchSide(FighterState.Jab, Stance.Southpaw));
        Assert.Equal(BodySide.Left, StanceRules.PunchSide(FighterState.Cross, Stance.Southpaw));
        Assert.Equal(-1, StanceRules.MirrorSign(Stance.Southpaw));
    }

    [Fact]
    public void PunchSide_ForAnythingButAPunch_Throws() =>
        Assert.Throws<ArgumentException>(() => StanceRules.PunchSide(FighterState.SlipLeft, Stance.Orthodox));

    [Fact]
    public void Parse_AcceptsTheTwoDataNames_RejectsOthers()
    {
        Assert.Equal(Stance.Orthodox, StanceRules.Parse("orthodox"));
        Assert.Equal(Stance.Southpaw, StanceRules.Parse("southpaw"));
        Assert.Null(StanceRules.Parse("lefty"));
        Assert.Null(StanceRules.Parse(null));
    }

    [Fact]
    public void DisplayName_IsCapitalised()
    {
        Assert.Equal("Orthodox", StanceRules.DisplayName(Stance.Orthodox));
        Assert.Equal("Southpaw", StanceRules.DisplayName(Stance.Southpaw));
    }

    [Fact]
    public void Toggle_FlipsBetweenTheTwo()
    {
        Assert.Equal(Stance.Southpaw, StanceRules.Toggle(Stance.Orthodox));
        Assert.Equal(Stance.Orthodox, StanceRules.Toggle(Stance.Southpaw));
    }

    // ---- MoveSet.DefaultStance (AC1) ----

    [Fact]
    public void ShippedData_DefaultStanceIsOrthodox() => Assert.Equal(Stance.Orthodox, Moves.DefaultStance);

    [Theory]
    [InlineData("southpaw", Stance.Southpaw)]
    [InlineData("orthodox", Stance.Orthodox)]
    public void DefaultStance_ReadsTheDataValue(string value, Stance expected)
    {
        var text = TestData.Edited(r => r["stance"]!["default"] = value);
        Assert.Equal(expected, Parse(text).DefaultStance);
    }

    [Theory]
    [InlineData("lefty")]
    [InlineData("")]
    [InlineData("Orthodox")]
    public void BadDefaultStance_Throws_NamingTheKey(string value)
    {
        var text = TestData.Edited(r => r["stance"]!["default"] = value);
        var ex = Assert.Throws<MoveDataException>(() => Parse(text));
        Assert.Contains("stance.default", ex.Message);
    }

    [Fact]
    public void NonStringDefaultStance_Throws_NamingTheKey()
    {
        var text = TestData.Edited(r => r["stance"]!["default"] = 3);
        var ex = Assert.Throws<MoveDataException>(() => Parse(text));
        Assert.Contains("stance.default", ex.Message);
    }

    [Fact]
    public void MissingDefaultStance_AndMissingBlock_Throw_NamingTheKey()
    {
        var noKey = TestData.Edited(r => ((JsonObject)r["stance"]!).Remove("default"));
        var noBlock = TestData.Edited(r => ((JsonObject)r).Remove("stance"));
        foreach (var text in new[] { noKey, noBlock })
        {
            var ex = Assert.Throws<MoveDataException>(() => Parse(text));
            Assert.Contains("stance", ex.Message);
        }
    }

    // ---- Pose mirroring (AC17) ----

    private static PoseBlend[] Timeline(Stance stance, FighterInput first, FighterInput held, int ticks)
    {
        var f = NewFighter();
        var list = new PoseBlend[ticks];
        for (var t = 0; t < ticks; t++)
        {
            f.Tick(t == 0 ? first : held);
            list[t] = PoseBlend.From(f, Moves, stance);
        }

        return list;
    }

    [Fact]
    public void SouthpawPose_IsTheMirrorOfOrthodox_OnEveryTick_OfEveryMove()
    {
        var guard = new FighterInput(GuardHeld: true);
        var cases = new (FighterInput First, FighterInput Held, int Ticks)[]
        {
            (default, default, 5),
            (new FighterInput(JabPressed: true), default, 20),
            (new FighterInput(CrossPressed: true), default, 28),
            (guard, guard, 10),
            (new FighterInput(SlipLeftPressed: true), default, 32),
            (new FighterInput(SlipRightPressed: true), default, 32),
        };

        foreach (var (first, held, ticks) in cases)
        {
            var orthodox = Timeline(Stance.Orthodox, first, held, ticks);
            var southpaw = Timeline(Stance.Southpaw, first, held, ticks);
            for (var t = 0; t < ticks; t++)
            {
                var o = orthodox[t];
                var s = southpaw[t];
                Assert.Equal(o.LeftExtend, s.RightExtend, 9);
                Assert.Equal(o.RightExtend, s.LeftExtend, 9);
                Assert.Equal(o.TorsoTurn, -s.TorsoTurn, 9);
                Assert.Equal(o.Guard, s.Guard, 9);
                Assert.Equal(o.Slip, s.Slip, 9);
                Assert.Equal(o.SlipSide, s.SlipSide);
            }
        }
    }

    [Fact]
    public void Jab_MovesTheLeadGlove_Cross_TheRear_InBothStances()
    {
        var jab = NewFighter();
        jab.Tick(new FighterInput(JabPressed: true));
        var cross = NewFighter();
        cross.Tick(new FighterInput(CrossPressed: true));

        var orthodoxJab = PoseBlend.From(jab, Moves, Stance.Orthodox);
        var southpawJab = PoseBlend.From(jab, Moves, Stance.Southpaw);
        var orthodoxCross = PoseBlend.From(cross, Moves, Stance.Orthodox);
        var southpawCross = PoseBlend.From(cross, Moves, Stance.Southpaw);

        Assert.True(orthodoxJab.LeftExtend > 0);
        Assert.Equal(0, orthodoxJab.RightExtend);
        Assert.Equal(0, orthodoxJab.TorsoTurn);
        Assert.True(southpawJab.RightExtend > 0);
        Assert.Equal(0, southpawJab.LeftExtend);
        Assert.Equal(0, southpawJab.TorsoTurn);
        Assert.True(orthodoxCross.RightExtend > 0);
        Assert.Equal(0, orthodoxCross.LeftExtend);
        Assert.True(orthodoxCross.TorsoTurn > 0);
        Assert.True(southpawCross.LeftExtend > 0);
        Assert.Equal(0, southpawCross.RightExtend);
        Assert.Equal(-orthodoxCross.TorsoTurn, southpawCross.TorsoTurn, 9);
    }

    // ---- Stance changes no timing (AC18 last clause, AC26) ----

    [Theory]
    [MemberData(nameof(BothStances))]
    public void OneTwo_RunsTheSameTicks_AndTheLeadPunchStartsIt(Stance stance)
    {
        var f = NewFighter();
        f.Tick(new FighterInput(JabPressed: true));
        var lead = PoseBlend.From(f, Moves, stance);
        Assert.True(StanceRules.LeadSide(stance) == BodySide.Left ? lead.LeftExtend > 0 : lead.RightExtend > 0);

        var jab = Moves.Get(FighterState.Jab);
        for (var i = 1; i < jab.StartupFrames + jab.ActiveFrames + 1; i++)
        {
            f.Tick(default);
        }

        f.Tick(new FighterInput(CrossPressed: true));
        Assert.Equal(FighterState.Cross, f.State);
        Assert.Equal(MovePhase.Startup, f.Phase);
        var cross = Moves.Get(FighterState.Cross);
        for (var i = 1; i < cross.TotalFrames; i++)
        {
            f.Tick(default);
            Assert.Equal(FighterState.Cross, f.State);
        }

        f.Tick(default);
        Assert.Equal(FighterState.Idle, f.State);
    }
}
