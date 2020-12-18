using System;

namespace UnityEngine {

    public interface IUpdater {

        /// <summary>
        /// Register an <see cref="Action"/> to be called every frame for the component with a given instance ID.
        /// </summary>
        /// <param name="instanceId">The instance ID of the component that will be updated every frame (returned by <see cref="Updatable.InstanceID"/> or <see cref="UnityEngine.Object.GetInstanceID"/>).</param>
        /// <param name="updateAction">The <see cref="Action"/> to be called every frame.</param>
        void RegisterUpdate(int instanceId, Action<float> updateAction);
        /// <summary>
        /// Unregister an <see cref="Action"/> from being called every frame for the component with the specified instance ID.
        /// </summary>
        /// <param name="instanceId">The instance ID of the component that no longer needs to be updated every frame (returned by <see cref="Updatable.InstanceID"/> or <see cref="UnityEngine.Object.GetInstanceID"/>).</param>
        void UnregisterUpdate(int instanceId);

        /// <summary>
        /// Register an <see cref="Action"/> to be called physics every frame for the component with the specified instance ID.
        /// </summary>
        /// <param name="instanceId">The instance ID of the component that will be updated every physics frame (returned by <see cref="Updatable.InstanceID"/> or <see cref="UnityEngine.Object.GetInstanceID"/>).</param>
        /// <param name="fixedUpdateAction">The <see cref="Action"/> to be called every physics frame.</param>
        void RegisterFixedUpdate(int instanceId, Action<float> fixedUpdateAction);
        /// <summary>
        /// Unregister an <see cref="Action"/> from being called every physics frame for the component with the specified instance ID.
        /// </summary>
        /// <param name="instanceId">The instance ID of the component that no longer needs to be updated every physics frame (returned by <see cref="Updatable.InstanceID"/> or <see cref="UnityEngine.Object.GetInstanceID"/>).</param>
        void UnregisterFixedUpdate(int instanceId);

        /// <summary>
        /// Register an <see cref="Action"/> to be called at the end of every frame for the component with the specified instance ID.
        /// </summary>
        /// <param name="instanceId">The instance ID of the component that will be updated at the end of every frame (returned by <see cref="Updatable.InstanceID"/> or <see cref="UnityEngine.Object.GetInstanceID"/>).</param>
        /// <param name="lateUpdateAction">The <see cref="Action"/> to be called at the end of every frame.</param>
        void RegisterLateUpdate(int instanceId, Action<float> lateUpdateAction);
        /// <summary>
        /// Unregister an <see cref="Action"/> from being called at the end of every frame for the component with the specified instance ID.
        /// </summary>
        /// <param name="instanceId">The instance ID of the component that no longer needs to be updated at the end of every frame (returned by <see cref="Updatable.InstanceID"/> or <see cref="UnityEngine.Object.GetInstanceID"/>).</param>
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

}
