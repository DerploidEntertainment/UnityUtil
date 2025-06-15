using System;

namespace UnityUtil;

public class AppEnvironment(string environmentName) : IAppEnvironment
{
    public string EnvironmentName { get; private set; } = environmentName ?? throw new ArgumentNullException(nameof(environmentName));
}
