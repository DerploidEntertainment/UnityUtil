using UnityEngine;

using System;
using System.Linq;
using System.Collections.Generic;

using Danware.Unity.Input;

namespace Danware.Unity.Inventory {

    public class Hotbar : MonoBehaviour {
        // ABSTRACT DATA TYPES
        public class HotbarEventArgs : EventArgs {
            public HotbarEventArgs(Hotbar hotbar) {
                Hotbar = hotbar;
            }
            public Hotbar Hotbar { get; }
        }
        public class SlotEventArgs : HotbarEventArgs {
            public SlotEventArgs(Hotbar hotbar, int slot) : base(hotbar) {
                Slot = slot;
            }
            public int Slot { get; }
        }

        // HIDDEN FIELDS
        private EventHandler<SlotEventArgs> _filledInvoker;
        private EventHandler<SlotEventArgs> _emptiedInvoker;
        private EventHandler<SlotEventArgs> _equippedInvoker;
        private EventHandler<SlotEventArgs> _uneqippedInvoker;
        private GameObject[] _slots;
        private List<int> _slotsEquipped;

        // INSPECTOR FIELDS
        [Header("Inputs")]
        public StartStopInput DropInput;
        public StartStopInputArray EquipInput;

        [Header("Options")]
        public Inventory Inventory;
        [Range(0f, 10f)]
        public int NumSlots = 10;
        [Range(0f, 10f)]
        public int NumEquippableSlots = 1;
        public bool EquipOnCollect = false;

        // API INTERFACE
        public event EventHandler<SlotEventArgs> SlotFilled {
            add { _filledInvoker += value; }
            remove { _filledInvoker -= value; }
        }
        public event EventHandler<SlotEventArgs> SlotEmptied {
            add { _emptiedInvoker += value; }
            remove { _emptiedInvoker -= value; }
        }
        public event EventHandler<SlotEventArgs> SlotEquipped {
            add { _equippedInvoker += value; }
            remove { _equippedInvoker -= value; }
        }
        public event EventHandler<SlotEventArgs> SlotUnequipped {
            add { _uneqippedInvoker += value; }
            remove { _uneqippedInvoker -= value; }
        }
        public GameObject[] Slots {
            get { return _slots.ToArray(); }
        }
        public int[] EquippedSlots {
            get { return _slotsEquipped.ToArray(); }
        }
        public void EquipSlot(int slot) {
            // Only equip this slot if it is not already equipped and slots are available
            if (!_slotsEquipped.Contains(slot) && _slotsEquipped.Count < NumEquippableSlots)
                equipSlot(slot);
        }
        public void UnequipSlot(int slot) {
            // Don't try to unequip slots that have not already been equipped
            if (_slotsEquipped.Contains(slot))
                unequipSlot(slot);
        }
        public void UnequipAll() {
            unequipAll();
        }

        // EVENT HANDLERS
        private void Awake() {
            Debug.AssertFormat(NumEquippableSlots < NumSlots, "Hotbar {0} was given {1} equippable slots but has only {2} total slots!", this.name, NumEquippableSlots, NumSlots);

            _slots = new GameObject[NumSlots];
            _slotsEquipped = new List<int>(NumEquippableSlots);

            Inventory.ItemCollected += handleItemCollected;
            Inventory.ItemDropped += handleItemDropped;
        }
        private void Update() {
            // Get player input
            bool dropped = DropInput.Started;
            bool[] toggled = EquipInput.Started;

            // If the player pressed Drop, then Drop all currently equipped items
            if (dropped) {
                int[] equipped = _slotsEquipped.ToArray();
                foreach (int slot in equipped)
                    Inventory.DropItem(_slots[slot]);
            }

            // If the player pressed any equip item buttons, then toggle those slots' equipped states
            toggleSlots(toggled.ToArray());
        }

        // HELPER FUNCTIONS
        private void handleItemCollected(object sender, Inventory.ItemEventArgs e) {
            // Place the newly collected item in the first available slot
            GameObject item = e.Item;
            for (int s = 0; s < NumSlots; ++s) {
                if (_slots[s] != null)
                    continue;

                // Raise the slot filled event
                _slots[s] = item;
                SlotEventArgs args = new SlotEventArgs(this, s);
                _filledInvoker?.Invoke(this, args);

                // Equip the item (and it alone), if requested
                if (EquipOnCollect) {
                    unequipAll();
                    equipSlot(s);
                }
                break;
            }
        }
        private void handleItemDropped(object sender, Inventory.ItemEventArgs e) {
            // Clear this item's slot if it was actually on the Hotbar, unequipping it first if necessary
            GameObject item = e.Item;
            for (int s = 0; s < NumSlots; ++s) {
                if (_slots[s] != item)
                    continue;

                UnequipSlot(s);
                _slots[s] = null;
                SlotEventArgs args = new SlotEventArgs(this, s);
                _emptiedInvoker?.Invoke(this, args);
                break;
            }
        }
        private void toggleSlots(bool[] toggled) {
            // If no slots were toggled then just return
            int numToggled = toggled.Count(s => s == true);
            if (numToggled == 0)
                return;

            short count = 0;
            short togglesToRemove = (short)Mathf.Max(numToggled - NumEquippableSlots, 0f);
            for (int s = NumSlots - 1; s >= 0; --s) {
                if (toggled[s]) {
                    if (count < togglesToRemove)
                        ++count;
                    else {
                        bool equipped = _slotsEquipped.Contains(s);
                        if (!equipped)
                            equipSlot(s);
                        else
                            unequipSlot(s);
                    }
                }
                else {
                    bool equipped = _slotsEquipped.Contains(s);
                    if (equipped)
                    unequipSlot(s);
            }
        }
        }
        private void equipSlot(int slot) {
            // Equip the slot (i.e., if an item is there, activate its GameObject and render its model)
            _slotsEquipped.Add(slot);
            GameObject item = _slots[slot];
            if (item != null) {
                item.SetActive(true);
                Equippable eq = item.GetComponent<Equippable>();
                eq?.Equip(item.transform);
            }

            // Raise the slot equipped event
            Debug.LogFormat("Hotbar {0} equipped slot {1} in frame {2}", this.name, slot, Time.frameCount);
            SlotEventArgs args = new SlotEventArgs(this, slot);
            _equippedInvoker?.Invoke(this, args);
        }
        private void unequipSlot(int slot) {
            // Unequip the slot (deactivate its GameObject), if one was provided
            _slotsEquipped.Remove(slot);
            GameObject item = _slots[slot];
            if (item != null)
                item.SetActive(false);

            // Raise the slot unequipped event
            Debug.LogFormat("Hotbar {0} unequipped slot {1} in frame {2}", this.name, slot, Time.frameCount);
            SlotEventArgs args = new SlotEventArgs(this, slot);
            _uneqippedInvoker?.Invoke(this, args);
        }
        private void unequipAll() {
            int[] slots = _slotsEquipped.ToArray();
            foreach (int slot in slots)
                unequipSlot(slot);
        }
    }

}
