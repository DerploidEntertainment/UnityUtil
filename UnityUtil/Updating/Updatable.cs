using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace UnityUtil {

    public abstract class Updatable : MonoBehaviour {

        // HIDDEN FIELDS
        [Inject]
        protected Updater Updater;

        /// <summary>
        /// If <see langword="true"/>, then this <see cref="UnityUtil.Updatable"/> will have its Update actions registered/unregistered automatically when it is enabled/disabled.
        /// If <see langword="false"/>, then the Update actions must be registered/unregistered manually (best for when updates are only meant to be registered under specific/rare circumstances).
        /// <summary>
        protected bool RegisterUpdatesAutomatically = false;

        protected Action BetterUpdate;
        protected Action BetterFixedUpdate;
        protected Action BetterLateUpdate;

        // EVENT HANDLERS
        protected void Awake() {
            DependencyInjector.Inject(this);

            Assert.IsNotNull(Updater, this.GetDependencyAssertion(nameof(this.Updater)));

            BetterAwake();
        }
        protected void OnEnable() {
            if (RegisterUpdatesAutomatically) {
                // Validate that Components with auto-update-registration have provided at least one Update Action for registration
                Assert.IsFalse(
                    BetterUpdate == null && BetterFixedUpdate == null && BetterLateUpdate == null,
                    this.GetHierarchyNameWithType() + " did not set any Update Actions for automatic registration!"
                );
                int id = GetInstanceID();
                if (BetterUpdate != null)
                    Updater.RegisterUpdate(id, BetterUpdate);
                if (BetterFixedUpdate != null)
                    Updater.RegisterFixedUpdate(id, BetterFixedUpdate);
                if (BetterLateUpdate != null)
                    Updater.RegisterLateUpdate(id, BetterLateUpdate);
            }

            BetterOnEnable();
        }
        protected void OnDisable() {
            if (RegisterUpdatesAutomatically) {
                int id = GetInstanceID();
                if (BetterUpdate != null)
                    Updater.UnregisterUpdate(id);
                if (BetterFixedUpdate != null)
                    Updater.UnregisterFixedUpdate(id);
                if (BetterLateUpdate != null)
                    Updater.UnregisterLateUpdate(id);
            }

            BetterOnDisable();
        }

        /// <summary>
        /// Actions to run during Awake().  Declaring Awake() on a subclass would hide the implementation in <see cref="UnityUtil.Updatable"/>, so this method was provided for subclasses to provide additional Awake() functionality.
        /// </summary>
        protected virtual void BetterAwake() { }
        /// <summary>
        /// Actions to run during OnEnable().  Declaring OnEnable() on a subclass would hide the implementation in <see cref="UnityUtil.Updatable"/>, so this method was provided for subclasses to provide additional OnEnable() functionality.
        /// </summary>
        protected virtual void BetterOnEnable() { }
        /// <summary>
        /// Actions to run during OnDisable().  Declaring OnDisable() on a subclass would hide the implementation in <see cref="UnityUtil.Updatable"/>, so this method was provided for subclasses to provide additional OnDisable() functionality.
        /// </summary>
        protected virtual void BetterOnDisable() { }

    }

}
