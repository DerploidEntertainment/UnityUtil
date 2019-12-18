using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.Logging;

namespace UnityEngine {

    [CreateAssetMenu(menuName = nameof(UnityUtil) + "/" + nameof(CsvConfigurationSource), fileName = "appsettings.csv.asset")]
    public class CsvConfigurationSource : ConfigurationSource {

        [Tooltip("Path to a CSV file under a Resources/ folder. No matter what the full path of the file is, the directory name up to and including 'Resources/' must be omitted. Leading and trailing slashes, and .csv extension, must be omitted. Must use forward slashes, not backslashes (even on Windows).")]
        public string ResourceName = "appsettings";

        public override IDictionary<string, object> LoadConfigs() {
            IDictionary<string, object> baseValues = base.LoadConfigs();

            string resFileName = $"{ResourceName}.csv";
            Logger.Log($"Loading configs from CSV configuration file '{resFileName}'...", context: this);

            // Load the specified resource CSV file, if it exists
            TextAsset txt = Resources.Load<TextAsset>(ResourceName);
            if (txt == null) {
                string notFoundMsg = $"Expected CSV file ('{resFileName}') for configuration source could not be found. Make sure the file exists and is not locked by any other applications.";
                if (Required)
                    throw new FileNotFoundException(notFoundMsg, ResourceName);
                Logger.LogWarning(notFoundMsg, context: this);
                return new Dictionary<string, object>();
            }

            // Read the config keys/values into a Dictionary
            // TODO: Assume that CSV contents is encoded, not plaintext
            var values = txt.text
                .Split('\n')
                .Where(cfg => !string.IsNullOrWhiteSpace(cfg))
                .Select((cfg, line) => {
                    string[] tokens = cfg.Split(',');
                    if (tokens.Length != 2)
                        throw new InvalidDataException($"Each line of CSV configuration file '{resFileName}' must contain exactly two fields, the config key and value. Line {line + 1} had {tokens.Length}.");
                    return (Key: tokens[0], Value: tokens[1]);
                })
                .GroupBy(kv => kv.Key)
                .ToDictionary(g => g.Key, g => {
                    var keyVals = g.ToArray();
                    if (keyVals.Length > 1)
                        Logger.LogWarning($"Duplicate config key ('{g.Key}') detected in CSV configuration file '{resFileName}'. Keeping the last value...", context: this);
                    return (object)keyVals[keyVals.Length - 1].Value;
                });

            Logger.Log($"Successfully loaded {values.Count} configs from CSV configuration file '{resFileName}'.", context: this);

            return values.Concat(baseValues).ToDictionary(x => x.Key, x => x.Value);
        }
    }

}
