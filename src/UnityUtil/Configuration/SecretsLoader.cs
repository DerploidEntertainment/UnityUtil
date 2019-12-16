using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Logging;

namespace UnityEngine {

    public abstract class SecretsLoader {

        protected readonly ILoggerProvider LoggerProvider;
        private readonly ILogger _logger;

        protected SecretsLoader(ILoggerProvider loggerProvider) {
            LoggerProvider = loggerProvider;
            _logger = LoggerProvider.GetLogger(this);
        }

        public void LoadSecrets(IEnumerable<ISecretSource> secretSources) {
            using var async = new AsyncCaller(LoggerProvider);
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
