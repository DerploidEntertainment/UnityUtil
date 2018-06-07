using System;
using System.Collections.Generic;

namespace UnityEngine {

    [DisallowMultipleComponent]
    public class Updater : MonoBehaviour, IUpdater {

        private struct UpdateAction {
            public int InstanceID;
            public Action Action;
        }

        private float _tSinceTrim = 0f;

        private IDictionary<int, int> _updateIndices = new Dictionary<int, int>();
        private IDictionary<int, int> _fixedIndices = new Dictionary<int, int>();
        private IDictionary<int, int> _lateIndices = new Dictionary<int, int>();

        private List<UpdateAction> _updates = new List<UpdateAction>();
        private List<UpdateAction> _fixed = new List<UpdateAction>();
        private List<UpdateAction> _late = new List<UpdateAction>();

        // INSPECTOR FIELDS
        [Tooltip("Every time this many seconds passes (in real time, not game time), the update action lists will have their capacities trimmed, if possible, using the List<T>.TrimExcess() method.")]
        public float TrimPeriod = 30f;

        // API INTERFACE
        /// <inheritdoc/>
        public void RegisterUpdate(int instanceID, Action action) {
            if (action == null)
                throw new ArgumentNullException(nameof(instanceID));
            if (_updateIndices.ContainsKey(instanceID))
                throw new ArgumentException($"{this.GetHierarchyNameWithType()} has already registered an object with {nameof(instanceID)} {instanceID} for updates!", nameof(instanceID));

            // Store the new Update action at that index
            // Store the index of this new Update action
            _updateIndices.Add(instanceID, _updates.Count);
            _updates.Add(new UpdateAction() {
                InstanceID = instanceID,
                Action = action
            });
        }
        /// <inheritdoc/>
        public void UnregisterUpdate(int instanceID) {
            if (!_updateIndices.TryGetValue(instanceID, out int index))
                throw new ArgumentException($"{this.GetHierarchyNameWithType()} cannot unregister the update Action for the object with {nameof(instanceID)} {instanceID} because no such Action was ever registered!", nameof(instanceID));

            UpdateAction lastAction = _updates[_updates.Count - 1];
            _updateIndices[lastAction.InstanceID] = index;
            _updates[index] = lastAction;
            _updates.RemoveAt(_updates.Count - 1);
            _updateIndices.Remove(instanceID);
        }

        /// <inheritdoc/>
        public void RegisterFixedUpdate(int instanceID, Action action) {
            if (action == null)
                throw new ArgumentNullException(nameof(instanceID));
            if (_fixedIndices.ContainsKey(instanceID))
                throw new ArgumentException($"{this.GetHierarchyNameWithType()} has already registered an object with {nameof(instanceID)} {instanceID} for FixedUpdate!", nameof(instanceID));

            // Store the new Update action at that index
            // Store the index of this new Update action
            _fixedIndices.Add(instanceID, _fixed.Count);
            _fixed.Add(new UpdateAction() {
                InstanceID = instanceID,
                Action = action
            });
        }
        /// <inheritdoc/>
        public void UnregisterFixedUpdate(int instanceID) {
            if (!_fixedIndices.TryGetValue(instanceID, out int index))
                throw new ArgumentException($"{this.GetHierarchyNameWithType()} cannot unregister the FixedUpdate action for the object with {nameof(instanceID)} {instanceID} because no such action was ever registered!", nameof(instanceID));

            UpdateAction lastAction = _fixed[_fixed.Count - 1];
            _fixedIndices[lastAction.InstanceID] = index;
            _fixed[index] = lastAction;
            _fixed.RemoveAt(_fixed.Count - 1);
            _fixedIndices.Remove(instanceID);
        }

        /// <inheritdoc/>
        public void RegisterLateUpdate(int instanceID, Action action) {
            if (action == null)
                throw new ArgumentNullException(nameof(instanceID));
            if (_lateIndices.ContainsKey(instanceID))
                throw new ArgumentException($"{this.GetHierarchyNameWithType()} has already registered an object with {nameof(instanceID)} {instanceID} for LateUpdate!", nameof(instanceID));

            // Store the new Update action at that index
            // Store the index of this new Update action
            _lateIndices.Add(instanceID, _late.Count);
            _late.Add(new UpdateAction() {
                InstanceID = instanceID,
                Action = action
            });
        }
        /// <inheritdoc/>
        public void UnregisterLateUpdate(int instanceID) {
            if (!_lateIndices.TryGetValue(instanceID, out int index))
                throw new ArgumentException($"{this.GetHierarchyNameWithType()} cannot unregister the LateUpdate action for the object with {nameof(instanceID)} {instanceID} because no such action was ever registered!", nameof(instanceID));

            UpdateAction lastAction = _late[_late.Count - 1];
            _lateIndices[lastAction.InstanceID] = index;
            _late[index] = lastAction;
            _late.RemoveAt(_late.Count - 1);
            _lateIndices.Remove(instanceID);
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
                _updates[u].Action?.Invoke();
        }
        private void FixedUpdate() {
            for (int fu = 0; fu < _fixed.Count; ++fu)
                _fixed[fu].Action?.Invoke();
        }
        private void LateUpdate() {
            for (int lu = 0; lu < _late.Count; ++lu)
                _late[lu].Action?.Invoke();
        }

    }

}
