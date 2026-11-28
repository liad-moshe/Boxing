namespace Boxing.Core;

public enum SelectStep
{
    PickPlayer,
    PickOpponent,
    Done,
}

public enum SelectBackResult
{
    GoToMenu,
    GoToPlayerStep,
    Stay,
}

/// <summary>
/// Two-step character select state. <see cref="Confirm"/> returns false (state unchanged) for an id not in the
/// roster or once the step is <see cref="SelectStep.Done"/>.
/// </summary>
public sealed class CharacterSelectFlow
{
    private readonly FighterRoster _roster;
    private readonly Stance _defaultStance;

    public CharacterSelectFlow(FighterRoster roster, Stance defaultStance = Stance.Orthodox)
    {
        _roster = roster;
        _defaultStance = defaultStance;
        PlayerStance = defaultStance;
        OpponentStance = defaultStance;
    }

    public SelectStep Step { get; private set; } = SelectStep.PickPlayer;

    public string? PlayerId { get; private set; }

    public string? OpponentId { get; private set; }

    public Stance PlayerStance { get; private set; }

    public Stance OpponentStance { get; private set; }

    /// <summary>
    /// Flips the stance of the boxer being picked in the current step. Returns false and changes nothing once
    /// the step is <see cref="SelectStep.Done"/>.
    /// </summary>
    public bool ToggleStance()
    {
        switch (Step)
        {
            case SelectStep.PickPlayer:
                PlayerStance = StanceRules.Toggle(PlayerStance);
                return true;
            case SelectStep.PickOpponent:
                OpponentStance = StanceRules.Toggle(OpponentStance);
                return true;
            default:
                return false;
        }
    }

    public bool Confirm(string fighterId)
    {
        if (Step == SelectStep.Done || _roster.IndexOf(fighterId) < 0)
        {
            return false;
        }

        if (Step == SelectStep.PickPlayer)
        {
            PlayerId = fighterId;
            OpponentStance = _defaultStance;
            Step = SelectStep.PickOpponent;
        }
        else
        {
            OpponentId = fighterId;
            Step = SelectStep.Done;
        }

        return true;
    }

    public SelectBackResult Back()
    {
        switch (Step)
        {
            case SelectStep.PickPlayer:
                PlayerId = null;
                OpponentId = null;
                PlayerStance = _defaultStance;
                OpponentStance = _defaultStance;
                return SelectBackResult.GoToMenu;
            case SelectStep.PickOpponent:
                OpponentId = null;
                OpponentStance = _defaultStance;
                Step = SelectStep.PickPlayer;
                return SelectBackResult.GoToPlayerStep;
            default:
                return SelectBackResult.Stay;
        }
    }
}
