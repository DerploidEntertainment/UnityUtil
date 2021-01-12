using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.Logging;

namespace UnityEngine
{
    [CreateAssetMenu(menuName = nameof(UnityUtil) + "/" + "Configuration" + "/" + nameof(ScriptableObjectConfigurationSource), fileName = DefaultResourceName + ".cfgsource.asset")]
    public class ScriptableObjectConfigurationSource : ConfigurationSource
    {
        public const string DefaultResourceName = "appsettings";

        [Tooltip(
            "Path to a " + nameof(ConfigObject) + " file under a Resources/ folder. " +
            "No matter what the full path of the file is, the directory name up to and including 'Resources/' must be omitted. " +
            "Leading and trailing slashes, and .asset extension, must be omitted. " +
            "Must use forward slashes, not backslashes (even on Windows)."
        )]
        public string ResourceName = DefaultResourceName;

        public override IDictionary<string, object> LoadConfigs() {
            string resFileName = $"{ResourceName}.asset";
            Logger.Log($"Loading configs from ScriptableObject configuration file '{resFileName}'...", context: this);

            // Load the specified resource file, if it exists
            ConfigObject config = Resources.Load<ConfigObject>(ResourceName);
            if (config == null) {
                string notFoundMsg = $"ScriptableObject configuration file ('{resFileName}') could not be found. If this was not expected, make sure that the file exists and is not locked by another application.";
                if (Required)
                    throw new FileNotFoundException(notFoundMsg, ResourceName);
                Logger.Log(notFoundMsg, context: this);
                return new Dictionary<string, object>();
            }

            // Read the config keys/values into a Dictionary
            var values = config.Configs
                .Select(cfg => (cfg.Key, Value: cfg.GetValue()))
                .GroupBy(kv => kv.Key)
                .ToDictionary(g => g.Key, g => {
                    var keyVals = g.ToArray();
                    if (keyVals.Length > 1)
                        Logger.LogWarning($"Duplicate config key ('{g.Key}') detected in ScriptableObject configuration file '{resFileName}'. Keeping the last value...", context: this);
                    return keyVals[keyVals.Length - 1].Value;
                });

            Logger.Log($"Successfully loaded {values.Count} configs from ScriptableObject configuration file '{resFileName}'.", context: this);

            return values.ToDictionary(x => x.Key, x => x.Value);
        }
    }

}
