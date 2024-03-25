using System;

namespace UnityUtil.DependencyInjection;

internal readonly struct Service
{
    private readonly Lazy<object> _instance;

    public Service(Type serviceType, string tag, object instance)
    {
        ServiceType = serviceType;
        InjectTag = tag;
        _instance = new Lazy<object>(instance);
    }

    public Service(Type serviceType, string tag, Func<object> instanceFactory)
    {
        ServiceType = serviceType;
        InjectTag = tag;
        _instance = new Lazy<object>(instanceFactory);
    }

    public readonly Type ServiceType;

    /// <summary>
    /// Tag to disambiguate services of the same <see cref="ServiceType"/>.
    /// </summary>
    public readonly string InjectTag;

    public object Instance => _instance.Value;
}
