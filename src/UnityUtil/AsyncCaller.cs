using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace UnityUtil;

public delegate Task AsyncAction(CancellationToken cancellationToken);

public sealed class AsyncCaller : IDisposable
{
    private readonly RootLogger<AsyncCaller> _logger;
    private readonly CancellationTokenSource _cts = new();

    public AsyncCaller(ILoggerFactory loggerFactory)
    {
        _logger = new(loggerFactory, context: this);
    }

    public async void CallAsync(AsyncAction action) => await action.Invoke(_cts.Token).ConfigureAwait(false);

    public void Dispose()
    {
        try { _cts.Cancel(); }
        catch (Exception ex) {
            _logger.AsyncCallerDisposeFail(ex);
        }

        _cts.Dispose();
    }
}
