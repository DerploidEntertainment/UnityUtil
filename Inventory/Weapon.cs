using UnityEngine;
using U = UnityEngine;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Danware.Unity.Input;

namespace Danware.Unity.Inventory {

    public class Weapon : MonoBehaviour {
        // ABSTRACT DATA TYPES
        public struct TargetData {
            public TargetData(uint priority = 0) {
                Priority = priority;
                Callback = (hit) => { };
            }
            public uint Priority { get; set; }
            public Action<RaycastHit> Callback { get; set; }
        }
        public class WeaponEventArgs : EventArgs {
            public WeaponEventArgs(Weapon weapon) {
                Weapon = weapon;
            }
            public Weapon Weapon { get; }
        }
        public class AttackEventArgs : WeaponEventArgs {
            public AttackEventArgs(Weapon weapon) : base(weapon) { }
            public AttackEventArgs(Weapon weapon, Vector3 direction, RaycastHit[] hits) : base(weapon) {
                Direction = direction;
                Hits = hits;
            }
            public Vector3 Direction { get; }
            public RaycastHit[] Hits { get; }
            public IDictionary<RaycastHit, TargetData> TargetPriorities { get; } = new Dictionary<RaycastHit, TargetData>();
            public void Add(RaycastHit hitInfo, TargetData data) {
                // If this hit has already been added, adjust its associated data
                if (TargetPriorities.ContainsKey(hitInfo)) {
                    TargetData td = TargetPriorities[hitInfo];
                    td.Priority += data.Priority;
                    td.Callback += data.Callback;
                    TargetPriorities[hitInfo] = td;
                }

                // Otherwise, add the new association
                else
                    TargetPriorities.Add(hitInfo, data);
            }
        }
        public class OverheatChangedEventArgs : WeaponEventArgs {
            public OverheatChangedEventArgs(Weapon weapon, bool overheated) : base(weapon) {
                Overheated = overheated;
            }
            public bool Overheated { get; }
        }
        public class AmmoEventArgs : WeaponEventArgs {
            public AmmoEventArgs(Weapon weapon, int oldClipAmmo, int oldBackupAmmo, int newClipAmmo, int newBackupAmmo) : base(weapon) {
                OldClipAmmo = oldClipAmmo;
                OldBackupAmmo = oldBackupAmmo;
                NewClipAmmo = newClipAmmo;
                NewBackupAmmo = newBackupAmmo;
            }

            public int OldClipAmmo { get; }
            public int OldBackupAmmo { get; }
            public int NewClipAmmo { get; }
            public int NewBackupAmmo { get; }
        }
        private enum State {
            Idle,
            Charging,
        }

        // HIDDEN FIELDS
        private State _state = State.Idle;
        private bool _firstAttack = false;
        private bool _attacking = false;
        private bool _reloading = false;
        private bool _refactory = false;
        private bool _overheated = false;
        private bool _autoWaiting = false;
        private float _accuracyLerpT = 0f;
        private float _rangeLerpT = 0f;

        private EventHandler<WeaponEventArgs> _chargingInvoker;
        private EventHandler<AttackEventArgs> _attackedInvoker;
        private EventHandler<WeaponEventArgs> _failedInvoker;
        private EventHandler<OverheatChangedEventArgs> _overheatInvoker;
        private EventHandler<AmmoEventArgs> _reloadInvoker;

        // INSPECTOR FIELDS
        public StartStopInput AttackInput;
        public StartStopInput ReloadInput;
        [Tooltip(@"A case-insensitive string to identify different types of Weapons (e.g., ""Pistol""), for collecting ammo and other Weapon-specific functions.)")]
        public string WeaponName = "Weapon";
        public LayerMask AffectLayers;

        [Header("Range")]
        public float InitialRange;
        public float FinalRange;
        [Tooltip("For automatic Weapons, the range will lerp from the initial to the final value in this number of seconds")]
        public float RangeLerpTime = 1f;   // Seconds

        [Header("Attack Rates")]
        public bool Automatic;
        [Tooltip("Once an automatic Weapon starts firing, it maintains this many attacks per second.")]
        public float AttackRate = 5f;
        [Tooltip("When player stops attacking, this much time (in seconds) must pass before Weapon responds to input again.")]
        public float RefactoryPeriod = 0f;
        [Tooltip("Player must maintain the firing input for this much time (in seconds) before the Weapon will actually attack.")]
        public float TimeToCharge = 0f;
        [Tooltip("For automatic Weapons, this toggles whether each attack will need to recharge.  If toggled, then the firing rate dictates how much time will pass BETWEEN charges.")]
        public bool ChargeOnEveryAttack;

        [Header("Ammo")]
        public bool UseAmmo;
        public int MaxClipAmmo;
        public int CurrentClipAmmo;
        public int MaxClips;
        public int BackupAmmo;

        [Header("Overheating")]
        public bool UseHeat;
        public float MaxHeat = 100f;
        public float HeatPerAttack = 50f;
        public bool AbsoluteHeat = true;
        [Tooltip("The Weapon will cool by this many heat units (or this percent of MaxHeat) per second, unless overheated.")]
        public float CoolRate = 75f;  // Heat/sec or fraction/sec
        [Tooltip("Once overheated, this many seconds must pass before the Weapon will start cooling again.")]
        public float OverheatDuration = 2f;

        [Header("Accuracy")]
        [Range(0f, 90f)]
        public float InitialConeHalfAngle = 0f;
        [Range(0f, 90f)]
        public float FinalConeHalfAngle = 0f;
        [Tooltip("For automatic Weapons, the accuracy cone's half angle will lerp from the initial to the final value in this number of seconds")]
        public float AccuracyLerpTime = 1f;   // Seconds

        // API INTERFACE
        public float AccuracyConeHalfAngle => Mathf.LerpAngle(InitialConeHalfAngle, FinalConeHalfAngle, _accuracyLerpT);
        public float Range => Mathf.Lerp(InitialRange, FinalRange, _rangeLerpT);
        public float CurrentHeat { get; private set; } = 0f;
        public float ChargeFraction { get; private set; }
        public IEnumerator AttackContinuous(float seconds) {
            _firstAttack = true;
            _attacking = true;
            yield return new WaitForSeconds(seconds);
            _attacking = false;
        }
        public void Load(int ammo) {
            Debug.AssertFormat(ammo >= 0, $"Tried to reload Weapon {name} with a negative amount of ammo!");
            doLoad(ammo);
        }
        public void ReloadClip() {
            doReloadClip();
        }

        public event EventHandler<WeaponEventArgs> AttackChargeStarted {
            add { _chargingInvoker += value; }
            remove { _chargingInvoker -= value; }
        }
        public event EventHandler<AttackEventArgs> Attacked {
            add { _attackedInvoker += value; }
            remove { _attackedInvoker -= value; }
        }
        public event EventHandler<WeaponEventArgs> AttackFailed {
            add { _failedInvoker += value; }
            remove { _failedInvoker -= value; }
        }
        public event EventHandler<OverheatChangedEventArgs> OverheatStateChanged {
            add { _overheatInvoker += value; }
            remove { _overheatInvoker -= value; }
        }
        public event EventHandler<AmmoEventArgs> Reloaded {
            add { _reloadInvoker += value; }
            remove { _reloadInvoker -= value; }
        }

        // EVENT HANDLERS
        private void Update() {
            // Handle player input
            processInput();

            // Reload if the player requested it
            if (_reloading) {
                _reloading = false;
                doReloadClip();
            }

            // Update according to the Weapon's state
            switch (_state) {
                case State.Idle:
                    idleUpdate();
                    break;

                case State.Charging:
                    chargingUpdate();
                    break;
            }

            // No matter what, cool the Weapon if it is not overheated
            if (UseHeat && !_overheated) {
                float deltaHeat = CoolRate * Time.deltaTime * (AbsoluteHeat ? 1f : MaxHeat);
                CurrentHeat -= Mathf.Min(deltaHeat, CurrentHeat);
            }
        }
        private void OnDisable() {
            resetToIdle();
        }
        private void OnDrawGizmos() {
            float range = Mathf.Lerp(InitialRange, FinalRange, _rangeLerpT);
            Gizmos.DrawLine(transform.position, transform.TransformPoint(range * transform.forward));
        }

        // HELPERS
        private void processInput() {
            // If no attack input object was provided then just unset all attack flags
            if (AttackInput == null) {
                _firstAttack = false;
                _attacking = false;
            }

            // Otherwise, set/unset attack flags according to player input
            else {
                if (AttackInput.Started) {
                    _firstAttack = true;
                    _attacking = true;
                }

                if (AttackInput.Stopped)
                    _attacking = false;
            }

            // Set/unset reload flag according to player input
            _reloading = ReloadInput?.Started ?? false;
        }
        private void idleUpdate() {
            // If an attack is being attempted and we're ready to charge, then begin charging
            // Raise an event either way
            if (_firstAttack && !_refactory) {
                _firstAttack = false;
                if (canAttack()) {
                    _state = State.Charging;
                    onAttackChargeStarted();
                }
                else
                    onAttackFailed();
            }
        }
        private void chargingUpdate() {
            // If an attack is no longer being attempted, then reset values and return to the idle state
            if (!_attacking) {
                resetToIdle();
                return;
            }

            // Otherwise, continue charging
            bool tryAttack = true;
            if (ChargeFraction < 1f) {
                ChargeFraction += Time.deltaTime / TimeToCharge;
                tryAttack = (ChargeFraction >= 1f);
            }
            
            // Once fully charged and all timers have ended...
            if (tryAttack && !_autoWaiting) {
                // Either attack, if possible, or raise a failure event
                if (!canAttack())
                    onAttackFailed();
                else
                    attack();

                // Continue attacking at the attack rate if automatic, otherwise return to the idle state
                if (Automatic)
                    StartCoroutine(doAutomaticWaitPeriod());
                else {
                    resetToIdle();
                    StartCoroutine(doRefactoryPeriod());
                }
            }
        }
        private bool canAttack() {
            bool ready = (
                (!UseAmmo || CurrentClipAmmo > 0) &&
                (!UseHeat || CurrentHeat <= MaxHeat)
            );
            return ready;
        }
        private void attack() {
            // First adjust attack values (accuracy, ammo, heat, etc.)
            adjustAttackValues();

            // Get a random Ray within the accuracy cone
            float z = U.Random.Range(Mathf.Cos(Mathf.Deg2Rad * AccuracyConeHalfAngle), 1f);
            float theta = U.Random.Range(0f, 2 * Mathf.PI);
            float sqrtPart = Mathf.Sqrt(1 - z * z);
            Vector3 dir = new Vector3(sqrtPart * Mathf.Cos(theta), sqrtPart * Mathf.Sin(theta), z);
            Ray ray = new Ray(transform.position, transform.TransformDirection(dir));

            // Raycast into the scene with the given LayerMask
            // Raise the Fired event, allowing other components to select which targets to affect
            RaycastHit[] hits = Physics.RaycastAll(ray, Range, AffectLayers);
            hits = hits.OrderBy(h => h.distance).ToArray();
            AttackEventArgs args = onAttacked(ray.direction, hits);

            // Affect the closest, highest-priority target, if there is one
            RaycastHit[] orderedHits = (
                from tp in args.TargetPriorities
                orderby tp.Value.Priority descending,
                        tp.Key.distance
                select tp.Key
            ).ToArray();
            if (orderedHits.Length > 0) {
                RaycastHit closest = orderedHits[0];
                TargetData td = args.TargetPriorities[closest];
                td.Callback.Invoke(closest);
            }
        }
        private void resetToIdle() {
            // Reset attack values
            _accuracyLerpT = 0f;
            _rangeLerpT = 0f;

            // Adjust state
            ChargeFraction = 0f;
            _state = State.Idle;
        }
        private void adjustAttackValues() {
            // Adjust ammo, if necessary
            if (UseAmmo) {
                if (--CurrentClipAmmo < 0)
                    CurrentClipAmmo = 0;
            }

            // Adjust heat and raise events for overheating, if necessary
            if (UseHeat) {
                float deltaHeat = HeatPerAttack * (AbsoluteHeat ? 1f : MaxHeat);
                CurrentHeat += deltaHeat;
                if (CurrentHeat > MaxHeat) {
                    onOverheatChanged(true);
                    StartCoroutine(doOverheatDuration());
                }
            }

            // Adjust accuracy cone and range
            _accuracyLerpT = (AccuracyLerpTime == 0 ? 1f : _accuracyLerpT + (1f / AttackRate) / AccuracyLerpTime);
            _rangeLerpT = (RangeLerpTime == 0 ? 1f : _rangeLerpT + (1f / AttackRate) / RangeLerpTime);
        }
        private IEnumerator doRefactoryPeriod() {
            _refactory = true;
            yield return new WaitForSeconds(RefactoryPeriod);
            _refactory = false;
        }
        private IEnumerator doOverheatDuration() {
            // Prevent cooling while overheated
            _overheated = true;
            yield return new WaitForSeconds(OverheatDuration);
            _overheated = false;

            // Raise an event when the overheat duration is over
            onOverheatChanged(false);
        }
        private IEnumerator doAutomaticWaitPeriod() {
            // Prevent attacking any faster than the given attack rate
            _autoWaiting = true;
            yield return new WaitForSeconds(1f / AttackRate);
            _autoWaiting = false;

            // Reset the Weapon's charge if we are recharging on every attack
            if (ChargeOnEveryAttack) {
                ChargeFraction = 0f;
                onAttackChargeStarted();
            }
        }
        private void doReloadClip() {
            // Fill the current clip as much as possible from backup ammo
            int old = CurrentClipAmmo;
            int clipFill = MaxClipAmmo - CurrentClipAmmo;
            int availableAmmo = Mathf.Min(BackupAmmo, clipFill);
            CurrentClipAmmo += availableAmmo;
            BackupAmmo -= availableAmmo;

            // Raise the Reloaded event
            AmmoEventArgs args = new AmmoEventArgs(this, old, BackupAmmo, CurrentClipAmmo, BackupAmmo);
            _reloadInvoker?.Invoke(this, args);
        }
        private void doLoad(int ammo) {
            int oldClip = CurrentClipAmmo;
            int oldBackup = BackupAmmo;

            // Fill the current clip as much as possible
            int clipFill = MaxClipAmmo - CurrentClipAmmo;
            if (ammo < clipFill)
                CurrentClipAmmo += ammo;
            else {
                CurrentClipAmmo += clipFill;
                ammo -= clipFill;
            }

            // Fill the backup ammo as much as possible
            int maxBackup = MaxClipAmmo * (MaxClips - 1);

            // Any remaining ammo is ignored
            int backupFill = maxBackup - BackupAmmo;
            if (ammo < backupFill)
                BackupAmmo += ammo;
            else {
                BackupAmmo += backupFill;
                ammo -= backupFill;
            }

            // Raise the Reloaded event
            AmmoEventArgs args = new AmmoEventArgs(this, oldClip, oldBackup, CurrentClipAmmo, BackupAmmo);
            _reloadInvoker?.Invoke(this, args);
        }
        private void onAttackFailed() {
            WeaponEventArgs args = new WeaponEventArgs(this);
            _failedInvoker?.Invoke(this, args);

            return;
        }
        private void onAttackChargeStarted() {
            AttackEventArgs args = new AttackEventArgs(this);
            _chargingInvoker?.Invoke(this, args);

            return;
        }
        private AttackEventArgs onAttacked(Vector3 direction, RaycastHit[] hits) {
            AttackEventArgs args = new AttackEventArgs(this, direction, hits.ToArray());
            _attackedInvoker?.Invoke(this, args);

            return args;
        }
        private void onOverheatChanged(bool overheated) {
            // Raise the Overheat Changed event
            OverheatChangedEventArgs args = new OverheatChangedEventArgs(this, overheated);
            _overheatInvoker?.Invoke(this, args);

            return;
        }

    }

}
