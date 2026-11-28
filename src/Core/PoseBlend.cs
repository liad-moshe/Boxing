namespace Boxing.Core;

/// <summary>
/// How far each pose is blended in, 0 (the build's guard pose) to 1 (full), derived from the fighter's
/// state, phase, frame and stance. The Godot adapter turns these into part positions using the data file's
/// <see cref="VisualTuning"/> offsets. Idle is exactly all zero. Arms are anatomical (the boxer's own left and
/// right); the stance only decides which one a punch drives and which way the cross turns the torso, so a
/// southpaw pose is the orthodox pose reflected (left and right swapped, <see cref="TorsoTurn"/> negated).
/// </summary>
/// <param name="LeftExtend">Left glove punch extension.</param>
/// <param name="RightExtend">Right glove punch extension.</param>
/// <param name="TorsoTurn">Signed cross torso turn, -1..1: positive brings the right shoulder forward (orthodox cross), negative the left (southpaw cross).</param>
/// <param name="Guard">Both gloves raised and inward.</param>
/// <param name="Slip">Upper-body dip, lean and shoulder turn (not mirrored by stance: the side is the boxer's own).</param>
/// <param name="SlipSide">Which side the slip leans to; None when not slipping.</param>
public readonly record struct PoseBlend(double LeftExtend, double RightExtend, double TorsoTurn, double Guard, double Slip, EvadeSide SlipSide)
{
    public static PoseBlend From(PlayerFighter fighter, MoveSet moves, Stance stance)
    {
        switch (fighter.State)
        {
            case FighterState.GuardRaising:
                return new PoseBlend(0, 0, 0, fighter.GuardFrame / (double)moves.GuardRaiseFrames, 0, EvadeSide.None);
            case FighterState.Guarding:
                return new PoseBlend(0, 0, 0, 1, 0, EvadeSide.None);
            case FighterState.Jab:
            case FighterState.Cross:
                return Punch(fighter, moves, stance);
            case FighterState.SlipLeft:
                return new PoseBlend(0, 0, 0, 0, Amount(fighter, moves), EvadeSide.Left);
            case FighterState.SlipRight:
                return new PoseBlend(0, 0, 0, 0, Amount(fighter, moves), EvadeSide.Right);
            default:
                return default;
        }
    }

    private static PoseBlend Punch(PlayerFighter fighter, MoveSet moves, Stance stance)
    {
        var amount = Amount(fighter, moves);
        var left = StanceRules.PunchSide(fighter.State, stance) == BodySide.Left ? amount : 0;
        var right = StanceRules.PunchSide(fighter.State, stance) == BodySide.Right ? amount : 0;
        var turn = fighter.State == FighterState.Cross ? amount * StanceRules.MirrorSign(stance) : 0;
        return new PoseBlend(left, right, turn, 0, 0, EvadeSide.None);
    }

    /// <summary>Ramps up through Startup, holds 1 through Active, eases back down through Recovery (never exactly 0 until Idle).</summary>
    private static double Amount(PlayerFighter fighter, MoveSet moves)
    {
        var def = moves.Get(fighter.State);
        var frame = fighter.PhaseFrame;
        return fighter.Phase switch
        {
            MovePhase.Startup => frame / (double)def.StartupFrames,
            MovePhase.Active => 1.0,
            _ => (def.RecoveryFrames - frame + 1) / (double)(def.RecoveryFrames + 1),
        };
    }
}
