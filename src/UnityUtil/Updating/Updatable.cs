using System;
using UnityEngine;
using UnityUtil.DependencyInjection;

namespace UnityUtil.Updating;

/// <summary>
/// Base class for components that need to register with the UnityUtil update system.
/// </summary>
public abstract class Updatable : MonoBehaviour
{
    private IUpdater? _updater;
    private IRuntimeIdProvider? _runtimeIdProvider;

    private bool _awoken;

    /// <summary>
    /// Runtime instance ID of this component, used internally to register actions with the update system.
    /// Inheritors may use this value so they don't have to add duplicate dependencies on an <see cref="IRuntimeIdProvider"/>.
    /// </summary>
    public int InstanceId { get; private set; }

    private Action<float>? _updateAction;
    private Action<float>? _fixedUpdateAction;
    private Action<float>? _lateUpdateAction;

    public void Inject(IUpdater updater, IRuntimeIdProvider runtimeIdProvider)
    {
        _updater = updater;
        _runtimeIdProvider = runtimeIdProvider;
    }

    protected virtual void Awake()
    {
        DependencyInjector.Instance.ResolveDependenciesOf(this);

        InstanceId = _runtimeIdProvider!.GetId();
    }

    /// <summary>
    /// Register <paramref name="action"/> to be called during <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.Update.html"><c>Update</c></a>.
    /// It will automatically be unsubscribed and resubscribed as the component is disabled (or destroyed) and re-enabled.
    /// </summary>
    /// <param name="action">The action to be called during Update</param>
    protected void RegisterUpdate(Action<float> action)
    {
        _updateAction = action;
        if (enabled && _awoken) // Awoken check so action doesn't get registered twice (once in Awake, once in OnEnable)
            _updater!.RegisterUpdate(InstanceId, _updateAction);
    }

    /// <summary>
    /// Register <paramref name="action"/> to be called during <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.FixedUpdate.html"><c>FixedUpdate</c></a>.
    /// It will automatically be unsubscribed and resubscribed as the component is disabled (or destroyed) and re-enabled.
    /// </summary>
    /// <param name="action">The action to be called during FixedUpdate</param>
    protected void RegisterFixedUpdate(Action<float> action)
    {
        _fixedUpdateAction = action;
        if (enabled && _awoken) // Awoken check so action doesn't get registered twice (once in Awake, once in OnEnable)
            _updater!.RegisterFixedUpdate(InstanceId, _fixedUpdateAction);
    }

    /// <summary>
    /// Register <paramref name="action"/> to be called during <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.LateUpdate.html"><c>LateUpdate</c></a>.
    /// It will automatically be unsubscribed and resubscribed as the component is disabled (or destroyed) and re-enabled.
    /// </summary>
    /// <param name="action">The action to be called during LateUpdate</param>
    protected void RegisterLateUpdate(Action<float> action)
    {
        _lateUpdateAction = action;
        if (enabled && _awoken) // Awoken check so action doesn't get registered twice (once in Awake, once in OnEnable)
            _updater!.RegisterLateUpdate(InstanceId, _lateUpdateAction);
    }

    /// <summary>
    /// Unregister this component's <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.Update.html"><c>Update</c></a> action.
    /// </summary>
    /// <exception cref="InvalidOperationException">No <c>Update</c> action was ever registered for this component.</exception>
    protected void UnregisterUpdate()
    {
        if (enabled)
            _updater!.UnregisterUpdate(InstanceId);
        _updateAction = null;
    }

    /// <summary>
    /// Unregister this component's <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.FixedUpdate.html"><c>FixedUpdate</c></a> action.
    /// </summary>
    /// <exception cref="InvalidOperationException">No <c>FixedUpdate</c> action was ever registered for this component.</exception>
    protected void UnregisterFixedUpdate()
    {
        if (enabled)
            _updater!.UnregisterFixedUpdate(InstanceId);
        _fixedUpdateAction = null;
    }

    /// <summary>
    /// Unregister this component's <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.LateUpdate.html"><c>LateUpdate</c></a> action.
    /// </summary>
    /// <exception cref="InvalidOperationException">No <c>LateUpdate</c> action was ever registered for this component.</exception>
    protected void UnregisterLateUpdate()
    {
        if (enabled)
            _updater!.UnregisterLateUpdate(InstanceId);
        _lateUpdateAction = null;
    }

    protected virtual void OnEnable()
    {
        _awoken = true;

        if (_updateAction is not null)
            _updater!.RegisterUpdate(InstanceId, _updateAction);
        if (_fixedUpdateAction is not null)
            _updater!.RegisterFixedUpdate(InstanceId, _fixedUpdateAction);
        if (_lateUpdateAction is not null)
            _updater!.RegisterLateUpdate(InstanceId, _lateUpdateAction);
    }

    protected virtual void OnDisable()
    {
        if (_updateAction is not null)
            _updater!.UnregisterUpdate(InstanceId);
        if (_fixedUpdateAction is not null)
            _updater!.UnregisterFixedUpdate(InstanceId);
        if (_lateUpdateAction is not null)
            _updater!.UnregisterLateUpdate(InstanceId);
    }

}
