using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.Logging;

namespace UnityEngine {

    public class Configurator : MonoBehaviour, IConfigurator {

        private ILogger _logger;
        private IDictionary<string, object> _values = new Dictionary<string, object>();

        [Tooltip("Sources must be provided in reverse order of importance (i.e., configs in source 0 will override configs in source 1, which will override configs in source 2, etc.)")]
        public ConfigurationSource[] ConfigurationSources;

        public void Inject(ILoggerProvider loggerProvider) => _logger = loggerProvider.GetLogger(this);

        public void Configure(MonoBehaviour client) => Configure(new[] { client });
        public void Configure(IEnumerable<MonoBehaviour> clients) {
            if (_values == null) {
                DependencyInjector.ResolveDependenciesOf(this);
                loadConfigValues();
            }

            foreach (MonoBehaviour client in clients)
                configure(client);
        }

        private void loadConfigValues() {
            _logger.Log($"Loading {ConfigurationSources.Length} configuration sources...", context: this);

            int numLoaded = 0;
            for (int s = 0; s < ConfigurationSources.Length; ++s) {
                IDictionary<string, object> vals = ConfigurationSources[s].LoadConfigs();
                if (vals.Count > 0) {
                    ++numLoaded;
                    _values = _values
                        .Union(vals)
                        .GroupBy(pair => pair.Key, pair => pair.Value)
                        .ToDictionary(grp => grp.Key, grp => grp.First());
                }
            }
        }
        private void configure(MonoBehaviour client) {
            FieldInfo[] fields = client.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            // Get the config key associated with this client
            // The key is either the value of the string field tagged as the config key, or the name of the client's Type
            FieldInfo[] keyFields = fields.Where(f => f.GetCustomAttribute<ConfigKeyAttribute>(inherit: true) != null).ToArray();
            if (keyFields.Length > 1)
                throw new InvalidOperationException($"{client.name} could not be configured because it had multiple fields tagged with a {nameof(ConfigKeyAttribute)}.");
            string key;
            if (keyFields.Length == 0)
                key = client.GetType().Name;
            else if (keyFields[0].FieldType != typeof(string))
                throw new InvalidOperationException($"{client.name} could not be configured because the field tagged with a {nameof(ConfigKeyAttribute)} was not of String type.");
            else {
                key = (string)keyFields[0].GetValue(client);
                key = string.IsNullOrWhiteSpace(key) ? client.GetType().FullName : key.Trim();
            }

            // Set all fields on this client for which there is a config value
            for (int f = 0; f < fields.Length; ++f) {
                FieldInfo field = fields[f];
                string fieldKey = $"{key}.{field.Name}";
                object val = getValue(fieldKey, field.FieldType);
                if (val != null) {
                    field.SetValue(client, val);
                    _logger.Log($"Configured field '{field.Name}' of {client.GetHierarchyNameWithType()}.", context: this);
                }
            }
        }
        private object getValue(string fieldKey, Type fieldType) {
            bool found = _values.TryGetValue(fieldKey, out object val);
            if (found)
                return Convert.ChangeType(val, fieldType);
            return null;
        }

    }

}
