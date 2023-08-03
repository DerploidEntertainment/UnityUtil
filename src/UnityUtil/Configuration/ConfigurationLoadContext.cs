using System;
using System.Diagnostics.CodeAnalysis;

namespace UnityUtil.Configuration;

[Flags]
[SuppressMessage("Design", "CA1008:Enums should have zero value", Justification = "I think 'Never' sounds better here than 'None'. Suppression may not be necessary in future, see https://github.com/dotnet/roslyn-analyzers/issues/5777")]
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
