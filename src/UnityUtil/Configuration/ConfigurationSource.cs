using Microsoft.Extensions.Logging;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityUtil.Configuration;

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
