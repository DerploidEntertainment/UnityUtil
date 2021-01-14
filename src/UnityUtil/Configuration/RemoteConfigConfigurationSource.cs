using Sirenix.OdinInspector;
using System.Collections;
using Unity.RemoteConfig;

namespace UnityEngine
{
    [CreateAssetMenu(menuName = nameof(UnityUtil) + "/" + "Configuration" + "/" + nameof(RemoteConfigConfigurationSource), fileName = DefaultEnvironment + ".remoteConfig.asset")]
    public class RemoteConfigConfigurationSource : ConfigurationSource
    {
        // These structs are required in order to fetch configs. See https://docs.unity3d.com/Packages/com.unity.remote-config@2.0/manual/CodeIntegration.html
        private struct UserAttributes { }
        private struct AppAttributes { }

        private static int s_numLoads = 0;
        private bool _loaded = false;

        public const string DefaultEnvironment = "Release";

        [Tooltip("The Remote Config environment from which to request configuration settings")]
        [InfoBox("Note that you should only load one Remote Config environment per environment of your app. If you do not, you will likely experience errors with " + nameof(fetchCompleted) + " callbacks being called too many times.")]
        public string Environment = DefaultEnvironment;

        public override IEnumerator Load() {
            Log($"Loading configs from Remote Config environment '{Environment}'...");
            if (++s_numLoads > 1)
                LogError($"Attempt to load configs from {s_numLoads} Remote Config environments. Only one environment should ever be loaded.");

            ConfigManager.SetEnvironmentID(Environment);
            ConfigManager.FetchCompleted += fetchCompleted;
            ConfigManager.FetchConfigs(new UserAttributes(), new AppAttributes());

            while (!_loaded)
                yield return null;
        }

        private void fetchCompleted(ConfigResponse res)
        {
            _loaded = true;

            if (res.status == ConfigRequestStatus.Failed) {
                LogWarning("Something went wrong while loading configuration settings from Remote Config. Using default values.");
                return;
            }

            switch (res.requestOrigin) {
                case ConfigOrigin.Default:
                    Log($"No configuration settings loaded from Remote Config environment '{Environment}'. Using default values.");
                    break;

                case ConfigOrigin.Cached:
                    string[] cachedKeys = ConfigManager.appConfig.GetKeys();
                    Log($"No configuration settings loaded from Remote Config environment '{Environment}'. Using {cachedKeys.Length} cached values from a previous session.");
                    for (int x = 0; x < cachedKeys.Length; ++x)
                        LoadedConfigsHidden.Add(cachedKeys[x], ConfigManager.appConfig.GetString(cachedKeys[x]));
                    break;

                case ConfigOrigin.Remote:
                    string[] keys = ConfigManager.appConfig.GetKeys();
                    Log($"Successfully loaded {keys.Length} configuration settings from Remote Config environment '{Environment}'.");
                    for (int x = 0; x < keys.Length; ++x)
                        LoadedConfigsHidden.Add(keys[x], ConfigManager.appConfig.GetString(keys[x]));
                    break;
            }
        }

    }

}
