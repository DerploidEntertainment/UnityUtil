using Sirenix.OdinInspector;
using UnityEngine;
using UnityUtil.Updating;

namespace UnityUtil.Triggers;

public enum EnableDisableBehavior
{
    PauseResume,
    StopRestart,
    StopRestartAlways,
}

public abstract class StartStoppable : Updatable
{
    private bool _starting = true;
    private bool _wasRunningB4Disable;

    [Tooltip(
        "What should happen when this component is enabled/disabled? " +
        $"{nameof(EnableDisableBehavior.PauseResume)} will pause/resume it, if it was running. " +
        $"{nameof(EnableDisableBehavior.StopRestart)} will stop/restart it, if it was running. " +
        $"{nameof(EnableDisableBehavior.StopRestartAlways)} will stop/restart it, restarting it even if it was not previously running. " +
        $"In all cases, the first OnEnable is still controlled by {nameof(StartAutomatically)}."
    )]
    public EnableDisableBehavior EnableDisableBehavior = EnableDisableBehavior.PauseResume;
    [Tooltip("Should the repeater start automatically when this GameObject is enabled/started for the first time?")]
    public bool StartAutomatically = false;

    protected override void OnEnable()
    {
        base.OnEnable();

        // If the GameObject is starting (i.e., this is the first-ever call to OnEnable)...
        if (_starting) {
            _starting = false;
            if (StartAutomatically)
                DoRestart();
        }

        // Otherwise...
        else {
            switch (EnableDisableBehavior) {
                case EnableDisableBehavior.PauseResume:
                    if (_wasRunningB4Disable)
                        DoResume();
                    break;

                case EnableDisableBehavior.StopRestart:
                    if (_wasRunningB4Disable)
                        DoRestart();
                    break;

                case EnableDisableBehavior.StopRestartAlways:
                    DoRestart();
                    break;

                default:
                    throw UnityObjectExtensions.SwitchDefaultException(EnableDisableBehavior);
            }
        }
    }
    protected override void OnDisable()
    {
        base.OnDisable();

        _wasRunningB4Disable = Running;

        switch (EnableDisableBehavior) {
            case EnableDisableBehavior.PauseResume:
                DoPause();
                break;

            case EnableDisableBehavior.StopRestart:
            case EnableDisableBehavior.StopRestartAlways:
                DoStop();
                break;

            default:
                throw UnityObjectExtensions.SwitchDefaultException(EnableDisableBehavior);
        }
    }

    public bool Running { get; private set; }

    private const string GRP_BUTTONS = "Buttons";

    [Button("Start"), HorizontalGroup(GRP_BUTTONS)]
    public void StartBehavior()
    {
        this.AssertActiveAndEnabled("start");
        if (Running)
            return;

        DoRestart();
    }
    [Button("Restart"), HorizontalGroup(GRP_BUTTONS)]
    public void RestartBehavior()
    {
        this.AssertActiveAndEnabled("restart");
        if (Running)
            DoStop();
        DoRestart();
    }
    [Button("Pause"), HorizontalGroup(GRP_BUTTONS)]
    public void PauseBehavior()
    {
        this.AssertActiveAndEnabled("pause");
        DoPause();
    }
    [Button("Resume"), HorizontalGroup(GRP_BUTTONS)]
    public void ResumeBehavior()
    {
        this.AssertActiveAndEnabled("resume");
        DoResume();
    }
    [Button("Stop"), HorizontalGroup(GRP_BUTTONS)]
    public void StopBehavior()
    {
        this.AssertActiveAndEnabled("stop");
        if (!Running)
            return;

        DoStop();
    }

    protected virtual void DoRestart()
    {
        RegisterUpdate(DoUpdate);
        Running = true;
    }
    protected virtual void DoStop()
    {
        if (!Running)
            return;

        Running = false;
        UnregisterUpdate();
    }
    protected virtual void DoPause()
    {
        if (!Running)
            return;

        Running = false;
        UnregisterUpdate();
    }
    protected virtual void DoResume()
    {
        if (Running)
            return;

        Running = true;
        RegisterUpdate(DoUpdate);
    }
    protected abstract void DoUpdate(float deltaTime);

}
