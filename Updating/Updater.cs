using System;
using System.Collections.Generic;

namespace UnityUtil.Updating {

    public sealed class Updater : IUpdater {

        private IDictionary<int, int> _updateIndices = new Dictionary<int, int>();
        private IDictionary<int, int> _fixedIndices  = new Dictionary<int, int>();
        private IDictionary<int, int> _lateIndices   = new Dictionary<int, int>();

        private IList<Action> _updates = new List<Action>();
        private IList<Action> _fixed   = new List<Action>();
        private IList<Action> _late    = new List<Action>();

        private Stack<int> _updateNextIndex = new Stack<int>();
        private Stack<int> _fixedNextIndex  = new Stack<int>();
        private Stack<int> _lateNextIndex   = new Stack<int>();

        // ENFORCE SINGLETON PATTERN
        private static Updater s_instance = new Updater();
        private Updater() {
            _updateNextIndex.Push(0);
            _fixedNextIndex.Push(0);
            _lateNextIndex.Push(0);
        }

        /// <summary>
        /// The singleton <see cref="Updater"/> instance.
        /// </summary>
        public static Updater Instance => s_instance;

        // INTERFACE
        /// <inheritdoc/>
        public void RegisterUpdate(int instanceID, Action updateAction) {
            if (updateAction == null || _updateIndices.ContainsKey(instanceID))
                return;

            int index = _updateNextIndex.Pop();
            _updateIndices.Add(instanceID, index);
            if (index == _updates.Count - 1)
                _updates[index] = updateAction;
            else {
                _updates.Add(updateAction);
                _updateNextIndex.Push(index + 1);
            }
        }
        /// <inheritdoc/>
        public void UnregisterUpdate(int instanceID) {
            bool registered = _updateIndices.TryGetValue(instanceID, out int index);
            if (!registered)
                return;

            _updateIndices.Remove(instanceID);
            _updates[index] = null;
            _updateNextIndex.Push(index);
        }
        /// <inheritdoc/>
        public void RegisterFixedUpdate(int instanceID, Action fixedUpdateAction) {
            if (fixedUpdateAction == null || _fixedIndices.ContainsKey(instanceID))
                return;

            int index = _fixedNextIndex.Pop();
            _fixedIndices.Add(instanceID, index);
            if (index == _fixed.Count - 1)
                _fixed[index] = fixedUpdateAction;
            else {
                _fixed.Add(fixedUpdateAction);
                _fixedNextIndex.Push(index + 1);
            }
        }
        /// <inheritdoc/>
        public void UnregisterFixedUpdate(int instanceID) {
            bool registered = _fixedIndices.TryGetValue(instanceID, out int index);
            if (!registered)
                return;

            _fixedIndices.Remove(instanceID);
            _fixed[index] = null;
            _fixedNextIndex.Push(index);
        }
        /// <inheritdoc/>
        public void RegisterLateUpdate(int instanceID, Action lateUpdateAction) {
            if (lateUpdateAction == null || _lateIndices.ContainsKey(instanceID))
                return;

            int index = _lateNextIndex.Pop();
            _lateIndices.Add(instanceID, index);
            if (index == _late.Count - 1)
                _late[index] = lateUpdateAction;
            else {
                _late.Add(lateUpdateAction);
                _lateNextIndex.Push(index + 1);
            }
        }
        /// <inheritdoc/>
        public void UnregisterLateUpdate(int instanceID) {
            bool registered = _lateIndices.TryGetValue(instanceID, out int index);
            if (!registered)
                return;

            _lateIndices.Remove(instanceID);
            _late[index] = null;
            _lateNextIndex.Push(index);
        }

        /// <inheritdoc/>
        public void UpdateAll() {
            for (int u = 0; u < _updates.Count; ++u)
                _updates[u]?.Invoke();
        }
        /// <inheritdoc/>
        public void FixedUpdateAll() {
            for (int fu = 0; fu < _fixed.Count; ++fu)
                _fixed[fu]?.Invoke();
        }
        /// <inheritdoc/>
        public void LateUpdateAll() {
            for (int lu = 0; lu < _late.Count; ++lu)
                _late[lu]?.Invoke();
        }

    }

}
