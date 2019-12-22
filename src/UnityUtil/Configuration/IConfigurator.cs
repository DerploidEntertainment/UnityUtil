using System.Collections.Generic;

namespace UnityEngine {

    public interface IConfigurator {

        void Configure(object client);
        void Configure(IEnumerable<object> clients);

    }

}
