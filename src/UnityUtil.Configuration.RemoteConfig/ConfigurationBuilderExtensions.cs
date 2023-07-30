using Microsoft.Extensions.Configuration;
using System;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.RemoteConfig;

namespace UnityUtil.Configuration.RemoteConfig;

public static class ConfigurationBuilderExtensions
{
    internal struct DefaultFilterAttributes { }

    public static IConfigurationBuilder AddUnityRemoteConfig<TUser, TApp>(this IConfigurationBuilder builder,
        TUser userAttributes,
        TApp appAttributes,
        string? configType = null,
        bool initializeUnityServices = true,
        bool initializeUnityAuthentication = true,
        InitializationOptions? initializationOptions = null,
        SignInOptions? authenticationSignInOptions = null,
        Action<RemoteConfigService>? remoteConfigInitializer = null
    )
        where TUser : struct
        where TApp : struct
    => AddUnityRemoteConfig(builder,
        userAttributes,
        appAttributes,
        new DefaultFilterAttributes(),
        configType,
        initializeUnityServices,
        initializeUnityAuthentication,
        initializationOptions,
        authenticationSignInOptions,
        remoteConfigInitializer
    );

    public static IConfigurationBuilder AddUnityRemoteConfig<TUser, TApp, TFilter>(this IConfigurationBuilder builder,
        TUser userAttributes,
        TApp appAttributes,
        TFilter filterAttributes,
        string? configType = null,
        bool initializeUnityServices = true,
        bool initializeUnityAuthentication = true,
        InitializationOptions? initializationOptions = null,
        SignInOptions? authenticationSignInOptions = null,
        Action<RemoteConfigService>? remoteConfigInitializer = null
    )
        where TUser : struct
        where TApp : struct
        where TFilter : struct
    =>
        builder.Add(new RemoteConfigConfigurationSource<TUser, TApp, TFilter>() {
            UserAttributes = userAttributes,
            AppAttributes = appAttributes,
            FilterAttributes = filterAttributes,
            ConfigType = configType,
            InitializeUnityServices = initializeUnityServices,
            InitializeUnityAuthentication = initializeUnityAuthentication,
            InitializationOptions = initializationOptions,
            AuthenticationSignInOptions = authenticationSignInOptions,
            RemoteConfigInitializer = remoteConfigInitializer,
        });
}
