using UnityEngine;

namespace Danware.Unity.Updating {

    public abstract class FixedUpdatable : MonoBehaviour, IFixedUpdatable {

        /// <inheritdoc/>
        public abstract void UpdatableFixedUpdate();

        private void OnEnable() {
            FixedUpdater.Instance.Register(this);
            fixedUpdatableOnEnable();
        }
        private void OnDisable() {
            FixedUpdater.Instance.Unregister(this);
            fixedUpdatableOnDisable();
        }

        /// <summary>
        /// This function is called when the object becomes enabled and active.
        /// Since the base <see cref="Updatable"/> already has an <see cref="this.OnEnable"/> method,
        /// logic that would otherwise have been placed in a MonoBehaviour's OnEnable() method should be placed here.
        /// </summary>
        protected virtual void fixedUpdatableOnEnable() { }
        /// <summary>
        /// This function is called when the object becomes disabled or inactive.
        /// Since the base <see cref="Updatable"/> already has an <see cref="this.OnDisable"/> method,
        /// logic that would otherwise have been placed in a MonoBehaviour's OnDisable() method should be placed here.
        /// </summary>
        protected virtual void fixedUpdatableOnDisable() { }

    }

}
