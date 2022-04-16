using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine;

[CreateAssetMenu(menuName = $"{nameof(UnityUtil)}/Configuration/{nameof(ConfigurationSourceCollection)}", fileName = "configuration-sources.asset")]
public class ConfigurationSourceCollection : ScriptableObject, IEnumerable<ConfigurationSource>
{
    [Tooltip(
        "Sources must be provided in reverse order of importance (i.e., configs in source 0 will override configs in source 1, " +
        "which will override configs in source 2, etc.)"
    )]
    public ConfigurationSource[] ConfigurationSources = Array.Empty<ConfigurationSource>();

    public IEnumerator<ConfigurationSource> GetEnumerator() => ((IEnumerable<ConfigurationSource>)ConfigurationSources).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ConfigurationSources.GetEnumerator();
}
