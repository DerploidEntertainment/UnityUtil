using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace UnityUtil.Updating;

[DisallowMultipleComponent]
public class Updater : MonoBehaviour, IUpdater
{
    private readonly MultiCollection<int, Action<float>> _updates = [];
    private readonly MultiCollection<int, Action<float>> _fixed = [];
    private readonly MultiCollection<int, Action<float>> _late = [];

    public void RegisterUpdate(int instanceId, Action<float> updateAction)
    {
        if (_updates.ContainsKey(instanceId))
            throw new InvalidOperationException($"{this.GetHierarchyNameWithType()} has already registered an object with {nameof(instanceId)} {instanceId} for updates!");

        _updates.Add(instanceId, updateAction);
    }
    public void UnregisterUpdate(int instanceId)
    {
        if (!_updates.Remove(instanceId))
            throw new InvalidOperationException($"{this.GetHierarchyNameWithType()} could not unregister the update Action for the object with {nameof(instanceId)} {instanceId} because no such Action was ever registered!");
    }

    public void RegisterFixedUpdate(int instanceId, Action<float> fixedUpdateAction)
    {
        if (_fixed.ContainsKey(instanceId))
            throw new InvalidOperationException($"{this.GetHierarchyNameWithType()} has already registered an object with {nameof(instanceId)} {instanceId} for FixedUpdate!");

        _fixed.Add(instanceId, fixedUpdateAction);
    }
    public void UnregisterFixedUpdate(int instanceId)
    {
        if (!_fixed.Remove(instanceId))
            throw new InvalidOperationException($"{this.GetHierarchyNameWithType()} could not unregister the FixedUpdate action for the object with {nameof(instanceId)} {instanceId} because no such action was ever registered!");
    }

    public void RegisterLateUpdate(int instanceId, Action<float> lateUpdateAction)
    {
        if (_late.ContainsKey(instanceId))
            throw new InvalidOperationException($"{this.GetHierarchyNameWithType()} has already registered an object with {nameof(instanceId)} {instanceId} for LateUpdate!");

        _late.Add(instanceId, lateUpdateAction);
    }
    public void UnregisterLateUpdate(int instanceId)
    {
        if (!_late.Remove(instanceId))
            throw new InvalidOperationException($"{this.GetHierarchyNameWithType()} could not unregister the LateUpdate action for the object with {nameof(instanceId)} {instanceId} because no such action was ever registered!");
    }

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

    public void TrimStorage()
    {
        _updates.TrimExcess();
        _late.TrimExcess();
        _fixed.TrimExcess();
    }
}
