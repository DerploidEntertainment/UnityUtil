using Microsoft.Extensions.Configuration;
using System;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.RemoteConfig;

namespace UnityUtil.Configuration.RemoteConfig;

public class RemoteConfigConfigurationProvider<TUser, TApp, TFilter> : ConfigurationProvider
    where TUser : struct
    where TApp : struct
    where TFilter : struct
{
    private readonly RemoteConfigConfigurationSource<TUser, TApp, TFilter> _source;
    public RemoteConfigConfigurationProvider(RemoteConfigConfigurationSource<TUser, TApp, TFilter> source) {
        _source = source;
    }

    public override void Load()
    {
        if (_source.InitializeUnityServices) {
            if (_source.InitializationOptions is null)
                UnityServices.InitializeAsync().Wait();
            else
                UnityServices.InitializeAsync(_source.InitializationOptions).Wait();
        }

        if (_source.InitializeUnityAuthentication) {
            // Remote Config requires authentication for managing environment information
            if (!AuthenticationService.Instance.IsSignedIn) {
                if (_source.AuthenticationSignInOptions is null)
                    AuthenticationService.Instance.SignInAnonymouslyAsync().Wait();
                else
                    AuthenticationService.Instance.SignInAnonymouslyAsync(_source.AuthenticationSignInOptions).Wait();
            }
        }

        _source.RemoteConfigInitializer?.Invoke(RemoteConfigService.Instance);

        RemoteConfigService.Instance.FetchCompleted += fetchCompleted;
        if (_source.ConfigType is null)
            RemoteConfigService.Instance.FetchConfigsAsync(_source.UserAttributes, _source.AppAttributes, _source.FilterAttributes).Wait();
        else
            RemoteConfigService.Instance.FetchConfigsAsync(_source.ConfigType, _source.UserAttributes, _source.AppAttributes, _source.FilterAttributes).Wait();
    }

    // See https://docs.unity3d.com/Packages/com.unity.remote-config-runtime@3.0/manual/CodeIntegration.html#security
    public override void Set(string key, string value) => throw new InvalidOperationException("Unity Remote Config is read-only");

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
