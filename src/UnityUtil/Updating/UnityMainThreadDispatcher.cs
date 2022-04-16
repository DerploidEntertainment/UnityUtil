/*
 * This file copied from Pim de Witte's original repo on GitHub: https://github.com/PimDeWitte/UnityMainThreadDispatcher
 * That repo is licensed under the Apache License 2.0: https://spdx.org/licenses/Apache-2.0.html
 * That license requires me to document changes, so... 
 *      - I made a couple tweaks to satisfy Visual Studio's compiler/style warnings
 *      - It no longer derives from MonoBehaviour, and is just a simple class that registers itself with the update system
 *      - Added it to the UnityEngine namespace, like the rest of UnityUtil
 *      - Split out its public methods into a separate interface for dependency injection
 *      - Removed the Enqueue(IEnumerator) overload b/c I seldom use coroutines and consider them kind of an anti-pattern
*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UnityEngine;

/// <summary>
/// Original author: Pim de Witte (pimdewitte.com) and contributors, https://github.com/PimDeWitte/UnityMainThreadDispatcher.
/// A thread-safe class which holds a queue with actions to execute on the next update loop.
/// It can be used to make calls to Unity's main thread for things such as UI manipulation.
/// It was developed for use in combination with the Firebase Unity plugin, which uses separate threads for event handling.
/// </summary>
public class UnityMainThreadDispatcher : IUnityMainThreadDispatcher
{

    private readonly IUpdater _updater;
    private readonly Queue<Action> _actionQueue = new();
    public int InstanceID { get; private set; }

    public UnityMainThreadDispatcher(IUpdater updater, IRuntimeIdProvider runtimeIdProvider)
    {
        _updater = updater;
        InstanceID = runtimeIdProvider.GetId();

        _updater.RegisterUpdate(InstanceID, processActionQueue);
    }

    ~UnityMainThreadDispatcher()
    {
        _updater.UnregisterUpdate(InstanceID);
    }

    private void processActionQueue(float deltaTime)
    {
        lock (_actionQueue) {
            while (_actionQueue.Count > 0)
                _actionQueue.Dequeue().Invoke();
        }
    }

    /// <inheritdoc/>
    public void Enqueue(Action action) => _actionQueue.Enqueue(action);

    /// <inheritdoc/>
    public Task EnqueueAsync(Action action)
    {
        var tcs = new TaskCompletionSource<bool>();

        Enqueue(() => {
            try {
                action();
                tcs.TrySetResult(true);
            }

#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex) {
                tcs.TrySetException(ex);
            }
#pragma warning restore CA1031 // Do not catch general exception types
        });

        return tcs.Task;
    }

}
