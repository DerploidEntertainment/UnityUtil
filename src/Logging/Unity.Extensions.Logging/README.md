# Unity.Extensions.Logging

## Overview

This package contains Unity-specific extension methods for logging with [Microsoft.Extensions.Logging](https://www.nuget.org/packages/microsoft.extensions.logging/).

If you find this package useful, consider supporting its development!

<a href="https://www.buymeacoffee.com/shundra882n" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/v2/default-yellow.png" alt="Buy Me A Coffee" style="height: 40px !important;width: 145px !important;" ></a>

_Unity® and the Unity logo are trademarks of Unity Technologies._

## Installation

This package must be [installed from a Git URL](https://docs.unity3d.com/Manual/upm-ui-giturl.html).

Simply follow the instructions at that link, using the following URL:

```txt
https://github.com/DerploidEntertainment/UnityUtil.git?path=/UnityUtil/Assets/Unity.Extensions.Logging#<branch>
```

Replace `<branch>` with one of the branch names described in the [UnityUtil](../../../../README.md#installing) installing docs (e.g., `unity6`).
You can ignore the steps about installing Odin Inspector in those docs.

This package has only been tested on Unity 6, but it _should_ work with earlier Unity versions as well.

## Usage

This package makes it easy to associate your log messages with info about the logging `UnityEngine.Object`-derived instances
(`MonoBehaviour`s, `ScriptableObject`s, etc.), which logging providers can use when logging to the Unity Console.

All of the examples below assume that your logging code has access to an `ILoggerFactory` instance, via dependency injection or a static singleton, e.g.

### Unity `context`

This package's most common use case is for `UnityEngine.Object`-derived types that want to include their instance in the log.
Logging providers can then use this instance as the `context` parameter when writing to the Unity Console,
ensuring that when you click on log messages in the Unity Console, the logging object is highlighted in the Editor.
See Unity's [`Debug.Log()`](https://docs.unity3d.com/ScriptReference/Debug.Log.html) manual page for more explanation of the `context` parameter.

The following example creates an `ILogger` instance that will include the logging type instance as a
[scope property](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging#log-scopes) on all logs:

```cs
class MyLoggingType : UnityEngine.Object    // MonoBehaviour, ScriptableObject, etc.
{
    private ILogger<MyLoggingType> _logger;

    private void Awake() {  // Or Start(), or wherever your type sets up its logger
        _logger = _loggerFactory.CreateLogger(this);
    }

    public void SomeMethod() {
        _logger.LogInformation("Hello, world!");    // Automatically attaches `this` to the log message
    }
}
```

The name of the scope property is `UnityLogContext` by default.
If you want a different property name then you can change it with a settings object, like so:

```cs
_logger = _loggerFactory.CreateLogger(this, new UnityObjectLoggerSettings {
    UnityContextLogProperty = "Context",
});
```

You might change the property name, e.g., to avoid collisions with property names added by other enrichers,
or if you wanted to save a few bytes with shorter property names in your production logs.

If you don't want to include the scope property at all, then you can disable it like so:

```cs
_logger = _loggerFactory.CreateLogger(this, new UnityObjectLoggerSettings {
    AddUnityContext = false,
});
```

> [!NOTE]
> The value of the context scope property is technically a `ValueTuple<UnityEngine.Object>`,
> and the name of the property is technically prefixed with the [_structure capturing operator_](https://messagetemplates.org/#operators), `'@'`.
> This allows logging providers that follow the [Message Templates](https://messagetemplates.org) schema
> (which includes most modern .NET logging libraries, including Serilog and NLog)
> to "destructure" the property and extract the object instance wrapped in the `ValueTuple`,
> without changing how all `UnityEngine.Object` instances are destructured/formatted.

### Unity hierarchy name

In addition to (or instead of) including an object _instance_ in log messages, you might want to include an object's "hierarchy name".

For `GameObject` and `Component`-derived instances, the hierarchy name is
the name of the object's transform and all parent transforms, separated by a delimiter.
For all other `UnityEngine.Object` instances, the hierarchy name is simply the value of the object's `name` property.
This information is useful in logs from built Unity players where you can't click on a log in the Editor,
but still want to know _which_ instance of a type generated a log.

As an example, suppose that you have the following `GameObject` hierarchy:

![Example Unity object hierarcy: "object" under "parent" under "grandparent"](../../../../docs-assets/example-unity-hierarchy.png)

`object's` default hierarchy name is then `grandparent>parent>object`.

The following example creates an `ILogger` instance that will include the logging instance's hierarchy name as a
[scope property](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging#log-scopes) on all logs:

```cs
class MyLoggingType : UnityEngine.Object    // MonoBehaviour, ScriptableObject, etc.
{
    private ILogger<MyLoggingType> _logger;

    private void Awake() {  // Or Start(), or wherever your type sets up its logger
        _logger = _loggerFactory.CreateLogger(this, new UnityObjectLoggerSettings {
            AddHierarchyName = true,
        });
    }

    public void SomeMethod() {
        _logger.LogInformation("Hello, world!");    // Automatically attaches heirarchy name of `this` to the log message
    }
}
```

The name of the scope property is `UnityHierarchyName` by default.
You might want to change the property name, e.g., to avoid collisions with property names added by other libraries,
or if you wanted to save a few bytes with shorter property names in your production logs.
You can change the property name like so:

```cs
_logger = _loggerFactory.CreateLogger(this, new UnityObjectLoggerSettings {
    AddHierarchyName = true,
    HierarchyNameLogProperty = "Name",
});
```

To change the separator between transform names to something other than `>`, set the `ParentNameSeparator` property:

```cs
_logger = _loggerFactory.CreateLogger(this, new UnityObjectLoggerSettings {
    AddHierarchyName = true,
    ParentNameSeparator = "-",
});
```

Note that computing the hierarchy name requires walking the logging object's transform hierarchy on every log event,
which could get expensive for deeply childed objects.
That is why you must "opt in" to including the hierarchy name by setting `AddHierarchyName` to true.

As an additional optimization, this package assumes that object hierarches do not change, so that the hierarchy name can be cached.
If this is not true, and object's parent(s) can change, then indicate that its hierarchy is not static with code like this:

```cs
_logger = _loggerFactory.CreateLogger(this, new UnityObjectLoggerSettings {
    AddHierarchyName = true,
    HasStaticHierarchy = false,
});
```

## Notes for specific logging providers

### Serilog

When using the [Serilog logging provider](https://github.com/serilog/serilog-extensions-logging),
consider installing the following packages as well to get the best developer experience in Unity projects:

- [Serilog.Enrichers.Unity](../Serilog.Enrichers.Unity/README.md) to enrich log events with additional Unity data
- [Serilog.Sinks.Unity](../Serilog.Sinks.Unity/README.md) or [Serilog.Sinks.Unity3D](https://github.com/KuraiAndras/Serilog.Sinks.Unity3D) to add Unity as a Serilog sink
- [Unity.Extensions.Serilog](../Unity.Extensions.Serilog/README.md) for some other useful Serilog extension methods for Unity,
    including a destructuring policy needed to preserve the object `context` instance added by this (or any other) package.

## Legal

Copyright © 2025 - present Derploid® Entertainment, LLC. All rights reserved.

Unity® and the Unity logo are trademarks of Unity Technologies.
