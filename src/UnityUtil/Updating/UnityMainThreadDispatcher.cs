/*
 * This file copied from Pim de Witte's original repo on GitHub: https://github.com/PimDeWitte/UnityMainThreadDispatcher
 * That repo is licensed under the Apache License 2.0: https://spdx.org/licenses/Apache-2.0.html
 * That license requires me to document changes, so... 
 *      - I made a couple tweaks to satisfy Visual Studio's compiler/style warnings
 *      - It now derives from the UnityUtil Updatable class so that its part of our custom, managed update loop
 *      - Added it to the UnityEngine namespace, like the rest of UnityUtil
 *      - Split out its public methods into a separate interface for dependency injection
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace UnityEngine
{

    /// <summary>
    /// Original author: Pim de Witte (pimdewitte.com) and contributors, https://github.com/PimDeWitte/UnityMainThreadDispatcher.
    /// A thread-safe class which holds a queue with actions to execute on the next update loop.
    /// It can be used to make calls to Unity's main thread for things such as UI manipulation.
    /// It was developed for use in combination with the Firebase Unity plugin, which uses separate threads for event handling.
    /// </summary>
    public class UnityMainThreadDispatcher : Updatable, IUnityMainThreadDispatcher
    {

        private static readonly Queue<Action> s_actionQueue = new Queue<Action>();

        private void processActionQueue(float deltaTime) {
            lock (s_actionQueue) {
                while (s_actionQueue.Count > 0)
                    s_actionQueue.Dequeue().Invoke();
            }
        }

        /// <inheritdoc/>
        public void Enqueue(IEnumerator action) {
            lock (s_actionQueue) {
                s_actionQueue.Enqueue(() => StartCoroutine(action));
            }
        }

        /// <inheritdoc/>
        public void Enqueue(Action action) => Enqueue(actionEnumerator(action));

        /// <inheritdoc/>
        public Task EnqueueAsync(Action action) {
            var tcs = new TaskCompletionSource<bool>();

            Enqueue(actionEnumerator(() => {
                try {
                    action();
                    tcs.TrySetResult(true);
                }

                #pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex) {
                    tcs.TrySetException(ex);
                }
                #pragma warning restore CA1031 // Do not catch general exception types
            }));

            return tcs.Task;
        }

        private IEnumerator actionEnumerator(Action action) {
            action();
            yield return null;
        }

        protected override void Awake() {
            base.Awake();

            BetterUpdate = processActionQueue;
            RegisterUpdatesAutomatically = true;
        }

    }

}
