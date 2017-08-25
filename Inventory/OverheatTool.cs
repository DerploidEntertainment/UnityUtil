using UnityEngine;
using U = UnityEngine;
using UnityEngine.Events;
using UnityEngine.Assertions;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Danware.Unity.Input;

namespace Danware.Unity.Inventory {

    [RequireComponent(typeof(Tool))]
    public class OverheatTool : MonoBehaviour {
        // ABSTRACT DATA TYPES
        /// <summary>
        /// Type arguments are (bool isOverheated)
        /// </summary>
        [Serializable]
        public class OverheatChangedEvent : UnityEvent<bool> { }

        // HIDDEN FIELDS
        private Tool _tool;
        private Coroutine _overheatRoutine;

        // INSPECTOR FIELDS
        [Tooltip("The maximum amount of heat that can be generated before the Weapon becomes unusable (overheats).")]
        public float MaxHeat = 100f;
        [Tooltip("The amount of heat generated per use.")]
        public float HeatGeneratedPerUse = 50f;
        [Tooltip("If true, then the Weapon cools by an absolute amount per second.  If false, then the Weapon cools by a fraction of MaxHeat per second.")]
        public bool AbsoluteHeat = true;
        [Tooltip("The Weapon will cool by this many units (or this fraction of MaxHeat) per second, unless overheated.")]
        public float CoolRate = 75f;
        [Tooltip("Once overheated, this many seconds must pass before the Weapon will start cooling again.")]
        public float OverheatDuration = 2f;
        [Tooltip("The amount of heat that this Tool has when instantiated.  Must be <= MaxHeat.")]
        public int StartingHeat;

        // API INTERFACE
        public float CurrentHeat { get; private set; } = 0f;
        public OverheatChangedEvent OverheatStateChanged = new OverheatChangedEvent();

        // EVENT HANDLERS
        private void Awake() {
            Assert.IsTrue(StartingHeat <= MaxHeat, $"{nameof(OverheatTool)} {transform.parent.name}.{name} was started with {nameof(this.StartingHeat)} heat but it can only store a max of {MaxHeat}!");

            // Initialize heat
            CurrentHeat = StartingHeat;

            // Register Tool events
            _tool = GetComponent<Tool>();
            _tool.Using.AddListener(() =>
                _tool.Using.Cancel = CurrentHeat > MaxHeat);
            _tool.Used.AddListener(() => {
                float heat = HeatGeneratedPerUse * (AbsoluteHeat ? 1f : MaxHeat);
                CurrentHeat += heat;
                if (CurrentHeat > MaxHeat) {
                    OverheatStateChanged.Invoke(true);
                    _overheatRoutine = StartCoroutine(doOverheatDuration());
                }
            });
        }
        private void Update() {
            // Cool this Tool, unless it is overheated
            if (CurrentHeat > 0 && _overheatRoutine == null) {
                float rate = AbsoluteHeat ? CoolRate : CoolRate * MaxHeat;
                CurrentHeat = Mathf.Max(0, CurrentHeat - Time.deltaTime * rate);
            }
        }

        // HELPERS
        private IEnumerator doOverheatDuration() {
            yield return new WaitForSeconds(OverheatDuration);
            OverheatStateChanged.Invoke(false);

            _overheatRoutine = null;
        }

    }

}
