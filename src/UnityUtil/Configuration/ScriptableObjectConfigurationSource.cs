using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Logging;

namespace UnityUtil.Configuration;

[CreateAssetMenu(menuName = $"{nameof(UnityUtil)}/Configuration/{nameof(ScriptableObjectConfigurationSource)}", fileName = DefaultResourceName + ".cfgsource.asset")]
public class ScriptableObjectConfigurationSource : ConfigurationSource
{
    public const string DefaultResourceName = "appsettings";

    [Tooltip(
        $"Path to a {nameof(ConfigObject)} file under a Resources/ folder. " +
        "No matter what the full path of the file is, the directory name up to and including 'Resources/' must be omitted. " +
        "Leading and trailing slashes, and .asset extension, must be omitted. " +
        "Must use forward slashes, not backslashes (even on Windows)."
    )]
    public string ResourceName = DefaultResourceName;

    public override ConfigurationSourceLoadBehavior LoadBehavior => ConfigurationSourceLoadBehavior.SyncAndAsync;

    public override void Load()
    {
        base.Load();

        string resFileName = $"{ResourceName}.asset";
        Logger!.Log($"Loading configs synchronously from ScriptableObject configuration file '{resFileName}'...", context: this);

        ConfigObject config = Resources.Load<ConfigObject>(ResourceName);
        finishLoading(config, resFileName);
    }

    public override IEnumerator LoadAsync()
    {
        yield return base.LoadAsync();

        string resFileName = $"{ResourceName}.asset";
        Logger!.Log($"Loading configs asynchronously from ScriptableObject configuration file '{resFileName}'...", context: this);

        // Load the specified resource file, if it exists
        ResourceRequest req = Resources.LoadAsync<ConfigObject>(ResourceName);
        while (!req.isDone)
            yield return null;

        var config = (ConfigObject)req.asset;
        finishLoading(config, resFileName);
    }

    private void finishLoading(ConfigObject config, string resFileName)
    {
        if (config == null) {
            string notFoundMsg = $"ScriptableObject configuration file ('{resFileName}') could not be found. If this was not expected, make sure that the file exists and is not locked by another application.";
            if (Required)
                throw new FileNotFoundException(notFoundMsg, ResourceName);
            Logger!.Log(notFoundMsg, context: this);
            return;
        }

        // Read the config keys/values into a Dictionary
#pragma warning disable IDE0008 // Use explicit type
        var configGrps = config.Configs
            .Select(cfg => (cfg.Key, Value: cfg.GetValue()))
            .GroupBy(kv => kv.Key);
        foreach (var cfgGrp in configGrps) {
            var keyVals = cfgGrp.ToArray();
            if (keyVals.Length > 1)
                Logger!.LogWarning($"Duplicate config key ('{cfgGrp.Key}') detected in ScriptableObject configuration file '{resFileName}'. Keeping the last value...", context: this);
            LoadedConfigsHidden.Add(cfgGrp.Key, keyVals[^1].Value);
        }
#pragma warning restore IDE0008 // Use explicit type

        Logger!.Log($"Successfully loaded {LoadedConfigs.Count} configs from ScriptableObject configuration file '{resFileName}'.", context: this);
    }
}
