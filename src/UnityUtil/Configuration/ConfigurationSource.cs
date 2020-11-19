using System;
using System.Collections.Generic;
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

    public abstract class ConfigurationSource : ScriptableObject {

        public bool Required = true;
        [Tooltip(
            "In what contexts should we attempt to load this " + nameof(ConfigurationSource) + "? " +
            "E.g., only when entering Play Mode in the Editor, or only in Release builds. " +
            "One handy use of the Editor context is for " + nameof(ConfigurationSource) + "s whose corresponding config assets " +
            "are included under an Assets/**/Editor/ folder. This lets you keep those config assets out of builds so they don't take up space, " +
            "and then the configuration system won't attempt to load them or warn that they are missing."
        )]
        public ConfigurationLoadContext LoadContext;

        protected ILogger Logger;

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        private void Reset() {
            Required = true;
            LoadContext = ConfigurationLoadContext.Always;
        }

        public void Inject(ILoggerProvider loggerProvider) {
            // Because this is a ScriptableObject, there may not be a scene with registered service instances
            // when 11this method is called...hence the null checks

            Logger = loggerProvider.GetLogger(this);
        }

        public abstract IDictionary<string, object> LoadConfigs();

    }

}
