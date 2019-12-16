using System.Collections.Generic;
using UnityEngine.Logging;

namespace UnityEngine {

    public abstract class ConfigurationSource : ScriptableObject {

        public bool Required = true;

        protected ILogger Logger;

        public void Inject(ILoggerProvider loggerProvider) {
            Logger = loggerProvider.GetLogger(this);
        }

        public abstract IDictionary<string, object> LoadConfigs();

        protected virtual void Awake() {
            DependencyInjector.ResolveDependenciesOf(this);
        }

    }

}
