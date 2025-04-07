# Unity.Extensions.Serilog

## Overview

This package contains Unity-specific logging extension methods for [Serilog](https://serilog.net/).

If you find this package useful, consider supporting its development!

<a href="https://www.buymeacoffee.com/shundra882n" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/v2/default-yellow.png" alt="Buy Me A Coffee" style="height: 40px !important;width: 145px !important;" ></a>

_Unity® and the Unity logo are trademarks of Unity Technologies._

## Installation

This package must be [installed from a Git URL](https://docs.unity3d.com/Manual/upm-ui-giturl.html).

Simply follow the instructions at that link, using the following URL:

```txt
https://github.com/DerploidEntertainment/UnityUtil.git?path=/UnityUtil/Assets/Unity.Extensions.Serilog#<branch>
```

Replace `<branch>` with one of the branch names described in the [UnityUtil](../../../../README.md#installing) installing docs (e.g., `unity6`).
You can ignore the steps about installing Odin Inspector in those docs.

This package has only been tested on Unity 6, but it _should_ work with earlier Unity versions as well.

## Usage

> [!NOTE]
> Consider installing the following packages as well to get the best developer experience with Serilog in Unity projects:
>
> - [Serilog.Enrichers.Unity](../../Serilog.Enrichers.Unity/Documentation~/README.md) to enrich log events with additional Unity data
> - [Serilog.Sinks.Unity](../../Serilog.Sinks.Unity/Documentation~/README.md) or [Serilog.Sinks.Unity3D](https://github.com/KuraiAndras/Serilog.Sinks.Unity3D) to add Unity as a Serilog sink
> - [Unity.Extensions.Logging](../../Unity.Extensions.Logging/Documentation~/README.md) for some other useful Microsoft.Extensions.Logging extension methods for Unity

### Set `SelfLog`

As a general best practice, consider enabling Serilog's `SelfLog` to write to Unity's Console like so:

```cs
SelfLog.Enable(UnityEngine.Debug.LogError);     // Or .LogWarning()
```

This will cause any exceptions _within_ the Serilog pipeline to be shown as warnings or errors in the Unity Console,
as these exceptions almost always indicate a configuration error that require immediate developer attention.

### Set `MinimumLevel` from Unity

Use code like the following to have your Serilog configuration's `MinimumLevel` match the current Unity `ILogger.filterLogType`:

```cs
var logger = new LoggerConfiguration()
    .MinimumLevel.IsUnityFilterLogType()
    // Configure other Serilog enrichers, sinks, etc.
    .CreateLogger()
```

By default, this package uses [`Debug.unityLogger.filterLogType`](https://docs.unity3d.com/ScriptReference/Logger-filterLogType.html),
but you can also provide a specific `ILogger` instance:

```cs
ILogger myLogger;
var logger = new LoggerConfiguration()
    .MinimumLevel.IsUnityFilterLogType(myLogger)
    // Configure other Serilog enrichers, sinks, etc.
    .CreateLogger()
```

### Destructure Unity objects for `context`

Unity log messages can optionally include a `context` argument.
You can pass a `GameObject` or `Component`-derived instance to the message and then,
when you click on the message in the Unity Console, Unity will highlight that object in the Hierarchy window.
This feature is very powerful when debugging in the Editor.
See the [`Debug.Log()`](https://docs.unity3d.com/ScriptReference/Debug.Log.html) manual page for more info.

The Unity sinks provided by [Serilog.Sinks.Unity](../../Serilog.Sinks.Unity/Documentation~/README.md) and [Serilog.Sinks.Unity3D](https://github.com/KuraiAndras/Serilog.Sinks.Unity3D) can both set the `context` to the value of a log property; however, _setting_ this log property is quite tricky.
Serilog tries to serialize objects of unknown type (including `UnityEngine.Object`-derived types) by calling their `ToString()` method;
therefore, you cannot simply pass a `UnityEngine.Object` instance to `logger.Information()` or the related Serilog log methods.
Sinks would ignore the log property's serialized string value and your log messages would not have the desired `context`
(clicking them in the Console would not highlight anything).
The same is true if you try to push a `UnityEngine.Object` instance to the Serilog `LogContext`.
Instead, you must use a custom [enricher](https://github.com/serilog/serilog/wiki/Enrichment)
or [destructuring policy](https://github.com/serilog/serilog/wiki/Structured-Data#preserving-object-structure)
to preserve the original, unserialized object instance.

If that all sounds daunting, you can just use the `UnityLogContextDestructuringPolicy` from this package.
Simply set up your Serilog logging configuration like so:

```cs
var logger = new LoggerConfiguration()
    .Destructure.UnityObjectContext()
    // Configure other Serilog enrichers, sinks, etc.
    .CreateLogger()
```

Then you can include a Unity object instance as `context` wherever you emit logs, like so:

```cs
class MyLoggingType : UnityEngine.Object    // MonoBehaviour, ScriptableObject, etc.
{
    private ILogger _logger;

    public void SomeMethod() {
        using (LogContext.PushProperty("@UnityLogContext", ValueTuple.Create(this)))
            _logger.Information("Hello, world!");
    }
}
```

The value of the context scope property must be a `ValueTuple<T>`, not the raw Unity object instance,
and the name of the property must be prefixed with the [_structure capturing operator_](https://messagetemplates.org/#operators), `'@'`.
This allows Serilog to "destructure" the property and extract the object instance wrapped in the `ValueTuple`,
without changing how all `UnityEngine.Object` instances are destructured/formatted in your app.

You can use any log property name with `Serilog.Sinks.Unity`, as long as that sink is configured to _expect_ that log property
(it can also be configured to _remove_ that log property, since it's value is presumably not needed other than for `context`).
For the older `Serilog.Sinks.Unity3D` sink, the log property _must_ be named `%_DO_NOT_USE_UNITY_ID_DO_NOT_USE%`, and it will not be removed from the log event.

Also consider using Serilog behind Microsoft.Extensions.Logging.Abstractions;
if so, you can reference [Unity.Extensions.Logging](../../Unity.Extensions.Serilog/Documentation~/README.md),
which provides a `UnityObjectLogger` that automatically adds the context log property,
making your logging code less verbose (no extra `using` statements):

```cs
class MyLoggingType : UnityEngine.Object    // MonoBehaviour, ScriptableObject, etc.
{
    private ILogger<MyLoggingType> _logger;

    private void Awake() {  // Or Start(), or wherever your type sets up its logger
        _logger = _loggerFactory.CreateLogger(this);
    }

    public void SomeMethod() {
        _logger.LogInformation("Hello, world!");    // Automatically adds log property for `this`, to be used as `context` in sinks
    }
}
```

For completeness, if you are using the Microsoft Extensions library but do _not_ want to use `UnityObjectLogger`,
then you can still add the Unity object for `context` like so:

```cs
class MyLoggingType : UnityEngine.Object    // MonoBehaviour, ScriptableObject, etc.
{
    private ILogger<MyLoggingType> _logger;

    private void Awake() {  // Or Start(), or wherever your type sets up its logger
        _logger = _loggerFactory.CreateLogger<MyLoggingType>();
    }

    public void SomeMethod() {
        using (_logger.BeginScope(new Dictionary<string, object> { { "@UnityLogContext", ValueTuple.Create(this) } }))
            _logger.LogInformation("Hello, world!");

        // Or, when using Serilog.Extensions.Logging 9.0.0+ ...
        using (_logger.BeginScope(("@UnityLogContext", ValueTuple.Create(this))))
            _logger.LogInformation("Hello, world!");
    }
}
```

## Legal

Copyright © 2025 - present Derploid® Entertainment, LLC. All rights reserved.

Unity® and the Unity logo are trademarks of Unity Technologies.
