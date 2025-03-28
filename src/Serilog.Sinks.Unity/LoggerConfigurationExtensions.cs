using System;
using Serilog.Configuration;
using Serilog.Formatting;
using UE = UnityEngine;

namespace Serilog.Sinks.Unity;

/// <summary>
/// Extends <see cref="LoggerConfiguration"/> with methods to add Unity sinks.
/// </summary>
public static class LoggerConfigurationExtensions
{
    /// <summary>
    /// Write log events to the Unity Console.
    /// </summary>
    /// <param name="loggerSinkConfiguration">Logger sink configuration.</param>
    /// <param name="textFormatter"><inheritdoc cref="UnitySink(ITextFormatter, UE.ILogger, UnitySinkSettings)" path="/param[@name='textFormatter']"/></param>
    /// <param name="logger"><inheritdoc cref="UnitySink(ITextFormatter, UE.ILogger, UnitySinkSettings)" path="/param[@name='logger']"/></param>
    /// <param name="unitySinkSettings"><inheritdoc cref="UnitySink(ITextFormatter, UE.ILogger, UnitySinkSettings)" path="/param[@name='unitySinkSettings']"/></param>
    /// <returns>Configuration object allowing method chaining.</returns>
    public static LoggerConfiguration Unity(
        this LoggerSinkConfiguration loggerSinkConfiguration,
        ITextFormatter textFormatter,
        UE.ILogger? logger = null,
        UnitySinkSettings? unitySinkSettings = null
    ) =>
        loggerSinkConfiguration is null ? throw new ArgumentNullException(nameof(loggerSinkConfiguration))
        : loggerSinkConfiguration.Sink(new UnitySink(textFormatter, logger, unitySinkSettings));
}
