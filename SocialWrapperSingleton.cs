﻿using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.SocialPlatforms;

namespace Danware.Unity {

    /// <summary>
    /// Parameters are (string userId, string errors)
    /// Only one of these parameters will ever have a value, the other will be null
    /// </summary>
    [Serializable]
    public class UserEvent : UnityEvent<string, string> { }

    public class SocialWrapperSingleton : MonoBehaviour {

        // HIDDEN FIELDS
        private static int s_refs = 0;

        // INSPECTOR FIELDS
        public UserEvent AuthenticationFailed = new UserEvent();
        public UserEvent AuthenticationSucceeded = new UserEvent();

        // EVENT HANDLERS
        private void Awake() {
            // Make sure this component is a singleton
            ++s_refs;
            Assert.IsTrue(s_refs == 1, $"There can be only one instance of {typeof(SocialWrapperSingleton)} in a scene!  You have {s_refs}!");
        }
        private void Start() {
            ILocalUser user = Social.localUser;
            user.Authenticate((success, errors) => {
                if (!success) {
                    Debug.Log("Authentication failed!");
                    AuthenticationFailed.Invoke(null, errors);
                }
                else {
                    Debug.Log("Authentication succeeded!");
                    AuthenticationSucceeded.Invoke(user.id, null);
                }
            });
        }

    }

}
