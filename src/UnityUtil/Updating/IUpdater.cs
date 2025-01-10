using System;

namespace UnityUtil.Updating;

public interface IUpdater
{

    /// <summary>
    /// Add an <see cref="Action"/> to be called every frame for the component with <paramref name="instanceId"/>.
    /// Each instance ID may be associated with at most one of each of type of update action (<c>Update</c>, <c>FixedUpdate</c>, <c>LateUpdate</c>).
    /// </summary>
    /// <param name="instanceId">The instance ID of the component that will be updated every frame.</param>
    /// <param name="updateAction">The <see cref="Action"/> to be called every frame.</param>
    /// <exception cref="ArgumentException">An <c>Update</c> has already been associated with <paramref name="instanceId"/>.</exception>
    void AddUpdate(int instanceId, Action<float> updateAction);
    /// <summary>
    /// Remove the <see cref="Action"/> called every frame for the component with <paramref name="instanceId"/>
    /// and copy it to <paramref name="updateAction"/>.
    /// </summary>
    /// <param name="instanceId">The instance ID of the component that no longer needs to be updated every frame.</param>
    /// <param name="updateAction">The removed <c>Update</c> action.</param>
    /// <returns>
    /// <see langword="true"/> if the action is successfully found and removed; otherwise, <see langword="false"/>.
    /// </returns>
    bool RemoveUpdate(int instanceId, out Action<float> updateAction);
    /// <summary>
    /// Attempts to add an <see cref="Action"/> to be called every frame for the component with <paramref name="instanceId"/>.
    /// </summary>
    /// <param name="instanceId">The instance ID of the component that will be updated every frame.</param>
    /// <param name="updateAction">The <see cref="Action"/> to be called every frame.</param>
    /// <returns>
    /// <see langword="true"/> if the <see cref="Action"/> was successfully added; otherwise, <see langword="false"/>.
    /// </returns>
    bool TryAddUpdate(int instanceId, Action<float> updateAction);
    /// <summary>
    /// Attempts to get the <see cref="Action"/> called every frame for the component with <paramref name="instanceId"/>.
    /// </summary>
    /// <param name="instanceId">The instance ID of the component that is updated every frame.</param>
    /// <param name="updateAction">
    /// When this method returns, contains the <c>Update</c> action associated with <paramref name="instanceId"/>, 
    /// if found; otherwise, <see langword="null"/>. This parameter is passed uninitialized.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if an <c>Update</c> action is associated with <paramref name="instanceId"/>;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool TryGetUpdate(int instanceId, out Action<float> updateAction);
    /// <summary>
    /// Determines whether an <c>Update</c> action is associated with the component with <paramref name="instanceId"/>.
    /// </summary>
    /// <param name="instanceId">The instance ID of the component to check.</param>
    /// <returns>
    /// <see langword="true"/> if an <c>Update</c> action is associated with <paramref name="instanceId"/>; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool ContainsUpdate(int instanceId);

    /// <summary>
    /// Add an <see cref="Action"/> to be called every physics frame for the component with <paramref name="instanceId"/>.
    /// Each instance ID may be associated with at most one of each of type of update action (<c>Update</c>, <c>FixedUpdate</c>, <c>LateUpdate</c>).
    /// </summary>
    /// <param name="instanceId">The instance ID of the component that will be updated every frame.</param>
    /// <param name="fixedUpdateAction">The <see cref="Action"/> to be called every physics frame.</param>
    /// <exception cref="ArgumentException">A <c>FixedUpdate</c> has already been associated with <paramref name="instanceId"/>.</exception>
    void AddFixedUpdate(int instanceId, Action<float> fixedUpdateAction);
    /// <summary>
    /// Remove the <see cref="Action"/> called every physics frame for the component with <paramref name="instanceId"/>
    /// and copy it to <paramref name="fixedUpdateAction"/>.
    /// </summary>
    /// <param name="instanceId">The instance ID of the component that no longer needs to be updated every physics frame.</param>
    /// <param name="fixedUpdateAction">The removed <c>FixedUpdate</c> action.</param>
    /// <returns>
    /// <see langword="true"/> if the action is successfully found and removed; otherwise, <see langword="false"/>.
    /// </returns>
    bool RemoveFixedUpdate(int instanceId, out Action<float> fixedUpdateAction);
    /// <summary>
    /// Attempts to add an <see cref="Action"/> to be called every physics frame for the component with <paramref name="instanceId"/>.
    /// </summary>
    /// <param name="instanceId">The instance ID of the component that will be updated every physics frame.</param>
    /// <param name="fixedUpdateAction">The <see cref="Action"/> to be called every physics frame.</param>
    /// <returns>
    /// <see langword="true"/> if the <see cref="Action"/> was successfully added; otherwise, <see langword="false"/>.
    /// </returns>
    bool TryAddFixedUpdate(int instanceId, Action<float> fixedUpdateAction);
    /// <summary>
    /// Attempts to get the <see cref="Action"/> called every physics frame for the component with <paramref name="instanceId"/>.
    /// </summary>
    /// <param name="instanceId">The instance ID of the component that is updated every physics frame.</param>
    /// <param name="fixedUpdateAction">
    /// When this method returns, contains the <c>FixedUpdate</c> action associated with <paramref name="instanceId"/>, 
    /// if found; otherwise, <see langword="null"/>. This parameter is passed uninitialized.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a <c>FixedUpdate</c> action is associated with <paramref name="instanceId"/>;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool TryGetFixedUpdate(int instanceId, out Action<float> fixedUpdateAction);
    /// <summary>
    /// Determines whether a <c>FixedUpdate</c> action is associated with the component with <paramref name="instanceId"/>.
    /// </summary>
    /// <param name="instanceId">The instance ID of the component to check.</param>
    /// <returns>
    /// <see langword="true"/> if a <c>FixedUpdate</c> action is associated with <paramref name="instanceId"/>; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool ContainsFixedUpdate(int instanceId);

    /// <summary>
    /// Register an <see cref="Action"/> to be called at the end of every frame for the component with <paramref name="instanceId"/>.
    /// Each instance ID may be associated with at most one of each of type of update action (<c>Update</c>, <c>FixedUpdate</c>, <c>LateUpdate</c>).
    /// </summary>
    /// <param name="instanceId">The instance ID of the component that will be updated every frame.</param>
    /// <param name="lateUpdateAction">The <see cref="Action"/> to be called at the end of every frame.</param>
    /// <exception cref="ArgumentException">A <c>LateUpdate</c> has already been associated with <paramref name="instanceId"/>.</exception>
    void AddLateUpdate(int instanceId, Action<float> lateUpdateAction);
    /// <summary>
    /// Remove the <see cref="Action"/> called at the end of every frame for the component with <paramref name="instanceId"/>
    /// and copy it to <paramref name="lateUpdateAction"/>.
    /// </summary>
    /// <param name="instanceId">The instance ID of the component that no longer needs to be updated at the end of every frame.</param>
    /// <param name="lateUpdateAction">The removed <c>LateUpdate</c> action.</param>
    /// <returns>
    /// <see langword="true"/> if the action is successfully found and removed; otherwise, <see langword="false"/>.
    /// </returns>
    bool RemoveLateUpdate(int instanceId, out Action<float> lateUpdateAction);
    /// <summary>
    /// Attempts to add an <see cref="Action"/> to be called at the end of every frame for the component with <paramref name="instanceId"/>.
    /// </summary>
    /// <param name="instanceId">The instance ID of the component that will be updated at the end of every frame.</param>
    /// <param name="lateUpdateAction">The <see cref="Action"/> to be called at the end of every frame.</param>
    /// <returns>
    /// <see langword="true"/> if the <see cref="Action"/> was successfully added; otherwise, <see langword="false"/>.
    /// </returns>
    bool TryAddLateUpdate(int instanceId, Action<float> lateUpdateAction);
    /// <summary>
    /// Attempts to get the <see cref="Action"/> called at the end of every frame for the component with <paramref name="instanceId"/>.
    /// </summary>
    /// <param name="instanceId">The instance ID of the component that is updated at the end of every frame.</param>
    /// <param name="lateUpdateAction">
    /// When this method returns, contains the <c>LateUpdate</c> action associated with <paramref name="instanceId"/>, 
    /// if found; otherwise, <see langword="null"/>. This parameter is passed uninitialized.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a <c>LateUpdate</c> action is associated with <paramref name="instanceId"/>;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool TryGetLateUpdate(int instanceId, out Action<float> lateUpdateAction);
    /// <summary>
    /// Determines whether a <c>LateUpdate</c> action is associated with the component with <paramref name="instanceId"/>.
    /// </summary>
    /// <param name="instanceId">The instance ID of the component to check.</param>
    /// <returns>
    /// <see langword="true"/> if a <c>LateUpdate</c> action is associated with <paramref name="instanceId"/>; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool ContainsLateUpdate(int instanceId);

    /// <summary>
    /// Sets the capacity of all underlying collections to the actual number of elements in those collections.
    /// Depending on implementation, there may be separate collections for <c>Update</c>s, <c>FixedUpdate</c>s, etc.,
    /// the memory of which are all auto-allocated as more actions are added.
    /// Trimming the capacity of these collections is thus an important way of saving memory,
    /// e.g., after several updatable objects have been destroyed.
    /// Re-allocating memory is still costly though, so avoid calling this method super often, 
    /// ideally only during breaks in gameplay.
    /// </summary>
    void TrimExcess();

}
