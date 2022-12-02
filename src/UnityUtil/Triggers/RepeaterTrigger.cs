using System;
using UnityEngine;
using UnityEngine.Events;
using UnityUtil.Logging;

namespace UnityUtil.Triggers;

[Serializable]
public class CountEvent : UnityEvent<uint> { }

public class RepeaterTrigger : StartStoppable
{
    [Tooltip($"The time, in seconds, before the next (or first) {nameof(Tick)} event.")]
    public float TimeBeforeTick = 1f;

    [Tooltip($"The time, in seconds, that has passed since the previous {nameof(Tick)} event.")]
    public float TimeSincePreviousTick = 0f;

    [Tooltip($"How many {nameof(Tick)}s should be raised before stopping? Ignored if {nameof(TickForever)} is true.")]
    public uint NumTicks = 1u;

    [Tooltip(
        $"If true, then this {nameof(RepeaterTrigger)} will raise {nameof(Tick)} events forever. " +
        $"If false, then it will only {nameof(Tick)} {nameof(NumTicks)} times."
    )]
    public bool TickForever = false;

    [Tooltip($"Number of {nameof(Tick)} events that have already been raised.")]
    public uint NumPassedTicks = 0u;

    public bool Logging;

    public CountEvent Tick = new();
    public UnityEvent Stopped = new();
    public UnityEvent NumTicksReached = new();

    public float PercentProgress => TimeSincePreviousTick / TimeBeforeTick;

    protected override void DoRestart()
    {
        base.DoRestart();

        if (Logging)
            Logger!.Log("Starting.", context: this);

        TimeSincePreviousTick = 0f;
        NumPassedTicks = 0u;
    }
    protected override void DoStop()
    {
        base.DoStop();

        if (Logging)
            Logger!.Log($"Stopped.", context: this);
        Stopped.Invoke();
    }
    protected override void DoPause()
    {
        base.DoStop();

        if (Logging)
            Logger!.Log($"Paused.", context: this);
    }
    protected override void DoResume()
    {
        base.DoResume();

        if (Logging)
            Logger!.Log("Resumed.", context: this);
    }
    protected override void DoUpdate(float deltaTime)
    {
        // Update the time elapsed, if the Timer is running
        TimeSincePreviousTick += deltaTime;

        // If another Tick period has passed, then raise the Tick event
        if (TimeSincePreviousTick >= TimeBeforeTick) {
            if (Logging)
                Logger!.Log(TickForever ? "Tick!" : $"Tick {NumPassedTicks} / {NumTicks}", context: this);
            Tick.Invoke(NumPassedTicks);
            TimeSincePreviousTick = 0f;
            ++NumPassedTicks;
        }
        if (NumPassedTicks < NumTicks || TickForever)
            return;

        // If the desired number of ticks was reached, then stop the Timer
        if (Logging)
            Logger!.Log($"Reached {NumTicks} ticks.", context: this);
        NumTicksReached.Invoke();
        if (Running)   // May now be false if any UnityEvents manually stopped this repeater
            DoStop();
    }

}
