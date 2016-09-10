using UnityEngine;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Danware.Unity.Inventory {

    public class Inventory : MonoBehaviour {
        // ABSTRACT DATA TYPES
        private struct ItemData {
            public InventoryCollectible OldCollectible;
        }
        public class InventoryEventArgs : EventArgs {
            public Inventory Inventory { get; set; }
        }
        public class ItemEventArgs : InventoryEventArgs {
            public GameObject Item { get; set; }
        }

        // HIDDEN FIELDS
        private EventHandler<ItemEventArgs> _collectedInvoker;
        private EventHandler<ItemEventArgs> _droppedInvoker;
        private IDictionary<GameObject, ItemData> _items = new Dictionary<GameObject, ItemData>();

        // INSPECTOR FIELDS
        public int MaxItems = 10;
        public event EventHandler<ItemEventArgs> ItemCollected {
            add { _collectedInvoker += value; }
            remove { _collectedInvoker -= value; }
        }
        public event EventHandler<ItemEventArgs> ItemDropped {
            add { _droppedInvoker += value; }
            remove { _droppedInvoker -= value; }
        }

        // INTERFACE FUNCTIONS
        public ReadOnlyCollection<GameObject> Items {
            get {
                GameObject[] items = new GameObject[_items.Count];
                _items.Keys.CopyTo(items, 0);
                return new ReadOnlyCollection<GameObject>(items);
            }
        }
        public bool Give(InventoryCollectible collect) {
            // Make sure an actual Collectible was provided, and that there is room for it
            if (collect == null || _items.Count == MaxItems)
                return false;

            // Place the item in the Inventory
            GameObject item = collect.Item;
            ItemData data = new ItemData() {
                OldCollectible = collect,
            };
            _items.Add(item, data);

            // Raise the item collected event
            Debug.LogFormat("Inventory {0} collected {1} in frame {2}", this.name, item.name, Time.frameCount);
            ItemEventArgs args = new ItemEventArgs() {
                Inventory = this,
                Item = item,
            };
            _collectedInvoker?.Invoke(this, args);

            return true;
        }
        public void DropItem(GameObject item) {
            // Make sure a valid item was provided
            Debug.Assert(item != null, $"{nameof(Inventory)} {name} cannot drop null!");
            Debug.Assert(_items.ContainsKey(item), $"{nameof(Inventory)} {name} was told to drop an Item that it has not collected!");

            // If so, remove it from the Inventory
            InventoryCollectible collect = _items[item].OldCollectible;
            collect.Drop(transform);
            _items.Remove(item);

            // Raise the item dropped event
            Debug.Log($"{nameof(Inventory)} {name} dropped item {item.name} in frame {Time.frameCount}");
            ItemEventArgs args = new ItemEventArgs() {
                Inventory = this,
                Item = item,
            };
            _droppedInvoker?.Invoke(this, args);
        }
        public void DropAllItems() {
            GameObject[] items = new GameObject[_items.Count];
            _items.Keys.CopyTo(items, 0);
            foreach (GameObject item in items)
                DropItem(item);
        }

    }

}
