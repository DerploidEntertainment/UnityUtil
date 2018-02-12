﻿using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace UnityUtil {

    public abstract class BetterBehaviour : MonoBehaviour {

        // HIDDEN FIELDS
        /// <summary>
        /// If <see langword="true"/>, then this <see cref="UnityUtil.BetterBehaviour"/> will have its Update actions registered/unregistered automatically when it is enabled/disabled.
        /// If <see langword="false"/>, then the Update actions must be registered/unregistered manually (best for when updates are only meant to be registered under specific/rare circumstances).
        /// <summary>
        protected bool RegisterUpdatesAutomatically = true;

        protected int InstanceID;

        protected Action BetterUpdate;
        protected Action BetterFixedUpdate;
        protected Action BetterLateUpdate;

        [Inject]
        public UpdaterSingleton Updater;

        protected void Awake() {
            Assert.IsNotNull(Updater, this.GetAssociationAssertion(nameof(this.Updater), singleton: true));

            InstanceID = GetInstanceID();

            BetterAwake();
        }
        protected void OnEnable() {
            if (RegisterUpdatesAutomatically) {
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
