using System;
using System.Diagnostics.CodeAnalysis;

namespace UnityEngine {

    [DisallowMultipleComponent]
    public class Updater : MonoBehaviour, IUpdater
    {
        private readonly MultiCollection<int, Action<float>> _updates = new();
        private readonly MultiCollection<int, Action<float>> _fixed = new();
        private readonly MultiCollection<int, Action<float>> _late = new();

        public void RegisterUpdate(int instanceID, Action<float> action) {
            if (action == null)
                throw new ArgumentNullException(nameof(instanceID));
            if (_updates.ContainsKey(instanceID))
                throw new InvalidOperationException($"{this.GetHierarchyNameWithType()} has already registered an object with {nameof(instanceID)} {instanceID} for updates!");

            _updates.Add(instanceID, action);
        }
        public void UnregisterUpdate(int instanceID) {
            if (!_updates.Remove(instanceID))
                throw new InvalidOperationException($"{this.GetHierarchyNameWithType()} could not unregister the update Action for the object with {nameof(instanceID)} {instanceID} because no such Action was ever registered!");
        }

        public void RegisterFixedUpdate(int instanceID, Action<float> action) {
            if (action == null)
                throw new ArgumentNullException(nameof(instanceID));
            if (_fixed.ContainsKey(instanceID))
                throw new InvalidOperationException($"{this.GetHierarchyNameWithType()} has already registered an object with {nameof(instanceID)} {instanceID} for FixedUpdate!");

            _fixed.Add(instanceID, action);
        }
        public void UnregisterFixedUpdate(int instanceID) {
            if (!_fixed.Remove(instanceID))
                throw new InvalidOperationException($"{this.GetHierarchyNameWithType()} could not unregister the FixedUpdate action for the object with {nameof(instanceID)} {instanceID} because no such action was ever registered!");
        }

        public void RegisterLateUpdate(int instanceID, Action<float> action) {
            if (action == null)
                throw new ArgumentNullException(nameof(instanceID));
            if (_late.ContainsKey(instanceID))
                throw new InvalidOperationException($"{this.GetHierarchyNameWithType()} has already registered an object with {nameof(instanceID)} {instanceID} for LateUpdate!");

            _late.Add(instanceID, action);
        }
        public void UnregisterLateUpdate(int instanceID) {
            if (!_late.Remove(instanceID))
                throw new InvalidOperationException($"{this.GetHierarchyNameWithType()} could not unregister the LateUpdate action for the object with {nameof(instanceID)} {instanceID} because no such action was ever registered!");
        }

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
        private void Update() {
            for (int u = 0; u < _updates.Count; ++u)
                _updates[u](Time.deltaTime);
        }

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
        private void FixedUpdate() {
            for (int fu = 0; fu < _fixed.Count; ++fu)
                _fixed[fu](Time.fixedDeltaTime);
        }

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
        private void LateUpdate() {
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

}
