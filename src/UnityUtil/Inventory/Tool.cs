using System.Collections;
using UnityEngine.Events;
using UnityEngine.Inputs;
using UnityEngine.Logging;

namespace UnityEngine.Inventory {

    public class Tool : Updatable {
        // HIDDEN FIELDS
        private Coroutine _usingRoutine;
        private Coroutine _refractoryRoutine;
        private uint _numUses = 0u;

        // INSPECTOR FIELDS
        public ToolInfo Info;
        public StartStopInput UseInput;

        // API INTERFACE
        /// <summary>
        /// The current charge of this <see cref="Tool"/>.  0 is completely uncharged, 1 is completely charged.
        /// </summary>
        public float CurrentCharge { get; private set; } = 0f;
        public CancellableUnityEvent Using = new CancellableUnityEvent();
        public UnityEvent Used = new UnityEvent();
        public UnityEvent UseFailed = new UnityEvent();

        // EVENT HANDLERS
        protected override void BetterAwake() {
            this.AssertAssociation(Info, nameof(ToolInfo));
            this.AssertAssociation(UseInput, nameof(this.UseInput));

            BetterUpdate = doUpdate;
            RegisterUpdatesAutomatically = true;
        }
        private void doUpdate(float deltaTime) {
            // Start using when the use input starts
            if (UseInput.Started() && _usingRoutine == null && _refractoryRoutine == null)
                _usingRoutine = StartCoroutine(startUsing());

            // Stop using when the use input stops
            // If the Tool is automatic and the player got a use in before stopping the UseInput,
            // then start the refractory period still
            else if (UseInput.Stopped() && _usingRoutine != null) {
                StopCoroutine(_usingRoutine);
                _usingRoutine = null;
                CurrentCharge = 0f;
                if (_numUses > 0)
                    _refractoryRoutine = StartCoroutine(startRefractoryPeriod());
            }
        }
        protected override void BetterOnDisable() {
            if (_usingRoutine != null) {
                StopCoroutine(_usingRoutine);
                _usingRoutine = null;
                CurrentCharge = 0f;
            }
        }

        // HELPERS
        private IEnumerator startUsing() {
            _numUses = 0;
            do {

                // Raise the charging started event
                // If any listeners canceled using, then raise the Use Failed event
                Using.Invoke();
                if (Using.Cancel)
                    UseFailed.Invoke();

                // Otherwise, recharge the Tool (if necessary) then use it
                else {
                    if ((_numUses == 0 || Info.RechargeEveryUse) && Info.TimeToCharge > 0f) {
                        CurrentCharge = 0f;
                        while (CurrentCharge < 1f) {
                            CurrentCharge += Time.deltaTime / Info.TimeToCharge;
                            yield return null;
                        }
                        CurrentCharge = 1f;     // Clamp fraction
                    }
                    Used.Invoke();
                    ++_numUses;
                }

                // Pause to account for the firing rate, if necessary
                if (Info.AutomaticMode != AutomaticMode.SingleAction && !Info.RechargeEveryUse)
                    yield return new WaitForSeconds(1f / Info.AutomaticUseRate);

            // Continue using if the Tool is fully automatic or has not performed all of its semi-automatic uses
            } while (
                (Info.AutomaticMode == AutomaticMode.SemiAutomatic && _numUses < Info.SemiAutomaticUses) ||
                (Info.AutomaticMode == AutomaticMode.FullyAutomatic)
            );

            // Prevent using again for the duration of the refractory period
            _refractoryRoutine = StartCoroutine(startRefractoryPeriod());
            yield return _refractoryRoutine;

            CurrentCharge = 0f;
            _usingRoutine = null;
        }
        private IEnumerator startRefractoryPeriod() {
            yield return new WaitForSeconds(Info.RefactoryPeriod);
            _refractoryRoutine = null;
        }

    }

}
