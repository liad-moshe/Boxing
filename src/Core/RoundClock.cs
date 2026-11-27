using System;

namespace Boxing.Core;

/// <summary>
/// Counts down one round. Pure logic: the Godot layer calls <see cref="Tick"/> with frame time.
/// </summary>
public sealed class RoundClock
{
    public RoundClock(double durationSeconds)
    {
        if (double.IsNaN(durationSeconds) || durationSeconds <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(durationSeconds), durationSeconds, "Round duration must be positive.");
        }

        DurationSeconds = durationSeconds;
        RemainingSeconds = durationSeconds;
    }

    public double DurationSeconds { get; }

    public double RemainingSeconds { get; private set; }

    public bool IsOver => RemainingSeconds <= 0;

    /// <summary>Advances the clock. Negative or NaN time is ignored; the clock never goes below zero.</summary>
    public void Tick(double seconds)
    {
        if (double.IsNaN(seconds) || seconds <= 0 || IsOver)
        {
            return;
        }

        RemainingSeconds = Math.Max(0, RemainingSeconds - seconds);
    }
}
