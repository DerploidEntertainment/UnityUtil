using System;
using System.Diagnostics.CodeAnalysis;

namespace UnityEngine
{
    [CreateAssetMenu(menuName = $"{nameof(UnityUtil)}/Configuration/{nameof(ConfigurationSourceCollection)}", fileName = "configuration-sources.asset")]
    public class ConfigurationSourceCollection : ScriptableObject
    {
        [Tooltip(
            "Sources must be provided in reverse order of importance (i.e., configs in source 0 will override configs in source 1, " +
            "which will override configs in source 2, etc.)"
        )]
        public ConfigurationSource[] ConfigurationSources;

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        private void Reset() => ConfigurationSources = Array.Empty<ConfigurationSource>();
    }
}
