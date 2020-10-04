using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine.DependencyInjection;
using UnityEngine.Logging;

namespace UnityEngine {

#pragma warning disable CA2235 // Mark all non-serializable fields

    [Serializable]
    public class CachedConfiguration
    {
        private const string MSG_TOOLTIP_SHARED = "Configuration will be cached for this " + nameof(TypeName) + "/" + nameof(ConfigKey) + " pair. " +
            "That is, after a class instance with this type, and using this " +nameof(ConfigKey)+", " +
            "has had its fields configured via reflection, the reflected metadata and matching configs will be cached, so that " +
            "subsequent instances with the same " + nameof(TypeName) + "/" + nameof(ConfigKey) + " will be configured faster. " +
            "This is useful if you know you will have many configurable components in a scene with the same " +
            nameof(TypeName) + "/" + nameof(ConfigKey) + ".";

        [Tooltip(nameof(TypeName) + " should be of the form '<namespace1>.<namespace2>.<typename>'.\n\n" + MSG_TOOLTIP_SHARED)]
        public string TypeName;
        [Tooltip(
            "A blank string (default) is equivalent to the fully qualified type name. Leading/trailing whitespace is ignored. " +
            "That is, if " + nameof(ConfigKey) + " is blank, then class instances of " + nameof(TypeName) + " whose config key " +
            "(the field tagged with a " + nameof(ConfigKeyAttribute) + ") is blank or equal to " + nameof(TypeName) + " " +
            "will have their configuration cached.\n\n" +
            MSG_TOOLTIP_SHARED
        )]
        public string ConfigKey;
    }

#pragma warning restore CA2235 // Mark all non-serializable fields

    public class Configurator : MonoBehaviour, IConfigurator {

        private ILogger _logger;
        private IDictionary<string, object> _values;

        [Tooltip("Sources must be provided in reverse order of importance (i.e., configs in source 0 will override configs in source 1, which will override configs in source 2, etc.)")]
        public ConfigurationSource[] ConfigurationSources;

        [Tooltip(
            "Use these rules to cache commonly resolved configurations, speeding up Scene load times. " +
            "We use this whitelist approach because caching ALL configurations could use up significant memory, and could actually " +
            "worsen performance if many of the configurations were only resolved once."
        )]
        [TableList(AlwaysExpanded = true, ShowIndexLabels = false)]
        public CachedConfiguration[] CachedConfigurations;

        public void Inject(ILoggerProvider loggerProvider) => _logger = loggerProvider.GetLogger(this);

        private readonly IDictionary<Type, FieldInfo> _configKeyFields = new Dictionary<Type, FieldInfo>();
        private readonly IDictionary<(Type, string), Action<object>> _compiledConfigs = new Dictionary<(Type, string), Action<object>>();

        public static IDictionary<string, object> LoadConfigValues(IEnumerable<ConfigurationSource> configurationSources)
        {
            int numLoaded = 0;
            IDictionary<string, object> allVals = new Dictionary<string, object>();
            foreach (ConfigurationSource src in configurationSources) {
                // If this configuration source is not supposed to be loaded in this context, then skip it
                if (!(
                    ((src.LoadContext & ConfigurationLoadContext.Editor) > 0 && Application.isEditor) ||
                    ((src.LoadContext & ConfigurationLoadContext.DebugBuild) > 0 && Debug.isDebugBuild) ||
                    ((src.LoadContext & ConfigurationLoadContext.ReleaseBuild) > 0 && !Debug.isDebugBuild)
                ))
                    continue;

                DependencyInjector.Instance.ResolveDependenciesOf(src);

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
        public void Configure(IEnumerable<object> clients)
        {
            foreach (object client in clients)
                Configure(client);
        }
        public void Configure(object client)
        {
            if (_values == null) {
                DependencyInjector.Instance.ResolveDependenciesOf(this);
                _logger.Log($"Loading {ConfigurationSources.Length} configuration sources...", context: this);
                _values = LoadConfigValues(ConfigurationSources);
            }

            Type clientType = client.GetType();
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            FieldInfo[] fields = null;
            string clientName = (client as MonoBehaviour)?.GetHierarchyNameWithType() ?? (client as Object)?.name ?? $"{clientType.FullName} instance";

            // Get the config key associated with this client
            // The key is either the value of the string field tagged as the config key, or the full name of the client's Type
            string key;
            string defaultKey = DefaultConfigKey(client);
            if (_configKeyFields.TryGetValue(clientType, out FieldInfo keyField))
                key = (string)keyField.GetValue(client);
            else {
                fields = clientType.GetFields(bindingFlags);
                FieldInfo[] keyFields = fields.Where(f => f.GetCustomAttribute<ConfigKeyAttribute>(inherit: true) != null).ToArray();
                if (keyFields.Length > 1)
                    throw new InvalidOperationException($"{clientName} could not be configured because it had multiple fields tagged with a {nameof(ConfigKeyAttribute)}.");
                key = (keyFields.Length == 0)
                    ? defaultKey
                    : keyFields[0].FieldType == typeof(string)
                        ? (string)keyFields[0].GetValue(client)
                        : throw new InvalidOperationException($"{clientName} could not be configured because the field tagged with a {nameof(ConfigKeyAttribute)} was not of String type.");
                _configKeyFields.Add(clientType, keyFields[0]);
            }
            key = string.IsNullOrWhiteSpace(key) ? defaultKey : key.Trim();


            // If there's a cached configuration for this type/configKey, then use that
            if (_compiledConfigs.TryGetValue((clientType, key), out Action<object> compiledConfig)) {
                compiledConfig(client);
                return;
            }

            // Determine if this configuration should now be cached, by checking it against the provided whitelist
            // If so, we will build up the cached configuration while it is resolved via reflection
            bool cache = CachedConfigurations.Any(x =>
                clientType.FullName.Contains(x.TypeName) &&
                (x.ConfigKey == key || (x.ConfigKey == "" && key.Contains(x.TypeName))));
            IList<Expression> memberAssigns = new List<Expression>();
            ParameterExpression clientObjParam = cache ? Expression.Parameter(typeof(object), nameof(client)) : null;
            Expression clientParam = cache ? Expression.Convert(clientObjParam, clientType) : null;

            // Set all fields on this client for which there is a config value
            fields ??= clientType.GetFields(bindingFlags);
            for (int f = 0; f < fields.Length; ++f) {
                FieldInfo field = fields[f];
                string fieldKey = $"{key}.{field.Name}";
                object val = getValue(fieldKey, field.FieldType);
                if (val != null) {
                    field.SetValue(client, val);
                    if (cache)
                        memberAssigns.Add(Expression.Assign(Expression.MakeMemberAccess(clientParam, field), Expression.Constant(val)));
                    _logger.Log($"Will configure field '{field.Name}' of {clientType.Name} clients with value '{val}'", context: this);
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
                    if (cache)
                        memberAssigns.Add(Expression.Assign(Expression.MakeMemberAccess(clientParam, prop), Expression.Constant(val)));
                    _logger.Log($"Will configure property '{prop.Name}' of {clientType.Name} clients with value '{val}'", context: this);
                }
            }

            // Cache the configuration now, if requested
            if (cache) {
                compiledConfig = (Action<object>)Expression.Lambda(
                    body: Expression.Block(memberAssigns),
                    name: $"{nameof(Configure)}_{clientType.Name}_Generated",
                    parameters: new[] { clientObjParam }
                ).Compile();
                _compiledConfigs.Add((clientType, key), compiledConfig);
            }
        }

        public static string DefaultConfigKey(object client) => client.GetType().FullName;

        private object getValue(string memberKey, Type memberType) =>
            _values.TryGetValue(memberKey, out object val)
                ? Convert.ChangeType(val, memberType)
                : null;

    }

}
