using System;
using Microsoft.Extensions.Logging;
using UnityEngine;
using UnityUtil.DependencyInjection;
using static Microsoft.Extensions.Logging.LogLevel;
using MEL = Microsoft.Extensions.Logging;

namespace UnityUtil.Updating;

/// <summary>
/// Base class for components that need to register with the UnityUtil update system.
/// </summary>
public abstract class Updatable : MonoBehaviour
{
    private ILogger<Updatable>? _logger;
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

    private bool _onEnableCalled;

    public void Inject(ILoggerFactory loggerFactory, IRuntimeIdProvider runtimeIdProvider, IUpdater updater)
    {
        _logger = loggerFactory.CreateLogger(this);
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
        add("Update", Updater!.AddUpdate, ref _updateAction, action);

    /// <summary>
    /// Register <paramref name="action"/> to be called during <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.FixedUpdate.html"><c>FixedUpdate</c></a>.
    /// It will automatically be unsubscribed and resubscribed as the component is disabled (or destroyed) and re-enabled.
    /// </summary>
    /// <param name="action">The action to be called during <c>FixedUpdate</c></param>
    protected void AddFixedUpdate(Action<float> action) =>
        add("FixedUpdate", Updater!.AddFixedUpdate, ref _fixedUpdateAction, action);

    /// <summary>
    /// Register <paramref name="action"/> to be called during <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.LateUpdate.html"><c>LateUpdate</c></a>.
    /// It will automatically be unsubscribed and resubscribed as the component is disabled (or destroyed) and re-enabled.
    /// </summary>
    /// <param name="action">The action to be called during <c>LateUpdate</c></param>
    protected void AddLateUpdate(Action<float> action) =>
        add("LateUpdate", Updater!.AddLateUpdate, ref _lateUpdateAction, action);

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

        _onEnableCalled = true;
    }

    protected virtual void OnDisable()
    {
        if (_updateAction is not null)
            _ = Updater!.RemoveUpdate(InstanceId, out _);
        if (_fixedUpdateAction is not null)
            _ = Updater!.RemoveFixedUpdate(InstanceId, out _);
        if (_lateUpdateAction is not null)
            _ = Updater!.RemoveLateUpdate(InstanceId, out _);

        _onEnableCalled = false;
    }

    private void add(
        string updateType,
        Action<int, Action<float>> addAction,
        ref Action<float>? actionField,
        Action<float> action
    )
    {
        if (actionField == action) {
            log_AddingSameUpdate(updateType, InstanceId);
            return;
        }

        actionField = action;

        if (_onEnableCalled)
            addAction(InstanceId, action);
    }

    #region LoggerMessages

    private static readonly Action<MEL.ILogger, string, int, Exception?> ADDING_SAME_UPDATE_ACTION =
        LoggerMessage.Define<string, int>(Warning,
            new EventId(id: 0, nameof(log_AddingSameUpdate)),
            "Re-adding the same {UpdateType} action for instance ID '{InstanceId}'"
        );
    private void log_AddingSameUpdate(string updateType, int instanceId) => ADDING_SAME_UPDATE_ACTION(_logger!, updateType, instanceId, null);

    #endregion
}
