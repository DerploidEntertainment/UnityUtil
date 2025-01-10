using System;
using Microsoft.Extensions.Logging;
using UnityEngine;
using UnityUtil.DependencyInjection;

namespace UnityUtil.Updating;

/// <summary>
/// Base class for components that need to register with the UnityUtil update system.
/// </summary>
public abstract class Updatable : MonoBehaviour
{
    private RootLogger<Updatable>? _logger;
    private IRuntimeIdProvider? _runtimeIdProvider;
    protected IUpdater? Updater;

    /// <summary>
    /// Runtime instance ID of this component, used internally to register actions with the update system.
    /// Inheritors may use this value so they don't have to add duplicate dependencies on an <see cref="IRuntimeIdProvider"/>.
    /// </summary>
    public int InstanceId { get; private set; }

    private Action<float>? _updateAction;
    private Action<float>? _fixedUpdateAction;
    private Action<float>? _lateUpdateAction;

    public void Inject(ILoggerFactory loggerFactory, IRuntimeIdProvider runtimeIdProvider, IUpdater updater)
    {
        _logger = new(loggerFactory, context: this);
        Updater = updater;
        _runtimeIdProvider = runtimeIdProvider;
    }

    protected virtual void Awake()
    {
        DependencyInjector.Instance.ResolveDependenciesOf(this);

        InstanceId = _runtimeIdProvider!.GetNewId();
    }

    /// <summary>
    /// Register <paramref name="action"/> to be called during <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.Update.html"><c>Update</c></a>.
    /// It will automatically be unsubscribed and resubscribed as the component is disabled (or destroyed) and re-enabled.
    /// </summary>
    /// <param name="action">The action to be called during <c>Update</c></param>
    protected void AddUpdate(Action<float> action) =>
        add(Updater!.AddUpdate, ref _updateAction, action, _logger!.AddingSameUpdate, _logger!.AlreadyAddedOtherUpdate);

    /// <summary>
    /// Register <paramref name="action"/> to be called during <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.FixedUpdate.html"><c>FixedUpdate</c></a>.
    /// It will automatically be unsubscribed and resubscribed as the component is disabled (or destroyed) and re-enabled.
    /// </summary>
    /// <param name="action">The action to be called during <c>FixedUpdate</c></param>
    protected void AddFixedUpdate(Action<float> action) => 
        add(Updater!.AddFixedUpdate, ref _fixedUpdateAction, action, _logger!.AddingSameFixedUpdate, _logger!.AlreadyAddedOtherFixedUpdate);

    /// <summary>
    /// Register <paramref name="action"/> to be called during <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.LateUpdate.html"><c>LateUpdate</c></a>.
    /// It will automatically be unsubscribed and resubscribed as the component is disabled (or destroyed) and re-enabled.
    /// </summary>
    /// <param name="action">The action to be called during <c>LateUpdate</c></param>
    protected void AddLateUpdate(Action<float> action) => 
        add(Updater!.AddLateUpdate, ref _lateUpdateAction, action, _logger!.AddingSameLateUpdate, _logger!.AlreadyAddedOtherLateUpdate);

    /// <summary>
    /// Unregister this component's <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.Update.html"><c>Update</c></a> action.
    /// </summary>
    /// <exception cref="InvalidOperationException">No <c>Update</c> action was ever registered for this component.</exception>
    protected void RemoveUpdate()
    {
        _ = Updater!.RemoveUpdate(InstanceId, out _);
        _updateAction = null;
    }

    /// <summary>
    /// Unregister this component's <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.FixedUpdate.html"><c>FixedUpdate</c></a> action.
    /// </summary>
    /// <exception cref="InvalidOperationException">No <c>FixedUpdate</c> action was ever registered for this component.</exception>
    protected void RemoveFixedUpdate()
    {
        _ = Updater!.RemoveFixedUpdate(InstanceId, out _);
        _fixedUpdateAction = null;
    }

    /// <summary>
    /// Unregister this component's <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.LateUpdate.html"><c>LateUpdate</c></a> action.
    /// </summary>
    /// <exception cref="InvalidOperationException">No <c>LateUpdate</c> action was ever registered for this component.</exception>
    protected void RemoveLateUpdate()
    {
        _ = Updater!.RemoveLateUpdate(InstanceId, out _);
        _lateUpdateAction = null;
    }

    protected virtual void OnEnable()
    {
        if (_updateAction is not null)
            Updater!.AddUpdate(InstanceId, _updateAction);
        if (_fixedUpdateAction is not null)
            Updater!.AddFixedUpdate(InstanceId, _fixedUpdateAction);
        if (_lateUpdateAction is not null)
            Updater!.AddLateUpdate(InstanceId, _lateUpdateAction);
    }

    protected virtual void OnDisable()
    {
        if (_updateAction is not null)
            _ = Updater!.RemoveUpdate(InstanceId, out _);
        if (_fixedUpdateAction is not null)
            _ = Updater!.RemoveFixedUpdate(InstanceId, out _);
        if (_lateUpdateAction is not null)
            _ = Updater!.RemoveLateUpdate(InstanceId, out _);
    }

    private void add(
        Action<int, Action<float>> addAction,
        ref Action<float>? actionField,
        Action<float> action,
        Action<int> addSameAction,
        Func<int, Exception?, ArgumentException> alreadyAddedOtherFunc
    )
    {
        if (actionField == action)
        {
            addSameAction(InstanceId);
            return;
        }

        // If we ARE enabled, then action is already added, so the Add call below will already throw
        if (actionField is not null && !enabled)
            throw alreadyAddedOtherFunc(InstanceId, null);

        actionField = action;

        if (enabled)
            addAction(InstanceId, action);
    }
}
