using System;
using System.Threading;
using System.Threading.Tasks;

namespace UnityEngine {

    public delegate Task AsyncAction(CancellationToken cancellationToken);

    public sealed class AsyncCaller : IDisposable {

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        public async void CallAsync(AsyncAction action) => await action.Invoke(_cts.Token);

        public void Dispose() {
            try { _cts.Cancel(); }
            catch (Exception ex) {
                BetterLogger.LogError($" threw an exception during Dispose: {ex}");
            }

            _cts.Dispose();
        }
    }

}
