using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine.DependencyInjection;
using UnityEngine.Logging;

namespace UnityEngine {

    public class Configurator : MonoBehaviour, IConfigurator
    {

        public class ConfigurationCounts
        {
            public ConfigurationCounts(
                IReadOnlyDictionary<(Type, string), int> cachedConfigCounts,
                IReadOnlyDictionary<(Type, string), int> uncachedConfigCounts
            ) {
                Cached = cachedConfigCounts;
                Uncached = uncachedConfigCounts;
            }
            public IReadOnlyDictionary<(Type, string), int> Cached { get; }
            public IReadOnlyDictionary<(Type, string), int> Uncached { get; }
        }

        public const bool DefaultRecordConfigurationsOnAwake = false;

        private ILogger _logger;

        private bool _recording = false;
        private Dictionary<string, object> _configs;
        private readonly Dictionary<(Type, string), Action<object>> _compiledConfigs = new Dictionary<(Type, string), Action<object>>();
        private readonly Dictionary<(Type, string), int> _uncachedConfigCounts = new Dictionary<(Type, string), int>();
        private readonly Dictionary<(Type, string), int> _cachedConfigCounts = new Dictionary<(Type, string), int>();

        [Tooltip(
            "If true, then " + nameof(RecordingConfigurations) + " will be enabled when this Component awakes. " +
            "You can use this to ensure all configurations are recorded, even early in the Scene lifecycle before you have a chance " +
            "to press " + nameof(ToggleConfigurationRecording) + "."
        )]
        public bool RecordConfigurationsOnAwake = DefaultRecordConfigurationsOnAwake;

        [Tooltip(
            "Sources must be provided in reverse order of importance (i.e., configs in source 0 will override configs in source 1, " +
            "which will override configs in source 2, etc.)"
        )]
        public ConfigurationSource[] ConfigurationSources;

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        private void Reset()
        {
            RecordingConfigurations = DefaultRecordConfigurationsOnAwake;
            ConfigurationSources = Array.Empty<ConfigurationSource>();
        }

        public void Inject(ILoggerProvider loggerProvider) => _logger = loggerProvider.GetLogger(this);

        /// <summary>
        /// Use these rules to cache commonly resolved configurations, speeding up Scene load times.
        /// We use this whitelist approach because caching ALL configurations could use up significant memory, and could actually
        /// worsen performance if many of the configurations were only resolved once.
        /// </summary>
        /// <remarks>
        /// Configuration will be cached for each type/configKey pair.
        /// That is, after a class instance with one of these types, and using the matching configKey,
        /// has had its fields configured via reflection, the reflected metadata and matching configs will be cached, so that
        /// subsequent instances with the same type/configKey will be configured faster.
        /// This is useful if you know you will have many configurable components in a scene with the same type/configKey.
        /// A blank configKey (default) is equivalent to the fully qualified type name. Leading/trailing whitespace is ignored.
        /// That is, if configKey is blank, then class instances of the matching type whose config keys
        /// are blank or equal to that type will have their configuration cached.
        /// </remarks>
        public List<(Type, string ConfigKey)> CachedConfigurations { get; } = new List<(Type, string ConfigKey)>();

        public static Dictionary<string, object> LoadConfigValues(IEnumerable<ConfigurationSource> configurationSources) =>
            configurationSources
                .Where(x =>
                    ((x.LoadContext & ConfigurationLoadContext.Editor) > 0 && Application.isEditor) ||
                    ((x.LoadContext & ConfigurationLoadContext.DebugBuild) > 0 && Debug.isDebugBuild) ||
                    ((x.LoadContext & ConfigurationLoadContext.ReleaseBuild) > 0 && !Debug.isDebugBuild)
                ).SelectMany(x => {
                    DependencyInjector.Instance.ResolveDependenciesOf(x);
                    return x.LoadConfigs();
                })
                .GroupBy(x => x.Key, x => x.Value)
                .ToDictionary(grp => grp.Key, grp => grp.First());
        public void Configure(IEnumerable<(object, string)> clients)
        {
            foreach ((object clienObj, string configKey) in clients)
                Configure(clienObj, configKey);
        }
        public void Configure(object client, string configKey)
        {
            if (string.IsNullOrWhiteSpace(configKey))
                throw new ArgumentException($"'{nameof(configKey)}' cannot be null or whitespace", nameof(configKey));

            // Resolve our dependencies, if that hasn't been done already
            if (_configs == null) {
                DependencyInjector.Instance.ResolveDependenciesOf(this);
                _logger.Log($"Loading {ConfigurationSources.Length} configuration sources...", context: this);
                _configs = LoadConfigValues(ConfigurationSources);
                _recording = RecordConfigurationsOnAwake;
            }

            // If there's a cached configuration for this type/configKey, then use that
            // The config key is either the provided key (trimmed), or the full name of the client's Type
            Type clientType = client.GetType();
            string key = configKey.Trim();
            if (_compiledConfigs.TryGetValue((clientType, key), out Action<object> compiledConfig)) {
                compiledConfig(client);
                if (_recording)
                    _cachedConfigCounts[(clientType, key)] = _cachedConfigCounts.TryGetValue((clientType, key), out int count) ? count + 1 : 1;
                return;
            }

            // Determine if this configuration should now be cached, by checking it against the provided whitelist
            // If so, we will build up the cached configuration while it is resolved via reflection
            bool cache = CachedConfigurations.Any(x =>
                clientType == x.Item1 &&
                (x.ConfigKey == key || (x.ConfigKey.Length == 0 && key.Contains(x.Item1.FullName))));
            IList<Expression> memberAssigns = cache ? new List<Expression>() : null;
            ParameterExpression clientObjParam = cache ? Expression.Parameter(typeof(object), nameof(client)) : null;
            Expression clientParam = cache ? Expression.Convert(clientObjParam, clientType) : null;

            // Set all fields on this client for which there is a config value
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            FieldInfo[] fields = clientType.GetFields(bindingFlags);
            for (int f = 0; f < fields.Length; ++f) {
                FieldInfo field = fields[f];
                string fieldKey = $"{key}.{field.Name}";
                object val = getValue(fieldKey, field.FieldType);
                if (val != null) {
                    if (cache)
                        memberAssigns.Add(Expression.Assign(Expression.MakeMemberAccess(clientParam, field), Expression.Constant(val)));
                    else
                        field.SetValue(client, val);
                    _logger.Log($"Field '{field.Name}' of {clientType.Name} clients with config key '{key}' will be configured with value '{val}'", context: this);
                }
            }

            // Set all properties on this client for which there is a config value
            PropertyInfo[] props = clientType.GetProperties(bindingFlags);
            for (int p = 0; p < props.Length; ++p) {
                PropertyInfo prop = props[p];
                string propKey = $"{key}.{prop.Name}";
                object val = getValue(propKey, prop.PropertyType);
                if (val != null) {
                    if (cache)
                        memberAssigns.Add(Expression.Assign(Expression.MakeMemberAccess(clientParam, prop), Expression.Constant(val)));
                    else
                        prop.SetValue(client, val);
                    _logger.Log($"Property '{prop.Name}' of {clientType.Name} clients with config key '{key}' will be configured with value '{val}'", context: this);
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
                if (_recording)
                    _cachedConfigCounts[(clientType, key)] = 1;
            }
            else {
                if (_recording)
                    _uncachedConfigCounts[(clientType, key)] = _uncachedConfigCounts.TryGetValue((clientType, key), out int count) ? count + 1 : 1;
            }
        }

        /// <summary>
        /// Toggle recording of how many times configurations are looked up at runtime, for optimization purposes.
        /// When recording is toggled off, a summary report will be logged to the Unity Console.
        /// </summary>
        [Button]
        public void ToggleConfigurationRecording()
        {
            ConfigurationCounts counts = null;
            GetConfigurationCounts(ref counts);
            RecordingConfigurations = !_recording;
            if (_recording)
                return;

            Debug.Log($@"
Uncached configuration counts:
(If any of these counts are greater than 1, consider caching configurations for that Type/ConfigKey on the {nameof(Configurator)} to improve performance)
{getCountLines(counts.Uncached)}

Cached configuration counts:
(If any of these counts equal 1, consider NOT caching configurations for that Type/ConfigKey on the {nameof(Configurator)}, to speed up its configurations and save memory)
{getCountLines(counts.Cached)}
            ");

            static string getCountLines(IEnumerable<KeyValuePair<(Type Type, string ConfigKey), int>> counts) => string.Join(
                Environment.NewLine,
                counts.OrderByDescending(x => x.Value).Select(x => $"    ({x.Key.Type.FullName}, {x.Key.ConfigKey}): {x.Value}")
            );
        }

        /// <summary>
        /// Toggle recording how many times configurations are looked up at runtime, for optimization purposes.
        /// </summary>
        public bool RecordingConfigurations {
            get => _recording;
            set {
                if (_recording == value)
                    return;

                _recording = value;
                if (!_recording) {
                    _cachedConfigCounts.Clear();
                    _uncachedConfigCounts.Clear();
                }

                _logger?.Log($"{(_recording ? "Started" : "Stopped")} recording configurations");
            }
        }

        /// <summary>
        /// Get the number of times that each configuration has been resolved at runtime.
        /// There's no reason for this code to be in release builds though, hence the <see cref="ConditionalAttribute"/>
        /// (which also requires that it return <see langword="void"/> and not have <see langword="out"/> parameters).
        /// </summary>
        /// <param name="counts">Upon return, will contain the number of times that configurations were resolved.</param>
        public void GetConfigurationCounts(ref ConfigurationCounts counts) => counts = new ConfigurationCounts(
            cachedConfigCounts: new Dictionary<(Type, string), int>(_cachedConfigCounts),
            uncachedConfigCounts: new Dictionary<(Type, string), int>(_uncachedConfigCounts)
        );

        private object getValue(string memberKey, Type memberType) =>
            _configs.TryGetValue(memberKey, out object val)
                ? Convert.ChangeType(val, memberType)
                : null;

    }

}
