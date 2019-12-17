using System;
using UnityEngine.Events;
using UnityEngine.Logging;
using UnityEngine.SocialPlatforms;

namespace UnityEngine {

    /// <summary>
    /// Parameters are (string userId, string errors)
    /// Only one of these parameters will ever have a value, the other will be null
    /// </summary>
    [Serializable]
    public class UserEvent : UnityEvent<string, string> { }

    public class SocialWrapper : MonoBehaviour {

        private ILogger _logger;

        public UserEvent AuthenticationFailed = new UserEvent();
        public UserEvent AuthenticationSucceeded = new UserEvent();

        public void Inject(ILoggerProvider loggerProvider) => _logger = loggerProvider.GetLogger(this);

        private void Start() {
            DependencyInjector.ResolveDependenciesOf(this);

            ILocalUser user = Social.localUser;
            user.Authenticate((success, errors) => {
                if (!success) {
                    _logger.Log("Failed to authenticate!", context: this);
                    AuthenticationFailed.Invoke(null, errors);
                }
                else {
                    _logger.Log("Successfully authenticated!", context: this);
                    AuthenticationSucceeded.Invoke(user.id, null);
                }
            });
        }

    }

}
