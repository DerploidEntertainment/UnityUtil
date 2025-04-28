using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using UnityEngine;
using MEL = Microsoft.Extensions.Logging;

namespace Unity.Extensions.Logging.Tests.Editor;

public class UnityObjectLoggerTests
{
    private readonly Mock<MEL.ILogger> _logger = new();
    private readonly Mock<ILoggerFactory> _loggerFactory = new();
    private readonly GameObject _loggingObject = new();

    private Dictionary<string, object> _passedScopePropDict = [];
    private string? _passedScopePropStr = null;
    private (string, string)? _passedScopePropValTuple = null;
    private LogLevel? _passedLogLevel = null;
    private EventId? _passedEventId = null;
    private string? _passedMsgTemplate = null;

    [SetUp]
    public void SetUp()
    {
        _passedScopePropDict.Clear();
        _passedScopePropStr = null;
        _passedScopePropValTuple = null;
        _passedLogLevel = null;
        _passedEventId = null;
        _passedMsgTemplate = null;

        _logger.Reset();

        _ = _logger.Setup(x => x.BeginScope(It.IsAny<Dictionary<string, object>>()))
            .Callback((Dictionary<string, object> scopeProps) => _passedScopePropDict = _passedScopePropDict.Concat(scopeProps).ToDictionary(x => x.Key, x => x.Value));

        _ = _logger.Setup(x => x.BeginScope(It.IsAny<string>()))
            .Callback((string scopeProp) => _passedScopePropStr = scopeProp);

        _ = _logger.Setup(x => x.BeginScope(It.IsAny<(string, string)>()))
            .Callback(((string, string) scopeProp) => _passedScopePropValTuple = scopeProp);

        _ = _logger.Setup(x =>
            x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            )
        ).Callback((LogLevel logLevel, EventId eventId, object state, Exception? _, object _) => {
            _passedLogLevel = logLevel;
            _passedEventId = eventId;
            _passedMsgTemplate = state.ToString();
        });

        _loggerFactory.Reset();
        _ = _loggerFactory
            .Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(_logger.Object);
    }

    [Test]
    [TestCase(LogLevel.Debug)]
    [TestCase(LogLevel.Trace)]
    [TestCase(LogLevel.Information)]
    [TestCase(LogLevel.Warning)]
    [TestCase(LogLevel.Error)]
    [TestCase(LogLevel.Critical)]
    public void Log_SimpleMessageTemplate_LogsExpectedLogTypeAndMessage(LogLevel logLevel)
    {
        // ARRANGE
        const string MSG = "What up?";

        var unityObjectLogger = new UnityObjectLogger<GameObject>(_loggerFactory.Object, _loggingObject, new UnityObjectLoggerSettings());

        var eventId = new EventId(id: 5, nameof(Log_SimpleMessageTemplate_LogsExpectedLogTypeAndMessage));

        // ACT
        unityObjectLogger.Log(logLevel, eventId, MSG, exception: null, formatter: (state, ex) => "");

        // ASSERT
        _logger.Verify(x => x.BeginScope(It.IsAny<Dictionary<string, object?>>()), Times.Once());

        //_logger.Verify(x => x.Log());   // The param types on Log() just make it too dang hard to verify here...

        Assert.That(_passedLogLevel, Is.EqualTo(logLevel));
        Assert.That(_passedEventId, Is.EqualTo(eventId));
        Assert.That(_passedMsgTemplate, Is.EqualTo(MSG));
    }

    [Test]
    [TestCase("UnityLogContext")]
    [TestCase("Derp")]
    public void Log_AddsUnityContextLogProperty_WithCorrectName(string logPropertyName)
    {
        // ARRANGE
        const string MSG = "What up?";

        var unityObjectLoggerSettings = new UnityObjectLoggerSettings {
            AddUnityContext = true,
            UnityContextLogProperty = logPropertyName,
        };
        var unityObjectLogger = new UnityObjectLogger<GameObject>(_loggerFactory.Object, _loggingObject, unityObjectLoggerSettings);

        // ACT
        unityObjectLogger.LogInformation(new EventId(id: 5, nameof(Log_AddsUnityContextLogProperty_WithCorrectName)), MSG);

        // ASSERT
        Assert.That(_passedScopePropDict, Is.EquivalentTo(new Dictionary<string, object> {
            { $"@{logPropertyName}", ValueTuple.Create(_loggingObject) },
        }));
    }

    [Test]
    [TestCase("UnityHierarchyName")]
    [TestCase("Derp")]
    public void Log_AddsUnityHierarchyName_WithCorrectName(string logPropertyName)
    {
        // ARRANGE
        const string MSG = "What up?";

        var unityObjectLoggerSettings = new UnityObjectLoggerSettings {
            AddUnityContext = false,
            AddHierarchyName = true,
            HierarchyNameLogProperty = logPropertyName,
        };
        var unityObjectLogger = new UnityObjectLogger<GameObject>(_loggerFactory.Object, _loggingObject, unityObjectLoggerSettings);

        // ACT
        unityObjectLogger.LogInformation(new EventId(id: 5, nameof(Log_AddsUnityHierarchyName_WithCorrectName)), MSG);

        // ASSERT
        Assert.That(_passedScopePropDict, Contains.Key(logPropertyName));
    }

    [Test]
    public void Log_AddsScopeAsLogProperties()
    {
        // ARRANGE
        const string MSG = "What up, {Name}?";

        var unityObjectLoggerSettings = new UnityObjectLoggerSettings {
            AddUnityContext = true,
            AddHierarchyName = true,
        };
        var unityObjectLogger = new UnityObjectLogger<GameObject>(_loggerFactory.Object, _loggingObject, unityObjectLoggerSettings);

        // ACT
        using (unityObjectLogger.BeginScope("DatScope"))
        using (unityObjectLogger.BeginScope(("SomeString", "Value")))    // Stored as a string array. Support for storing it as a key/val pair isn't added til Serilog.Extensions.Logging 9.0.0, but our libraries are depending on the earliest dependency versions possible.
        using (unityObjectLogger.BeginScope(new Dictionary<string, object> {
            { "SomeInt", 5 },
            { "SomeBool", true },
        }))
            unityObjectLogger.LogInformation(new EventId(id: 5, nameof(Log_AddsScopeAsLogProperties)), MSG, "dawg");

        // ASSERT
        _logger.Verify(x => x.BeginScope(It.IsAny<It.IsAnyType>()), Times.Exactly(4));  // Above BeginScope() calls, plus one inside test class
        Assert.That(_passedScopePropDict, Is.EquivalentTo(new Dictionary<string, object> {
            { "SomeInt", 5 },
            { "SomeBool", true },
            { $"@{unityObjectLoggerSettings.UnityContextLogProperty}", ValueTuple.Create(_loggingObject) },
            { unityObjectLoggerSettings.HierarchyNameLogProperty, unityObjectLogger.GetHierarchyName() },
        }));
        Assert.That(_passedScopePropStr, Is.EqualTo("DatScope"));
        Assert.That(_passedScopePropValTuple, Is.EqualTo(("SomeString", "Value")));
    }

    [Test]
    [TestCase(" > ", "test-object", true, "parent", "test-object")]
    [TestCase(" > ", "test-object", false, "parent", "parent > test-object")]
    public void Log_AddsExpectedHierarchyName_StaticOrDynamicHierarchy(
        string parentNameSeparator,
        string objectName,
        bool hasStaticHierarchy,
        string parentName,
        string expectedHierarchyName
    )
    {
        // ARRANGE
        var parent = new GameObject(parentName);
        var loggingObject = new GameObject(objectName);

        var unityObjectLoggerSettings = new UnityObjectLoggerSettings {
            AddHierarchyName = true,
            HierarchyNameLogProperty = "UnityHierarchyName",
            ParentNameSeparator = parentNameSeparator,
            HasStaticHierarchy = hasStaticHierarchy,
        };
        var unityObjectLogger = new UnityObjectLogger<GameObject>(_loggerFactory.Object, loggingObject, unityObjectLoggerSettings);


        // ACT
        loggingObject.transform.SetParent(parent.transform);
        unityObjectLogger.LogInformation(new EventId(id: 5, nameof(Log_AddsExpectedHierarchyName_StaticOrDynamicHierarchy)), "What up?");

        // ASSERT
        Assert.That(_passedScopePropDict[unityObjectLoggerSettings.HierarchyNameLogProperty], Is.EqualTo(expectedHierarchyName));
    }

    [Test]
    [TestCase("/", new[] { "test-object" }, "test-object")]
    [TestCase("/", new[] { "parent", "test-object" }, "parent/test-object")]
    [TestCase("/", new[] { "parent1", "parent2", "test-object" }, "parent1/parent2/test-object")]
    [TestCase(">", new[] { "test-object" }, "test-object")]
    [TestCase(">", new[] { "parent", "test-object" }, "parent>test-object")]
    [TestCase(">", new[] { "parent1", "parent2", "test-object" }, "parent1>parent2>test-object")]
    [TestCase(" > ", new[] { "test-object" }, "test-object")]
    [TestCase(" > ", new[] { "parent", "test-object" }, "parent > test-object")]
    [TestCase(" > ", new[] { "parent1", "parent2", "test-object" }, "parent1 > parent2 > test-object")]
    public void GetHierarchyName_ReturnsCorrectName_DifferentNameSeparators(string parentNameSeparator, string[] hierarchyNames, string expectedHierarchyName)
    {
        // ARRANGE
        Transform? loggingTransform = null;
        foreach (string hierarchyName in hierarchyNames) {
            var obj = new GameObject(hierarchyName);
            obj.transform.SetParent(loggingTransform);
            loggingTransform = obj.transform;
        }

        var unityObjectLogger = new UnityObjectLogger<GameObject>(
            _loggerFactory.Object,
            loggingTransform!.gameObject,
            new UnityObjectLoggerSettings { AddHierarchyName = true, ParentNameSeparator = parentNameSeparator }
        );

        // ACT
        string actualHierarchyName = unityObjectLogger.GetHierarchyName();

        // ASSERT
        Assert.That(actualHierarchyName, Is.EqualTo(expectedHierarchyName));
    }

    [Test]
    public void GetHierarchyName_ReturnsCorrectName_DifferentHierarchies()
    {
        // ARRANGE
        var parent1 = new GameObject("parent1");
        var parent2 = new GameObject("parent2");
        var loggingObject = new GameObject("test-object");
        string expectedHierarchyName1 = "parent1>test-object";
        string expectedHierarchyName2 = "parent2>test-object";

        var unityObjectLogger = new UnityObjectLogger<GameObject>(
            _loggerFactory.Object,
            loggingObject,
            new UnityObjectLoggerSettings {
                AddHierarchyName = true,
                ParentNameSeparator = ">",
                HasStaticHierarchy = false
            }
        );

        // ACT
        loggingObject.transform.SetParent(parent1.transform);
        string hierarchyName1 = unityObjectLogger.GetHierarchyName();

        loggingObject.transform.SetParent(parent2.transform);
        string hierarchyName2 = unityObjectLogger.GetHierarchyName();

        // ASSERT
        Assert.That(hierarchyName1, Is.EqualTo(expectedHierarchyName1));
        Assert.That(hierarchyName2, Is.EqualTo(expectedHierarchyName2));
    }

    [Test]
    public void GetHierarchyName_ReturnsCorrectName_Component()
    {
        // ARRANGE
        var parent = new GameObject("parent");
        var obj = new GameObject("test-object");
        obj.transform.SetParent(parent.transform);

        string expectedHierarchyName = $"{parent.name}>{obj.name}";

        TestLoggingComponent loggingComponent = obj.AddComponent<TestLoggingComponent>();

        var unityObjectLogger = new UnityObjectLogger<TestLoggingComponent>(
            _loggerFactory.Object,
            loggingComponent,
            new UnityObjectLoggerSettings { AddHierarchyName = true, ParentNameSeparator = ">" }
        );

        // ACT
        string hierarchyName = unityObjectLogger.GetHierarchyName();

        // ASSERT
        Assert.That(hierarchyName, Is.EqualTo(expectedHierarchyName));
    }

    private sealed class DerpObject : ScriptableObject { }

    [Test]
    public void GetHierarchyName_ReturnsCorrectName_ScriptableObject()
    {
        // ARRANGE
        DerpObject loggingObject = ScriptableObject.CreateInstance<DerpObject>();
        loggingObject.name = "test-object";

        var unityObjectLogger = new UnityObjectLogger<DerpObject>(
            _loggerFactory.Object,
            loggingObject,
            new UnityObjectLoggerSettings { AddHierarchyName = true }
        );

        // ACT
        string hierarchyName = unityObjectLogger.GetHierarchyName();

        // ASSERT
        Assert.That(hierarchyName, Is.EqualTo(loggingObject.name));
    }

    [Test]
    public void Log_DefaultSettings_AddsExpectedLogProperties()
    {
        // ARRANGE
        var unityObjectLoggerSettings = new UnityObjectLoggerSettings();
        var unityObjectLogger = new UnityObjectLogger<GameObject>(_loggerFactory.Object, _loggingObject, unityObjectLoggerSettings);

        // ACT
        unityObjectLogger.LogInformation(new EventId(id: 5, nameof(Log_DefaultSettings_AddsExpectedLogProperties)), "What up?");

        // ASSERT
        _logger.Verify(x => x.BeginScope(It.IsAny<It.IsAnyType>()), Times.Exactly(1));  // Just one call inside test class
        Assert.That(_passedScopePropDict, Is.EquivalentTo(new Dictionary<string, object> {
            { $"@{unityObjectLoggerSettings.UnityContextLogProperty}", ValueTuple.Create(_loggingObject) },
        }));
    }

    [Test]
    public void Log_EmptySettings_AddsNoLogProperties()
    {
        // ARRANGE
        var unityObjectLoggerSettings = new UnityObjectLoggerSettings {
            AddUnityContext = false,
            AddHierarchyName = false,
        };
        var unityObjectLogger = new UnityObjectLogger<GameObject>(_loggerFactory.Object, _loggingObject, unityObjectLoggerSettings);

        // ACT
        unityObjectLogger.LogInformation(new EventId(id: 5, nameof(Log_DefaultSettings_AddsExpectedLogProperties)), "What up?");

        // ASSERT
        _logger.Verify(x => x.BeginScope(It.IsAny<It.IsAnyType>()), Times.Never());
        Assert.That(_passedScopePropDict, Is.Empty);
    }

    [Test]
    public void BeginScope_CallsInnerLogger()
    {
        // ARRANGE
        var unityObjectLogger = new UnityObjectLogger<GameObject>(_loggerFactory.Object, _loggingObject);

        // ACT
        using IDisposable? scope = unityObjectLogger.BeginScope("DatScope");

        // ASSERT
        _logger.Verify(x => x.BeginScope("DatScope"), Times.Once());
    }

    [Test]
    [TestCase(LogLevel.Debug)]
    [TestCase(LogLevel.Trace)]
    [TestCase(LogLevel.Information)]
    [TestCase(LogLevel.Warning)]
    [TestCase(LogLevel.Error)]
    [TestCase(LogLevel.Critical)]
    public void IsEnabled_CallsInnerLogger(LogLevel logLevel)
    {
        // ARRANGE
        var unityObjectLogger = new UnityObjectLogger<GameObject>(_loggerFactory.Object, _loggingObject);

        // ACT
        _ = unityObjectLogger.IsEnabled(logLevel);

        // ASSERT
        _logger.Verify(x => x.IsEnabled(logLevel), Times.Once());
    }
}
