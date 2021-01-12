using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.Logging;

namespace UnityEngine
{

    [Flags]
    public enum ConfigurationLoadContext {
        Never = 0b000,
        Editor = 0b001,
        DebugBuild = 0b010,
        ReleaseBuild = 0b100,
        Always = 0b111,
    }

    public abstract class ConfigurationSource : ScriptableObject
    {
        public static readonly IReadOnlyDictionary<string, object> EmptyConfigs = new Dictionary<string, object>();

        private ILogger _logger;
        protected IUnityMainThreadDispatcher UnityMainThreadDispatcher;

        protected readonly Dictionary<string, object> LoadedConfigsHidden = new Dictionary<string, object>();

        [field: ShowInInspector, SerializeField]
        public bool Required { get; private set; }

        [field: Tooltip(
            "In what contexts should we attempt to load this " + nameof(ConfigurationSource) + "? " +
            "E.g., only when entering Play Mode in the Editor, or only in Release builds. " +
            "One handy use of the Editor context is for " + nameof(ConfigurationSource) + "s whose corresponding config assets " +
            "are included under an Assets/**/Editor/ folder. This lets you keep those config assets out of builds so they don't take up space, " +
            "and then the configuration system won't attempt to load them or warn that they are missing."
        )]
        [field: ShowInInspector, SerializeField]
        public ConfigurationLoadContext LoadContext { get; private set; }

        [Conditional("UNITY_EDITOR")]
        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        private void Reset() {
            Required = true;
            LoadContext = ConfigurationLoadContext.Always;
        }

        public void Inject(ILoggerProvider loggerProvider, IUnityMainThreadDispatcher unityMainThreadDispatcher)
        {
            UnityMainThreadDispatcher = unityMainThreadDispatcher;
            _logger = loggerProvider.GetLogger(this);
        }

        public abstract IEnumerator Load();
        public IReadOnlyDictionary<string, object> LoadedConfigs => LoadedConfigsHidden;

        protected void Log(string message) => UnityMainThreadDispatcher.Enqueue(() => _logger.Log(message, context: this));
        protected void LogWarning(string message) => UnityMainThreadDispatcher.Enqueue(() => _logger.LogWarning(message, context: this));
        protected void LogError(string message) => UnityMainThreadDispatcher.Enqueue(() => _logger.LogError(message, context: this));
    }

}
