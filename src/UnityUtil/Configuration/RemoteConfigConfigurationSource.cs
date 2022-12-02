using System.Collections;
using Unity.RemoteConfig;
using UnityEngine;
using UnityUtil.Logging;

namespace UnityUtil.Configuration;

[CreateAssetMenu(menuName = $"{nameof(UnityUtil)}/Configuration/{nameof(RemoteConfigConfigurationSource)}", fileName = "remote-config")]
public class RemoteConfigConfigurationSource : ConfigurationSource
{
    // These structs are required in order to fetch configs. See https://docs.unity3d.com/Packages/com.unity.remote-config@2.0/manual/CodeIntegration.html
    private struct UserAttributes { }
    private struct AppAttributes { }

    private IAppEnvironment? _appEnvironment;

    private static int s_numLoads;
    private bool _fetchComplete;

    public void Inject(IAppEnvironment appEnvironment) => _appEnvironment = appEnvironment;

    public override ConfigurationSourceLoadBehavior LoadBehavior => ConfigurationSourceLoadBehavior.AsyncOnly;

    public override IEnumerator LoadAsync()
    {
        yield return base.LoadAsync();

        string env = _appEnvironment!.Name;
        Logger!.Log($"Loading configs asynchronously from Remote Config environment '{env}'...", context: this);
        if (++s_numLoads > 1)
            Logger!.LogError($"Attempt to load configs from {s_numLoads} Remote Config environments. Only one environment should ever be loaded.", context: this);

        _fetchComplete = false;

        ConfigManager.SetEnvironmentID(env);
        ConfigManager.FetchCompleted += fetchCompleted;
        ConfigManager.FetchConfigs(new UserAttributes(), new AppAttributes());

        while (!_fetchComplete)
            yield return null;
    }

    private void fetchCompleted(ConfigResponse res)
    {
        _fetchComplete = true;

        if (res.status == ConfigRequestStatus.Failed) {
            Logger!.LogWarning("Something went wrong while loading configuration settings from Remote Config. Using default values.", context: this);
            return;
        }

        string env = _appEnvironment!.Name;
        switch (res.requestOrigin) {
            case ConfigOrigin.Default:
                Logger!.Log($"No configuration settings loaded from Remote Config environment '{env}'. Using default values.", context: this);
                break;

            case ConfigOrigin.Cached:
                string[] cachedKeys = ConfigManager.appConfig.GetKeys();
                Logger!.Log($"No configuration settings loaded from Remote Config environment '{env}'. Using {cachedKeys.Length} cached values from a previous session.", context: this);
                for (int x = 0; x < cachedKeys.Length; ++x)
                    LoadedConfigsHidden.Add(cachedKeys[x], ConfigManager.appConfig.GetString(cachedKeys[x]));
                break;

            case ConfigOrigin.Remote:
                string[] keys = ConfigManager.appConfig.GetKeys();
                Logger!.Log($"Successfully loaded {keys.Length} configuration settings from Remote Config environment '{env}'.", context: this);
                for (int x = 0; x < keys.Length; ++x)
                    LoadedConfigsHidden.Add(keys[x], ConfigManager.appConfig.GetString(keys[x]));
                break;

        }
    }

}
