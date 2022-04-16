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
using System.Threading.Tasks;

namespace UnityEngine;

/// <summary>
/// Original author: Pim de Witte (pimdewitte.com) and contributors, https://github.com/PimDeWitte/UnityMainThreadDispatcher.
/// Encapsulates a thread-safe queue with actions to execute on the next update loop.
/// It can be used to make calls to Unity's main thread for things such as UI manipulation.
/// It was developed for use in combination with the Firebase Unity plugin, which uses separate threads for event handling.
/// </summary>
public interface IUnityMainThreadDispatcher
{

    /// <summary>
    /// Locks the queue and adds <paramref name="action"/> to the queue
    /// </summary>
    /// <param name="action">Action that will be executed from Unity's main thread.</param>
    void Enqueue(Action action);

    /// <summary>
    /// Locks the queue and adds <paramref name="action"/> to the queue, returning a <see cref="Task"/> which is completed when the action completes
    /// </summary>
    /// <param name="action">Action that will be executed from Unity's main thread.</param>
    /// <returns>A <see cref="Task"/> that can be awaited until the action completes</returns>
    public Task EnqueueAsync(Action action);

}
