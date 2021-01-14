using System;
using UnityEngine.DependencyInjection;

namespace UnityEngine
{

    public abstract class Updatable : Configurable {

        // HIDDEN FIELDS
        protected IUpdater Updater;
        private IRuntimeIdProvider _runtimeIdProvider;

        public int InstanceID { get; private set; }

        /// <summary>
        /// If <see langword="true"/>, then this <see cref="UnityEngine.Updatable"/> will have its Update actions registered/unregistered automatically when it is enabled/disabled.
        /// If <see langword="false"/>, then the Update actions must be registered/unregistered manually (best for when updates are only meant to be registered under specific/rare circumstances).
        /// <summary>
        protected bool RegisterUpdatesAutomatically = false;

        protected Action<float> BetterUpdate;
        protected Action<float> BetterFixedUpdate;
        protected Action<float> BetterLateUpdate;

        public void Inject(IUpdater updater, IRuntimeIdProvider runtimeIdProvider)
        {
            Updater = updater;
            _runtimeIdProvider = runtimeIdProvider;
        }

        // EVENT HANDLERS
        protected override void Awake() {
            base.Awake();

            InstanceID = _runtimeIdProvider.GetId();
        }
        protected virtual void OnEnable() {
            if (RegisterUpdatesAutomatically) {
                if (BetterUpdate != null)
                    Updater.RegisterUpdate(InstanceID, BetterUpdate);
                if (BetterFixedUpdate != null)
                    Updater.RegisterFixedUpdate(InstanceID, BetterFixedUpdate);
                if (BetterLateUpdate != null)
                    Updater.RegisterLateUpdate(InstanceID, BetterLateUpdate);
            }
        }
        protected virtual void OnDisable() {
            if (RegisterUpdatesAutomatically) {
                if (BetterUpdate != null)
                    Updater.UnregisterUpdate(InstanceID);
                if (BetterFixedUpdate != null)
                    Updater.UnregisterFixedUpdate(InstanceID);
                if (BetterLateUpdate != null)
                    Updater.UnregisterLateUpdate(InstanceID);
            }
        }

    }

}
