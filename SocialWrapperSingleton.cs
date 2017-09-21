using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.Events;
using System;

namespace Danware.Unity {

    /// <summary>
    /// Parameters are (string userId, string errors)
    /// Only one of these parameters will ever have a value, the other will be null
    /// </summary>
    [Serializable]
    public class UserEvent : UnityEvent<string, string> { }

    public class SocialWrapperSingleton : MonoBehaviour {

        public UserEvent AuthenticationFailed = new UserEvent();
        public UserEvent AuthenticationSucceeded = new UserEvent();

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
