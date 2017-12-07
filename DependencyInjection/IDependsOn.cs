using UnityEngine;

namespace Danware.Unity.DependencyInjection {

    public interface IDependsOn<T> {

        void Inject(T dependency);

    }

}
