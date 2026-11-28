using System;
using System.Collections.Generic;
using Xunit;

namespace Boxing.Core.Tests;

public class PlayerFighterTests
{
    private static readonly FighterInput None = default;
    private static readonly FighterInput PressJab = new(JabPressed: true);
    private static readonly FighterInput PressCross = new(CrossPressed: true);
    private static readonly FighterInput PressSlipLeft = new(SlipLeftPressed: true);
    private static readonly FighterInput PressSlipRight = new(SlipRightPressed: true);
    private static readonly FighterInput HoldGuard = new(GuardHeld: true);

    private static readonly MoveSet Moves = TestData.Moves();
    private static readonly FighterRoster Roster = TestData.Roster();

    private static PlayerFighter NewFighter() => new(Moves, Roster.PlayerX, Roster.OpponentX);

    private static PlayerFighter NewFighterAtGap(double gap) => new(Moves, Roster.OpponentX - gap, Roster.OpponentX);

    private static void TickN(PlayerFighter f, int n, FighterInput input)
    {
        for (var i = 0; i < n; i++)
        {
            f.Tick(input);
        }
    }

    private static void AssertAt(PlayerFighter f, FighterState state, MovePhase phase, int frame)
    {
        Assert.Equal(state, f.State);
        Assert.Equal(phase, f.Phase);
        Assert.Equal(frame, f.PhaseFrame);
    }

    /// <summary>Checks a move started on this tick runs exactly startup, active, recovery, then Idle on the next tick.</summary>
    private static void AssertTimeline(PlayerFighter f, FighterState state, FighterInput press)
    {
        var def = Moves.Get(state);
        f.Tick(press);
        for (var frame = 1; frame <= def.StartupFrames; frame++)
        {
            AssertAt(f, state, MovePhase.Startup, frame);
            f.Tick(None);
        }

        for (var frame = 1; frame <= def.ActiveFrames; frame++)
        {
            AssertAt(f, state, MovePhase.Active, frame);
            f.Tick(None);
        }

        for (var frame = 1; frame <= def.RecoveryFrames; frame++)
        {
            AssertAt(f, state, MovePhase.Recovery, frame);
            f.Tick(None);
        }

        AssertAt(f, FighterState.Idle, MovePhase.None, 0);
    }

    // ---- AC2, AC3, AC4: timelines ----

    [Fact]
    public void NewFighter_IsIdle_AtStart()
    {
        var f = NewFighter();

        AssertAt(f, FighterState.Idle, MovePhase.None, 0);
        Assert.Equal(Roster.PlayerX, f.X);
        Assert.Equal(Roster.OpponentX, f.OpponentX);
        Assert.False(f.IsBlocking);
        Assert.False(f.IsEvading);
        Assert.Equal(EvadeSide.None, f.EvadeSide);
        Assert.Equal(ContactKind.None, f.Contact);
    }

    [Fact]
    public void Jab_RunsItsTimeline_ThenIsIdle() => AssertTimeline(NewFighter(), FighterState.Jab, PressJab);

    [Fact]
    public void Cross_RunsItsTimeline_ThenIsIdle() => AssertTimeline(NewFighter(), FighterState.Cross, PressCross);

    [Fact]
    public void SlipLeft_RunsItsTimeline_ThenIsIdle() => AssertTimeline(NewFighter(), FighterState.SlipLeft, PressSlipLeft);

    [Fact]
    public void SlipRight_RunsItsTimeline_ThenIsIdle() => AssertTimeline(NewFighter(), FighterState.SlipRight, PressSlipRight);

    [Fact]
    public void Cross_IsSlowerThanJab_ButWithinOnePointFiveTimes()
    {
        var jab = Moves.Get(FighterState.Jab).TotalFrames;
        var cross = Moves.Get(FighterState.Cross).TotalFrames;

        Assert.True(cross > jab);
        Assert.True(cross <= 1.5 * jab);
    }

    [Theory]
    [InlineData(FighterState.SlipLeft, EvadeSide.Left)]
    [InlineData(FighterState.SlipRight, EvadeSide.Right)]
    public void Slip_EvadesOnlyInActive_WithItsSide(FighterState slip, EvadeSide side)
    {
        var f = NewFighter();
        var def = Moves.Get(slip);
        f.Tick(slip == FighterState.SlipLeft ? PressSlipLeft : PressSlipRight);
        for (var tick = 1; tick <= def.TotalFrames + 2; tick++)
        {
            var inActive = tick > def.StartupFrames && tick <= def.StartupFrames + def.ActiveFrames;
            Assert.Equal(inActive, f.IsEvading);
            Assert.Equal(inActive ? side : EvadeSide.None, f.EvadeSide);
            Assert.False(f.IsBlocking);
            f.Tick(None);
        }
    }

    // ---- AC5, AC7: ignored presses, no buffering ----

    [Fact]
    public void Jab_StartupAndActive_IgnoreEveryPress_AndNothingIsBuffered()
    {
        var f = NewFighter();
        var def = Moves.Get(FighterState.Jab);
        f.Tick(PressJab);
        var mash = new FighterInput(JabPressed: true, CrossPressed: true, SlipLeftPressed: true, SlipRightPressed: true);
        for (var tick = 2; tick <= def.StartupFrames + def.ActiveFrames; tick++)
        {
            f.Tick(mash);
            Assert.Equal(FighterState.Jab, f.State);
            Assert.Equal(tick <= def.StartupFrames ? MovePhase.Startup : MovePhase.Active, f.Phase);
        }

        // Recovery with no input, then Idle: nothing fires later.
        TickN(f, def.RecoveryFrames + 1, None);
        AssertAt(f, FighterState.Idle, MovePhase.None, 0);
        TickN(f, 5, None);
        AssertAt(f, FighterState.Idle, MovePhase.None, 0);
    }

    [Fact]
    public void Cross_IgnoresEveryPressInEveryPhase_AndRunsToItsEnd()
    {
        var f = NewFighter();
        var def = Moves.Get(FighterState.Cross);
        f.Tick(PressCross);
        var mash = new FighterInput(JabPressed: true, CrossPressed: true, SlipLeftPressed: true, SlipRightPressed: true);
        for (var tick = 2; tick <= def.TotalFrames; tick++)
        {
            f.Tick(mash);
            Assert.Equal(FighterState.Cross, f.State);
        }

        f.Tick(None);
        AssertAt(f, FighterState.Idle, MovePhase.None, 0);
    }

    // ---- AC6: jab recovery cancels into cross only ----

    [Fact]
    public void JabRecovery_CrossPress_CancelsIntoCrossStartup_OnSameTick()
    {
        var f = NewFighter();
        var jab = Moves.Get(FighterState.Jab);
        f.Tick(PressJab);
        TickN(f, jab.StartupFrames + jab.ActiveFrames - 1, None);
        AssertAt(f, FighterState.Jab, MovePhase.Active, jab.ActiveFrames);

        f.Tick(PressCross);

        AssertAt(f, FighterState.Cross, MovePhase.Startup, 1);
    }

    [Fact]
    public void OneTwo_EndsExactlyCrossTotalTicksAfterTheCancel()
    {
        var f = NewFighter();
        var jab = Moves.Get(FighterState.Jab);
        var cross = Moves.Get(FighterState.Cross);
        f.Tick(PressJab);
        TickN(f, jab.StartupFrames + jab.ActiveFrames - 1, None);
        f.Tick(PressCross); // first Recovery tick
        AssertAt(f, FighterState.Cross, MovePhase.Startup, 1);

        TickN(f, cross.TotalFrames - 1, None);
        Assert.Equal(FighterState.Cross, f.State);
        f.Tick(None);

        AssertAt(f, FighterState.Idle, MovePhase.None, 0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void JabRecovery_JabAndSlipPresses_AreIgnored(int which)
    {
        var f = NewFighter();
        var jab = Moves.Get(FighterState.Jab);
        f.Tick(PressJab);
        TickN(f, jab.StartupFrames + jab.ActiveFrames - 1, None);
        var press = which switch { 0 => PressJab, 1 => PressSlipLeft, _ => PressSlipRight };

        f.Tick(press);

        AssertAt(f, FighterState.Jab, MovePhase.Recovery, 1);
    }

    [Fact]
    public void JabStartup_CrossPress_IsIgnored_CancelsOnlyFromRecovery()
    {
        var f = NewFighter();
        f.Tick(PressJab);

        f.Tick(PressCross);

        AssertAt(f, FighterState.Jab, MovePhase.Startup, 2);
    }

    // ---- AC8: slip recovery cancels into punches, never into a slip ----

    [Theory]
    [InlineData(FighterState.SlipLeft)]
    [InlineData(FighterState.SlipRight)]
    public void SlipRecovery_PunchPress_CancelsIntoThatPunch_OnSameTick(FighterState slip)
    {
        foreach (var (press, expected) in new[] { (PressJab, FighterState.Jab), (PressCross, FighterState.Cross) })
        {
            var f = NewFighter();
            var def = Moves.Get(slip);
            f.Tick(slip == FighterState.SlipLeft ? PressSlipLeft : PressSlipRight);
            TickN(f, def.StartupFrames + def.ActiveFrames - 1, None);
            AssertAt(f, slip, MovePhase.Active, def.ActiveFrames);

            f.Tick(press);

            AssertAt(f, expected, MovePhase.Startup, 1);
        }
    }

    [Theory]
    [InlineData(FighterState.SlipLeft)]
    [InlineData(FighterState.SlipRight)]
    public void SlipRecovery_AnySlipPress_IsIgnored_IncludingOtherSide(FighterState slip)
    {
        foreach (var press in new[] { PressSlipLeft, PressSlipRight })
        {
            var f = NewFighter();
            var def = Moves.Get(slip);
            f.Tick(slip == FighterState.SlipLeft ? PressSlipLeft : PressSlipRight);
            TickN(f, def.StartupFrames + def.ActiveFrames - 1, None);

            f.Tick(press);

            AssertAt(f, slip, MovePhase.Recovery, 1);
        }
    }

    [Theory]
    [InlineData(FighterState.SlipLeft)]
    [InlineData(FighterState.SlipRight)]
    public void Slip_StartupAndActive_IgnoreEveryPress_AndKeepTheirSide(FighterState slip)
    {
        var f = NewFighter();
        var def = Moves.Get(slip);
        f.Tick(slip == FighterState.SlipLeft ? PressSlipLeft : PressSlipRight);
        var mash = new FighterInput(JabPressed: true, CrossPressed: true, SlipLeftPressed: true, SlipRightPressed: true);
        for (var tick = 2; tick <= def.StartupFrames + def.ActiveFrames; tick++)
        {
            f.Tick(mash);
            Assert.Equal(slip, f.State);
            Assert.NotEqual(MovePhase.Recovery, f.Phase);
        }
    }

    // ---- AC9, AC10: guard ----

    [Fact]
    public void Guard_RaisesForRaiseFrames_ThenGuardsWhileHeld_AndReleasesToIdleOnThatTick()
    {
        var f = NewFighter();
        for (var tick = 1; tick <= Moves.GuardRaiseFrames; tick++)
        {
            f.Tick(HoldGuard);
            Assert.Equal(FighterState.GuardRaising, f.State);
            Assert.False(f.IsBlocking);
        }

        for (var tick = 0; tick < 30; tick++)
        {
            f.Tick(HoldGuard);
            Assert.Equal(FighterState.Guarding, f.State);
            Assert.True(f.IsBlocking);
        }

        f.Tick(None);

        AssertAt(f, FighterState.Idle, MovePhase.None, 0);
        Assert.False(f.IsBlocking);
    }

    [Fact]
    public void Guard_ReleasedWhileRaising_ReturnsToIdleAtOnce()
    {
        var f = NewFighter();
        f.Tick(HoldGuard);
        Assert.Equal(FighterState.GuardRaising, f.State);

        f.Tick(None);

        Assert.Equal(FighterState.Idle, f.State);
    }

    [Fact]
    public void Guarding_IgnoresJabAndCross()
    {
        var f = NewFighter();
        TickN(f, Moves.GuardRaiseFrames + 2, HoldGuard);

        f.Tick(new FighterInput(GuardHeld: true, JabPressed: true));
        Assert.Equal(FighterState.Guarding, f.State);
        f.Tick(new FighterInput(GuardHeld: true, CrossPressed: true));
        Assert.Equal(FighterState.Guarding, f.State);
    }

    [Theory]
    [InlineData(FighterState.SlipLeft)]
    [InlineData(FighterState.SlipRight)]
    public void Guarding_SlipPress_StartsThatSlip_AndGuardResumesAfterIt(FighterState slip)
    {
        var f = NewFighter();
        var def = Moves.Get(slip);
        TickN(f, Moves.GuardRaiseFrames + 2, HoldGuard);
        Assert.Equal(FighterState.Guarding, f.State);

        f.Tick(new FighterInput(GuardHeld: true, SlipLeftPressed: slip == FighterState.SlipLeft, SlipRightPressed: slip == FighterState.SlipRight));
        AssertAt(f, slip, MovePhase.Startup, 1);
        Assert.False(f.IsBlocking);

        TickN(f, def.TotalFrames - 1, HoldGuard);
        Assert.Equal(slip, f.State);
        f.Tick(HoldGuard);
        Assert.Equal(FighterState.GuardRaising, f.State);
        TickN(f, Moves.GuardRaiseFrames, HoldGuard);
        Assert.Equal(FighterState.Guarding, f.State);
    }

    [Fact]
    public void GuardHeldDuringAMove_IsIgnored_ThenGuardRaisesOnTheTickTheMoveEnds()
    {
        var f = NewFighter();
        var jab = Moves.Get(FighterState.Jab);
        f.Tick(PressJab);
        TickN(f, jab.TotalFrames - 1, HoldGuard);
        Assert.Equal(FighterState.Jab, f.State);

        f.Tick(HoldGuard);

        AssertAt(f, FighterState.GuardRaising, MovePhase.None, 0);
    }

    // ---- AC15: same-tick priority ----

    [Fact]
    public void SameTick_SlipLeftBeatsEverything()
    {
        var f = NewFighter();

        f.Tick(new FighterInput(JabPressed: true, CrossPressed: true, SlipLeftPressed: true, SlipRightPressed: true));

        AssertAt(f, FighterState.SlipLeft, MovePhase.Startup, 1);
    }

    [Fact]
    public void SameTick_SlipRightBeatsPunches()
    {
        var f = NewFighter();

        f.Tick(new FighterInput(JabPressed: true, CrossPressed: true, SlipRightPressed: true));

        AssertAt(f, FighterState.SlipRight, MovePhase.Startup, 1);
    }

    [Fact]
    public void SameTick_JabBeatsCross()
    {
        var f = NewFighter();

        f.Tick(new FighterInput(JabPressed: true, CrossPressed: true));

        AssertAt(f, FighterState.Jab, MovePhase.Startup, 1);
    }

    [Fact]
    public void SameTick_GuardWinsOverAPunch_FromIdle()
    {
        var f = NewFighter();

        f.Tick(new FighterInput(GuardHeld: true, JabPressed: true));

        AssertAt(f, FighterState.GuardRaising, MovePhase.None, 0);
    }

    [Fact]
    public void SameTick_IllegalWinnerIsSkipped_SoALegalCancelStillFires()
    {
        // In jab Recovery a slip press is illegal but Cross is a legal cancel: the cross must not be thrown away.
        var f = NewFighter();
        var jab = Moves.Get(FighterState.Jab);
        f.Tick(PressJab);
        TickN(f, jab.StartupFrames + jab.ActiveFrames - 1, None);

        f.Tick(new FighterInput(CrossPressed: true, SlipLeftPressed: true));

        AssertAt(f, FighterState.Cross, MovePhase.Startup, 1);
    }

    // ---- AC11, AC12, AC13: movement ----

    private static FighterInput Walk(int axis, bool guard = false) => new(MoveAxis: axis, GuardHeld: guard);

    [Fact]
    public void Walking_ForwardAndBack_ChangesXByExactlySpeedOverTickRatePerTick()
    {
        var f = NewFighter();
        var step = Moves.WalkSpeedMps / Moves.TickRate;

        TickN(f, 10, Walk(1));
        Assert.Equal(Roster.PlayerX + (10 * step), f.X, precision: 6);

        TickN(f, 25, Walk(-1));
        Assert.Equal(Roster.PlayerX + (10 * step) - (25 * step), f.X, precision: 6);
    }

    [Fact]
    public void Walking_BothOrNeitherKeyHeld_LeavesXUnchanged()
    {
        var f = NewFighter();
        TickN(f, 10, None);
        Assert.Equal(Roster.PlayerX, f.X);

        // Forward and back together arrive as axis 0.
        TickN(f, 10, Walk(0));
        Assert.Equal(Roster.PlayerX, f.X);
    }

    [Fact]
    public void Walking_WhileGuarding_IsSlowedByTheMultiplier_RaisingAndGuarding()
    {
        var f = NewFighter();
        var step = Moves.WalkSpeedMps / Moves.TickRate * Moves.GuardMoveSpeedMultiplier;
        var before = f.X;

        f.Tick(Walk(1, guard: true));
        Assert.Equal(FighterState.GuardRaising, f.State);
        Assert.Equal(before + step, f.X, precision: 6);

        TickN(f, Moves.GuardRaiseFrames + 1, Walk(1, guard: true));
        Assert.Equal(FighterState.Guarding, f.State);
        before = f.X;
        f.Tick(Walk(1, guard: true));
        Assert.Equal(before + step, f.X, precision: 6);
    }

    [Theory]
    [InlineData(FighterState.Jab)]
    [InlineData(FighterState.Cross)]
    [InlineData(FighterState.SlipLeft)]
    [InlineData(FighterState.SlipRight)]
    public void Moves_DoNotWalk_AndWalkingResumesOnTheFreeTick(FighterState move)
    {
        var f = NewFighter();
        var def = Moves.Get(move);
        var press = move switch
        {
            FighterState.Jab => new FighterInput(MoveAxis: 1, JabPressed: true),
            FighterState.Cross => new FighterInput(MoveAxis: 1, CrossPressed: true),
            FighterState.SlipLeft => new FighterInput(MoveAxis: 1, SlipLeftPressed: true),
            _ => new FighterInput(MoveAxis: 1, SlipRightPressed: true),
        };
        var start = f.X;

        f.Tick(press);
        TickN(f, def.TotalFrames - 1, Walk(1));
        Assert.Equal(move, f.State);
        Assert.Equal(start, f.X);

        f.Tick(Walk(1));
        Assert.Equal(FighterState.Idle, f.State);
        Assert.Equal(start + (Moves.WalkSpeedMps / Moves.TickRate), f.X, precision: 6);
    }

    [Fact]
    public void Walking_BackForTenSeconds_StopsExactlyAtMinX_AndNeverLeavesTheBounds()
    {
        var f = NewFighter();
        for (var i = 0; i < 10 * Moves.TickRate; i++)
        {
            f.Tick(Walk(-1));
            Assert.InRange(f.X, Moves.MinX, Roster.OpponentX - Moves.MinSeparationM);
        }

        Assert.Equal(Moves.MinX, f.X);
    }

    [Fact]
    public void Walking_ForwardForTenSeconds_StopsExactlyAtMinSeparation_AndNeverPassesTheOpponent()
    {
        var f = NewFighter();
        for (var i = 0; i < 10 * Moves.TickRate; i++)
        {
            f.Tick(Walk(1));
            Assert.InRange(f.X, Moves.MinX, Roster.OpponentX - Moves.MinSeparationM);
        }

        Assert.Equal(Roster.OpponentX - Moves.MinSeparationM, f.X);
    }

    // ---- AC14: contact ----

    private static List<ContactKind> RunAndCollectContacts(PlayerFighter f, FighterInput press, int total)
    {
        var contacts = new List<ContactKind>();
        f.Tick(press);
        if (f.Contact != ContactKind.None)
        {
            contacts.Add(f.Contact);
        }

        for (var i = 1; i < total + 2; i++)
        {
            f.Tick(None);
            if (f.Contact != ContactKind.None)
            {
                contacts.Add(f.Contact);
            }
        }

        return contacts;
    }

    [Fact]
    public void Jab_InReach_ReportsExactlyOneContact_OnItsFirstActiveTick()
    {
        var f = NewFighterAtGap(Moves.Get(FighterState.Jab).ReachM - 0.01);
        var def = Moves.Get(FighterState.Jab);
        f.Tick(PressJab);
        for (var tick = 1; tick <= def.TotalFrames; tick++)
        {
            var firstActive = tick == def.StartupFrames + 1;
            Assert.Equal(firstActive ? ContactKind.Jab : ContactKind.None, f.Contact);
            f.Tick(None);
        }

        Assert.Equal(ContactKind.None, f.Contact);
    }

    [Fact]
    public void Jab_ExactlyAtReach_Contacts()
    {
        var f = NewFighterAtGap(Moves.Get(FighterState.Jab).ReachM);

        var contacts = RunAndCollectContacts(f, PressJab, Moves.Get(FighterState.Jab).TotalFrames);

        Assert.Equal(new[] { ContactKind.Jab }, contacts);
    }

    [Fact]
    public void BetweenJabAndCrossReach_JabMisses_CrossContactsOnce()
    {
        var gap = (Moves.Get(FighterState.Jab).ReachM + Moves.Get(FighterState.Cross).ReachM) / 2;

        var jabContacts = RunAndCollectContacts(NewFighterAtGap(gap), PressJab, Moves.Get(FighterState.Jab).TotalFrames);
        var crossContacts = RunAndCollectContacts(NewFighterAtGap(gap), PressCross, Moves.Get(FighterState.Cross).TotalFrames);

        Assert.Empty(jabContacts);
        Assert.Equal(new[] { ContactKind.Cross }, crossContacts);
    }

    [Fact]
    public void BeyondCrossReach_NeitherPunchContacts()
    {
        var gap = Moves.Get(FighterState.Cross).ReachM + 0.05;

        Assert.Empty(RunAndCollectContacts(NewFighterAtGap(gap), PressJab, 40));
        Assert.Empty(RunAndCollectContacts(NewFighterAtGap(gap), PressCross, 40));
    }

    [Fact]
    public void Slips_NeverContact()
    {
        Assert.Empty(RunAndCollectContacts(NewFighterAtGap(0.8), PressSlipLeft, 40));
        Assert.Empty(RunAndCollectContacts(NewFighterAtGap(0.8), PressSlipRight, 40));
    }

    [Fact]
    public void StartPosition_JabIsOutOfRange_NoContact()
    {
        var contacts = RunAndCollectContacts(NewFighter(), PressJab, 40);

        Assert.Empty(contacts);
    }

    // ---- AC16 ----

    [Fact]
    public void OpponentX_NeverChanges_ThroughAnyInputSequence()
    {
        var f = NewFighter();
        var inputs = new[] { PressJab, PressCross, PressSlipLeft, PressSlipRight, HoldGuard, Walk(1), Walk(-1), None };
        var rng = new Random(7);
        for (var i = 0; i < 2000; i++)
        {
            f.Tick(inputs[rng.Next(inputs.Length)]);
            Assert.Equal(Roster.OpponentX, f.OpponentX);
        }
    }

    [Fact]
    public void ConstructorStartOutsideTheBounds_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PlayerFighter(Moves, Roster.OpponentX, Roster.OpponentX));
        Assert.Throws<ArgumentOutOfRangeException>(() => new PlayerFighter(Moves, Moves.MinX - 1, Roster.OpponentX));
    }
}
