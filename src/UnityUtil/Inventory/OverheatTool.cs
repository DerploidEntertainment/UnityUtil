using System;
using System.Collections;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.Logging;

namespace UnityEngine.Inventory {

    // ABSTRACT DATA TYPES
    /// <summary>
    /// Type arguments are (bool isOverheated)
    /// </summary>
    [Serializable]
    public class OverheatChangedEvent : UnityEvent<bool> { }

    [RequireComponent(typeof(Tool))]
    public class OverheatTool : Updatable {

        // HIDDEN FIELDS
        private Tool _tool;
        private Coroutine _overheatRoutine;

        // INSPECTOR FIELDS
        public OverheatToolInfo Info;

        // API INTERFACE
        public float CurrentHeat { get; private set; } = 0f;
        public OverheatChangedEvent OverheatStateChanged = new();

        // EVENT HANDLERS
        protected override void Awake() {
            base.Awake();

            this.AssertAssociation(Info, nameof(OverheatToolInfo));
            Assert.IsTrue(Info.StartingHeat <= Info.MaxHeat, $"{this.GetHierarchyNameWithType()} was started with {nameof(this.Info.StartingHeat)} heat but it can only store a max of {this.Info.MaxHeat}!");

            BetterUpdate = doUpdate;
            RegisterUpdatesAutomatically = true;

            // Initialize heat
            CurrentHeat = Info.StartingHeat;

            // Register Tool events
            _tool = GetComponent<Tool>();
            _tool.Using.AddListener(() =>
                _tool.Using.Cancel = CurrentHeat > Info.MaxHeat);
            _tool.Used.AddListener(() => {
                float heat = Info.HeatGeneratedPerUse * (Info.AbsoluteHeat ? 1f : Info.MaxHeat);
                CurrentHeat += heat;
                if (CurrentHeat > Info.MaxHeat) {
                    OverheatStateChanged.Invoke(true);
                    _overheatRoutine = StartCoroutine(doOverheatDuration());
                }
            });
        }
        private void doUpdate(float deltaTime) {
            // Cool this Tool, unless it is overheated
            if (CurrentHeat > 0 && _overheatRoutine is null) {
                float rate = Info.AbsoluteHeat ? Info.CoolRate : Info.CoolRate * Info.MaxHeat;
                CurrentHeat = Mathf.Max(0, CurrentHeat - deltaTime * rate);
            }
        }

        // HELPERS
        private IEnumerator doOverheatDuration() {
            yield return new WaitForSeconds(Info.OverheatDuration);
            OverheatStateChanged.Invoke(false);

            _overheatRoutine = null;
        }

    }

}
