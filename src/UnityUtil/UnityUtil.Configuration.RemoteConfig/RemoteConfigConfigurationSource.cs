using System;
using Microsoft.Extensions.Configuration;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.RemoteConfig;

namespace UnityUtil.Configuration.RemoteConfig;

public class RemoteConfigConfigurationSource<TUser, TApp, TFilter>(
    TUser userAttributes,
    TApp appAttributes,
    TFilter filterAttributes
) : IConfigurationSource
    where TUser : struct
    where TApp : struct
    where TFilter : struct
{
    // These structs are required in order to fetch configs. See https://docs.unity3d.com/Packages/com.unity.remote-config@2.0/manual/CodeIntegration.html
    public TUser UserAttributes { get; set; } = userAttributes;
    public TApp AppAttributes { get; set; } = appAttributes;
    public TFilter FilterAttributes { get; set; } = filterAttributes;
    public string? ConfigType { get; set; }

    public bool InitializeUnityServices { get; set; } = true;
    public bool InitializeUnityAuthentication { get; set; } = true;
    public InitializationOptions? InitializationOptions { get; set; }
    public SignInOptions? AuthenticationSignInOptions { get; set; }
    public Action<RemoteConfigService>? RemoteConfigInitializer { get; set; }

    public IConfigurationProvider Build(IConfigurationBuilder builder) =>
        new RemoteConfigConfigurationProvider<TUser, TApp, TFilter>(this);
}
