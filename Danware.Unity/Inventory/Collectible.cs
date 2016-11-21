using System;
using UnityEngine;

namespace Danware.Unity.Inventory {
    
    public class Collectible : MonoBehaviour {

        // ABSTRACT DATA TYPES
        public class CollectibleEventArgs : EventArgs {
            public CollectibleEventArgs(Collectible collectible) {
                Collectible = collectible;
            }
            public Collectible Collectible { get; }
        }
        public class CollectedEventArgs : CollectibleEventArgs {
            public CollectedEventArgs(Collectible collectible, Transform targetRoot) : base (collectible) {
                TargetRoot = targetRoot;
            }
            public Transform TargetRoot { get; }
        }

        // HIDDEN FIELDS
        private EventHandler<CollectedEventArgs> _collectInvoker;

        // API INTERFACE
        public void Collect(Transform targetRoot) {
            CollectedEventArgs args = new CollectedEventArgs(this, targetRoot);
            _collectInvoker?.Invoke(this, args);
        }
        public event EventHandler<CollectedEventArgs> Collected {
            add { _collectInvoker += value; }
            remove { _collectInvoker -= value; }
        }

    }

}
