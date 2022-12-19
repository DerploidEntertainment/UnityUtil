using Microsoft.Extensions.Logging;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityUtil.Configuration;

[Flags]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1008:Enums should have zero value", Justification = "I think 'Never' sounds better here than 'None'. Suppression may not be necessary in future, see https://github.com/dotnet/roslyn-analyzers/issues/5777")]
public enum ConfigurationLoadContext
{
    Never = 0b0000,
    BuildScript = 0b0001,
    PlayMode = 0b0010,
    DebugBuild = 0b0100,
    ReleaseBuild = 0b1000,
    Always = 0b1111,
}

public enum ConfigurationSourceLoadBehavior
{
    SyncOnly,
    AsyncOnly,
    SyncAndAsync,
}

public abstract class ConfigurationSource : ScriptableObject
{
    public static readonly IReadOnlyDictionary<string, object> EmptyConfigs = new Dictionary<string, object>();

    private ConfigurationLogger<ConfigurationSource>? _logger;

    protected readonly Dictionary<string, object> LoadedConfigsHidden = new();

    [field: ShowInInspector, SerializeField]
    public bool Required { get; private set; } = true;

    [field: Tooltip(
        $"In what contexts should we attempt to load this {nameof(ConfigurationSource)}? " +
        "E.g., only when entering Play Mode in the Editor, or only in Release builds. " +
        $"One handy use of the Editor context is for {nameof(ConfigurationSource)}s whose corresponding config assets " +
        "are included under an Assets/**/Editor/ folder. This lets you keep those config assets out of builds so they don't take up space, " +
        "and then the configuration system won't attempt to load them or warn that they are missing."
    )]
    [field: ShowInInspector, SerializeField]
    public ConfigurationLoadContext LoadContext { get; private set; } = ConfigurationLoadContext.Always;

    public abstract ConfigurationSourceLoadBehavior LoadBehavior { get; }

    public virtual void Inject(ILoggerFactory loggerFactory) => _logger = new(loggerFactory, context: this);

    public virtual void Load() => LoadedConfigsHidden.Clear();
    public virtual IEnumerator LoadAsync() { LoadedConfigsHidden.Clear(); yield return null; }
    public IReadOnlyDictionary<string, object> LoadedConfigs => LoadedConfigsHidden;

}
