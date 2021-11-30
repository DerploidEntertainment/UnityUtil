using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine.DependencyInjection;
using UnityEngine.Logging;

namespace UnityEngine
{
    public class ConfigurationCounts
    {
        public ConfigurationCounts(
            IReadOnlyDictionary<(Type, string), int> cachedConfigCounts,
            IReadOnlyDictionary<(Type, string), int> uncachedConfigCounts
        )
        {
            Cached = cachedConfigCounts;
            Uncached = uncachedConfigCounts;
        }
        public IReadOnlyDictionary<(Type, string), int> Cached { get; }
        public IReadOnlyDictionary<(Type, string), int> Uncached { get; }
    }

    public class Configurator : IConfigurator
    {
        private const BindingFlags BINDING_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private readonly ILogger _logger;

        private bool _loading = false;
        private bool _recording = false;
        private readonly HashSet<ConfigurationSource> _loadedCfgSources = new();
        private readonly Dictionary<string, object> _configs = new();
        private readonly HashSet<(Type, string)> _cachedConfigurations = new();
        private readonly Dictionary<(Type, string), Action<object>> _compiledConfigs = new();
        private readonly Dictionary<(Type, string), int> _uncachedConfigCounts = new();
        private readonly Dictionary<(Type, string), int> _cachedConfigCounts = new();

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
        public IReadOnlyCollection<(Type, string ConfigKey)> CachedConfigurations => _cachedConfigurations;

        public Configurator(ILoggerProvider loggerProvider) => _logger = loggerProvider.GetLogger(this);

        #region Loading configuration sources

        public event EventHandler<IReadOnlyDictionary<string, object>>? LoadingComplete;

        public void LoadConfigs(IEnumerable<ConfigurationSource> configurationSources)
        {
            List<ConfigurationSource> cfgSourcesToLoad = getConfigurationSourcesToLoad(configurationSources, async: false);

            for (int x = 0; x < cfgSourcesToLoad.Count; ++x) {
                cfgSourcesToLoad[x].Load();
                _loadedCfgSources.Add(cfgSourcesToLoad[x]);
            }

            finishLoading(cfgSourcesToLoad);
        }

        public IEnumerator LoadConfigsAsync(IEnumerable<ConfigurationSource> configurationSources)
        {
            if (_loading)
                yield break;

            _loading = true;

            // Load all configuration sources in parallel
            List<ConfigurationSource> cfgSourcesToLoad = getConfigurationSourcesToLoad(configurationSources, async: true);
            var loadingCfgSources = cfgSourcesToLoad.Select(x => (configurationSource: x, enumerator: x.LoadAsync())).ToList();

            while (loadingCfgSources.Count > 0) {
                for (int x = loadingCfgSources.Count - 1; x >= 0; --x) {
                    bool srcStillLoading = loadingCfgSources[x].enumerator.MoveNext();
                    if (!srcStillLoading) {
                        _loadedCfgSources.Add(loadingCfgSources[x].configurationSource);
                        loadingCfgSources.RemoveAt(x);
                    }
                }
                yield return null;
            }

            finishLoading(cfgSourcesToLoad);
        }

        private List<ConfigurationSource> getConfigurationSourcesToLoad(IEnumerable<ConfigurationSource> configurationSources, bool async)
        {
            ConfigurationLoadContext currLoadContext = getCurrentConfigurationLoadContext();

            var cfgSourcesToLoad = new List<ConfigurationSource>();
            foreach (ConfigurationSource cfgSrc in configurationSources) {
                if (
                    (async && cfgSrc.LoadBehavior == ConfigurationSourceLoadBehavior.SyncOnly) ||
                    (!async && cfgSrc.LoadBehavior == ConfigurationSourceLoadBehavior.AsyncOnly)
                ) {
                    _logger.Log($"{nameof(ConfigurationSource)} '{cfgSrc.name}' will not be loaded because it does not support {(async ? "a" : "")}synchronous loading");
                    continue;
                }

                if ((cfgSrc.LoadContext & currLoadContext) == 0) {
                    _logger.Log($"{nameof(ConfigurationSource)} '{cfgSrc.name}' will not be loaded because it was not set to load in the context '{currLoadContext}'");
                    continue;
                }

                if (_loadedCfgSources.Contains(cfgSrc))
                    continue;

                DependencyInjector.Instance.ResolveDependenciesOf(cfgSrc);
                cfgSourcesToLoad.Add(cfgSrc);
            }

            return cfgSourcesToLoad;
        }
        private static ConfigurationLoadContext getCurrentConfigurationLoadContext() =>
            Device.Application.isEditor
                ? (Device.Application.isPlaying ? ConfigurationLoadContext.PlayMode : ConfigurationLoadContext.BuildScript)
                : (Debug.isDebugBuild ? ConfigurationLoadContext.DebugBuild : ConfigurationLoadContext.ReleaseBuild);
        private void finishLoading(IEnumerable<ConfigurationSource> configurationSources)
        {
            // Once all config sources have been loaded, deduplicate keys in the order provided
            IEnumerable<(string key, object val)> configs = configurationSources
                .SelectMany(x => x.LoadedConfigs)
                .GroupBy(x => x.Key, x => x.Value)
                .Select(grp => (key: grp.Key, val: grp.First()));

            foreach ((string key, object val) in configs)
                _configs.Add(key, val);

            LoadingComplete?.Invoke(sender: this, _configs);

            _loading = false;
        }

        #endregion

        #region Applying configs to clients

        public void CacheConfiguration(Type clientType, string configKey) => _cachedConfigurations.Add((clientType, configKey));

        public void Configure(object client, string configKey)
        {
            if (string.IsNullOrWhiteSpace(configKey))
                throw new ArgumentException($"'{nameof(configKey)}' cannot be null or whitespace", nameof(configKey));

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
            IList<Expression>? memberAssigns = cache ? new List<Expression>() : null;
            ParameterExpression? clientObjParam = cache ? Expression.Parameter(typeof(object), nameof(client)) : null;
            Expression? clientParam = cache ? Expression.Convert(clientObjParam, clientType) : null;

            // Set all fields on this client for which there is a config value
            string? clientName = (client is not Object clientObj) ? null : (client is Component component ? component.GetHierarchyName() : clientObj.name);
            string? quotedClientName = clientName is null ? null : $"'{clientName}' ";
            FieldInfo[] fields = clientType.GetFields(BINDING_FLAGS);
            for (int f = 0; f < fields.Length; ++f) {
                FieldInfo field = fields[f];
                string fieldKey = $"{key}.{field.Name}";
                if (tryGetTypedValue(fieldKey, field.FieldType, out object? val)) {
                    if (cache)
                        memberAssigns!.Add(Expression.Assign(Expression.MakeMemberAccess(clientParam, field), Expression.Constant(val, field.FieldType)));
                    else
                        field.SetValue(client, val);
                    
                    _logger.Log($"Field '{field.Name}' of {clientType.Name} client {quotedClientName}with config key '{key}' will be configured with value '{val}'");
                }
            }

            // Set all properties on this client for which there is a config value
            PropertyInfo[] props = clientType.GetProperties(BINDING_FLAGS);
            for (int p = 0; p < props.Length; ++p) {
                PropertyInfo prop = props[p];
                string propKey = $"{key}.{prop.Name}";
                if (tryGetTypedValue(propKey, prop.PropertyType, out object? val)) {
                    if (cache)
                        memberAssigns!.Add(Expression.Assign(Expression.MakeMemberAccess(clientParam, prop), Expression.Constant(val, prop.PropertyType)));
                    else
                        prop.SetValue(client, val);
                    _logger.Log($"Property '{prop.Name}' of {clientType.Name} client {quotedClientName}with config key '{key}' will be configured with value '{val}'");
                }
            }

            // Cache the configuration now, if requested
            if (cache) {
                compiledConfig = Expression.Lambda<Action<object>>(
                    body: Expression.Block(memberAssigns),
                    name: $"{nameof(Configure)}_{clientType.Name}_Generated",
                    parameters: new[] { clientObjParam }
                ).Compile();
                _compiledConfigs.Add((clientType, key), compiledConfig);
                compiledConfig(client);     // Don't forget to CALL the cached config method!!
                if (_recording)
                    _cachedConfigCounts[(clientType, key)] = 1;
            }
            else {
                if (_recording)
                    _uncachedConfigCounts[(clientType, key)] = _uncachedConfigCounts.TryGetValue((clientType, key), out int count) ? count + 1 : 1;
            }
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
        [Conditional("DEBUG")]
        public void GetConfigurationCounts([NotNull] ref ConfigurationCounts? counts) => counts = new ConfigurationCounts(
            cachedConfigCounts: new Dictionary<(Type, string), int>(_cachedConfigCounts),
            uncachedConfigCounts: new Dictionary<(Type, string), int>(_uncachedConfigCounts)
        );

        private bool tryGetTypedValue(string memberKey, Type memberType, out object? typedValue)
        {
            typedValue = null;

            bool hasValue = _configs.TryGetValue(memberKey, out object configVal);
            if (!hasValue)
                return false;

            string? errMsg = null;
            try { typedValue = Convert.ChangeType(configVal, memberType); }
            catch (InvalidCastException ex) { errMsg = ex.Message; }
            catch (FormatException ex) { errMsg = ex.Message; }
            catch (OverflowException ex) { errMsg = ex.Message; }

            if (errMsg is not null) {
                _logger.LogWarning($"Error converting value '{configVal}' to type '{memberType.FullName}' for member '{memberKey}': {errMsg} This config will be skipped.");
                return false;
            }

            return true;
        }

        #endregion

    }

}
