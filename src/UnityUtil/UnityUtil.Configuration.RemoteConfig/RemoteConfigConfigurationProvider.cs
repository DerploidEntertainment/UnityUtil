using System;
using Microsoft.Extensions.Configuration;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.RemoteConfig;

namespace UnityUtil.Configuration.RemoteConfig;

public class RemoteConfigConfigurationProvider<TUser, TApp, TFilter>(
    RemoteConfigConfigurationSource<TUser, TApp, TFilter> source
) : ConfigurationProvider
    where TUser : struct
    where TApp : struct
    where TFilter : struct
{
    public override void Load()
    {
        if (source.InitializeUnityServices) {
            if (source.InitializationOptions is null)
                UnityServices.InitializeAsync().Wait();
            else
                UnityServices.InitializeAsync(source.InitializationOptions).Wait();
        }

        // Remote Config requires authentication for managing environment information
        if (source.InitializeUnityAuthentication && !AuthenticationService.Instance.IsSignedIn)
            AuthenticationService.Instance.SignInAnonymouslyAsync(source.AuthenticationSignInOptions).Wait();

        RemoteConfigService remoteConfig = RemoteConfigService.Instance;
        source.RemoteConfigInitializer?.Invoke(remoteConfig);

        // DO NOT fetch configs async, even with Task.Wait(), as this will cause a deadlock on the Unity main thread
        remoteConfig.FetchCompleted += fetchCompleted;
        if (source.ConfigType is null)
            remoteConfig.FetchConfigs(source.UserAttributes, source.AppAttributes, source.FilterAttributes);
        else
            remoteConfig.FetchConfigs(source.ConfigType, source.UserAttributes, source.AppAttributes, source.FilterAttributes);
    }

    // See https://docs.unity3d.com/Packages/com.unity.remote-config-runtime@3.0/manual/CodeIntegration.html#security
    public override void Set(string key, string? value) => throw new InvalidOperationException("Unity Remote Config is read-only");

    private void fetchCompleted(ConfigResponse configResponse)
    {
        if (configResponse.status == ConfigRequestStatus.Failed)
            return;

        switch (configResponse.requestOrigin) {
            case ConfigOrigin.Default:
                break;

            case ConfigOrigin.Cached:
            case ConfigOrigin.Remote:
                RuntimeConfig runtimeConfig = RemoteConfigService.Instance.appConfig;
                foreach (string key in runtimeConfig.GetKeys())
                    Data.Add(key, runtimeConfig.GetString(key));
                break;
        }
    }
}
