# Serilog.Sinks.Unity

![Serilog.Sinks.Unity](../../../docs-assets/serilog-sinks.png)

## Overview

This package contains a [Serilog](https://serilog.net/) sink that writes logs to the Unity Console window, optionally including `tag` strings and `context` objects.

This package is a "spiritual successor" to [KuraiAndras/Serilog.Sinks.Unity3D](https://github.com/KuraiAndras/Serilog.Sinks.Unity3D),
which has not been well maintained since December 2022, and lacks some important configuration options and documentation.

If you find this package useful, consider supporting its development!

<a href="https://www.buymeacoffee.com/shundra882n" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/v2/default-yellow.png" alt="Buy Me A Coffee" style="height: 40px !important;width: 145px !important;" ></a>

_Unity® and the Unity logo are trademarks of Unity Technologies._

## Installation

This package must be [installed from a Git URL](https://docs.unity3d.com/Manual/upm-ui-giturl.html).

Simply follow the instructions at that link, using the following URL:

```txt
https://github.com/DerploidEntertainment/UnityUtil.git?path=/UnityUtil/Assets/Serilog.Sinks.Unity#<branch>
```

Replace `<branch>` with one of the branch names described in the [UnityUtil](../../../../README.md#installing) installing docs (e.g., `unity6`).
You can ignore the steps about installing Odin Inspector in those docs.

This package has only been tested on Unity 6, but it _should_ work with earlier Unity versions as well.

## Usage

> [!NOTE]
> Consider installing the following packages as well to get the best developer experience with Serilog in Unity projects:
>
> - [Serilog.Enrichers.Unity](../Serilog.Enrichers.Unity/README.md) to enrich log events with additional Unity data
> - [Unity.Extensions.Logging](../Unity.Extensions.Logging/README.md) for some other useful Microsoft.Extensions.Logging extension methods for Unity
> - [Unity.Extensions.Serilog](../Unity.Extensions.Serilog/README.md) for some other useful Serilog extension methods for Unity

To use this sink with default settings, simply add the following to your app startup code:

```cs
var logger = new Serilog.LoggerConfiguration()
    .WriteTo.Unity()
    .CreateLogger();
```

Then, a log event like `logger.Information("Hello, world!")` will write `"Hello, world!"` out to the Unity Console and associated log files.

### Custom Unity logger

By default, this sink will write to [`UnityEngine.Debug.unityLogger`](https://docs.unity3d.com/ScriptReference/Debug-unityLogger.html).
You can have this sink use a custom `UnityEngine.ILogger` instance like so:

```cs
class MyCustomLogger : UnityEngine.ILogger
{
    // ...
}

var logger = new Serilog.LoggerConfiguration()
    .WriteTo.Unity(logger: new MyCustomLogger())
    .CreateLogger();
```

> [!WARNING]
> You are responsible for the [thread-safety](#thread-safety) of custom `ILogger` instances.

### Custom log formatting

By default, this sink will format log events using Serilog's built-in `MessageTemplateTextFormatter`
with the output template `"[{Level:u3}] {Message:lj}{NewLine}{Exception}"`.
You can have this sink use a custom output template or format provider like so:

```cs
var logger = new Serilog.LoggerConfiguration()
    .WriteTo.Unity(
        outputTemplate: "{Message:lj} is my message",
        formatProvider: CultureInfo.GetCulture("es-MX")
    )
    .CreateLogger();
```

You can also pass in a custom `ITextFormatter` instance.
For example, in production environments where log compactness and queryability are more important than readability,
you might use a formatter from [Serilog.Formatting.Compact](https://github.com/serilog/serilog-formatting-compact) like so:

```cs
var logger = new Serilog.LoggerConfiguration()
    .WriteTo.Unity(new CompactJsonFormatter())
    .CreateLogger();
```

### Custom Unity log `tag`

Unity log messages can optionally include a `tag` argument (see [`UnityEngine.ILogger.Log()`](https://docs.unity3d.com/ScriptReference/Logger.Log.html)).
This sink can set the `tag` to the value of any log property, like so:

```cs
var logger = new Serilog.LoggerConfiguration()
    .WriteTo.Unity(new UnitySinkSettings{ UnityTagLogProperty = "UnityLogTag" })
    .CreateLogger();

// In logging code...
logger.Information("Hello, my tag is '{UnityLogTag}'", "TestTag");
```

You might change the property name, e.g., to avoid collisions with property names added by other enrichers,
or if you wanted to save a few bytes with shorter property names in your production logs.

To keep the tag out of your log message templates, you could instead use Serilog's `LogContext` like so:

```cs
var logger = new Serilog.LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Unity(new UnitySinkSettings{ UnityTagLogProperty = "UnityLogTag" })
    .CreateLogger();

// In logging code...
using (LogContext.PushProperty("UnityLogTag", "TestTag"))
    logger.Information("Hello, world!");
```

> [!NOTE]
> When abstracting Serilog behind [Microsoft.Extensions.Logging](https://github.com/dotnet/runtime/tree/main/src/libraries/Microsoft.Extensions.Logging),
> you would use `ILogger.BeginScope()` rather than Serilog's `LogContext.PushProperty()` directly.

Unity suggests setting the `tag` value to the full name of the logging type.
Serilog already provides a `Logger.ForContext<T>()` method that automatically adds a log property named `SourceContext` with type `T`'s full name.
This sink sets `UnityTagLogProperty` to `SourceContext` by default, simplifying this common setup to just:

```cs
var rootLogger = new Serilog.LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Unity()
    .CreateLogger();

// In logging type's initialization code...
var logger = rootLogger.ForContext<MyLoggingType>()

// In logging code...
logger.Information("Hello, world!");
```

> [!NOTE]
> When abstracting Serilog behind Microsoft.Extensions.Logging,
> you would use `ILoggerFactory.CreateLogger<MyLoggingType>()` rather than Serilog's `Logger.ForContext<>()` directly.

If you don't want this sink to look for a `tag` string at all, then just set `UnityTagLogProperty` to `null`:

```cs
var logger = new Serilog.LoggerConfiguration()
    .WriteTo.Unity(new UnitySinkSettings{ UnityTagLogProperty = null })
    .CreateLogger();
```

When including the logging type's full name (or any string) as the Unity log message's `tag`, there's no reason to also render it elsewhere in the message
(e.g., when using a formatter like `JsonFormatter` that formats the message with _all_ log properties).
Therefore, this sink _removes_ the `tag` log property by default.

To disable this behavior and keep the `tag` log property in the formatted message, pass a settings object like so:

```cs
var logger = new Serilog.LoggerConfiguration()
    .WriteTo.Unity(new UnitySinkSettings{ RemoveUnityTagLogPropertyIfPresent = false })
    .CreateLogger();
```

### Custom Unity log `context`

Unity log messages can optionally include a `context` argument.
You can pass a `GameObject` or `Component`-derived instance to the message and then,
when you click on the message in the Unity Console, Unity will highlight that object in the Hierarchy window.
This feature is very powerful when debugging in the Editor.
See the [`Debug.Log()`](https://docs.unity3d.com/ScriptReference/Debug.Log.html) manual page for more info.

This sink can set the `context` to the value of any log property, like so (the default is `UnityLogContext`):

```cs
var logger = new Serilog.LoggerConfiguration()
    .WriteTo.Unity(new UnitySinkSettings{ UnityContextLogProperty = "UnityLogContext" })
    .CreateLogger();
```

You might change the property name, e.g., to avoid collisions with property names added by other enrichers,
or if you wanted to save a few bytes with shorter property names in your production logs.

However, _setting_ this log property is quite tricky.
Serilog tries to serialize objects of unknown type (including `UnityEngine.Object`-derived types) by calling their `ToString()` method;
therefore, you cannot simply pass a `UnityEngine.Object` instance to `logger.Information()` or the related Serilog log methods.
This sink would ignore the log property's serialized string value and your log messages would not have the desired `context`
(clicking them in the Console would not highlight anything).
The same is true if you try to push a `UnityEngine.Object` instance to the Serilog `LogContext`.
Instead, you must use a custom [enricher](https://github.com/serilog/serilog/wiki/Enrichment)
or [destructuring policy](https://github.com/serilog/serilog/wiki/Structured-Data#preserving-object-structure)
to preserve the original, unserialized object instance.

If that all sounds daunting, you can just use the `UnityLogContextDestructuringPolicy` from
[Unity.Extensions.Serilog](../Unity.Extensions.Serilog/README.md).
That policy is in a separate library in case you need to preserve Unity objects for other sinks too.
Also consider using Serilog behind Microsoft.Extensions.Logging.Abstractions;
if you do then you can reference [Unity.Extensions.Logging](../Unity.Extensions.Logging/README.md),
which provides a `UnityObjectLogger` that automatically adds the logging Unity object instance to log events.

If you don't want this sink to look for a `context` object at all, then just set `UnityContextLogProperty` to `null`:

```cs
var logger = new Serilog.LoggerConfiguration()
    .WriteTo.Unity(new UnitySinkSettings{ UnityContextLogProperty = null })
    .CreateLogger();
```

When including a `UnityEngine.Object` as the Unity log message's `context`, there's no reason to also render it elsewhere in the message
(e.g., when using a formatter like `JsonFormatter` that formats the message with _all_ log properties).
Rendering the object elsewhere would probably just serialize it to a type name anyway, as described above.
Therefore, this sink _removes_ the `context` log property by default.

To disable this behavior and keep the `context` log property in the formatted message, pass a settings object like so:

```cs
var logger = new Serilog.LoggerConfiguration()
    .WriteTo.Unity(new UnitySinkSettings{ RemoveUnityContextLogPropertyIfPresent = false })
    .CreateLogger();
```

### Sink log level

As with other Serilog sinks, you can configure the minimum level for events passed through the sink:

```cs
var logger = new Serilog.LoggerConfiguration()
    .WriteTo.Unity(restrictedToMinimumLevel: LogEventLevel.Verbose)
    .CreateLogger();
```

Or, provide a switch allowing the pass-through minimum level to be changed at runtime:

```cs
LoggingLevelSwitch levelSwitch;
var logger = new Serilog.LoggerConfiguration()
    .WriteTo.Unity(levelSwitch: levelSwitch)
    .CreateLogger();
```

### Set `SelfLog`

As a general best practice, consider enabling Serilog's `SelfLog` to write to Unity's Console like so:

```cs
SelfLog.Enable(UnityEngine.Debug.LogError);     // Or .LogWarning()
```

This will cause any exceptions _within_ the Serilog pipeline to be shown as warnings or errors in the Unity Console,
as these exceptions almost always indicate a configuration error that require immediate developer attention.

### `restrictedToMinimumLevel` and `levelSwitch`

Like any other sink...

### Rich text

By default (or when using any string template-based formatter similar to `MessageTemplateTextFormatter`),
message templates emitted through this sink may contain [Rich Text](https://docs.unity3d.com/Manual/StyledText.html) markup.

For example, this setup will show the log event level of all messages in **bold**:

```cs
var logger = new Serilog.LoggerConfiguration()
    .WriteTo.Unity(outputTemplate: "<b>[{Level:u3}]</b> {Message:lj}{NewLine}{Exception}"),
    .CreateLogger();
```

When using formatters that are _not_ string template-based, like `JsonFormatter`, these rich text tags are mostly irrelevant.
You _could_ include them in the values of log properties, but that would most likely add unnecessary bytes to your logs in production.

### Thread safety

This sink uses Unity's `Debug.unityLogger` by default, which is thread-safe.
You can thus emit log events (i.e., call `logger.Information()` and related methods) from any thread without synchronization.

However, _you_ are responsible for thread safety in the following cases:

- You use a [custom `UnityEngine.ILogger` instance](#custom-unity-logger)
- Your sink relies on other Serilog objects (enrichers, formatters, etc.) that are not thread-safe.
    For example, you might have an enricher that adds values from the Unity runtime, like the current frame or scene time,
    which can only be accessed from the Unity main thread.

These cases _may_ mean that:

- You can only emit log events from the Unity main thread,
- You must synchronize with the main thread before logging,
    (e.g., with a call to [`Awaitable.MainThreadAsync()`](https://docs.unity3d.com/ScriptReference/Awaitable.MainThreadAsync.html) on Unity 6+),
- Or, your `UnityEngine.ILogger` implementation must internally synchronize with the main thread
    (e.g., with a library like [UnityMainThreadDispatcher](https://github.com/PimDeWitte/UnityMainThreadDispatcher) or [UniTask](https://github.com/Cysharp/UniTask)).

## Legal

Copyright © 2025 - present Derploid® Entertainment, LLC. All rights reserved.

Unity® and the Unity logo are trademarks of Unity Technologies.
