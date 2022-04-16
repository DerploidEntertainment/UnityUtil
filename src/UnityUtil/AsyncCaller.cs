using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Logging;

namespace UnityEngine;

public delegate Task AsyncAction(CancellationToken cancellationToken);

public sealed class AsyncCaller : IDisposable
{
    private readonly ILogger _logger;
    private readonly CancellationTokenSource _cts = new();

    public AsyncCaller(ILoggerProvider loggerProvider)
    {
        _logger = loggerProvider.GetLogger(this);
    }

    public async void CallAsync(AsyncAction action) => await action.Invoke(_cts.Token).ConfigureAwait(false);

    public void Dispose()
    {
        try { _cts.Cancel(); }
        catch (Exception ex) {
            _logger.LogError($"Threw an exception during Dispose: {ex}");
        }

        _cts.Dispose();
    }
}
