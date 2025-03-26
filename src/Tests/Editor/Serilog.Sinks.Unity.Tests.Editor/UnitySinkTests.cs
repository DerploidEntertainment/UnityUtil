using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Serilog.Events;
using Serilog.Formatting.Json;
using UnityEngine;
using UnityUtil.Editor;
using UnityUtil.Editor.Tests;
using UE = UnityEngine;

namespace Serilog.Sinks.Unity.Tests.Editor;

public class UnitySinkTests : BaseEditModeTestFixture
{
    [Test]
    public void Emit_CanUseAnyUnityLogger()
    {
        // ARRANGE
        var logger = new Mock<UE.ILogger>();
        var unitySink = new UnitySink(new JsonFormatter(), logger.Object);

        var logEvent = new LogEvent(
            DateTimeOffset.UtcNow,
            LogEventLevel.Information,
            exception: null,
            new MessageTemplate("What up, {Name}?", tokens: []),
            [
                new LogEventProperty("Name", new ScalarValue("dawg"))
            ]
        );

        Regex expectedRegex = getExpectedJsonRegex(
            @"What up, \{Name\}\?",
            logPropertyRegexes: new Dictionary<string, string> {
                { "Name", "dawg" },
            }
        );

        // ACT
        unitySink.Emit(logEvent);

        // ASSERT
        logger.Verify(x => x.Log(LogType.Log, It.IsRegex(expectedRegex.ToString())), Times.Once());
    }

    [Test]
    [TestCase(LogEventLevel.Verbose, LogType.Log)]
    [TestCase(LogEventLevel.Debug, LogType.Log)]
    [TestCase(LogEventLevel.Information, LogType.Log)]
    [TestCase(LogEventLevel.Warning, LogType.Warning)]
    [TestCase(LogEventLevel.Error, LogType.Error)]
    [TestCase(LogEventLevel.Fatal, LogType.Error)]
    public void Emit_SimpleMessageTemplate_LogsExpectedLogTypeAndMessage(LogEventLevel logEventLevel, LogType expectedLogType)
    {
        // ARRANGE
        var unitySink = new UnitySink(new JsonFormatter());
        var logEvent = new LogEvent(DateTimeOffset.UtcNow, logEventLevel, exception: null, new MessageTemplate("What up, {Name}?", tokens: []), []);
        Regex expectedRegex = getExpectedJsonRegex(@"What up, \{Name\}\?", logEventLevel);

        // ACT / ASSERT
        EditModeTestHelpers.ExpectLog(expectedLogType, expectedRegex);
        unitySink.Emit(logEvent);
    }

    [Test]
    public void Emit_IncludesLogProperties()
    {
        // ARRANGE
        var unitySink = new UnitySink(new JsonFormatter());

        var logEvent = new LogEvent(
            DateTimeOffset.UtcNow,
            LogEventLevel.Information,
            exception: null,
            new MessageTemplate("What up, {Name}?", tokens: []),
            [
                new LogEventProperty("Name", new ScalarValue("dawg"))
            ]
        );

        Regex expectedRegex = getExpectedJsonRegex(
            @"What up, \{Name\}\?",
            logPropertyRegexes: new Dictionary<string, string> {
                { "Name", "dawg" },
            }
        );

        // ACT / ASSERT
        EditModeTestHelpers.ExpectLog(LogType.Log, expectedRegex);
        unitySink.Emit(logEvent);
    }

    [Test]
    public void Emit_CanIncludeTagAndNoContext([Values] bool removeUnityTagLogPropertyIfPresent, [Values] bool addLogEventProperty)
    {
        // ARRANGE
        const string TAG_PROPERTY = "TestTag";
        const string TAG = "Cow say";
        var unitySink = new UnitySink(
            new JsonFormatter(),
            unitySinkSettings: new UnitySinkSettings {
                UnityTagLogProperty = TAG_PROPERTY,
                RemoveUnityTagLogPropertyIfPresent = removeUnityTagLogPropertyIfPresent
            }
        );

        List<LogEventProperty> logEventProperties = [new LogEventProperty(TAG_PROPERTY, new ScalarValue(TAG))];
        if (addLogEventProperty)
            logEventProperties.Add(new LogEventProperty("Name", new ScalarValue("dawg")));

        string text = addLogEventProperty ? "What up, {Name}?" : "What up";
        var logEvent = new LogEvent(DateTimeOffset.UtcNow, LogEventLevel.Information, exception: null, new MessageTemplate(text, tokens: []), logEventProperties);

        var logPropertyRegexes = new Dictionary<string, string>();
        if (!removeUnityTagLogPropertyIfPresent)
            logPropertyRegexes.Add(TAG_PROPERTY, TAG);
        if (addLogEventProperty)
            logPropertyRegexes.Add("Name", "dawg");

        string msgTemplateRegex = addLogEventProperty ? @"What up, \{Name\}\?" : "What up";
        Regex expectedRegex = getExpectedJsonRegex(msgTemplateRegex, tagRegex: TAG, logPropertyRegexes: logPropertyRegexes);

        // ACT / ASSERT
        EditModeTestHelpers.ExpectLog(LogType.Log, expectedRegex);
        unitySink.Emit(logEvent);
    }

    [Test]
    public void Emit_CanIncludeContextAndNoTag([Values] bool removeUnityContextLogPropertyIfPresent, [Values] bool addLogEventProperty)
    {
        // ARRANGE
        const string CONTEXT_PROPERTY = "TestContext";
        UE.Object context = new GameObject().transform;
        var unitySink = new UnitySink(
            new JsonFormatter(),
            unitySinkSettings: new UnitySinkSettings {
                UnityContextLogProperty = CONTEXT_PROPERTY,
                RemoveUnityContextLogPropertyIfPresent = removeUnityContextLogPropertyIfPresent
            }
        );

        List<LogEventProperty> logEventProperties = [new LogEventProperty(CONTEXT_PROPERTY, new ScalarValue(context))];
        if (addLogEventProperty)
            logEventProperties.Add(new LogEventProperty("Name", new ScalarValue("dawg")));

        string text = addLogEventProperty ? "What up, {Name}?" : "What up";
        var logEvent = new LogEvent(DateTimeOffset.UtcNow, LogEventLevel.Information, exception: null, new MessageTemplate(text, tokens: []), logEventProperties);

        var logPropertyRegexes = new Dictionary<string, string>();
        if (!removeUnityContextLogPropertyIfPresent) {
            string contextRegex = context.ToString().Replace("(", "\\(").Replace(")", "\\)").Replace(".", "\\.");
            logPropertyRegexes.Add(CONTEXT_PROPERTY, contextRegex);
        }
        if (addLogEventProperty)
            logPropertyRegexes.Add("Name", "dawg");

        string msgTemplateRegex = addLogEventProperty ? @"What up, \{Name\}\?" : "What up";
        Regex expectedRegex = getExpectedJsonRegex(msgTemplateRegex, logPropertyRegexes: logPropertyRegexes);

        // ACT / ASSERT
        EditModeTestHelpers.ExpectLog(LogType.Log, expectedRegex);
        unitySink.Emit(logEvent);
    }

    [Test]
    public void Emit_CanIncludeTagAndContext([Values] bool removeUnityTagLogPropertyIfPresent, [Values] bool removeUnityContextLogPropertyIfPresent, [Values] bool addLogEventProperty)
    {
        // ARRANGE
        const string TAG_PROPERTY = "TestTag";
        const string TAG = "Cow say";
        const string CONTEXT_PROPERTY = "TestContext";
        UE.Object context = new GameObject().transform;
        var unitySink = new UnitySink(
            new JsonFormatter(),
            unitySinkSettings: new UnitySinkSettings {
                UnityTagLogProperty = TAG_PROPERTY,
                UnityContextLogProperty = CONTEXT_PROPERTY,
                RemoveUnityTagLogPropertyIfPresent = removeUnityTagLogPropertyIfPresent,
                RemoveUnityContextLogPropertyIfPresent = removeUnityContextLogPropertyIfPresent,
            }
        );

        List<LogEventProperty> logEventProperties = [
            new LogEventProperty(TAG_PROPERTY, new ScalarValue(TAG)),
            new LogEventProperty(CONTEXT_PROPERTY, new ScalarValue(context))
        ];
        if (addLogEventProperty)
            logEventProperties.Add(new LogEventProperty("Name", new ScalarValue("dawg")));

        string text = addLogEventProperty ? "What up, {Name}?" : "What up";
        var logEvent = new LogEvent(DateTimeOffset.UtcNow, LogEventLevel.Information, exception: null, new MessageTemplate(text, tokens: []), logEventProperties);

        var logPropertyRegexes = new Dictionary<string, string>();
        if (!removeUnityTagLogPropertyIfPresent)
            logPropertyRegexes.Add(TAG_PROPERTY, TAG);
        if (!removeUnityContextLogPropertyIfPresent) {
            string contextRegex = context.ToString().Replace("(", "\\(").Replace(")", "\\)").Replace(".", "\\.");
            logPropertyRegexes.Add(CONTEXT_PROPERTY, contextRegex);
        }
        if (addLogEventProperty)
            logPropertyRegexes.Add("Name", "dawg");

        string msgTemplateRegex = addLogEventProperty ? @"What up, \{Name\}\?" : "What up";
        Regex expectedRegex = getExpectedJsonRegex(msgTemplateRegex, logPropertyRegexes: logPropertyRegexes);

        // ACT / ASSERT
        EditModeTestHelpers.ExpectLog(LogType.Log, expectedRegex);
        unitySink.Emit(logEvent);
    }

    [Test]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "*Async suffix for tests isn't very helpful")]
    public async Task Emit_AlwaysLogsOnUnityMainThread()
    {
        // ARRANGE
        var unitySink = new UnitySink(new JsonFormatter());

        var logEvent = new LogEvent(
            DateTimeOffset.UtcNow,
            LogEventLevel.Information,
            exception: null,
            new MessageTemplate("What up, {Name}?", tokens: []),
            [
                new LogEventProperty("Name", new ScalarValue("dawg"))
            ]
        );

        Regex expectedRegex = getExpectedJsonRegex(
            @"What up, \{Name\}\?",
            logPropertyRegexes: new Dictionary<string, string> {
                { "Name", "dawg" },
            }
        );

        // ACT / ASSERT
        EditModeTestHelpers.ExpectLog(LogType.Log, expectedRegex);

        // Emit log on threadpool thread and await it as it runs on Unity main thread
        await Task.Run(() => unitySink.Emit(logEvent));
    }

    private static Regex getExpectedJsonRegex(
        string msgTemplateRegex,
        LogEventLevel logEventLevel = LogEventLevel.Information,
        string? tagRegex = null,
        Dictionary<string, string>? logPropertyRegexes = null
    )
    {
        string tagStr = tagRegex is null ? "" : tagRegex + ": ";
        string logPropsStr = (logPropertyRegexes?.Count ?? 0) == 0 ? "" :
            $@",""Properties"":\{{{string.Join(",", logPropertyRegexes.Select(kv => $"\"{kv.Key}\":\"{kv.Value}\""))}\}}";
        return new(@$"{tagStr}\{{""Timestamp"":""{@"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{7}\+00:00"}"",""Level"":""{logEventLevel}"",""MessageTemplate"":""{msgTemplateRegex}""{logPropsStr}\}}");
    }
}
