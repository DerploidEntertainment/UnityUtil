using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Debugging;
using Serilog.Enrichers.Unity;
using Serilog.Formatting.Json;
using Serilog.Sinks.Unity;
using Unity.Extensions.Serilog;

namespace Unity.Extensions.Logging.Tests.Editor;

public static class TestHelpers
{
    /// <summary>
    /// Creates a <see cref="LoggerFactory"/> that wraps Serilog with default enricher, sink, and other configuration for Unity.
    /// </summary>
    /// <returns></returns>
    public static ILoggerFactory BuildDefaultLoggerFactoryForUnity()
    {
        SelfLog.Enable(UnityEngine.Debug.LogWarning);
        return new LoggerFactory()
            .AddSerilog(
                new LoggerConfiguration()
                .MinimumLevel.IsUnityFilterLogType()
                .Enrich.FromLogContext()
                .Enrich.WithUnityData()
                .Destructure.UnityObjectContext()
                .WriteTo.Unity(new JsonFormatter())
                .CreateLogger(),
                dispose: true
            );
    }
}
