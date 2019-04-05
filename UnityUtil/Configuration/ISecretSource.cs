using System;
using System.Threading;
using System.Threading.Tasks;

namespace UnityEngine {

    public interface ISecretSource {

        Task<string> LoadSecrets(CancellationToken cancellationToken);

    }

}
