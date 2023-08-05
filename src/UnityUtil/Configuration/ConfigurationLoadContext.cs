using System;

namespace UnityUtil.Configuration;

[Flags]
public enum ConfigurationLoadContext
{
    Never = 0b0000,
    BuildScript = 0b0001,
    PlayMode = 0b0010,
    DebugBuild = 0b0100,
    ReleaseBuild = 0b1000,
    AnyBuild = 0b1100,
    Always = 0b1111,
}
