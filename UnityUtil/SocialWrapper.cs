using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.SocialPlatforms;

namespace UnityUtil {

    /// <summary>
    /// Parameters are (string userId, string errors)
    /// Only one of these parameters will ever have a value, the other will be null
    /// </summary>
    [Serializable]
    public class UserEvent : UnityEvent<string, string> { }

    public class SocialWrapper : MonoBehaviour {

        // INSPECTOR FIELDS
        public UserEvent AuthenticationFailed = new UserEvent();
        public UserEvent AuthenticationSucceeded = new UserEvent();

        // EVENT HANDLERS
        private void Start() {
            ILocalUser user = Social.localUser;
            user.Authenticate((success, errors) => {
                if (!success) {
                    this.Log(" failed to authenticate!");
                    AuthenticationFailed.Invoke(null, errors);
                }
                else {
                    this.Log(" successfully authenticated!");
                    AuthenticationSucceeded.Invoke(user.id, null);
                }
            });
        }

    }

}
