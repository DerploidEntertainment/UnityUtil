using System;
using Moq;
using NUnit.Framework;
using Serilog.Core;
using Serilog.Events;
using UnityEngine;

namespace Serilog.Enrichers.Unity.Tests.Editor;

public class UnityLogEnricherTests
{
    private readonly Mock<ILogEventPropertyFactory> _logEventPropertyFactory = new();

    [SetUp]
    public void SetUp()
    {
        _logEventPropertyFactory.Reset();
        _ = _logEventPropertyFactory
            .Setup(x => x.CreateProperty(It.IsAny<string>(), It.IsAny<object>(), false))
            .Returns((string name, object value, bool destructureObjects) => new LogEventProperty(name, new ScalarValue(value)));
    }

    [Test]
    public void Enrich_CanAddNoLogProperties()
    {
        // ARRANGE
        var unityLogEnricher = new UnityLogEnricher(new UnityLogEnricherSettings { WithFrameCount = false });    // All other properties are set to false by default
        var logEvent = new LogEvent(DateTimeOffset.UtcNow, LogEventLevel.Information, exception: null, new MessageTemplate("What up", tokens: []), []);
        var logEventPropertyFactory = new Mock<ILogEventPropertyFactory>();

        // ACT
        unityLogEnricher.Enrich(logEvent, logEventPropertyFactory.Object);

        // ASSERT
        Assert.That(logEvent.Properties, Is.Empty);
    }

    [Test]
    [TestCase("UnityFrameCount")]
    [TestCase("Derp")]
    public void Enrich_CanAddLogProperty_FrameCount(string logPropertyName) =>
        assertTimeLogProperty(
            new UnityLogEnricherSettings {
                WithFrameCount = true,
                FrameCountLogProperty = logPropertyName
            },
            logPropertyName, Time.frameCount, Time.frameCount
        );

    [Test]
    [TestCase("UnityTimeSinceLevelLoad")]
    [TestCase("Derp")]
    public void Enrich_CanAddLogProperty_TimeSinceLevelLoad(string logPropertyName) =>
        assertTimeLogProperty(
            new UnityLogEnricherSettings {
                WithTimeSinceLevelLoad = true,
                TimeSinceLevelLoadLogProperty = logPropertyName
            },
            logPropertyName, Time.timeSinceLevelLoad, Time.timeSinceLevelLoad + 0.005f
        );

    [Test]
    [TestCase("UnityTimeSinceLevelLoadAsDouble")]
    [TestCase("Derp")]
    public void Enrich_CanAddLogProperty_TimeSinceLevelLoadAsDouble(string logPropertyName) =>
        assertTimeLogProperty(
            new UnityLogEnricherSettings {
                WithTimeSinceLevelLoadAsDouble = true,
                TimeSinceLevelLoadAsDoubleLogProperty = logPropertyName
            },
            logPropertyName, Time.timeSinceLevelLoadAsDouble, Time.timeSinceLevelLoadAsDouble + 0.005d
        );

    [Test]
    [TestCase("UnityUnscaledTime")]
    [TestCase("Derp")]
    public void Enrich_CanAddLogProperty_UnscaledTime(string logPropertyName) =>
        assertTimeLogProperty(
            new UnityLogEnricherSettings {
                WithUnscaledTime = true,
                UnscaledTimeLogProperty = logPropertyName
            },
            logPropertyName, Time.unscaledTime, Time.unscaledTime + 0.005f
        );

    [Test]
    [TestCase("UnityUnscaledTimeAsDouble")]
    [TestCase("Derp")]
    public void Enrich_CanAddLogProperty_UnscaledTimeAsDouble(string logPropertyName) =>
        assertTimeLogProperty(
            new UnityLogEnricherSettings {
                WithUnscaledTimeAsDouble = true,
                UnscaledTimeAsDoubleLogProperty = logPropertyName
            },
            logPropertyName, Time.unscaledTimeAsDouble, Time.unscaledTimeAsDouble + 0.005d
        );

    [Test]
    [TestCase("UnityTime")]
    [TestCase("Derp")]
    public void Enrich_CanAddLogProperty_Time(string logPropertyName) =>
        assertTimeLogProperty(
            new UnityLogEnricherSettings {
                WithTime = true,
                TimeLogProperty = logPropertyName
            },
            logPropertyName, Time.time, Time.time + 0.005f
        );

    [Test]
    [TestCase("UnityTimeAsDouble")]
    [TestCase("Derp")]
    public void Enrich_CanAddLogProperty_TimeAsDouble(string logPropertyName) =>
        assertTimeLogProperty(
            new UnityLogEnricherSettings {
                WithTimeAsDouble = true,
                TimeAsDoubleLogProperty = logPropertyName
            },
            logPropertyName, Time.timeAsDouble, Time.timeAsDouble + 0.005d
        );

    [Test]
    public void Enrich_CanAddAllLogProperties()
    {
        // ARRANGE
        var unityLogEnricherSettings = new UnityLogEnricherSettings {
            WithFrameCount = true,
            WithTimeSinceLevelLoad = true,
            WithTimeSinceLevelLoadAsDouble = true,
            WithUnscaledTime = true,
            WithUnscaledTimeAsDouble = true,
            WithTime = true,
            WithTimeAsDouble = true
        };
        var unityLogEnricher = new UnityLogEnricher(unityLogEnricherSettings);
        var logEvent = new LogEvent(DateTimeOffset.UtcNow, LogEventLevel.Information, exception: null, new MessageTemplate("What up", tokens: []), []);

        // ACT
        unityLogEnricher.Enrich(logEvent, _logEventPropertyFactory.Object);

        // ASSERT
        Assert.That(logEvent.Properties.Keys, Is.EquivalentTo(new[] {   // Only asserting presence of keys; values are asserted in other tests
            unityLogEnricherSettings.FrameCountLogProperty,
            unityLogEnricherSettings.TimeSinceLevelLoadLogProperty,
            unityLogEnricherSettings.TimeSinceLevelLoadAsDoubleLogProperty,
            unityLogEnricherSettings.UnscaledTimeLogProperty,
            unityLogEnricherSettings.UnscaledTimeAsDoubleLogProperty,
            unityLogEnricherSettings.TimeLogProperty,
            unityLogEnricherSettings.TimeAsDoubleLogProperty,
        }));
    }

    private void assertTimeLogProperty<T>(UnityLogEnricherSettings unityLogEnricherSettings, string logPropertyName, T minValue, T maxValue) where T : IComparable
    {
        // ARRANGE
        var unityLogEnricher = new UnityLogEnricher(unityLogEnricherSettings);
        var logEvent = new LogEvent(DateTimeOffset.UtcNow, LogEventLevel.Information, exception: null, new MessageTemplate("What up", tokens: []), []);

        // ACT
        unityLogEnricher.Enrich(logEvent, _logEventPropertyFactory.Object);

        // ASSERT
        Assert.That(logEvent.Properties, Does.ContainKey(logPropertyName));

        object? actual = (logEvent.Properties[logPropertyName] as ScalarValue)?.Value;
        Assert.That(actual, Is.InRange(minValue, maxValue));
    }
}
