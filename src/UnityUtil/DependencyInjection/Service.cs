using System;

namespace UnityEngine.DependencyInjection
{
    internal readonly struct Service
    {
        public Service(Type serviceType, string tag, object instance)
        {
            ServiceType = serviceType;
            Tag = tag;
            Instance = instance;
        }
        public readonly Type ServiceType;
        public readonly string Tag;
        public readonly object Instance;
    }

}
