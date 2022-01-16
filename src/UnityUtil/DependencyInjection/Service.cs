using System;

namespace UnityEngine.DependencyInjection
{
    internal readonly struct Service
    {
        private readonly Lazy<object> _instance;

        public Service(Type serviceType, string tag, object instance)
        {
            ServiceType = serviceType;
            Tag = tag;
            _instance = new Lazy<object>(instance);
        }

        public Service(Type serviceType, string tag, Func<object> instanceFactory)
        {
            ServiceType = serviceType;
            Tag = tag;
            _instance = new Lazy<object>(instanceFactory);
        }

        public readonly Type ServiceType;
        public readonly string Tag;
        public object Instance => _instance.Value;
    }

}
