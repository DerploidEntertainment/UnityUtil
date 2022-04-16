using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine.Logging;

namespace UnityEngine;

[CreateAssetMenu(menuName = $"{nameof(UnityUtil)}/Configuration/{nameof(CsvConfigurationSource)}", fileName = DefaultResourceName + ".csv.asset")]
public class CsvConfigurationSource : ConfigurationSource
{
    public const string DefaultResourceName = "appsettings";

    [Tooltip(
        "Path to a CSV file under a Resources/ folder. " +
        "No matter what the full path of the file is, the directory name up to and including 'Resources/' must be omitted. " +
        "Leading and trailing slashes, and .csv extension, must be omitted. " +
        "Must use forward slashes, not backslashes (even on Windows)."
    )]
    public string ResourceName = DefaultResourceName;

    public override ConfigurationSourceLoadBehavior LoadBehavior => ConfigurationSourceLoadBehavior.SyncAndAsync;

    public override void Load()
    {
        base.Load();

        string resFileName = $"{ResourceName}.csv";
        Logger!.Log($"Loading configs synchronously from CSV configuration file '{resFileName}'...", context: this);

        TextAsset txt = Resources.Load<TextAsset>(ResourceName);
        finishLoading(txt, resFileName);
    }

    public override IEnumerator LoadAsync()
    {
        yield return base.LoadAsync();

        string resFileName = $"{ResourceName}.csv";
        Logger!.Log($"Loading configs asynchronously from CSV configuration file '{resFileName}'...", context: this);

        ResourceRequest req = Resources.LoadAsync<TextAsset>(ResourceName);
        while (!req.isDone)
            yield return null;

        var txt = (TextAsset)req.asset;
        finishLoading(txt, resFileName);
    }

    private void finishLoading(TextAsset txt, string resFileName)
    {
        if (txt == null) {
            string notFoundMsg = $"CSV configuration file ('{resFileName}') could not be found. If this was not expected, make sure that the file exists and is not locked by another application.";
            if (Required)
                throw new FileNotFoundException(notFoundMsg, ResourceName);
            Logger!.Log(notFoundMsg, context: this);
            return;
        }

        // Read the config keys/values into a Dictionary
#pragma warning disable IDE0008 // Use explicit type
        var configGrps = txt.text
            .Split('\n')
            .Where(cfg => !string.IsNullOrWhiteSpace(cfg))
            .Select((cfg, line) => {
                string[] tokens = cfg.Split(',');
                return tokens.Length == 2
                    ? (Key: tokens[0], Value: tokens[1])
                    : throw new InvalidDataException($"Each line of CSV configuration file '{resFileName}' must contain exactly two fields, the config key and value. Line {line + 1} had {tokens.Length}.");
            })
            .GroupBy(kv => kv.Key);
        foreach (var cfgGrp in configGrps) {
            var keyVals = cfgGrp.ToArray();
            if (keyVals.Length > 1)
                Logger!.LogWarning($"Duplicate config key ('{cfgGrp.Key}') detected in CSV configuration file '{resFileName}'. Keeping the last value...", context: this);
            LoadedConfigsHidden.Add(cfgGrp.Key, keyVals[^1].Value);
        }
#pragma warning restore IDE0008 // Use explicit type

        Logger!.Log($"Successfully loaded {LoadedConfigs.Count} configs from CSV configuration file '{resFileName}'.", context: this);
    }
}
