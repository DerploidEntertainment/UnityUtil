# UnityUtil.Configuration.RemoteConfig

[< Back to main README](../../README.md)

**Work in progress!**

This repo has been under open-source development since ~2017, but only since late 2022 has it been seriously considered for "usability" by 3rd parties,
so documentation content/organization are still in development.

## Overview

This project implements a [Microsoft.Extensions.Configuration](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration) configuration provider
for Unity's [RemoteConfig](https://docs.unity.com/ugs/manual/remote-config/manual/WhatsRemoteConfig) service.
With it, you can add RemoteConfig settings to your game's configuration with code like the following:

```cs
private async Task<IConfigurationRoot> buildConfigurationAsync() {
    IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();

    // Add lower-priority configuration sources...

    if (!Application.isEditor || Application.isPlaying) {
        configurationBuilder.AddUnityRemoteConfig(
            new TUser(),
            new TApp(),
            remoteConfigInitializer: remoteCfg => {
                remoteCfg.SetEnvironmentID(/* ... */);
                // Init other RemoteConfig properties
            }
        );
    }

    // Add higher-priority configuration sources...

    return configurationBuilder.Build();
}
```

Unfortunately, there are some unavoidable issues with the `RemoteConfigConfigurationProvider`:

- It cannot load settings by `Wait`ing the `Task` returned by `RemoteConfigService.FetchConfigsAsync`,
  as this would lead to a deadlock with the Unity main thread
- It cannot load settings in a `FetchCompleted` listener callback after calling `RemoteConfigService.FetchConfigs`,
  as this callback runs at an indeterminate time.
  The `RemoteConfigConfigurationSource` constructor could accept a callback and add _that_ as a listener to `FetchCompleted`,
  but that breaks the encapsulation/contract of configuration sources.
  Consumers could also wait for a short time after after building configuration to ensure that `FetchCompleted` has been triggered,
  but the amount of time would be arbitrary--either too short or longer than necessary.

As such, development of this package is currently halted until either:

1. The Microsoft Configuration system supports [building configuration asynchronously](https://github.com/dotnet/runtime/issues/79193), or
2. Unity RemoteConfig supports [synchronous fetching without a callback](https://forum.unity.com/threads/fetchconfigs-synchronously-without-callback-to-support-ms-extensions-configuration.1481922/)

In the meantime, developers wishing to treat Unity RemoteConfig as an MS configuration source can simply
`await RemoteConfigService.FetchConfigsAsync()` in their initialization logic (e.g., in an `async Start` method),
then pass the fetched configs to `IConfigurationBuilder.AddInMemoryCollection()` like in the code below.
Unity's `Task`-handling logic will avoid the deadlocks mentioned above.

```cs
private async void Start()
{
    IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();

    // Add lower-priority configuration sources...

    if (!Application.isEditor || Application.isPlaying) {
        RemoteConfigService.Instance.SetEnvironmentID(/* ... */);
        // Init other RemoteConfig properties
        RuntimeConfig runtimeConfig = await RemoteConfigService.Instance.FetchConfigsAsync(new TUser(), new TApp());
        configurationBuilder.AddInMemoryCollection(
            runtimeConfig.GetKeys().Select(x => KeyValuePair.Create(x, (string?)runtimeConfig.GetString(x)))
        );
    }

    // Add higher-priority configuration sources...

    IConfigurationRoot config = configurationBuilder.Build();
}
```

Copyright © 2017 - present Dan Vicarel & Contributors. All rights reserved.
