using System;

namespace UnityEngine;

public interface IUpdater
{

    /// <summary>
    /// Register an <see cref="Action"/> to be called every frame for the component with a given instance ID.
    /// </summary>
    /// <param name="instanceId">The instance ID of the component that will be updated every frame. Must be unique among all registered instances.</param>
    /// <param name="updateAction">The <see cref="Action"/> to be called every frame.</param>
    /// <exception cref="InvalidOperationException">An Update <see cref="Action"/> has already been registered for <paramref name="instanceId"/>.</exception>
    void RegisterUpdate(int instanceId, Action<float> updateAction);
    /// <summary>
    /// Unregister an <see cref="Action"/> from being called every frame for the component with the specified instance ID.
    /// </summary>
    /// <param name="instanceId">
    /// The instance ID of the component that no longer needs to be updated every frame.
    /// Must have been previously registered with <see cref="RegisterUpdate(int, Action{float})"/>.
    /// </param>
    /// <exception cref="InvalidOperationException">No Update <see cref="Action"/> was ever registered for <paramref name="instanceId"/>.</exception>
    void UnregisterUpdate(int instanceId);

    /// <summary>
    /// Register an <see cref="Action"/> to be called physics every frame for the component with the specified instance ID.
    /// </summary>
    /// <param name="instanceId">The instance ID of the component that will be updated every physics frame. Must be unique among all registered instances.</param>
    /// <param name="fixedUpdateAction">The <see cref="Action"/> to be called every physics frame.</param>
    /// <exception cref="InvalidOperationException">A FixedUpdate <see cref="Action"/> has already been registered for <paramref name="instanceId"/>.</exception>
    void RegisterFixedUpdate(int instanceId, Action<float> fixedUpdateAction);
    /// <summary>
    /// Unregister an <see cref="Action"/> from being called every physics frame for the component with the specified instance ID.
    /// </summary>
    /// <param name="instanceId">
    /// The instance ID of the component that no longer needs to be updated every physics frame.
    /// Must have been previously registered with <see cref="RegisterFixedUpdate(int, Action{float})"/>.
    /// </param>
    /// <exception cref="InvalidOperationException">No FixedUpdate <see cref="Action"/> was ever registered for <paramref name="instanceId"/>.</exception>
    void UnregisterFixedUpdate(int instanceId);

    /// <summary>
    /// Register an <see cref="Action"/> to be called at the end of every frame for the component with the specified instance ID.
    /// </summary>
    /// <param name="instanceId">The instance ID of the component that will be updated at the end of every frame. Must be unique among all registered instances.</param>
    /// <param name="lateUpdateAction">The <see cref="Action"/> to be called at the end of every frame.</param>
    /// <exception cref="InvalidOperationException">A LateUpdate <see cref="Action"/> has already been registered for <paramref name="instanceId"/>.</exception>
    void RegisterLateUpdate(int instanceId, Action<float> lateUpdateAction);
    /// <summary>
    /// Unregister an <see cref="Action"/> from being called at the end of every frame for the component with the specified instance ID.
    /// </summary>
    /// <param name="instanceId">
    /// The instance ID of the component that no longer needs to be updated at the end of every frame.
    /// Must have been previously registered with <see cref="RegisterLateUpdate(int, Action{float})"/>.
    /// </param>
    /// <exception cref="InvalidOperationException">No LateUpdate <see cref="Action"/> was ever registered for <paramref name="instanceId"/>.</exception>
    void UnregisterLateUpdate(int instanceId);

    /// <summary>
    /// Sets the capacity of all underlying collections to the actual number of elements in those collections.
    /// Depending on implementation, there may be separate collections for Update actions, FixedUpdate actions, etc.,
    /// the memory of which are all auto-allocated as more actions are added.
    /// Trimming the capacity of these collections is thus an important way of saving memory, e.g., after several updatable
    /// objects have been destroyed.
    /// </summary>
    void TrimStorage();

}
