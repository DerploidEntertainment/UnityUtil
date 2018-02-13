using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace UnityUtil {

    [DisallowMultipleComponent]
    public class Updater : MonoBehaviour, IUpdater {

        private IDictionary<int, int> _updateIndices = new Dictionary<int, int>();
        private IDictionary<int, int> _fixedIndices = new Dictionary<int, int>();
        private IDictionary<int, int> _lateIndices = new Dictionary<int, int>();

        private IList<Action> _updates = new List<Action>();
        private IList<Action> _fixed = new List<Action>();
        private IList<Action> _late = new List<Action>();

        private Stack<int> _updateNextIndex = new Stack<int>(new[] { 0 });     // These could be Queues too, doesn't really matter
        private Stack<int> _fixedNextIndex = new Stack<int>(new[] { 0 });
        private Stack<int> _lateNextIndex = new Stack<int>(new[] { 0 });

        // INTERFACE
        /// <inheritdoc/>
        public void RegisterUpdate(int instanceID, Action updateAction) {
            if (updateAction == null)
                throw new ArgumentNullException(nameof(instanceID));

            // Store the index of this new Update action, making sure not to Pop that index until it has been used
            int index = _updateNextIndex.Peek();
            _updateIndices.Add(instanceID, index);
            _updateNextIndex.Pop();

            // Store the new Update action at that index
            if (index == _updates.Count) {
                _updates.Add(updateAction);
                _updateNextIndex.Push(_updates.Count);
            }
            else
                _updates[index] = updateAction;
        }
        /// <inheritdoc/>
        public void UnregisterUpdate(int instanceID) {
            int index = _updateIndices[instanceID];
            _updateIndices.Remove(instanceID);
            _updates[index] = null;
            _updateNextIndex.Push(index);
        }

        /// <inheritdoc/>
        public void RegisterFixedUpdate(int instanceID, Action fixedUpdateAction) {
            if (fixedUpdateAction == null)
                throw new ArgumentNullException(nameof(instanceID));

            // Store the index of this new FixedUpdate action, making sure not to Pop that index until it has been used
            int index = _fixedNextIndex.Peek();
            _fixedIndices.Add(instanceID, index);
            _fixedNextIndex.Pop();

            // Store the index of this new FixedUpdate action, making sure not to Pop that index until it has been used
            if (index == _fixed.Count) {
                _fixed.Add(fixedUpdateAction);
                _fixedNextIndex.Push(_fixed.Count);
            }
            else
                _fixed[index] = fixedUpdateAction;
        }
        /// <inheritdoc/>
        public void UnregisterFixedUpdate(int instanceID) {
            int index = _fixedIndices[instanceID];
            _fixedIndices.Remove(instanceID);
            _fixed[index] = null;
            _fixedNextIndex.Push(index);
        }

        /// <inheritdoc/>
        public void RegisterLateUpdate(int instanceID, Action lateUpdateAction) {
            if (lateUpdateAction == null)
                throw new ArgumentNullException(nameof(instanceID));

            // Store the index of this new LateUpdate action, making sure not to Pop that index until it has been used
            int index = _lateNextIndex.Peek();
            _lateIndices.Add(instanceID, index);
            _lateNextIndex.Pop();

            // Store the index of this new LateUpdate action, making sure not to Pop that index until it has been used
            if (index == _late.Count) {
                _late.Add(lateUpdateAction);
                _lateNextIndex.Push(_late.Count);
            }
            else
                _late[index] = lateUpdateAction;
        }
        /// <inheritdoc/>
        public void UnregisterLateUpdate(int instanceID) {
            int index = _lateIndices[instanceID];
            _lateIndices.Remove(instanceID);
            _late[index] = null;
            _lateNextIndex.Push(index);
        }

        // EVENT HANDLERS
        private void Update() {
            for (int u = 0; u < _updates.Count; ++u)
                _updates[u]?.Invoke();
        }
        private void FixedUpdate() {
            for (int fu = 0; fu < _fixed.Count; ++fu)
                _fixed[fu]?.Invoke();
        }
        private void LateUpdate() {
            for (int lu = 0; lu < _late.Count; ++lu)
                _late[lu]?.Invoke();
        }

    }

}
