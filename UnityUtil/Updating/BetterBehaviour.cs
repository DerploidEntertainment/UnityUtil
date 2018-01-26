using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace UnityUtil {

    public abstract class BetterBehaviour : MonoBehaviour {

        protected int _instanceID;

        protected Action _betterUpdate;
        protected Action _betterFixedUpdate;
        protected Action _betterLateUpdate;

        [Inject]
        public UpdaterSingleton Updater;

        protected void Awake() {
            Assert.IsNotNull(Updater, this.GetAssociationAssertion(nameof(this.Updater), singleton: true));

            _instanceID = GetInstanceID();

            BetterAwake();
        }
        protected void OnEnable() {
            if  (_betterUpdate != null)
                Updater.RegisterUpdate(_instanceID, _betterUpdate);
            if (_betterFixedUpdate != null)
                Updater.RegisterFixedUpdate(_instanceID, _betterFixedUpdate);
            if (_betterLateUpdate != null)
                Updater.RegisterLateUpdate(_instanceID, _betterLateUpdate);

            BetterOnEnable();
        }
        protected void OnDisable() {
            if (_betterUpdate != null)
                Updater.UnregisterUpdate(_instanceID);
            if (_betterFixedUpdate != null)
                Updater.UnregisterFixedUpdate(_instanceID);
            if (_betterLateUpdate != null)
                Updater.UnregisterLateUpdate(_instanceID);

            BetterOnDisable();
        }

        protected virtual void BetterAwake() { }
        protected virtual void BetterOnEnable() { }
        protected virtual void BetterOnDisable() { }

    }

}
