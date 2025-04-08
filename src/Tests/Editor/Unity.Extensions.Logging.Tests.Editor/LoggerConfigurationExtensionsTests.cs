using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Moq;
using NUnit.Framework;
using Serilog;
using Serilog.Core;
using Serilog.Enrichers.Unity;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.Unity;
using Unity.Extensions.Serilog;
using UnityEngine;
using S = Serilog.Core;

namespace Unity.Extensions.Logging.Tests.Editor;

public class LoggerConfigurationExtensionsTests
{
    [Test]
    [TestCase(LogEventLevel.Verbose, LogType.Log)]
    [TestCase(LogEventLevel.Debug, LogType.Log)]
    [TestCase(LogEventLevel.Information, LogType.Log)]
    [TestCase(LogEventLevel.Warning, LogType.Warning)]
    [TestCase(LogEventLevel.Error, LogType.Error)]
    [TestCase(LogEventLevel.Fatal, LogType.Error)]
    public void AddUnity_SimpleMessageTemplate_LogsExpectedLogType(LogEventLevel logLevel, LogType expectedLogType)
    {
        // ARRANGE
        const string MSG = "What up?";

        var unityLogger = new Mock<UnityEngine.ILogger>();
        using S.Logger logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Is(logLevel)
            .WriteTo.Unity(new JsonFormatter(), unityLogger.Object)
            .CreateLogger();

        // ACT
        logger.Write(logLevel, MSG);

        // ASSERT
        unityLogger.Verify(x => x.Log(expectedLogType, It.IsAny<object>()), Times.Once());
    }

    [Test]
    public void AddUnity_MinimumLevel_CanBeSetByUnityFilterLogType([Values] bool setMinimumLevelFromUnityFilterLogType)
    {
        // ARRANGE

        var unityLogger = new Mock<UnityEngine.ILogger>();
        _ = unityLogger.SetupGet(x => x.filterLogType).Returns(LogType.Log);    // Lowest Unity log type

        LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Fatal()   // Highest log level, ignores all but Fatal logs unless overriden by Unity
            .WriteTo.Unity(new JsonFormatter(), unityLogger.Object);
        if (setMinimumLevelFromUnityFilterLogType)
            loggerConfiguration = loggerConfiguration.MinimumLevel.IsUnityFilterLogType(unityLogger.Object);
        using S.Logger logger = loggerConfiguration.CreateLogger();

        // ACT
        logger.Information("What up?");   // Lowest log level

        // ASSERT
        Times expectedTimes = setMinimumLevelFromUnityFilterLogType ? Times.Once() : Times.Never();
        unityLogger.Verify(x => x.Log(LogType.Log, It.IsAny<object>()), expectedTimes);
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

        using S.Logger logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithUnityData()
            .WriteTo.Unity(new JsonFormatter(), unityLogger.Object)
            .CreateLogger();

        string expectedJsonRegex = getExpectedJsonRegex(
            @"What up, \{Name\}\?",
            logPropertyRegexes: new Dictionary<string, object> {
                { "Name", "dawg" },
                { "UnityFrameCount", Time.frameCount },
            }
        );

        // ACT
        logger.Information(MSG, "dawg");

        // ASSERT
        unityLogger.Verify(x => x.Log(LogType.Log, It.IsRegex(expectedJsonRegex)), Times.Once());
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

        using S.Logger logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithUnityData(new() { WithFrameCount = false })
            .WriteTo.Unity(new JsonFormatter(), unityLogger.Object, new() {
                UnityTagLogProperty = null,
                UnityContextLogProperty = null
            })
            .CreateLogger();

        string expectedJsonRegex = getExpectedJsonRegex(@"What up\?");

        // ACT
        logger.Information(MSG);

        // ASSERT
        unityLogger.Verify(x => x.Log(LogType.Log, It.IsRegex(expectedJsonRegex)), Times.Once());
    }

    [Test]
    public void AddUnity_CanDisableUnityLogSink([Values] bool useUnitySink)
    {
        // ARRANGE
        var unityLogger = new Mock<UnityEngine.ILogger>();
        var unitySinkSettings = new UnitySinkSettings();

        var loggerConfiguration = new LoggerConfiguration();
        if (useUnitySink)
            loggerConfiguration = loggerConfiguration.WriteTo.Unity(new JsonFormatter(), unityLogger.Object, unitySinkSettings);
        global::Serilog.ILogger loggerWithTag = loggerConfiguration
            .Enrich.FromLogContext()
            .Enrich.WithUnityData()
            .CreateLogger()
            .ForContext<GameObject>()
            .ForContext("UnityLogTag", "Cow say");

        // ACT
        loggerWithTag.Information("What up?");

        // ASSERT
        // Unity logging occurred iff Unity sink was used
        if (useUnitySink) {
            unityLogger.Verify(x =>
                x.Log(
                    LogType.Log,
                    "UnityEngine.GameObject",
                    It.Is<string>(x =>
                        x.Contains(@"""UnityLogTag"":""Cow say""")
                        && !x.Contains(@$"""{Constants.SourceContextPropertyName}"":""UnityEngine.GameObject""")
                    )
                ), Times.Once()
            );
        }
        else {
            unityLogger.Verify(x => x.Log(It.IsAny<LogType>(), It.IsAny<object>()), Times.Never());
            unityLogger.Verify(x => x.Log(It.IsAny<LogType>(), It.IsAny<object>(), It.IsAny<Object>()), Times.Never());
            unityLogger.Verify(x => x.Log(It.IsAny<LogType>(), It.IsAny<string>(), It.IsAny<object>()), Times.Never());
            unityLogger.Verify(x => x.Log(It.IsAny<LogType>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<Object>()), Times.Never());
            unityLogger.Verify(x => x.Log(It.IsAny<object>()), Times.Never());
            unityLogger.Verify(x => x.Log(It.IsAny<string>(), It.IsAny<object>()), Times.Never());
            unityLogger.Verify(x => x.Log(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<Object>()), Times.Never());
        }
    }

    [Test]
    public void AddUnity_CanDisableUnityLogEnricher([Values] bool useUnityEnricher)
    {
        // ARRANGE
        var unityLogger = new Mock<UnityEngine.ILogger>();
        var unityLogEnricherSettings = new UnityLogEnricherSettings();

        var loggerConfiguration = new LoggerConfiguration();
        if (useUnityEnricher)
            loggerConfiguration = loggerConfiguration.Enrich.WithUnityData(unityLogEnricherSettings);
        using S.Logger logger = loggerConfiguration
            .Enrich.FromLogContext()
            .WriteTo.Unity(new JsonFormatter(), unityLogger.Object)
            .CreateLogger();

        // ACT
        logger.Information("What up?");

        // ASSERT
        // Extra log property was included in log event iff enricher was used
        unityLogger.Verify(x =>
            x.Log(
                LogType.Log,
                It.Is<string>(x => Regex.Match(x, $@"""{unityLogEnricherSettings.FrameCountLogProperty}"":\d+[,}}]").Success == useUnityEnricher)
            ), Times.Once()
        );
    }

    [Test]
    public void AddUnity_CanSpecifySinkSettings([Values] bool useExplicitSinkSettings)
    {
        // ARRANGE
        var unityLogger = new Mock<UnityEngine.ILogger>();
        var unitySinkSettings = new UnitySinkSettings() {
            UnityContextLogProperty = null,
            UnityTagLogProperty = null,
        };

        global::Serilog.ILogger loggerWithTag = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Unity(new JsonFormatter(), unityLogger.Object, useExplicitSinkSettings ? unitySinkSettings : null)
            .CreateLogger()
            .ForContext<GameObject>()
            .ForContext("UnityLogTag", "Cow say");

        // ACT
        loggerWithTag.Information("What up?");

        // ASSERT
        // Context log property still included in (not removed from) log event iff explicit sink settings were used
        if (useExplicitSinkSettings) {
            unityLogger.Verify(x =>
                x.Log(
                    LogType.Log,
                    It.Is<string>(x =>
                        x.Contains(@"""UnityLogTag"":""Cow say""")
                        && x.Contains(@$"""{Constants.SourceContextPropertyName}"":""UnityEngine.GameObject""")
                    )
                ), Times.Once()
            );
        }
        else {
            unityLogger.Verify(x =>
                x.Log(
                    LogType.Log,
                    "UnityEngine.GameObject",
                    It.Is<string>(x => x.Contains("\"UnityLogTag\":\"Cow say\""))
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

        using S.Logger logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithUnityData(useExplicitEnricherSettings ? unityLogEnricherSettings : null)
            .WriteTo.Unity(new JsonFormatter(), unityLogger.Object)
            .CreateLogger();

        // ACT
        logger.Information("What up?");

        // ASSERT
        // Extra log property was included in log event iff explicit enricher settings were used
        unityLogger.Verify(x =>
            x.Log(
                LogType.Log,
                It.Is<string>(x => Regex.Match(x, $@"""{unityLogEnricherSettings.TimeAsDoubleLogProperty}"":[0-9.]+[,}}]").Success == useExplicitEnricherSettings)
            ), Times.Once()
        );
    }

    private static string getExpectedJsonRegex(
        string msgTemplateRegex,
        LogEventLevel logEventLevel = LogEventLevel.Information,
        Dictionary<string, object>? logPropertyRegexes = null
    )
    {
        string? logPropsStr = (logPropertyRegexes?.Count ?? 0) == 0 ? null :
            string.Join(",", logPropertyRegexes.Select(kv => $"\"{kv.Key}\":{(kv.Value is string s ? $"\"{s}\"" : kv.Value)}"));
        return @"\{" +
            $@"""Timestamp"":""{@"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{7}[-+]\d{2}:00"}""," +
            $@"""Level"":""{logEventLevel}""," +
            $@"""MessageTemplate"":""{msgTemplateRegex}""" +
            (logPropsStr is null ? "" : $@",""Properties"":\{{{logPropsStr}\}}") +
        @"\}";
    }
}
