# UnityUtil

[< Back to main README](../../README.md)

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

Order of service registration does not matter.
Multiple scenes may have scene-specific composition roots, even multiple such roots;
their registered services will all be combined.
You should also define an `OnDestroy` method in these components, so that services can be unregistered when the scene unloads.
This allows a game to dynamically register and unregister a scene's services at runtime.
We recommend setting composition roots components to run as early as possible in the Unity Script Execution Order,
so that their services are registered before all other components' `Awake` methods run.
Note, however, that an error will result if multiple composition roots
try to register a service with the same parameters. In this case, it may be better to create a 'base' scene
with all common services, so that they are each registered once, or register the services with different tags.

## Logging

All UnityUtil namespaces use the `Microsoft.Extensions.Logging.Abstractions` types for logging.
These types are designed by Microsoft to abstract any actual logging framework used by apps/games, such as Serilog, NLog, Log4Net, etc.
See [Logging in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging) for more info (especially if you're new to log levels, scopes, message templates, etc.).
The MS Extension libraries are published as NuGet packages, as are most .NET logging frameworks.
By adding the UnityNuGet UPM scoped registry (described in [Installing](#installing)), you can effectively use any logging framework with UnityUtil in a ".NET native" way.
This is useful, as most .NET log frameworks are much more powerful/extensible than [Unity's built-in logger](https://docs.unity3d.com/ScriptReference/Debug-unityLogger.html).

For example, if you wanted to use Serilog in your game code, you would add the following UPM (NuGet) packages to your Unity project
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

The `org.nuget.serilog` package adds Serilog itself, while `org.nuget.serilog.extensions.logging` adds a "glue" library to make Serilog usable with `Microsoft.Extensions.Logging.Abstractions`.

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
        _logger!.LogInformation(new EventId(100, "DoingStuff"), "Currently doing stuff in {frame}...", Time.frameCount);
    }
}
```

The MS `ILogger` extension methods expect every log message to have an [`EventId`](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging?tabs=command-line#log-event-id),
which encapsulates an integer ID and a string name.
This presents a challenge, as every UnityUtil library, along with _your_ app, are now sharing the same "space" of `EventId`s.
To prevent ID "collisions", you can define a custom logger class for your app that exposes a unique public method for every log message.

Following the example above, you could define the following custom logger:

```cs
public void MyAppLogger<T> {
    private readonly ILogger<T> _logger;
    // ...
    public void MyAppLogger(ILoggerFactory loggerFactory) {
        _logger = loggerFactory.CreateLogger<T>();
    }
    // ...
    public void DoStuff(int frame) {
        _logger.LogInformation(new EventId(100, nameof(DoStuff)), "Currently doing stuff in {{{nameof(frame)}}}...", frame);
    }
}
```

and use it in `MyClass` as follows:

```cs
class MyClass {
    private MyAppLogger<MyClass>? _logger;
    // ...
    public void Inject (ILoggerFactory loggerFactory) {
        _logger = new(loggerFactory);
    }
    // ...
    public void DoStuff() {
        _logger!.DoStuff(Time.frameCount);
    }
}
```

While this "custom logger pattern" is more verbose, it provides a number of benefits (all of which are visible in the above code):

1. All `EventId` IDs are now visible in a single file (`MyAppLogger.cs`), making it simpler to find and prevent ID collisions.
1. All `EventId` names can use the `nameof` operator on the name of the method, making it easier to keep your log event names unique and free of typos.
1. All log event messages are now visible in a single file, making it easier to keep wording and log property names consistent.
1. The log methods can accept and work with parameters, keeping your calling code clean and free of log-specific logic.
    For example, your log method might accept a single parameter, but access multiple members of that parameter in the encapsulated log message.
1. Your log methods can also use the `nameof` operator in the underlying log message, to keep log property names free of typos.

However, you still have to ensure that all log events have a unique ID.
UnityUtil provides a `BaseUnityUtilLogger` class from which you can derive to simplify the custom logger pattern.
Using it, the `MyAppLogger` example above would become:

```cs
public void MyAppLogger<T> : BaseUnityUtilLogger<T> {
    public void MyAppLogger(ILoggerFactory loggerFactory, T context)
        : base(loggerFactory, context, eventIdOffset: 1_000_000) { }
    // ...
    public void DoStuff(int frame) {
        LogInformation(id: 0, nameof(DoStuff), "Currently doing stuff in {{{nameof(frame)}}}...", frame);
    }
}
```

First, notice that `BaseUnityUtilLogger`'s constructor requires a `context` parameter.
This is useful in Unity code: if your logging object derives from `MonoBeheviour`,
then it is saved to your log message's [scope](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging#log-scopes),
which specific logging frameworks can then extract and use as the `context` for underlying Unity [`Debug.Log`](https://docs.unity3d.com/ScriptReference/Debug.Log.html) calls.
In short, when you then click on the log event message in the Unity Console, the Editor highlights the logging object in the Hierarchy Window.
This can be very useful for understanding which components on which GameObjects are generating which logs during play testing.

Next, notice that `BaseUnityUtilLogger`'s constructor requires an `eventIdOffset` parameter.
This offset is added to the ID passed in calls to its protected `Log` method.
In other words, you can use simple increasing IDs (0, 1, 2, etc.) in your custom logger class,
and `BaseUnityUtilLogger.Log` converts those IDs into "disambiguated" IDs that are unique across all UnityUtil libraries _and_ your code!
The rule of thumb here is that every assembly (UnityUtil library or your app) gets a "subspace" of messages per `LogLevel` (`Information`, `Warning,`Error`, etc.).
Every ID's first digit matches its`LogLevel`enum value. For example, all`Warning`messages (enum value`3`), start with a`3`.

Each UnityUtil library gets 1000 messages per `LogLevel` by default;
your app can use as many messages as it wants, as long as they use a base `eventIdOffset` of 1,000,000 or more.
See [`BaseUnityUtilLogger`'s API documentation](./src/UnityUtil/Logging/BaseUnityUtilLogger.cs) for a deeper explanation of how it "partitions" the `EventId` ID space.
The "registered" `eventIdOffset`s for the UnityUtil namespaces are shown below.
If you are developing a library to work with UnityUtil, please avoid using these offsets, and/or submit a PR adding your library to this list
so that other authors know not to collide with _your_ library's IDs!

- 0: `UnityUtil`
- 1000: Reserved
- 2000: `UnityUtil.DependencyInjection`
- 3000: `UnityUtil.Inputs`
- 4000: `UnityUtil.Interaction`
- 5000: `UnityUtil.Inventories`
- 6000: `UnityUtil.Legal`
- 7000: `UnityUtil.Logging`
- 8000: `UnityUtil.Math`
- 9000: `UnityUtil.Movement`
- 10,000: `UnityUtil.Physics`
- 11,000: `UnityUtil.Physics2D`
- 12,000: `UnityUtil.Storage`
- 13,000: `UnityUtil.Triggers`
- 14,000: `UnityUtil.UI`
- 15,000: `UnityUtil.Updating`
- 16,000: `UnityUtil.Editor`

## Math

**Docs coming soon!**

## Storage

**Docs coming soon!**

## Updating

**Docs coming soon!**

Copyright © 2017 - present Dan Vicarel & Contributors. All rights reserved.
