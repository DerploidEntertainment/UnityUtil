using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Unity.Extensions.Logging.Tests.Editor;

/// <summary>
/// Create an instance of this component in a test scene to verify that logging with Serilog and Microsoft.Extensions.Logging works as expected in Unity.
/// </summary>
public class TestLoggingComponent : MonoBehaviour
{
    private ILogger<TestLoggingComponent>? _logger;

    /// <summary>
    /// Emits a test log with example scope properties.
    /// After invoking this method via its Odin button in the Inspector, verify that:
    /// <list type="number">
    /// <item>The log message appears in the Unity console.</item>
    /// <item>The log message includes the expected log properties.</item>
    /// <item>The type name appears as the tag in the Unity log.</item>
    /// <item>Clicking the log in the console highlights the component in the Hierarchy view.</item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// In production code, we would just inject an <see cref="ILoggerFactory"/> rather than using a static helper method.
    /// Still, see how the Serilog configuration is encapsulated elsewhere, so we aren't <see langword="using"/> any Serilog namespaces in this file.
    /// </remarks>
    [Button]
    public void TestLog()
    {
        _logger ??= TestHelpers.BuildDefaultLoggerFactoryForUnity().CreateLogger(this);

        using (_logger!.BeginScope("DatScope"))
        using (_logger!.BeginScope(("SomeString", "Value")))    // Stored as a string array. Support for storing it as a key/val pair isn't added til Serilog.Extensions.Logging 2.0.0, but our libraries are depending on the earliest dependency versions possible.
        using (_logger!.BeginScope(new Dictionary<string, object> {
            { "SomeInt", 5 },
            { "SomeBool", true },
        }))
            _logger!.LogInformation(new EventId(5, nameof(TestLog)), "Ey yo what up!");
    }
};
