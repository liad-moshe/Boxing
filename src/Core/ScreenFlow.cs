namespace Boxing.Core;

public enum Screen
{
    Menu,
    CharacterSelect,
    Ring,
}

public enum BackResult
{
    QuitApp,
    GoToMenu,
}

/// <summary>Pure screen-flow decisions; Godot adapters perform the returned action.</summary>
public static class ScreenFlow
{
    public static BackResult OnBack(Screen current) => current switch
    {
        Screen.Ring => BackResult.GoToMenu,
        // Select step 2 -> step 1 is decided by CharacterSelectFlow.Back; this is the step-1 answer.
        Screen.CharacterSelect => BackResult.GoToMenu,
        _ => BackResult.QuitApp,
    };
}
