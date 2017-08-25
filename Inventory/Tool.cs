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

    public enum AutomaticMode {
        SingleAction,
        SemiAutomatic,
        FullyAutomatic,
    }

    public class Tool : MonoBehaviour {
        // HIDDEN FIELDS
        private Coroutine _usingRoutine;
        private Coroutine _refractoryRoutine;
        private uint _numUses = 0u;

        // INSPECTOR FIELDS
        public StartStopInput UseInput;
        [Space]
        public AutomaticMode AutomaticMode;
        [Tooltip("The UseInput must be maintained for this many seconds before the Tool will perform its first use.")]
        public float TimeToCharge = 0f;
        [Tooltip("Once a (semi-)automatic Tool starts being used, it performs this many uses per second while UseInput is maintained.  Ignored if AutomaticMode is SingleAction.")]
        public float AutomaticUseRate = 1f;
        [Tooltip("A semi-automatic Tool performs this many uses once UseInput is started.  UseInput will be ignored until these uses have all completed.  This value is ignored if AutomaticMode is not set to SemiAutomatic.")]
        public int SemiAutomaticUses = 3;
        [Tooltip("After the last use, this many seconds must pass before the Tool will respond to UseInput again.")]
        public float RefactoryPeriod = 0f;
        [Tooltip("For (semi-)automatic Tools, if this value is true, then AutomaticUseRate will be ignored, and the Tool will recharge before every use while UseInput is maintained.")]
        public bool RechargeEveryUse = false;

        // API INTERFACE
        /// <summary>
        /// The current charge of this Tool.  0 is completely uncharged, 1 is completely charged.
        /// </summary>
        public float CurrentCharge { get; private set; } = 0f;
        public CancellableUnityEvent Using = new CancellableUnityEvent();
        public UnityEvent Used = new UnityEvent();
        public UnityEvent UseFailed = new UnityEvent();

        // EVENT HANDLERS
        private void Awake() =>
            Assert.IsNotNull(UseInput, $"{nameof(Weapon)} {transform.parent.name}.{name} must be associated with an {nameof(this.UseInput)}!");
        private void Update() {
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
        private void OnDisable() {
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
                    if ((_numUses == 0 || RechargeEveryUse) && TimeToCharge > 0f) {
                        CurrentCharge = 0f;
                        while (CurrentCharge < 1f) {
                            CurrentCharge += Time.deltaTime / TimeToCharge;
                            yield return null;
                        }
                        CurrentCharge = 1f;     // Clamp fraction
                    }
                    Used.Invoke();
                    ++_numUses;
                }

                // Pause to account for the firing rate, if necessary
                if (AutomaticMode != AutomaticMode.SingleAction && !RechargeEveryUse)
                    yield return new WaitForSeconds(1f / AutomaticUseRate);

            // Continue using if the Tool is fully automatic or has not performed all of its semi-automatic uses
            } while (
                (AutomaticMode == AutomaticMode.SemiAutomatic && _numUses < SemiAutomaticUses) ||
                (AutomaticMode == AutomaticMode.FullyAutomatic)
            );

            // Prevent using again for the duration of the refractory period
            _refractoryRoutine = StartCoroutine(startRefractoryPeriod());
            yield return _refractoryRoutine;

            CurrentCharge = 0f;
            _usingRoutine = null;
        }
        private IEnumerator startRefractoryPeriod() {
            yield return new WaitForSeconds(RefactoryPeriod);
            _refractoryRoutine = null;
        }

    }

}
