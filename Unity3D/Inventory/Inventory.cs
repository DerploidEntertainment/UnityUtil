﻿using UnityEngine;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Danware.Unity3D.Inventory {

    public class Inventory : MonoBehaviour {
        // ABSTRACT DATA TYPES
        private struct ItemData {
            public Collectible OldCollectible;
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
        public float DroppedItemRefactoryPeriod = 1.5f;
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
            get { return new ReadOnlyCollection<GameObject>(_items.Keys.ToList()); }
        }
        public void Collect(Collectible collect) {
            Debug.AssertFormat(_items.Count < MaxItems, "Inventory {0} cannot collect {1} because the former is already full!", this.name, collect.name);
            Debug.AssertFormat(collect != null, "Inventory {0} cannot collect null!", this.name);

            // Parent the Collectible's contained item to this Inventory's GameObject
            GameObject item = collect.Item;
            item.transform.parent = transform;
            item.transform.localPosition = new Vector3(0f, 0f, 0f);
            item.transform.localRotation = Quaternion.identity;

            // Deactivate the old Collectible
            collect.gameObject.SetActive(false);

            // Place the item in the Inventory
            ItemData data = new ItemData() {
                OldCollectible = collect,
            };
            _items.Add(item, data);

            // Raise the item collected event
            Debug.LogFormat("Inventory {0} collected {1} in frame {2}", this.name, collect.name, Time.frameCount);
            ItemEventArgs args = new ItemEventArgs() {
                Inventory = this,
                Item = item,
            };
            _collectedInvoker?.Invoke(this, args);
        }
        public void DropItem(GameObject item) {
            if (item == null)
                return;

            // Drop it as a new Collectible
            Collectible collect = _items[item].OldCollectible;
            collect.transform.position = transform.position;
            item.transform.parent = collect.transform;
            collect.gameObject.SetActive(true);

            // Remove the item from the Inventory
            _items.Remove(item);

            // Prevent the Collectible from being collected again until a refactory period has passed
            StartCoroutine(pauseCollectibility(collect));

            // Raise the item dropped event
            Debug.LogFormat("Inventory {0} dropped item {1} in frame {2}", this.name, item.name, Time.frameCount);
            ItemEventArgs args = new ItemEventArgs() {
                Inventory = this,
                Item = item,
            };
            _droppedInvoker?.Invoke(this, args);
        }
        public void DropAll() {
            GameObject[] items = _items.Keys.ToArray();
            foreach (GameObject item in items)
                DropItem(item);
        }

        // HELPER FUNCTIONS
        private IEnumerator pauseCollectibility(Collectible collect) {
            collect.enabled = false;
            yield return new WaitForSeconds(DroppedItemRefactoryPeriod);
            collect.enabled = true;
        }

    }

}
