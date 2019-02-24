using UnityEngine.Events;
using UnityEngine.Inputs;
using UnityEngine.Logging;

namespace UnityEngine.Inventory {

    public class Tool : Updatable {
        // HIDDEN FIELDS
        private bool _using = false;
        private bool _refactory = false;
        private uint _numUses = 0u;
        private bool _useFailed = false;

        private float _tSinceAutoUse = 0f;
        private float _tRefactory = 0f;
        private float _tCharged = 0f;

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
            // Start using when the use input starts, unless we're in the refactory period
            if (UseInput.Started() && !_refactory) {
                _using = true;
                _useFailed = false;
                _tSinceAutoUse = 0f;
                CurrentCharge = 0f;
                _numUses = 0u;
            }

            // Once using, stop using when the use input stops
            // Only start the refractory period if at least one use was made before the input stopped
            else if (_using && UseInput.Stopped()) {
                if (_numUses > 0)
                    _refactory = true;
                _using = false;
            }

            // If we're in the refactory period, then just keep waiting until that's over
            else if (_refactory) {
                _tRefactory += Time.deltaTime;
                if (_tRefactory >= Info.RefactoryPeriod) {
                    _tRefactory = 0f;
                    _refactory = false;
                }
            }

            // Othwerise, continue using the Tool...
            else if (_using) {

                // If use just started, then raise the using event
                // If any listeners cancel using, then raise the Use Failed event
                if (_tSinceAutoUse == 0f && !_useFailed) {
                    Using.Invoke();
                    _useFailed = Using.Cancel;
                    if (_useFailed)
                        UseFailed.Invoke();
                    else
                        _tCharged = 0f;
                }

                // If use failed, then only continue if this Tool is fully automatic or has not yet performed all of its semi-automatic uses
                bool tryAnotherUse =
                    Info.AutomaticMode == AutomaticMode.FullyAutomatic ||
                    (Info.AutomaticMode == AutomaticMode.SemiAutomatic && _numUses < Info.SemiAutomaticUses);
                if (_useFailed && !tryAnotherUse)
                    return;

                // If use has not failed, then actually perform the use now,
                // or continue waiting for the next automatic use, if applicable
                if (_tSinceAutoUse == 0f && !_useFailed) {
                    Used.Invoke();
                    ++_numUses;
                }
                else if (tryAnotherUse) {
                    _tSinceAutoUse += Time.deltaTime;
                    if (_tSinceAutoUse >= 1f / Info.AutomaticUseRate) {
                        _tSinceAutoUse = 0f;
                        _useFailed = false;
                    }
                }


                if (_tCharged < Info.TimeToCharge)
                    _tCharged += Time.deltaTime;
                else {
                    _tCharged -= Info.TimeToCharge;
                }


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
                }

                // Pause to account for the firing rate, if necessary
                if (Info.AutomaticMode != AutomaticMode.SingleAction && !Info.RechargeEveryUse)
                    yield return new WaitForSeconds(1f / Info.AutomaticUseRate);

                CurrentCharge = 0f;
            }
        }
        protected override void BetterOnDisable() {
            CurrentCharge = 0f;
            _numUses = 0u;
            _using = false;
            _refactory = false;
            _tRefactory = 0f;
            _tSinceAutoUse = 0f;
        }

    }

}
