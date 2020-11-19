using System.Collections.Generic;

namespace UnityEngine {

    public interface IConfigurator {

        void Configure(object client, string cacheKey);
        void Configure(IEnumerable<(object, string)> clients);

    }

}
