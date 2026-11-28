namespace Boxing.Core;

/// <summary>
/// The "Contact: Jab" / "Contact: Cross" text and how long it stays. Counts whole ticks: a contact shown on
/// tick T stays visible through tick T + duration - 1. A newer contact replaces the text and restarts the timer.
/// Call <see cref="Tick"/> once per simulation tick, before <see cref="Show"/> for that tick's contact.
/// </summary>
public sealed class ContactBanner
{
    private readonly int _duration;
    private int _remaining;
    private string _text = string.Empty;

    public ContactBanner(int durationTicks)
    {
        _duration = durationTicks;
    }

    public bool IsVisible => _remaining > 0;

    public string Text => IsVisible ? _text : string.Empty;

    public void Show(ContactKind kind)
    {
        if (kind == ContactKind.None)
        {
            return;
        }

        _text = kind == ContactKind.Jab ? "Contact: Jab" : "Contact: Cross";
        _remaining = _duration;
    }

    public void Tick()
    {
        if (_remaining > 0)
        {
            _remaining--;
        }
    }
}
