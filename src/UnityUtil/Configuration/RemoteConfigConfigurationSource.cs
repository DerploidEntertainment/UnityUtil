using Microsoft.Extensions.Logging;
using System.Collections;
using Unity.RemoteConfig;
using UnityEngine;

namespace UnityUtil.Configuration;

[CreateAssetMenu(menuName = $"{nameof(UnityUtil)}/Configuration/{nameof(RemoteConfigConfigurationSource)}", fileName = "remote-config")]
public class RemoteConfigConfigurationSource : ConfigurationSource
{
    // These structs are required in order to fetch configs. See https://docs.unity3d.com/Packages/com.unity.remote-config@2.0/manual/CodeIntegration.html
    private struct UserAttributes { }
    private struct AppAttributes { }

    private ConfigurationLogger<RemoteConfigConfigurationSource>? _logger;
    private IAppEnvironment? _appEnvironment;

    private static int s_numLoads;
    private bool _fetchComplete;

    public void Inject(ILoggerFactory loggerFactory, IAppEnvironment appEnvironment)
    {
        _appEnvironment = appEnvironment;
        _logger = new(loggerFactory, context: this);
    }

    public override ConfigurationSourceLoadBehavior LoadBehavior => ConfigurationSourceLoadBehavior.AsyncOnly;

    public override IEnumerator LoadAsync()
    {
        yield return base.LoadAsync();

        string env = _appEnvironment!.Name;
        _logger!.RemoteConfigLoadingAsync(env);
        if (++s_numLoads > 1)
            _logger!.RemoteConfigLoadingFailMultipleEnvironments(s_numLoads);

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
            _logger!.RemoteConfigLoadingFail();
            return;
        }

        string env = _appEnvironment!.Name;
        switch (res.requestOrigin) {
            case ConfigOrigin.Default:
                _logger!.RemoteConfigNothingLoaded(env);
                break;

            case ConfigOrigin.Cached:
                string[] cachedKeys = ConfigManager.appConfig.GetKeys();
                _logger!.RemoteConfigUsingCache(env, cachedKeys.Length);
                for (int x = 0; x < cachedKeys.Length; ++x)
                    LoadedConfigsHidden.Add(cachedKeys[x], ConfigManager.appConfig.GetString(cachedKeys[x]));
                break;

            case ConfigOrigin.Remote:
                string[] keys = ConfigManager.appConfig.GetKeys();
                _logger!.RemoteConfigLoadSuccess(env, keys.Length);
                for (int x = 0; x < keys.Length; ++x)
                    LoadedConfigsHidden.Add(keys[x], ConfigManager.appConfig.GetString(keys[x]));
                break;

        }
    }

}
