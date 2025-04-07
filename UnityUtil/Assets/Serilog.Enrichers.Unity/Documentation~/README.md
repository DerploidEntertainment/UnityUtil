# Serilog.Enrichers.Unity

![Serilog.Enrichers.Unity](../../../../docs-assets/serilog-enrichers.png)

## Overview

This package contains a [Serilog](https://serilog.net/) enricher that dynamically adds Unity data to your log events.

If you find this package useful, consider supporting its development!

<a href="https://www.buymeacoffee.com/shundra882n" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/v2/default-yellow.png" alt="Buy Me A Coffee" style="height: 40px !important;width: 145px !important;" ></a>

_Unity® and the Unity logo are trademarks of Unity Technologies._

## Installation

This package must be [installed from a Git URL](https://docs.unity3d.com/Manual/upm-ui-giturl.html).

Simply follow the instructions at that link, using the following URL:

```txt
https://github.com/DerploidEntertainment/UnityUtil.git?path=/UnityUtil/Assets/Serilog.Enrichers.Unity#<branch>
```

Replace `<branch>` with one of the branch names described in the [UnityUtil](../../../../README.md#installing) installing docs (e.g., `unity6`).
You can ignore the steps about installing Odin Inspector in those docs.

This package has only been tested on Unity 6, but it _should_ work with earlier Unity versions as well.

## Usage

> [!NOTE]
> Consider installing the following packages as well to get the best developer experience with Serilog in Unity projects:
>
> - [Serilog.Sinks.Unity](../../Serilog.Sinks.Unity/Documentation~/README.md) or [Serilog.Sinks.Unity3D](https://github.com/KuraiAndras/Serilog.Sinks.Unity3D) to add Unity as a Serilog sink
> - [Unity.Extensions.Serilog](../../Unity.Extensions.Serilog/Documentation~/README.md) for some other useful Serilog extension methods for Unity
> - [Unity.Extensions.Logging](../../Unity.Extensions.Logging/Documentation~/README.md) for some other useful Microsoft.Extensions.Logging extension methods for Unity

To use this enricher with default settings, simply add code like the following:

```cs
var logger = new Serilog.LoggerConfiguration()
    .Enrich.WithUnityData()
    // Configure other Serilog enrichers, sinks, etc.
    .CreateLogger();
```

To configure which log properties are added by this enricher, and the names of those properties,
you can provide an explicit `UnityLogEnricherSettings` object, like so:

```cs
var logger = new Serilog.LoggerConfiguration()
    .Enrich.WithUnityData(new UnityLogEnricherSettings {
        WithUnscaledTime = true,
        UnscaledTimeLogProperty = "UT",
    })
    // Configure other Serilog enrichers, sinks, etc.
    .CreateLogger();
```

This would add a `LogEventProperty` to your `LogEvent`s with name `"UT"`
and value equal to Unity's [`Time.unscaledTime`](https://docs.unity3d.com/ScriptReference/Time-unscaledTime.html) at the time of the log.

You might change the property names, e.g., if you wanted to save a few bytes with shorter property names in your production logs.

> [!NOTE]
> You can easily add additional, unchanging properties from Unity not covered by this enricher
> using Serilog's built-in `Enrich.WithProperty()` method. For example:

```cs
var logger = new Serilog.LoggerConfiguration()
    .Enrich.WithProperty("Platform", Application.platform)
    // ...
    .CreateLogger();
```

> [!WARNING]
> The above approach must only be used for values that do not change during the application's lifetime.
> For example, if you added Unity's [`Time`](https://docs.unity3d.com/ScriptReference/Time.html) properties
> in this way then every log would show time values from when the logger configuration was defined (typically app start),
> not from when an actual log event occurs!

Read the XML docs on [`UnityLogEnricher`](../../../../src/Logging/Serilog.Enrichers.Unity/UnityLogEnricher.cs) for a list of all log properties
that this enricher can add.

## Legal

Copyright © 2024 - present Derploid® Entertainment, LLC. All rights reserved.

Unity® and the Unity logo are trademarks of Unity Technologies.
