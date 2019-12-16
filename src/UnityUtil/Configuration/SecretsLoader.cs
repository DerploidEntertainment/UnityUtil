using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UnityEngine {

    public abstract class SecretsLoader {

        public void LoadSecrets(IEnumerable<ISecretSource> secretSources) {
            using var async = new AsyncCaller();
            async.CallAsync(async cancellationToken => {
                string[] secrets = await Task.WhenAll(secretSources.Select(s => s.LoadSecrets(cancellationToken)));
                secrets = secrets.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                if (!cancellationToken.IsCancellationRequested)
                    await SaveSecretsAsync(secrets, cancellationToken);
            });
        }

        protected abstract Task SaveSecretsAsync(string[] secrets, CancellationToken cancellationToken);

    }

}
