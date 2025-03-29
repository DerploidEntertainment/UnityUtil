using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Formatting.Json;
using Unity.Extensions.Serilog;

namespace Unity.Extensions.Logging.Tests.Editor;

public static class TestHelpers
{
    /// <summary>
    /// Creates a <see cref="LoggerFactory"/> that wraps Serilog with default enricher, sink, and other configuration for Unity.
    /// </summary>
    /// <returns></returns>
    public static ILoggerFactory BuildDefaultLoggerFactoryForUnity() =>
        new LoggerFactory()
            .AddSerilog(
                new LoggerConfiguration()
                .Enrich.FromLogContext()
                .AddUnity(new JsonFormatter())
                .CreateLogger(),
                dispose: true
            );
}
