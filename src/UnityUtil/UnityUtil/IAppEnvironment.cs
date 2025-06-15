namespace UnityUtil;

/// <summary>
/// Provides information about the environment in which an application is running.
/// Based on the <a href="https://github.com/dotnet/dotnet/blob/main/src/runtime/src/libraries/Microsoft.Extensions.Hosting.Abstractions/src/IHostEnvironment.cs"><c>Microsoft.Extensions.Hosting.Abstractions.IHostEnvironment</c></a> interface.
/// </summary>
public interface IAppEnvironment
{
    /// <summary>
    /// Gets or sets the name of the environment.
    /// This property must be set to the value of the "environment" key in configuration at boot time.
    /// </summary>
    string EnvironmentName { get; }
}
