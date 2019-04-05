using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UnityEngine {

    public abstract class SecretsLoader {

        public void LoadSecrets(IEnumerable<ISecretSource> secretSources) {
            var async = new AsyncCaller();
            async.CallAsync(async ct => {
                string[] secrets = await Task.WhenAll(secretSources.Select(s => s.LoadSecrets(ct)));
                secrets = secrets.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                if (!ct.IsCancellationRequested)
                    await SaveSecretsAsync(secrets, ct);
            });
        }

        protected abstract Task SaveSecretsAsync(string[] secrets, CancellationToken cancellationToken);

    }

}
