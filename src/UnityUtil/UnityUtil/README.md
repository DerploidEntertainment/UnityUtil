# UnityUtil

[< Back to main README](../../../README.md)

A set of utility classes and components useful to any Unity project, 2D or 3D.

Included utilities cover:

- [Dependency Injection](#dependency-injection)
- [Logging](#logging)
- [Math](#math)
- [Storage](#storage)
- [Updating](#updating)

**Work in progress!**

This repo has been under open-source development since ~2017, but only since late 2022 has it been seriously considered for "usability" by 3rd parties,
so documentation content/organization are still in development.

## Dependency Injection

**Work in progress!**

- Order of service registration does not matter.
- Multiple scenes may have scene-specific "composition root" components, even multiple such roots in a single scene;
    their registered services will all be combined.
- You should also define an `OnDestroy` method in these components, so that services can be unregistered when the scene unloads.
    This allows a game to dynamically register and unregister a scene's services at runtime.
- We recommend setting composition root components to run as early as possible in the Unity Script Execution Order,
    so that their services are registered before all other components' `Awake` methods run.
- Note, however, that an error will result if multiple composition roots
    try to register a service with the same parameters. In this case, it may be better to create a 'base' scene
    with all common services, so that they are each registered once (e.g., services for logging, player identity, analytics, etc.)
    You could also register the services with different tags.

## Logging

All UnityUtil namespaces use the `Microsoft.Extensions.Logging.Abstractions` types for logging.
These types are designed by Microsoft to abstract any actual logging framework used by apps/games, such as Serilog, NLog, Log4Net, etc.
See [Logging in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging) for more info (especially if you're new to log levels, scopes, message templates, etc.).

The MS Extension libraries are published as NuGet packages, as are most .NET logging frameworks.
By adding the UnityNuGet UPM scoped registry (described in [Installing](#installing)), you can effectively use any logging framework with UnityUtil in a ".NET native" way.
This is useful, as most .NET log frameworks are much more powerful/extensible than [Unity's built-in logger](https://docs.unity3d.com/ScriptReference/Debug-unityLogger.html).

For example, if you wanted to use [Serilog](https://serilog.net/) in your game code, you would add the following UPM (NuGet) packages to your Unity project
(via the Package Manager window or by manually editing `Packages/manifest.json`).

```jsonc
// Packages/manifest.json
{
    "dependencies": {
        "org.nuget.serilog": <version>",
        "org.nuget.serilog.extensions.logging": "<version>",
        // ...
    },
    // ...
}
```

The `org.nuget.serilog` package adds Serilog itself, while `org.nuget.serilog.extensions.logging` adds extension methods and types to make Serilog usable with `Microsoft.Extensions.Logging.Abstractions`.

With this setup, you can register an `ILoggerFactory` for [dependency injection](#dependency-injection).
Client types can then inject this service and use it as follows:

```cs
class MyClass {
    private ILogger<MyClass>? _logger;
    // ...
    public void Inject(ILoggerFactory loggerFactory) {
        _logger = loggerFactory.CreateLogger<MyClass>();
    }
    // ...
    public void DoStuff() {
        _logger!.LogInformation(new EventId(100, nameof(DoStuff)), "Currently doing stuff in {frame}...", Time.frameCount);
    }
}
```

### Log event IDs

The MS `ILogger` extension methods expect every log message to have an [`EventId`](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging?tabs=command-line#log-event-id),
which encapsulates an integer ID and a string name.
This presents a challenge, as every UnityUtil library, along with _your_ app, are now sharing the same "space" of `EventId`s.
To prevent ID "collisions", UnityUtil just uses `0` for all `EventId.Id` values,
while still providing unique _`EventId.Name`_ values to help you identify specific log messages.
You only need to ensure uniqueness of _your_ log event IDs, or you can take the UnityUtil approach of setting them all to `0` also. 🤷‍♂️

UnityUtil also provides basic `UnityDebugLogger(Factory)` types that you can use to output log messages directly to Unity's Console.
You might use these types in Editor code or automated tests, where you still want to see log messages in the Console but don't need a complete log framework setup.

For example:

```cs
class MyClass {
    public void DoStuff() {
        ILogger<MyClass> logger = new UnityDebugLoggerFactory().CreateLogger<MyClass>();
        logger.LogInformation(new EventId(100, nameof(DoStuff)), "Currently doing stuff in {frame}...", Time.frameCount);
    }
}
```

### Logger extensions classes

All logging code should ideally occur in a "logger extensions" class, like so:

```cs
// In LoggerExtensions.cs
internal static class LoggerExtensions
{
    public static void DoingStuff(this ILogger logger, int frameCount) {
        logger.LogInformation(new EventId(100, nameof(DoStuff)), "Currently doing stuff in {frame}...", Time.frameCount);
    }
}

// In MyClass.cs
class MyClass {
    public void DoStuff() {
        ILogger<MyClass> logger = new UnityDebugLoggerFactory().CreateLogger<MyClass>();
        logger.DoingStuff(Time.frameCount);
    }
}
```

You could have multiple such classes in your app; e.g., one for each namespace.

Logger extensions class provide a number of benefits:

1. All `EventId` names can use the `nameof` operator on the name of the method, making it easier to keep your log event names unique and free of typos.
1. Your business logic becomes easier to read without long, "noisy" log message strings scattered throughout.
1. All log event messages are now visible in a single file, making it easier to keep wording and log property names consistent.
1. The log methods can accept and work with parameters, keeping your calling code clean and free of log-specific logic.
    For example, your log method might accept a single object parameter, but access multiple members of that object in the encapsulated log message.
1. Your log methods can use the `nameof` operator on the names of parameters as the names of log properties, to keep those names free of typos.
1. Your log methods can use the MS library's `LoggerMessage` class for [high-performance logging](https://learn.microsoft.com/en-us/dotnet/core/extensions/high-performance-logging).

> [!NOTE]
> Consider also using the [Unity.Extensions.Logging](../../../src/Logging/Unity.Extensions.Logging/README.md) package
> if you plan to use a logging provider that writes to Unity's Console,
> as that package simplifies the addition of `tag` strings and `context` objects to Unity Console messages.

### Notes for specific logging providers

UnityUtil also maintains some custom UPM packages to get the best developer experience with specific logging providers, described here.

#### Serilog

- [Serilog.Enrichers.Unity](../Serilog.Enrichers.Unity/README.md): enrich log events with additional Unity data
- [Serilog.Sinks.Unity](../Serilog.Sinks.Unity/README.md): add Unity as a Serilog sink (the older [Serilog.Sinks.Unity3D](https://github.com/KuraiAndras/Serilog.Sinks.Unity3D) is also an option)
- [Unity.Extensions.Serilog](../Unity.Extensions.Serilog/README.md): other useful Serilog extension methods for Unity,
    including a destructuring policy needed to preserve `UnityEngine.Object` "context" instances

## Math

**Docs coming soon!**

## Storage

**Docs coming soon!**

## Updating

**Docs coming soon!**

Copyright © 2017 - present Dan Vicarel & Contributors. All rights reserved.
