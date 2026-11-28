using System;

namespace Boxing.Core;

/// <summary>
/// One tick of player input. The four <c>Pressed</c> flags are true only on the tick of a fresh press
/// (never on key repeat or while held). <c>MoveAxis</c> is -1 (back), 0 or +1 (forward).
/// </summary>
public readonly record struct FighterInput(
    int MoveAxis = 0,
    bool GuardHeld = false,
    bool JabPressed = false,
    bool CrossPressed = false,
    bool SlipLeftPressed = false,
    bool SlipRightPressed = false);

/// <summary>
/// The player's move state machine, advanced in whole ticks (60 per second from the data file).
/// A move started on tick N occupies ticks N to N + startup + active + recovery - 1; on the next tick the
/// fighter is free and that tick's input is processed normally. Order inside one tick: (1) move timer and
/// move end, (2) guard held or released, (3) fresh presses, (4) walking, (5) contact check.
/// The opponent is idle: only its fixed X is known here.
/// </summary>
public sealed class PlayerFighter
{
    private const double ReachEpsilon = 1e-9;

    private readonly MoveSet _moves;
    private readonly double _maxX;
    private int _moveTick;
    private bool _contactReported;

    public PlayerFighter(MoveSet moves, double startX, double opponentX)
    {
        _moves = moves;
        OpponentX = opponentX;
        _maxX = opponentX - moves.MinSeparationM;
        if (startX < moves.MinX || startX > _maxX)
        {
            throw new ArgumentOutOfRangeException(nameof(startX), startX, $"Start X must be between {moves.MinX} and {_maxX}.");
        }

        X = startX;
    }

    public FighterState State { get; private set; } = FighterState.Idle;

    /// <summary>Position along the move's timeline; None unless State is a timed move.</summary>
    public MovePhase Phase
    {
        get
        {
            if (!MoveSet.IsTimedMove(State))
            {
                return MovePhase.None;
            }

            var def = _moves.Get(State);
            if (_moveTick <= def.StartupFrames)
            {
                return MovePhase.Startup;
            }

            return _moveTick <= def.StartupFrames + def.ActiveFrames ? MovePhase.Active : MovePhase.Recovery;
        }
    }

    /// <summary>1-based tick inside the current phase; 0 when Phase is None.</summary>
    public int PhaseFrame
    {
        get
        {
            var def = MoveSet.IsTimedMove(State) ? _moves.Get(State) : null;
            return Phase switch
            {
                MovePhase.Startup => _moveTick,
                MovePhase.Active => _moveTick - def!.StartupFrames,
                MovePhase.Recovery => _moveTick - def!.StartupFrames - def.ActiveFrames,
                _ => 0,
            };
        }
    }

    /// <summary>1-based tick inside GuardRaising (1 to raiseFrames); 0 in every other state.</summary>
    public int GuardFrame { get; private set; }

    public double X { get; private set; }

    /// <summary>Fixed opponent position; never changes.</summary>
    public double OpponentX { get; }

    public bool IsBlocking => State == FighterState.Guarding;

    public bool IsEvading => IsSlip(State) && Phase == MovePhase.Active;

    public EvadeSide EvadeSide => !IsEvading ? EvadeSide.None : (State == FighterState.SlipLeft ? EvadeSide.Left : EvadeSide.Right);

    /// <summary>The punch contact reported by the latest tick (None on most ticks).</summary>
    public ContactKind Contact { get; private set; }

    public void Tick(FighterInput input)
    {
        Contact = ContactKind.None;

        // (1) Move timer and move end.
        if (MoveSet.IsTimedMove(State))
        {
            _moveTick++;
            if (_moveTick > _moves.Get(State).TotalFrames)
            {
                State = FighterState.Idle;
                _moveTick = 0;
            }
        }

        // (2) Guard is level-based and only changes while the fighter is free.
        if (IsFree(State))
        {
            UpdateGuard(input.GuardHeld);
        }

        // (3) Fresh presses. Priority: SlipLeft, SlipRight, Jab, Cross. The first press that is legal now wins.
        if (input.SlipLeftPressed && CanStart(FighterState.SlipLeft))
        {
            Start(FighterState.SlipLeft);
        }
        else if (input.SlipRightPressed && CanStart(FighterState.SlipRight))
        {
            Start(FighterState.SlipRight);
        }
        else if (input.JabPressed && CanStart(FighterState.Jab))
        {
            Start(FighterState.Jab);
        }
        else if (input.CrossPressed && CanStart(FighterState.Cross))
        {
            Start(FighterState.Cross);
        }

        // (4) Walking, then the bounds every tick.
        if (IsFree(State))
        {
            var step = Math.Sign(input.MoveAxis) * _moves.WalkSpeedMps / _moves.TickRate;
            if (State is FighterState.GuardRaising or FighterState.Guarding)
            {
                step *= _moves.GuardMoveSpeedMultiplier;
            }

            X += step;
        }

        X = Math.Min(Math.Max(X, _moves.MinX), _maxX);

        // (5) Contact: first Active tick of a punch whose reach covers the gap.
        if (State is FighterState.Jab or FighterState.Cross && Phase == MovePhase.Active && !_contactReported
            && OpponentX - X <= _moves.Get(State).ReachM + ReachEpsilon)
        {
            _contactReported = true;
            Contact = State == FighterState.Jab ? ContactKind.Jab : ContactKind.Cross;
        }
    }

    private static bool IsSlip(FighterState state) => state is FighterState.SlipLeft or FighterState.SlipRight;

    private static bool IsFree(FighterState state) =>
        state is FighterState.Idle or FighterState.GuardRaising or FighterState.Guarding;

    private void UpdateGuard(bool held)
    {
        if (!held)
        {
            State = FighterState.Idle;
            GuardFrame = 0;
        }
        else if (State == FighterState.Idle)
        {
            State = FighterState.GuardRaising;
            GuardFrame = 1;
        }
        else if (State == FighterState.GuardRaising)
        {
            GuardFrame++;
            if (GuardFrame > _moves.GuardRaiseFrames)
            {
                State = FighterState.Guarding;
                GuardFrame = 0;
            }
        }
    }

    private bool CanStart(FighterState move)
    {
        switch (State)
        {
            case FighterState.Idle:
                return true;
            case FighterState.GuardRaising:
            case FighterState.Guarding:
                return IsSlip(move);
            default:
                // A timed move: only its Recovery can be cancelled, and only into the data file's targets.
                if (Phase != MovePhase.Recovery)
                {
                    return false;
                }

                foreach (var target in _moves.Get(State).CancelInto)
                {
                    if (target == move)
                    {
                        return true;
                    }
                }

                return false;
        }
    }

    private void Start(FighterState move)
    {
        State = move;
        _moveTick = 1;
        GuardFrame = 0;
        _contactReported = false;
    }
}
