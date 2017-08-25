using UnityEngine;
using UnityEngine.Assertions;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Danware.Unity.Inventory {
    
    public class Inventory : MonoBehaviour {
        // ABSTRACT DATA TYPES
        public class InventoryEventArgs : EventArgs {
            public InventoryEventArgs(Inventory inventory) {
                Inventory = inventory;
            }
            public Inventory Inventory { get; }
        }
        public class InventoryItemEventArgs : InventoryEventArgs {
            public InventoryItemEventArgs(Inventory inventory, InventoryCollectible item) : base(inventory) {
                Collectible = item;
            }
            public InventoryCollectible Collectible { get; }
        }

        // HIDDEN FIELDS
        private IList<InventoryCollectible> _collectibles = new List<InventoryCollectible>();
        private EventHandler<InventoryItemEventArgs> _collectedInvoker;
        private EventHandler<InventoryItemEventArgs> _droppedInvoker;

        // INSPECTOR FIELDS
        public int MaxItems = 10;
        [Tooltip("If dropped, items will take this many seconds to become collectible again")]
        public float DropRefactoryPeriod = 1.5f;
        public Vector3 LocalDropOffset = Vector3.one;
        public event EventHandler<InventoryItemEventArgs> ItemCollected {
            add { _collectedInvoker += value; }
            remove { _collectedInvoker -= value; }
        }
        public event EventHandler<InventoryItemEventArgs> ItemDropped {
            add { _droppedInvoker += value; }
            remove { _droppedInvoker -= value; }
        }

        // INTERFACE FUNCTIONS
        public ReadOnlyCollection<GameObject> Items {
            get {
                IList<GameObject> items = _collectibles.Select(c => c.ItemRoot).ToList();
                return new ReadOnlyCollection<GameObject>(items);
            }
        }
        public bool Collect(InventoryCollectible collectible) {
            // Make sure an actual Collectible was provided, and that there is room for it
            Assert.IsNotNull(collectible, $"{nameof(Inventory)} {transform.parent.name}.{name} was told to collect null in frame {Time.frameCount}!");

            // Collect it, if there is room
            bool canCollect = (_collectibles.Count < MaxItems && !_collectibles.Contains(collectible));
            if (canCollect)
                doCollect(collectible);

            return canCollect;
        }
        public void Drop(InventoryCollectible collectible) {
            // Make sure a valid collectible was provided
            Assert.IsNotNull(collectible, $"{nameof(Inventory)} {transform.parent.name}.{name} cannot drop null!");
            Assert.IsTrue(_collectibles.Contains(collectible), $"{nameof(Inventory)} {transform.parent.name}.{name} was told to drop an {nameof(InventoryCollectible)} that it never collected!");

            StartCoroutine(doDrop(collectible));
        }
        public void DropAll() {
            IList<InventoryCollectible> collectibles = _collectibles;
            collectibles.DoWith(c => doDrop(c));
        }

        // HELPERS
        private void doCollect(InventoryCollectible collectible) {
            // Place the collectible in the Inventory
            _collectibles.Add(collectible);

            // Do collect actions
            Transform itemTrans = collectible.ItemRoot.transform;
            itemTrans.parent = transform;
            itemTrans.localPosition = new Vector3(0f, 0f, 0f);
            itemTrans.localRotation = Quaternion.identity;
            collectible.Root.SetActive(false);

            // Raise the item collected event
            Debug.Log($"{nameof(Inventory)} {name} collected {collectible.ItemRoot.name} in frame {Time.frameCount}");
            var args = new InventoryItemEventArgs(this, collectible);
            _collectedInvoker?.Invoke(this, args);
        }
        private IEnumerator doDrop(InventoryCollectible collectible) {
            // Drop it as a new Collectible
            collectible.Root.SetActive(true);
            collectible.transform.position = transform.TransformPoint(LocalDropOffset);
            Transform itemTrans = collectible.ItemRoot.transform;
            itemTrans.parent = transform;

            // Remove the provided collectible from the Inventory
            _collectibles.Remove(collectible);

            // Raise the item dropped event
            Debug.Log($"{nameof(Inventory)} {name} dropped {collectible.ItemRoot.name} in frame {Time.frameCount}");
            var args = new InventoryItemEventArgs(this, collectible);
            _droppedInvoker?.Invoke(this, args);

            // Prevent its re-collection for the requested duration
            collectible.CollidersToToggle.DoWith(c => c.enabled = false);
            yield return new WaitForSeconds(DropRefactoryPeriod);
            collectible.CollidersToToggle.DoWith(c => c.enabled = true);
        }

    }

}
