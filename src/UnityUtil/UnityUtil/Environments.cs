namespace UnityUtil;

/// <summary>
/// Commonly used environment names.
/// Heavily based on the <a href="https://github.com/dotnet/dotnet/blob/main/src/runtime/src/libraries/Microsoft.Extensions.Hosting.Abstractions/src/Environments.cs"><c>Microsoft.Extensions.Hosting.Abstractions.Environments</c></a> class.
/// </summary>
public static class Environments
{
    /// <summary>
    /// Specifies the Development environment.
    /// </summary>
    /// <remarks>The development environment can enable features that shouldn't be exposed in production. Because of the performance cost, scope validation and dependency validation only happens in development.</remarks>
    public static readonly string Development = "Development";

    /// <summary>
    /// Specifies the Staging environment.
    /// </summary>
    /// <remarks>The staging environment can be used to validate app changes before changing the environment to production.</remarks>
    public static readonly string Staging = "Staging";

    /// <summary>
    /// Specifies the Production environment.
    /// </summary>
    /// <remarks>The production environment should be configured to maximize security, performance, and application robustness.</remarks>
    public static readonly string Production = "Production";
}
