using System;
using UnityEngine;
using UnityEngine.Assertions;
using U = UnityEngine;

namespace UnityUtil {

    public abstract class BetterBehaviour : MonoBehaviour {

        // HIDDEN FIELDS

        /// <summary>
        /// If <see langword="true"/>, then this <see cref="UnityUtil.BetterBehaviour"/> will have its Update actions registered/unregistered automatically when it is enabled/disabled.
        /// If <see langword="false"/>, then the Update actions must be registered/unregistered manually (best for when updates are only meant to be registered under specific/rare circumstances).
        /// <summary>
        protected bool RegisterUpdatesAutomatically = false;

        protected int InstanceID;

        protected Action BetterUpdate;
        protected Action BetterFixedUpdate;
        protected Action BetterLateUpdate;

        private static DependencyInjector s_injector;
        protected DependencyInjector DependencyInjector {
            get {
                s_injector = s_injector ?? GameObject.FindWithTag(DependencyInjector.Tag)?.GetComponent<DependencyInjector>();
                return s_injector;
            }
        }

        [Header(nameof(UnityUtil.BetterBehaviour) + " Fields")]
        [Inject]
        public Updater Updater;
        [Tooltip("If true, then this GameObject will not be destroyed automatically when this Scene is unloaded.  If false, then it will be destroyed as normal when the Scene unloads.  Changing this value in the Inspector while in Play mode will have no affect.")]
        public bool DontDestroyOnLoad;

        // EVENT HANDLERS
        protected void Awake() {
            DependencyInjector.Inject(this);

            Assert.IsNotNull(Updater, this.GetDependencyAssertion(nameof(this.Updater)));

            InstanceID = GetInstanceID();

            if (DontDestroyOnLoad)
                U.Object.DontDestroyOnLoad(this);

            BetterAwake();
        }
        protected void OnEnable() {
            if (RegisterUpdatesAutomatically) {
                // Validate that Components with auto-update-registration have provided at least one Update Action for registration
                Assert.IsFalse(
                    BetterUpdate == null && BetterFixedUpdate == null && BetterLateUpdate == null,
                    this.GetHierarchyNameWithType() + " did not set any Update Actions for automatic registration!"
                );
                if (BetterUpdate != null)
                    Updater.RegisterUpdate(InstanceID, BetterUpdate);
                if (BetterFixedUpdate != null)
                    Updater.RegisterFixedUpdate(InstanceID, BetterFixedUpdate);
                if (BetterLateUpdate != null)
                    Updater.RegisterLateUpdate(InstanceID, BetterLateUpdate);
            }

            BetterOnEnable();
        }
        protected void OnDisable() {
            if (RegisterUpdatesAutomatically) {
                if (BetterUpdate != null)
                    Updater.UnregisterUpdate(InstanceID);
                if (BetterFixedUpdate != null)
                    Updater.UnregisterFixedUpdate(InstanceID);
                if (BetterLateUpdate != null)
                    Updater.UnregisterLateUpdate(InstanceID);
            }

            BetterOnDisable();
        }

        /// <summary>
        /// Actions to run during Awake().  Declaring Awake() on a subclass would hide the implementation in <see cref="UnityUtil.BetterBehaviour"/>, so this method was provided for subclasses to provide additional Awake() functionality.
        /// </summary>
        protected virtual void BetterAwake() { }
        /// <summary>
        /// Actions to run during OnEnable().  Declaring OnEnable() on a subclass would hide the implementation in <see cref="UnityUtil.BetterBehaviour"/>, so this method was provided for subclasses to provide additional OnEnable() functionality.
        /// </summary>
        protected virtual void BetterOnEnable() { }
        /// <summary>
        /// Actions to run during OnDisable().  Declaring OnDisable() on a subclass would hide the implementation in <see cref="UnityUtil.BetterBehaviour"/>, so this method was provided for subclasses to provide additional OnDisable() functionality.
        /// </summary>
        protected virtual void BetterOnDisable() { }

    }

}
