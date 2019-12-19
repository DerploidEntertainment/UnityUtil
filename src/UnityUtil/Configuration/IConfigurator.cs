using System.Collections.Generic;

namespace UnityEngine {

    public interface IConfigurator {

        void Configure(MonoBehaviour client);
        void Configure(IEnumerable<MonoBehaviour> clients);

    }

}
