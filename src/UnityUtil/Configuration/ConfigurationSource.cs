using System.Collections.Generic;
using UnityEngine.Logging;

namespace UnityEngine {

    public abstract class ConfigurationSource : ScriptableObject {

        public bool Required = true;

        protected ILogger Logger;

        public void Inject(ILoggerProvider loggerProvider) {
            Logger = loggerProvider.GetLogger(this);
        }

        public virtual IDictionary<string, object> LoadConfigs() {
            if (Logger == null)
                DependencyInjector.ResolveDependenciesOf(this);

            return new Dictionary<string, object>();
        }

    }

}
