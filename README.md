# UnityUtil

A set of utility classes and components useful to any Unity project, 2D or 3D.

## Contents

- [Installing](#installing)
  - [Updating](#updating)
- [Features](#features)
  - [Configuration](#configuration)
  - [Dependency Injection](#dependency-injection)
  - [Inputs](#inputs)
  - [Interaction](#interaction)
  - [Inventories](#inventories)
  - [Legal](#legal)
  - [Logging](#logging)
  - [Math](#math)
  - [Movement](#movement)
  - [Physics](#physics)
  - [Physics2D](#physics2d)
  - [Storage](#storage)
  - [Triggers](#triggers)
  - [UI](#ui)
- [Support](#support)
- [Contributing](#contributing)
- [License](#license)

## Installing

1. Make sure you have both [Git](https://git-scm.com/) and [Git LFS](https://git-lfs.github.com/) installed before adding this package to your Unity project.
2. Add the [UnityNuGet](https://github.com/xoofx/UnityNuGet) scoped registry so that you can install NuGet packages through the Unity Package Manager.
3. Install dependencies in your Unity project. This is an opinionated list of 3rd party assets/packages that UnityUtil leverages for certain features.
    Unfortunately, some of these assets cost money. In the future, UnityUtil's features will be broken up into separate packages,
    so that users can ignore specific packages and not spend money on their Asset Store dependencies.
    - [Odin Inspector](https://odininspector.com/) (v3.0.12 or above).
        After installing, close the Editor and copy the `Sirenix/` folder from `Assets/` to a new `odininspector/` folder under `Packages/`.
        Also add a `package.json` file to the new folder as described in [Odin's docs](https://odininspector.com/tutorials/getting-started/install-odin-inspector-as-a-unity-package).
        Re-open Unity to see Odin installed as an embedded package.
4. In the Unity Editor, open the [Package Manager window](https://docs.unity3d.com/Manual/upm-ui.html), click the `+` button in the upper-left and choose `Add package from git URL...`.
5. Paste one of the following URLs:
    - `https://github.com/DerploidEntertainment/UnityUtil.git?path=/UnityUtil/Assets/UnityUtil#main` for the latest stable version
    - `https://github.com/DerploidEntertainment/UnityUtil.git?path=/UnityUtil/Assets/UnityUtil#<branch>` for experimental features on `<branch>`

### Updating

You can update this package from Unity's Package Manager window, even when it is imported as a git repo.
Doing so will update the commit from which changes are imported.
As the API stabilizes, I will move this package to [OpenUPM](https://openupm.com/) and add a changelog to make its versioning more clear.

There are also some things to note when updating the following dependencies:

- Odin Inspector: don't forget to repeat the process above to make Odin an _embedded UPM package_, not just a folder under `Assets/`.
    You must also bump the `version` field in the `odininspector` package's' `package.json`.

## Features

**Work in progress!**

This package has been under open-source development since ~2017, but only since late 2022 has it been seriously considered for "usability" by 3rd parties,
so documentation content/organization are still in development.

### General notes

Sometimes, you need to preserve code elements from [managed code stripping](https://docs.unity3d.com/Manual/ManagedCodeStripping.html) during builds.
For example, your app may produce runtime code that doesn't exist when Unity performs the static analysis, e.g. through reflection and/or dependency injection.
You can use Unity's `PreserveAttribute` mechansim to preserve these elements in your own code;
however, UnityUtil intentionally does _not_ annotate any code with `[Preserve]` so that you have total control over the size of your builds.
Therefore, if you need to preserve UnityUtil code elements (types, methods, etc.),
then you must use the [`link.xml` approach](https://docs.unity3d.com/Manual/ManagedCodeStripping.html#LinkXMLAnnotation) described in the Unity Manual.

### Configuration

**Docs coming soon!**

### Dependency Injection

**Docs coming soon!**

### Inputs

**Docs coming soon!**

### Interaction

**Docs coming soon!**

### Inventories

**Docs coming soon!**

### Legal

**Docs coming soon!**

### Logging

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
Using it, the `MyAppLogger` example above would becomes:

```cs
public void MyAppLogger<T> : BaseUnityUtilLogger<T> {
    public void MyAppLogger(ILoggerFactory loggerFactory, T context)
        : base(loggerFactory, context, eventIdOffset: 1_000_000) { }
    // ...
    public void DoStuff(int frame) {
        Log(id: 0, nameof(DoStuff), Information, "Currently doing stuff in {{{nameof(frame)}}}...", frame);
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
so that other authors know not collide with _your_ library's IDs!

- `UnityUtil`: 0
- `UnityUtil.Configuration`: 1000
- `UnityUtil.DependencyInjection`: 2000
- `UnityUtil.Inputs`: 3000
- `UnityUtil.Interaction`: 4000
- `UnityUtil.Inventories`: 5000
- `UnityUtil.Legal`: 6000
- `UnityUtil.Logging`: 7000
- `UnityUtil.Math`: 8000
- `UnityUtil.Movement`: 9000
- `UnityUtil.Physics`: 10,000
- `UnityUtil.Physics2D`: 11,000
- `UnityUtil.Storage`: 12,000
- `UnityUtil.Triggers`: 13,000
- `UnityUtil.UI`: 14,000
- `UnityUtil.Updating`: 15,000
- `UnityUtil.Editor`: 16,000

### Math

**Docs coming soon!**

### Movement

**Docs coming soon!**

### Physics

**Docs coming soon!**

### Physics2D

**Docs coming soon!**

### Storage

**Docs coming soon!**

### Triggers

**Docs coming soon!**

### UI

**Docs coming soon!**

## Support

For bug reports and feature requests, please search through the existing [Issues](https://github.com/DerploidEntertainment/UnityUtil/issues) first, then create a new one if necessary.

## Contributing

Make sure you have [Git LFS](https://git-lfs.github.com/) installed before cloning this repo.

To build/test changes to this package locally, you can:

- Open the test Unity project under the [`UnityUtil/`](./UnityUtil) subfolder.
    There you can run play/edit mode tests from the [Test Runner window](https://docs.unity3d.com/Packages/com.unity.test-framework@1.3/manual/workflow-run-test.html).
- Open the Visual Studio solution under the [`src/`](./src) subfolder.
    Building that solution will automatically re-export DLLs/PDBs to the above Unity project.
- Import the package locally in a test project. Simply create a new test project (or open an existing one),
    then import this package [from the local folder](https://docs.unity3d.com/Manual/upm-localpath.html) where you cloned it.

See the [Contributing docs](./CONTRIBUTING.md) for more info.

## License

[MIT](./LICENSE.md)
