using System;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;
using UE = UnityEngine;

namespace Serilog.Sinks.Unity;

/// <summary>
/// Extends <see cref="LoggerSinkConfiguration"/> with Unity sinks.
/// </summary>
public static class LoggerSinkConfigurationExtensions
{
    /// <summary>
    /// Write log events to the Unity Console.
    /// </summary>
    /// <param name="loggerSinkConfiguration">Logger sink configuration.</param>
    /// <param name="textFormatter"><inheritdoc cref="UnitySink(ITextFormatter, UE.ILogger, UnitySinkSettings)" path="/param[@name='textFormatter']"/></param>
    /// <param name="logger"><inheritdoc cref="UnitySink(ITextFormatter, UE.ILogger, UnitySinkSettings)" path="/param[@name='logger']"/></param>
    /// <param name="unitySinkSettings"><inheritdoc cref="UnitySink(ITextFormatter, UE.ILogger, UnitySinkSettings)" path="/param[@name='unitySinkSettings']"/></param>
    /// <param name="restrictedToMinimumLevel"><inheritdoc cref="LoggerSinkConfiguration.Sink(ILogEventSink, LogEventLevel, LoggingLevelSwitch)" path="/param[@name='restrictedToMinimumLevel']"/></param>
    /// <param name="levelSwitch"><inheritdoc cref="LoggerSinkConfiguration.Sink(ILogEventSink, LogEventLevel, LoggingLevelSwitch)" path="/param[@name='levelSwitch']"/></param>
    /// <returns>Configuration object allowing method chaining.</returns>
    public static LoggerConfiguration Unity(
        this LoggerSinkConfiguration loggerSinkConfiguration,
        ITextFormatter textFormatter,
        UE.ILogger? logger = null,
        UnitySinkSettings? unitySinkSettings = null,
        LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose,
        LoggingLevelSwitch? levelSwitch = null
    ) =>
        loggerSinkConfiguration is null
            ? throw new ArgumentNullException(nameof(loggerSinkConfiguration))
            : loggerSinkConfiguration.Sink(new UnitySink(textFormatter, logger, unitySinkSettings), restrictedToMinimumLevel, levelSwitch);

    /// <summary>
    /// <inheritdoc cref="Unity(LoggerSinkConfiguration, ITextFormatter, UE.ILogger?, UnitySinkSettings?, LogEventLevel, LoggingLevelSwitch?)" path="/summary"/>
    /// </summary>
    /// <param name="loggerSinkConfiguration">Logger sink configuration.</param>
    /// <param name="outputTemplate"><inheritdoc cref="MessageTemplateTextFormatter(string, IFormatProvider)" path="/param[@name='outputTemplate']"/></param>
    /// <param name="formatProvider"><inheritdoc cref="MessageTemplateTextFormatter(string, IFormatProvider)" path="/param[@name='formatProvider']"/></param>
    /// <param name="logger"><inheritdoc cref="UnitySink(ITextFormatter, UE.ILogger, UnitySinkSettings)" path="/param[@name='logger']"/></param>
    /// <param name="unitySinkSettings"><inheritdoc cref="UnitySink(ITextFormatter, UE.ILogger, UnitySinkSettings)" path="/param[@name='unitySinkSettings']"/></param>
    /// <param name="restrictedToMinimumLevel"><inheritdoc cref="LoggerSinkConfiguration.Sink(ILogEventSink, LogEventLevel, LoggingLevelSwitch)" path="/param[@name='restrictedToMinimumLevel']"/></param>
    /// <param name="levelSwitch"><inheritdoc cref="LoggerSinkConfiguration.Sink(ILogEventSink, LogEventLevel, LoggingLevelSwitch)" path="/param[@name='levelSwitch']"/></param>
    /// <returns>
    /// <inheritdoc cref="Unity(LoggerSinkConfiguration, ITextFormatter, UE.ILogger?, UnitySinkSettings?, LogEventLevel, LoggingLevelSwitch?)" path="/returns"/>
    /// </returns>
    public static LoggerConfiguration Unity(
        this LoggerSinkConfiguration loggerSinkConfiguration,
        string? outputTemplate = "[{Level:u3}] {Message:lj}{NewLine}{Exception}",
        IFormatProvider? formatProvider = null,
        UE.ILogger? logger = null,
        UnitySinkSettings? unitySinkSettings = null,
        LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose,
        LoggingLevelSwitch? levelSwitch = null
    )
    {
        if (loggerSinkConfiguration is null)
            throw new ArgumentNullException(nameof(loggerSinkConfiguration));

        // In newer Serilog versions, the IFormatProvider param arg is clearly null by default
        var messageTemplateTextFormatter = new MessageTemplateTextFormatter(outputTemplate, formatProvider);

        return loggerSinkConfiguration.Sink(new UnitySink(messageTemplateTextFormatter, logger, unitySinkSettings), restrictedToMinimumLevel, levelSwitch);
    }
}
