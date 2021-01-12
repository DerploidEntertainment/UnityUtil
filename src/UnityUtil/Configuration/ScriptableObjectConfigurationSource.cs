using System.Collections;
using System.IO;
using System.Linq;

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

        public override IEnumerator Load() {
            string resFileName = $"{ResourceName}.asset";
            Log($"Loading configs from ScriptableObject configuration file '{resFileName}'...");

            // Load the specified resource file, if it exists
            ResourceRequest req = Resources.LoadAsync<ConfigObject>(ResourceName);
            while (!req.isDone)
                yield return null;

            var config = (ConfigObject)req.asset;
            if (config == null) {
                string notFoundMsg = $"ScriptableObject configuration file ('{resFileName}') could not be found. If this was not expected, make sure that the file exists and is not locked by another application.";
                if (Required)
                    throw new FileNotFoundException(notFoundMsg, ResourceName);
                Log(notFoundMsg);
                yield break;
            }

            // Read the config keys/values into a Dictionary
            var configGrps = config.Configs
                .Select(cfg => (cfg.Key, Value: cfg.GetValue()))
                .GroupBy(kv => kv.Key);
            foreach (var cfgGrp in configGrps) {
                var keyVals = cfgGrp.ToArray();
                if (keyVals.Length > 1)
                    LogWarning($"Duplicate config key ('{cfgGrp.Key}') detected in ScriptableObject configuration file '{resFileName}'. Keeping the last value...");
                LoadedConfigsHidden.Add(cfgGrp.Key, keyVals[keyVals.Length - 1].Value);
            }

            Log($"Successfully loaded {LoadedConfigs.Count} configs from ScriptableObject configuration file '{resFileName}'.");
        }
    }

}
