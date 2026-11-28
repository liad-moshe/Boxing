namespace Boxing.Core;

/// <summary>
/// How far each pose is blended in, 0 (the build's guard pose) to 1 (full), derived from the fighter's
/// state, phase and frame. The Godot adapter turns these into part positions using the data file's
/// <see cref="VisualTuning"/> offsets. Idle is exactly all zero.
/// </summary>
/// <param name="LeadExtend">Lead (left) glove punch extension: the jab.</param>
/// <param name="RearExtend">Rear (right) glove punch extension and torso turn: the cross.</param>
/// <param name="Guard">Both gloves raised and inward.</param>
/// <param name="Slip">Upper-body dip, lean and shoulder turn.</param>
/// <param name="SlipSide">Which side the slip leans to; None when not slipping.</param>
public readonly record struct PoseBlend(double LeadExtend, double RearExtend, double Guard, double Slip, EvadeSide SlipSide)
{
    public static PoseBlend From(PlayerFighter fighter, MoveSet moves)
    {
        switch (fighter.State)
        {
            case FighterState.GuardRaising:
                return new PoseBlend(0, 0, fighter.GuardFrame / (double)moves.GuardRaiseFrames, 0, EvadeSide.None);
            case FighterState.Guarding:
                return new PoseBlend(0, 0, 1, 0, EvadeSide.None);
            case FighterState.Jab:
                return new PoseBlend(Amount(fighter, moves), 0, 0, 0, EvadeSide.None);
            case FighterState.Cross:
                return new PoseBlend(0, Amount(fighter, moves), 0, 0, EvadeSide.None);
            case FighterState.SlipLeft:
                return new PoseBlend(0, 0, 0, Amount(fighter, moves), EvadeSide.Left);
            case FighterState.SlipRight:
                return new PoseBlend(0, 0, 0, Amount(fighter, moves), EvadeSide.Right);
            default:
                return default;
        }
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
