using System;
using UnityEngine;
using UnityUtil.DependencyInjection;

namespace UnityUtil.Updating;

public abstract class Updatable : MonoBehaviour
{
    protected IUpdater? Updater;
    private IRuntimeIdProvider? _runtimeIdProvider;

    public int InstanceId { get; private set; }

    /// <summary>
    /// If <see langword="true"/>, then this <see cref="Updatable"/> will have its Update actions registered/unregistered automatically when it is enabled/disabled.
    /// If <see langword="false"/>, then the Update actions must be registered/unregistered manually (best for when updates are only meant to be registered under specific/rare circumstances).
    /// </summary>
    protected bool RegisterUpdatesAutomatically;

    protected Action<float>? BetterUpdate;
    protected Action<float>? BetterFixedUpdate;
    protected Action<float>? BetterLateUpdate;

    public void Inject(IUpdater updater, IRuntimeIdProvider runtimeIdProvider)
    {
        Updater = updater;
        _runtimeIdProvider = runtimeIdProvider;
    }

    protected virtual void Awake()
    {
        DependencyInjector.Instance.ResolveDependenciesOf(this);

        InstanceId = _runtimeIdProvider!.GetId();
    }
    protected virtual void OnEnable()
    {
        if (RegisterUpdatesAutomatically) {
            if (BetterUpdate is not null)
                Updater!.RegisterUpdate(InstanceId, BetterUpdate);
            if (BetterFixedUpdate is not null)
                Updater!.RegisterFixedUpdate(InstanceId, BetterFixedUpdate);
            if (BetterLateUpdate is not null)
                Updater!.RegisterLateUpdate(InstanceId, BetterLateUpdate);
        }
    }
    protected virtual void OnDisable()
    {
        if (RegisterUpdatesAutomatically) {
            if (BetterUpdate is not null)
                Updater!.UnregisterUpdate(InstanceId);
            if (BetterFixedUpdate is not null)
                Updater!.UnregisterFixedUpdate(InstanceId);
            if (BetterLateUpdate is not null)
                Updater!.UnregisterLateUpdate(InstanceId);
        }
    }

}
