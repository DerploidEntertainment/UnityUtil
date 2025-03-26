using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Serilog;
using Serilog.Enrichers.Unity;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.Unity;
using UnityEngine;
using MEL = Microsoft.Extensions.Logging;

namespace Unity.Extensions.Logging.Tests.Editor;

public class LoggerConfigurationExtensionsTests
{
    private readonly GameObject _loggingObject = new();

    private static void setupLogger(Mock<MEL.ILogger> logger, out Dictionary<string, object> passedScopePropDict)
    {
        LogLevel? passedLogLevel = null;
        EventId? passedEventId = null;
        string? passedMsgTemplate = null;

        Dictionary<string, object> scopePropDict = [];
        passedScopePropDict = scopePropDict;
        _ = logger.Setup(x => x.BeginScope(It.IsAny<Dictionary<string, object>>()))
            .Callback((Dictionary<string, object> scopeProps) => {
                foreach (KeyValuePair<string, object> kv in scopeProps)
                    scopePropDict.Add(kv.Key, kv.Value);
            });

        _ = logger.Setup(x =>
            x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            )
        ).Callback((LogLevel logLevel, EventId eventId, object state, Exception? _, object _) => {
            passedLogLevel = logLevel;
            passedEventId = eventId;
            passedMsgTemplate = state.ToString();
        });
    }

    [Test]
    [TestCase(LogLevel.Trace, LogType.Log)]
    [TestCase(LogLevel.Debug, LogType.Log)]
    [TestCase(LogLevel.Information, LogType.Log)]
    [TestCase(LogLevel.Warning, LogType.Warning)]
    [TestCase(LogLevel.Error, LogType.Error)]
    [TestCase(LogLevel.Critical, LogType.Error)]
    public void AddUnity_SimpleMessageTemplate_LogsExpectedLogType(LogLevel logLevel, LogType expectedLogType)
    {
        // ARRANGE
        const string MSG = "What up?";

        var unityLogger = new Mock<UnityEngine.ILogger>();
        ILoggerFactory loggerFactory = new LoggerFactory()
            .AddSerilog(
                new LoggerConfiguration()
                .Enrich.FromLogContext()
                .AddUnity(new JsonFormatter(), unityLogger.Object)
                .MinimumLevel.Verbose()   // Overwrites MinimumLevel from previous extension method calls
                .CreateLogger(),
                dispose: true
            );
        var unityObjectLogger = new UnityObjectLogger<GameObject>(loggerFactory, _loggingObject, new UnityObjectLoggerSettings());

        var eventId = new EventId(id: 5, nameof(AddUnity_SimpleMessageTemplate_LogsExpectedLogType));

        // ACT
        unityObjectLogger.Log(logLevel, eventId, MSG);

        // ASSERT
        unityLogger.Verify(x => x.Log(expectedLogType, "UnityEngine.GameObject", It.IsAny<string>(), _loggingObject), Times.Once());
    }

    [Test]
    public void AddUnity_MinimumLevel_CanBeSetByUnityFilterLogType([Values] bool setMinimumLevelFromUnityFilterLogType)
    {
        // ARRANGE

        var unityLogger = new Mock<UnityEngine.ILogger>();
        _ = unityLogger.SetupGet(x => x.filterLogType).Returns(LogType.Log);    // Lowest Unity log type

        ILoggerFactory loggerFactory = new LoggerFactory()
            .AddSerilog(
                new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Fatal() // Highest log level, ignores all but Fatal logs unless overriden by Unity
                .AddUnity(new JsonFormatter(), unityLogger.Object, setMinimumLevelFromUnityFilterLogType: setMinimumLevelFromUnityFilterLogType)
                .CreateLogger(),
                dispose: true
            );
        var unityObjectLogger = new UnityObjectLogger<GameObject>(loggerFactory, _loggingObject, new UnityObjectLoggerSettings());


        // ACT
        unityObjectLogger.LogInformation(new EventId(id: 5, nameof(AddUnity_MinimumLevel_CanBeSetByUnityFilterLogType)), "What up?");   // Lowest log level

        // ASSERT
        Times expectedTimes = setMinimumLevelFromUnityFilterLogType ? Times.Once() : Times.Never();
        unityLogger.Verify(x => x.Log(LogType.Log, "UnityEngine.GameObject", It.IsAny<string>(), _loggingObject), expectedTimes);
    }

    [Test]
    public void AddUnity_DefaultSettings_AddsExpectedLogProperties()
    {
        // ARRANGE
        const string MSG = "What up, {Name}?";

        string capturedMsg = "";
        var unityLogger = new Mock<UnityEngine.ILogger>();
        _ = unityLogger.Setup(x => x.Log(It.IsAny<LogType>(), It.IsAny<object>()))
            .Callback((LogType _, object message) => capturedMsg = message.ToString());

        ILoggerFactory loggerFactory = new LoggerFactory()
            .AddSerilog(
                new LoggerConfiguration()
                .Enrich.FromLogContext()
                .AddUnity(new JsonFormatter(), unityLogger.Object)
                .CreateLogger(),
                dispose: true
            );

        ILogger<GameObject> logger = loggerFactory.CreateLogger(_loggingObject);

        var eventId = new EventId(id: 5, nameof(AddUnity_DefaultSettings_AddsExpectedLogProperties));
        string expectedJsonRegex = getExpectedJsonRegex(
            @"What up, \{Name\}\?",
            eventId,
            messageLogPropertyRegexes: new Dictionary<string, object> {
                { "Name", "dawg" },
            },
            enchrichedLogPropertyRegexes: new Dictionary<string, object> {
                { "UnityFrameCount", Time.frameCount },
            }
        );

        // ACT
        logger.LogInformation(eventId, MSG, "dawg");

        // ASSERT
        unityLogger.Verify(x => x.Log(LogType.Log, "UnityEngine.GameObject", It.IsRegex(expectedJsonRegex), _loggingObject), Times.Once());
    }

    [Test]
    public void AddUnity_EmptySettings_AddsExpectedLogProperties()
    {
        // ARRANGE
        const string MSG = "What up?";

        string capturedMsg = "";
        var unityLogger = new Mock<UnityEngine.ILogger>();
        _ = unityLogger.Setup(x => x.Log(It.IsAny<LogType>(), It.IsAny<object>()))
            .Callback((LogType _, object message) => capturedMsg = message.ToString());

        ILoggerFactory loggerFactory = new LoggerFactory()
            .AddSerilog(
                new LoggerConfiguration()
                .Enrich.FromLogContext()
                .AddUnity(
                    new JsonFormatter(),
                    unityLogger.Object,
                    unityLogEnricherSettings: new() { WithFrameCount = false },
                    unitySinkSettings: new() { UnityTagLogProperty = null, UnityContextLogProperty = null }
                )
                .CreateLogger(),
                dispose: true
            );

        ILogger<GameObject> logger = loggerFactory.CreateLogger(
            _loggingObject,
            new() {
                EnrichWithHierarchyName = false,
                EnrichWithUnityContext = false
            }
        );

        var eventId = new EventId(id: 5, nameof(AddUnity_EmptySettings_AddsExpectedLogProperties));
        string expectedJsonRegex = getExpectedJsonRegex(
            @"What up\?",
            eventId,
            enchrichedLogPropertyRegexes: new Dictionary<string, object> {
                { "SourceContext", @"UnityEngine\.GameObject" },
            }
        );

        // ACT
        logger.LogInformation(eventId, MSG);

        // ASSERT
        unityLogger.Verify(x => x.Log(LogType.Log, It.IsRegex(expectedJsonRegex)), Times.Once());
    }

    [Test]
    public void AddUnity_CanSpecifySinkSettings([Values] bool useExplicitSinkSettings)
    {
        // ARRANGE
        var unityLogger = new Mock<UnityEngine.ILogger>();
        var unitySinkSettings = new UnitySinkSettings() {
            UnityContextLogProperty = null
        };
        ILoggerFactory loggerFactory = new LoggerFactory()
            .AddSerilog(
                new LoggerConfiguration()
                .Enrich.FromLogContext()
                .AddUnity(
                    new JsonFormatter(),
                    unityLogger.Object,
                    unitySinkSettings: useExplicitSinkSettings ? unitySinkSettings : null
                )
                .CreateLogger(),
                dispose: true
            );

        ILogger<GameObject> logger = loggerFactory.CreateLogger(_loggingObject);

        // ACT
        using (logger.BeginScope(new Dictionary<string, object> { { "UnityLogTag", "Cow say" } }))
            logger.LogInformation(new EventId(id: 5, nameof(AddUnity_CanSpecifySinkSettings)), "What up?");

        // ASSERT
        // Context log property still included in (not removed from) log event iff explicit sink settings were used
        if (useExplicitSinkSettings) {
            unityLogger.Verify(x =>
                x.Log(
                    LogType.Log,
                    "Cow say",
                    It.Is<string>(x =>
                        Regex.Match(x, @"""UnityLogContext"":\{.+\}[,}]").Success
                        && !x.Contains(@"""UnityLogTag"":""Cow say""")
                    )
                ), Times.Once()
            );
        }
        else {
            unityLogger.Verify(x =>
                x.Log(
                    LogType.Log,
                    "UnityEngine.GameObject",
                    It.Is<string>(x =>
                        !Regex.Match(x, @"""UnityLogContext"":\{.+\}[,}]").Success
                        && x.Contains(@"""UnityLogTag"":""Cow say""")
                    ),
                    _loggingObject
                ), Times.Once()
            );
        }
    }

    [Test]
    public void AddUnity_CanSpecifyEnricherSettings([Values] bool useExplicitEnricherSettings)
    {
        // ARRANGE
        var unityLogger = new Mock<UnityEngine.ILogger>();
        var unityLogEnricherSettings = new UnityLogEnricherSettings() {
            WithTimeAsDouble = true,
            TimeAsDoubleLogProperty = "Derp"
        };
        ILoggerFactory loggerFactory = new LoggerFactory()
            .AddSerilog(
                new LoggerConfiguration()
                .Enrich.FromLogContext()
                .AddUnity(
                    new JsonFormatter(),
                    unityLogger.Object,
                    unityLogEnricherSettings: useExplicitEnricherSettings ? unityLogEnricherSettings : null
                )
                .CreateLogger(),
                dispose: true
            );

        ILogger<GameObject> logger = loggerFactory.CreateLogger(_loggingObject);

        // ACT
        logger.LogInformation(new EventId(id: 5, nameof(AddUnity_CanSpecifyEnricherSettings)), "What up?");

        // ASSERT
        // Extra log property was included in log event iff explicit enricher settings were used
        unityLogger.Verify(x =>
            x.Log(
                LogType.Log,
                "UnityEngine.GameObject",
                It.Is<string>(x => Regex.Match(x, $@"""{unityLogEnricherSettings.TimeAsDoubleLogProperty}"":[0-9.]+[,}}]").Success == useExplicitEnricherSettings),
                _loggingObject
            ), Times.Once()
        );
    }

    private static string getExpectedJsonRegex(
        string msgTemplateRegex,
        EventId eventId,
        LogEventLevel logEventLevel = LogEventLevel.Information,
        string? tagRegex = null,
        Dictionary<string, object>? messageLogPropertyRegexes = null,
        Dictionary<string, object>? enchrichedLogPropertyRegexes = null
    )
    {
        string tagStr = tagRegex is null ? "" : tagRegex + ": ";
        string eventIdPropStr = $@"""EventId"":\{{""Id"":{eventId.Id},""Name"":""{eventId.Name}""\}}";
        string msgLogPropsStr = (messageLogPropertyRegexes?.Count ?? 0) == 0 ? "" :
            string.Join(",", messageLogPropertyRegexes.Select(kv => $"\"{kv.Key}\":{(kv.Value is string s ? $"\"{s}\"" : kv.Value)}"));
        string enrichedLogPropsStr = (enchrichedLogPropertyRegexes?.Count ?? 0) == 0 ? "" :
            string.Join(",", enchrichedLogPropertyRegexes.Select(kv => $"\"{kv.Key}\":{(kv.Value is string s ? $"\"{s}\"" : kv.Value)}"));
        return tagStr + @"\{" +
            $@"""Timestamp"":""{@"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{7}[-+]\d{2}:00"}""," +
            $@"""Level"":""{logEventLevel}""," +
            $@"""MessageTemplate"":""{msgTemplateRegex}""," +
            $@"""Properties"":\{{" +
                string.Join(',', new[] { msgLogPropsStr, eventIdPropStr, enrichedLogPropsStr }.Where(x => !string.IsNullOrEmpty(x))) +
            @"\}" +
        @"\}";
    }
}
