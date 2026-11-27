namespace Boxing.Core;

public enum Screen
{
    Menu,
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
        _ => BackResult.QuitApp,
    };
}
