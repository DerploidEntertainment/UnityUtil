using System.Collections.Generic;

namespace UnityEngine {

    public abstract class ConfigurationSource : ScriptableObject {

        public bool Required = true;

        public abstract IDictionary<string, object> LoadConfigs();

    }

}
