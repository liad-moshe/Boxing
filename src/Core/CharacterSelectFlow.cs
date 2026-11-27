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

    public CharacterSelectFlow(FighterRoster roster)
    {
        _roster = roster;
    }

    public SelectStep Step { get; private set; } = SelectStep.PickPlayer;

    public string? PlayerId { get; private set; }

    public string? OpponentId { get; private set; }

    public bool Confirm(string fighterId)
    {
        if (Step == SelectStep.Done || _roster.IndexOf(fighterId) < 0)
        {
            return false;
        }

        if (Step == SelectStep.PickPlayer)
        {
            PlayerId = fighterId;
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
                return SelectBackResult.GoToMenu;
            case SelectStep.PickOpponent:
                OpponentId = null;
                Step = SelectStep.PickPlayer;
                return SelectBackResult.GoToPlayerStep;
            default:
                return SelectBackResult.Stay;
        }
    }
}
