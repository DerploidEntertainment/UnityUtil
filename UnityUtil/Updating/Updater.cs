using System;

namespace UnityEngine {

    [DisallowMultipleComponent]
    public class Updater : MonoBehaviour, IUpdater {

        private float _tSinceTrim = 0f;

        private readonly MultiCollection<int, Action> _updates = new MultiCollection<int, Action>();
        private readonly MultiCollection<int, Action> _fixed = new MultiCollection<int, Action>();
        private readonly MultiCollection<int, Action> _late = new MultiCollection<int, Action>();

        // INSPECTOR FIELDS
        [Tooltip("Every time this many seconds passes (in real time, not game time), the update action lists will have their capacities trimmed, if possible, using the List<T>.TrimExcess() method.")]
        public float TrimPeriod = 30f;

        // API INTERFACE
        /// <inheritdoc/>
        public void RegisterUpdate(int instanceID, Action action) {
            if (action == null)
                throw new ArgumentNullException(nameof(instanceID));
            if (_updates.ContainsKey(instanceID))
                throw new ArgumentException($"{this.GetHierarchyNameWithType()} has already registered an object with {nameof(instanceID)} {instanceID} for updates!", nameof(instanceID));

            _updates.Add(instanceID, action);
        }
        /// <inheritdoc/>
        public void UnregisterUpdate(int instanceID) {
            if (!_updates.Remove(instanceID))
                throw new ArgumentException($"{this.GetHierarchyNameWithType()} could not unregister the update Action for the object with {nameof(instanceID)} {instanceID} because no such Action was ever registered!", nameof(instanceID));
        }

        /// <inheritdoc/>
        public void RegisterFixedUpdate(int instanceID, Action action) {
            if (action == null)
                throw new ArgumentNullException(nameof(instanceID));
            if (_fixed.ContainsKey(instanceID))
                throw new ArgumentException($"{this.GetHierarchyNameWithType()} has already registered an object with {nameof(instanceID)} {instanceID} for FixedUpdate!", nameof(instanceID));

            _fixed.Add(instanceID, action);
        }
        /// <inheritdoc/>
        public void UnregisterFixedUpdate(int instanceID) {
            if (!_fixed.Remove(instanceID))
                throw new ArgumentException($"{this.GetHierarchyNameWithType()} could not unregister the FixedUpdate action for the object with {nameof(instanceID)} {instanceID} because no such action was ever registered!", nameof(instanceID));
        }

        /// <inheritdoc/>
        public void RegisterLateUpdate(int instanceID, Action action) {
            if (action == null)
                throw new ArgumentNullException(nameof(instanceID));
            if (_late.ContainsKey(instanceID))
                throw new ArgumentException($"{this.GetHierarchyNameWithType()} has already registered an object with {nameof(instanceID)} {instanceID} for LateUpdate!", nameof(instanceID));

            _late.Add(instanceID, action);
        }
        /// <inheritdoc/>
        public void UnregisterLateUpdate(int instanceID) {
            if (!_late.Remove(instanceID))
                throw new ArgumentException($"{this.GetHierarchyNameWithType()} could not unregister the LateUpdate action for the object with {nameof(instanceID)} {instanceID} because no such action was ever registered!", nameof(instanceID));
        }

        // EVENT HANDLERS
        private void Update() {
            _tSinceTrim += Time.unscaledDeltaTime;
            if (_tSinceTrim >= TrimPeriod) {
                _tSinceTrim -= TrimPeriod;
                _updates.TrimExcess();
                _late.TrimExcess();
                _fixed.TrimExcess();
            }

            for (int u = 0; u < _updates.Count; ++u)
                _updates[u]();
        }
        private void FixedUpdate() {
            for (int fu = 0; fu < _fixed.Count; ++fu)
                _fixed[fu]();
        }
        private void LateUpdate() {
            for (int lu = 0; lu < _late.Count; ++lu)
                _late[lu]();
        }

    }

}
