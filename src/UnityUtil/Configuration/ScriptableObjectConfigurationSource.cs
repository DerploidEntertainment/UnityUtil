using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.Logging;

namespace UnityEngine {

    [CreateAssetMenu(menuName = "UnityUtil/" + nameof(ScriptableObjectConfigurationSource), fileName = "appsettings.cfgsource.asset")]
    public class ScriptableObjectConfigurationSource : ConfigurationSource {

        [Tooltip("Path to a " + nameof(ConfigObject) + " file under a Resources/ folder. No matter what the full path of the file is, the directory name up to and including 'Resources/' must be omitted. Leading and trailing slashes, and .asset extension, must be omitted. Must use forward slashes, not backslashes (even on Windows).")]
        public string ResourceName = "appsettings";

        public override IDictionary<string, object> LoadConfigs() {
            string resFileName = $"{ResourceName}.asset";
            Logger.Log($"Loading configs from ScriptableObject configuration file '{resFileName}'...");

            // Load the specified resource file, if it exists
            ConfigObject config = Resources.Load<ConfigObject>(ResourceName);
            if (config == null) {
                string notFoundMsg = $"Expected ScriptableObject file ('{resFileName}') for configuration source could not be found. Make sure the file exists and is not locked by any other applications.";
                if (Required)
                    throw new FileNotFoundException(notFoundMsg, ResourceName);
                Logger.LogWarning(notFoundMsg);
                return new Dictionary<string, object>();
            }

            // Read the config keys/values into a Dictionary
            var values = config.Configs
                .Select(cfg => (cfg.Key, Value: cfg.GetValue()))
                .GroupBy(kv => kv.Key)
                .ToDictionary(g => g.Key, g => {
                    var keyVals = g.ToArray();
                    if (keyVals.Length > 1)
                        Logger.LogWarning($"Duplicate config key ('{g.Key}') detected in ScriptableObject configuration file '{resFileName}'. Keeping the last value...");
                    return keyVals[keyVals.Length - 1].Value;
                });

            Logger.Log($"Successfully loaded {values.Count} configs from ScriptableObject configuration file '{resFileName}'.");

            return values;
        }
    }

}
