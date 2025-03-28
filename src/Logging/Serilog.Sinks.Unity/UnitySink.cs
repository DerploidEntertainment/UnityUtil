using System;
using System.IO;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Json;
using UnityEngine;
using UE = UnityEngine;

namespace Serilog.Sinks.Unity;

/// <summary>
/// Writes log events to the Unity Console.
/// </summary>
/// <param name="textFormatter">
/// A formatter, such as <see cref="JsonFormatter"/>, to convert the log events into text for the Unity Console.
/// </param>
/// <param name="logger">
/// The Unity <see cref="UE.ILogger"/> backing this sink.
/// If <see langword="null"/> then the default <see cref="Debug.unityLogger"/> will be used.
/// </param>
/// <param name="unitySinkSettings"><inheritdoc cref="UnitySinkSettings" path="/summary"/></param>
public class UnitySink(ITextFormatter textFormatter, UE.ILogger? logger = null, UnitySinkSettings? unitySinkSettings = null) : ILogEventSink
{
    private readonly ITextFormatter _textFormatter = textFormatter;
    private readonly UE.ILogger _logger = logger ?? Debug.unityLogger;
    private readonly UnitySinkSettings _unitySinkSettings = unitySinkSettings ?? new UnitySinkSettings();

    /// <inheritdoc/>
    public void Emit(LogEvent logEvent)
    {
        LogType logType = logEvent.Level switch {
            LogEventLevel.Verbose or LogEventLevel.Debug or LogEventLevel.Information => LogType.Log,
            LogEventLevel.Warning => LogType.Warning,
            LogEventLevel.Error or LogEventLevel.Fatal => LogType.Error,
            _ => throw new InvalidOperationException($"Unknown {nameof(LogEventLevel)}: {logEvent.Level}"),
        };

        UE.Object? context = null;
        if (_unitySinkSettings.UnityContextLogProperty is not null) {
            context = logEvent.Properties.TryGetValue(_unitySinkSettings.UnityContextLogProperty, out LogEventPropertyValue contextPropertyValue)
                ? (contextPropertyValue as ScalarValue)?.Value as UE.Object
                : null;
            if (_unitySinkSettings.RemoveUnityContextLogPropertyIfPresent)
                logEvent.RemovePropertyIfPresent(_unitySinkSettings.UnityContextLogProperty);    // No need to duplicate property between Unity log context and message
        }

        // Try to find an appropriate tag for the Unity log from log properties.
        // No need to duplicate that value between the tag and logged properties, so remove the property if found.
        string? tag = null;
        if (_unitySinkSettings.UnityTagLogProperty is not null) {
            tag = logEvent.Properties.TryGetValue(_unitySinkSettings.UnityTagLogProperty, out LogEventPropertyValue propertyValue)
                ? (propertyValue as ScalarValue)?.Value as string
                : null;
            if (_unitySinkSettings.RemoveUnityTagLogPropertyIfPresent)
                logEvent.RemovePropertyIfPresent(_unitySinkSettings.UnityTagLogProperty);
        }

        // Generate formatted message last, to account for adjustments to log event properties
        using var stringWriter = new StringWriter();
        _textFormatter.Format(logEvent, stringWriter);
        string message = stringWriter.ToString();

        // According to these two Unity Discussions moderators, Debug.Log() is actually threadsafe
        // https://discussions.unity.com/t/debugging-from-threads/595708/3
        // https://discussions.unity.com/t/thread-safety-and-debug-log/106140/2
        if (context == null && tag is null) // `UnityEngine.Object` overloads `==` operator and treats null differently, so we need to use that
            _logger.Log(logType, message);

        else if (context == null && tag is not null)
            _logger.Log(logType, tag, message);

        else if (context != null && tag is null)
            _logger.Log(logType, (object)message, context);

        else
            _logger.Log(logType, tag, message, context);
    }
}
