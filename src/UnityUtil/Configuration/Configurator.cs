using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.Logging;

namespace UnityEngine {

    public class Configurator : MonoBehaviour, IConfigurator {

        private ILogger _logger;
        private IDictionary<string, object> _values;

        [Tooltip("Sources must be provided in reverse order of importance (i.e., configs in source 0 will override configs in source 1, which will override configs in source 2, etc.)")]
        public ConfigurationSource[] ConfigurationSources;

        public void Inject(ILoggerProvider loggerProvider) => _logger = loggerProvider.GetLogger(this);

        public void Configure(object client) => Configure(new[] { client });
        public void Configure(IEnumerable<object> clients) {
            if (_values == null) {
                DependencyInjector.ResolveDependenciesOf(this);
                _logger.Log($"Loading {ConfigurationSources.Length} configuration sources...", context: this);
                _values = LoadConfigValues(ConfigurationSources);
            }

            foreach (object client in clients)
                configure(client);
        }

        public static IDictionary<string, object> LoadConfigValues(IEnumerable<ConfigurationSource> configurationSources) {
            int numLoaded = 0;
            IDictionary<string, object> allVals = new Dictionary<string, object>();
            foreach (ConfigurationSource src in configurationSources) {
                // If this configuration source is not supposed to be loaded in this context, then skip it
                if (
                    !((src.LoadContext & ConfigurationLoadContext.Editor) > 0 && Application.isEditor)
                    && !((src.LoadContext & ConfigurationLoadContext.DebugBuild) > 0 && Debug.isDebugBuild)
                    && !((src.LoadContext & ConfigurationLoadContext.ReleaseBuild) > 0 && !Debug.isDebugBuild)
                )
                    continue;

                IDictionary<string, object> vals = src.LoadConfigs();
                if (vals.Count > 0) {
                    ++numLoaded;
                    allVals = allVals
                        .Union(vals)
                        .GroupBy(pair => pair.Key, pair => pair.Value)
                        .ToDictionary(grp => grp.Key, grp => grp.First());
                }
            }

            return allVals;
        }
        public static string DefaultConfigKey(object client) => client.GetType().FullName;

        private void configure(object client) {
            Type clientType = client.GetType();
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            FieldInfo[] fields = clientType.GetFields(bindingFlags);
            string clientName = (client as MonoBehaviour)?.GetHierarchyNameWithType() ?? (client as Object)?.name ?? $"{clientType.FullName} instance";

            // Get the config key associated with this client
            // The key is either the value of the string field tagged as the config key, or the full name of the client's Type
            FieldInfo[] keyFields = fields.Where(f => f.GetCustomAttribute<ConfigKeyAttribute>(inherit: true) != null).ToArray();
            if (keyFields.Length > 1)
                throw new InvalidOperationException($"{clientName} could not be configured because it had multiple fields tagged with a {nameof(ConfigKeyAttribute)}.");
            string key;
            string defaultKey = DefaultConfigKey(client);
            if (keyFields.Length == 0)
                key = defaultKey;
            else if (keyFields[0].FieldType != typeof(string))
                throw new InvalidOperationException($"{clientName} could not be configured because the field tagged with a {nameof(ConfigKeyAttribute)} was not of String type.");
            else {
                key = (string)keyFields[0].GetValue(client);
                key = string.IsNullOrWhiteSpace(key) ? defaultKey : key.Trim();
            }

            // Set all fields on this client for which there is a config value
            for (int f = 0; f < fields.Length; ++f) {
                FieldInfo field = fields[f];
                string fieldKey = $"{key}.{field.Name}";
                object val = getValue(fieldKey, field.FieldType);
                if (val != null) {
                    field.SetValue(client, val);
                    _logger.Log($"Configured field '{field.Name}' of {clientName}.", context: this);
                }
            }

            // Set all properties on this client for which there is a config value
            PropertyInfo[] props = clientType.GetProperties(bindingFlags);
            for (int p = 0; p < props.Length; ++p) {
                PropertyInfo prop = props[p];
                string propKey = $"{key}.{prop.Name}";
                object val = getValue(propKey, prop.PropertyType);
                if (val != null) {
                    prop.SetValue(client, val);
                    _logger.Log($"Configured property '{prop.Name}' of {clientName}.", context: this);
                }
            }
        }
        private object getValue(string memberKey, Type memberType) {
            bool found = _values.TryGetValue(memberKey, out object val);
            if (found)
                return Convert.ChangeType(val, memberType);
            return null;
        }

    }

}
