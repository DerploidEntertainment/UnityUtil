using Microsoft.Extensions.Logging;
using System;
using UnityEngine;
using UnityUtil.DependencyInjection;

namespace UnityUtil.Configuration;

public abstract class Configurable : MonoBehaviour
{
    protected IConfigurator? Configurator;
    protected ILoggerFactory? LoggerFactory;

    [Tooltip(
        "The key by which to look up configuration for this Component. " +
        "A blank string (default) is equivalent to the fully qualified type name. " +
        "Leading/trailing whitespace is ignored. For example, all components of type 'Derp' in the namespace 'MyGame', " +
        "will use the default config key 'MyGame.Derp', and their field config keys will have the form 'MyGame.Derp.<fieldname>'. " +
        "If you want a particular 'Derp' instance to be individually configurable, then you must give it a unique config key, and " +
        "then its field config keys would have the form '<configkey>.<fieldname>'."
    )]
    public string ConfigKey = "";

#if UNITY_EDITOR
    protected virtual void Reset() => ConfigKey = DefaultConfigKey(GetType());
#endif

    protected virtual void Awake()
    {
        DependencyInjector.Instance.ResolveDependenciesOf(this);

        ConfigKey = string.IsNullOrWhiteSpace(ConfigKey) ? DefaultConfigKey(GetType()) : ConfigKey;
        Configurator!.Configure(this, ConfigKey);
    }

    public void Inject(IConfigurator configurator, ILoggerFactory loggerFactory)
    {
        Configurator = configurator;
        LoggerFactory = loggerFactory;
    }

    public static string DefaultConfigKey(Type clientType) => clientType.FullName;
}
