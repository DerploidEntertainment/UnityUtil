using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using UnityEngine;
using UnityUtil.DependencyInjection;

namespace UnityUtil.Updating;

[DisallowMultipleComponent]
public class Updater : MonoBehaviour, IUpdater
{
    private RootLogger<Updater>? _logger;

    private readonly FastIndexableDictionary<int, Action<float>> _updates = new();
    private readonly FastIndexableDictionary<int, Action<float>> _fixed = new();
    private readonly FastIndexableDictionary<int, Action<float>> _late = new();

    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void Awake() => DependencyInjector.Instance.ResolveDependenciesOf(this);

    public void Inject(ILoggerFactory loggerFactory)
    {
        _logger = new(loggerFactory, context: this);
    }

    /// <inheritdoc/>
    public void AddUpdate(int instanceId, Action<float> updateAction)
    {
        try {
            _updates.Add(instanceId, updateAction);
        }
        catch (ArgumentException ex) {
            throw _logger!.AlreadyAddedOtherUpdate(instanceId, ex);
        }
    }
    /// <inheritdoc/>
    public bool RemoveUpdate(int instanceId, out Action<float> updateAction) => _updates.Remove(instanceId, out updateAction);
    /// <inheritdoc/>
    public bool TryAddUpdate(int instanceId, Action<float> updateAction) => _updates.TryAdd(instanceId, updateAction);
    /// <inheritdoc/>
    public bool TryGetUpdate(int instanceId, out Action<float> updateAction) => _updates.TryGetValue(instanceId, out updateAction);
    /// <inheritdoc/>
    public bool ContainsUpdate(int instanceId) => _updates.ContainsKey(instanceId);

    /// <inheritdoc/>
    public void AddFixedUpdate(int instanceId, Action<float> fixedUpdateAction)
    {
        try {
            _fixed.Add(instanceId, fixedUpdateAction);
        }
        catch (ArgumentException ex) {
            throw _logger!.AlreadyAddedOtherFixedUpdate(instanceId, ex);
        }
    }
    /// <inheritdoc/>
    public bool RemoveFixedUpdate(int instanceId, out Action<float> fixedUpdateAction) => _fixed.Remove(instanceId, out fixedUpdateAction);
    /// <inheritdoc/>
    public bool TryAddFixedUpdate(int instanceId, Action<float> fixedUpdateAction) => _fixed.TryAdd(instanceId, fixedUpdateAction);
    /// <inheritdoc/>
    public bool TryGetFixedUpdate(int instanceId, out Action<float> fixedUpdateAction) => _fixed.TryGetValue(instanceId, out fixedUpdateAction);
    /// <inheritdoc/>
    public bool ContainsFixedUpdate(int instanceId) => _fixed.ContainsKey(instanceId);

    /// <inheritdoc/>
    public void AddLateUpdate(int instanceId, Action<float> lateUpdateAction)
    {
        try {
            _late.Add(instanceId, lateUpdateAction);
        }
        catch (ArgumentException ex) {
            throw _logger!.AlreadyAddedOtherLateUpdate(instanceId, ex);
        }
    }
    /// <inheritdoc/>
    public bool RemoveLateUpdate(int instanceId, out Action<float> lateUpdateAction) => _late.Remove(instanceId, out lateUpdateAction);
    /// <inheritdoc/>
    public bool TryAddLateUpdate(int instanceId, Action<float> lateUpdateAction) => _late.TryAdd(instanceId, lateUpdateAction);
    /// <inheritdoc/>
    public bool TryGetLateUpdate(int instanceId, out Action<float> lateUpdateAction) => _late.TryGetValue(instanceId, out lateUpdateAction);
    /// <inheritdoc/>
    public bool ContainsLateUpdate(int instanceId) => _late.ContainsKey(instanceId);

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void Update()
    {
        for (int u = 0; u < _updates.Count; ++u)
            _updates[u](Time.deltaTime);
    }

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void FixedUpdate()
    {
        for (int fu = 0; fu < _fixed.Count; ++fu)
            _fixed[fu](Time.fixedDeltaTime);
    }

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void LateUpdate()
    {
        for (int lu = 0; lu < _late.Count; ++lu)
            _late[lu](Time.deltaTime);
    }

    /// <inheritdoc/>
    public void TrimExcess()
    {
        _updates.TrimExcess();
        _late.TrimExcess();
        _fixed.TrimExcess();
    }
}
