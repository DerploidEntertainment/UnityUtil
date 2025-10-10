using Sirenix.OdinInspector;
using UnityEngine;
using UnityUtil.Updating;

namespace UnityUtil.Triggers;

public enum EnableDisableBehavior
{
    PauseResume,
    StopRestartIfRunning,
    StopRestartAlways,
}

public abstract class StartStoppable : Updatable
{
    private bool _starting = true;
    private bool _wasRunningB4Disable;

    [Tooltip(
        "What should happen when this component is enabled/disabled?" +
        $"\n" +
        $"\n- {nameof(EnableDisableBehavior.PauseResume)} will pause/resume it, if it was running. " +
        $"\n- {nameof(EnableDisableBehavior.StopRestartIfRunning)} will stop/restart it, if it was running. " +
        $"\n- {nameof(EnableDisableBehavior.StopRestartAlways)} will stop/restart it, restarting it even if it was not previously running. " +
        $"\n" +
        $"\nIn all cases, the behavior of the first OnEnable() call is still controlled by {nameof(StartOnFirstOnEnable)}."
    )]
    public EnableDisableBehavior EnableDisableBehavior = EnableDisableBehavior.PauseResume;
    [Tooltip("Should this component start automatically during its first OnEnable() call?")]
    [LabelText("Start on First OnEnable")]
    public bool StartOnFirstOnEnable = false;

    protected override void OnEnable()
    {
        base.OnEnable();

        // If the GameObject is starting (i.e., this is the first-ever call to OnEnable)...
        if (_starting) {
            _starting = false;
            if (StartOnFirstOnEnable)
                DoRestart();
        }

        // Otherwise...
        else {
            switch (EnableDisableBehavior) {
                case EnableDisableBehavior.PauseResume:
                    if (_wasRunningB4Disable)
                        DoResume();
                    break;

                case EnableDisableBehavior.StopRestartIfRunning:
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

            case EnableDisableBehavior.StopRestartIfRunning:
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
        this.ThrowIfInactiveOrDisabled("start");
        if (Running)
            return;

        DoRestart();
    }
    [Button("Restart"), HorizontalGroup(GRP_BUTTONS)]
    public void RestartBehavior()
    {
        this.ThrowIfInactiveOrDisabled("restart");
        if (Running)
            DoStop();
        DoRestart();
    }
    [Button("Pause"), HorizontalGroup(GRP_BUTTONS)]
    public void PauseBehavior()
    {
        this.ThrowIfInactiveOrDisabled("pause");
        DoPause();
    }
    [Button("Resume"), HorizontalGroup(GRP_BUTTONS)]
    public void ResumeBehavior()
    {
        this.ThrowIfInactiveOrDisabled("resume");
        DoResume();
    }
    [Button("Stop"), HorizontalGroup(GRP_BUTTONS)]
    public void StopBehavior()
    {
        this.ThrowIfInactiveOrDisabled("stop");
        if (!Running)
            return;

        DoStop();
    }

    protected virtual void DoRestart()
    {
        AddUpdate(DoUpdate);
        Running = true;
    }
    protected virtual void DoStop()
    {
        if (!Running)
            return;

        Running = false;
        RemoveUpdate();
    }
    protected virtual void DoPause()
    {
        if (!Running)
            return;

        Running = false;
        RemoveUpdate();
    }
    protected virtual void DoResume()
    {
        if (Running)
            return;

        Running = true;
        AddUpdate(DoUpdate);
    }
    protected abstract void DoUpdate(float deltaTime);

}
