using System;

namespace UnityUtil;

/// <summary>
/// Extension methods for <see cref="IAppEnvironment"/>.
/// Heavily based on the <a href="https://github.com/dotnet/dotnet/blob/main/src/runtime/src/libraries/Microsoft.Extensions.Hosting.Abstractions/src/HostEnvironmentEnvExtensions.cs"><c>Microsoft.Extensions.Hosting.Abstractions.HostEnvironmentEnvExtensions</c></a> class.
/// </summary>
public static class AppEnvironmentEnvExtensions
{
    /// <summary>
    /// Checks if the current app environment name is <see cref="Environments.Development"/>.
    /// </summary>
    /// <param name="appEnvironment">An instance of <see cref="IAppEnvironment"/>.</param>
    /// <returns><see langword="true"/> if the environment name is <see cref="Environments.Development"/>; otherwise, <see langword="false"/>.</returns>
    public static bool IsDevelopment(this IAppEnvironment appEnvironment) =>
        appEnvironment is null
            ? throw new ArgumentNullException(nameof(appEnvironment))
            : appEnvironment.IsEnvironment(Environments.Development);

    /// <summary>
    /// Checks if the current app environment name is <see cref="Environments.Staging"/>.
    /// </summary>
    /// <param name="appEnvironment">An instance of <see cref="IAppEnvironment"/>.</param>
    /// <returns><see langword="true"/> if the environment name is <see cref="Environments.Staging"/>; otherwise, <see langword="false"/>.</returns>
    public static bool IsStaging(this IAppEnvironment appEnvironment) =>
        appEnvironment is null
            ? throw new ArgumentNullException(nameof(appEnvironment))
            : appEnvironment.IsEnvironment(Environments.Staging);

    /// <summary>
    /// Checks if the current app environment name is <see cref="Environments.Production"/>.
    /// </summary>
    /// <param name="appEnvironment">An instance of <see cref="IAppEnvironment"/>.</param>
    /// <returns><see langword="true"/> if the environment name is <see cref="Environments.Production"/>; otherwise, <see langword="false"/>.</returns>
    public static bool IsProduction(this IAppEnvironment appEnvironment) =>
        appEnvironment is null
            ? throw new ArgumentNullException(nameof(appEnvironment))
            : appEnvironment.IsEnvironment(Environments.Production);

    /// <summary>
    /// Compares the current app environment name against the specified value.
    /// </summary>
    /// <param name="appEnvironment">An instance of <see cref="IAppEnvironment"/>.</param>
    /// <param name="environmentName">Environment name to validate against.</param>
    /// <returns><see langword="true"/> if the specified name is the same as the current environment; otherwise, <see langword="false"/>.</returns>
    public static bool IsEnvironment(this IAppEnvironment appEnvironment, string environmentName) =>
        appEnvironment is null
            ? throw new ArgumentNullException(nameof(appEnvironment))
            : string.Equals(
                appEnvironment.EnvironmentName,
                environmentName,
                StringComparison.OrdinalIgnoreCase
            );
}
