using System;
using System.Linq;
using UnityEngine.Assertions;
using UnityEngine.Events;
using U = UnityEngine;

namespace UnityEngine.Inventory {

    [RequireComponent(typeof(Tool))]
    public class Weapon : MonoBehaviour {
        // ABSTRACT DATA TYPES
        [Serializable]
        public class AttackEvent : UnityEvent<Ray, RaycastHit[]> { }

        // HIDDEN FIELDS
        private Tool _tool;
        private float _accuracyLerpT = 0f;

        // INSPECTOR FIELDS
        public WeaponInfo Info;

        // API INTERFACE
        public AttackEvent Attacked = new AttackEvent();
        public float AccuracyConeHalfAngle => Mathf.LerpAngle(Info.InitialConeHalfAngle, Info.FinalConeHalfAngle, _accuracyLerpT);

        // EVENT HANDLERS
        private void Awake() {
            Assert.IsNotNull(Info, this.GetAssociationAssertion(nameof(UnityEngine.Inventory.WeaponInfo)));

            // Register Tool events
            _tool = GetComponent<Tool>();
            _tool.Using.AddListener(() => _accuracyLerpT = 0f);
            _tool.Used.AddListener(attack);
        }
        private void OnDrawGizmos() =>
            Gizmos.DrawLine(transform.position, transform.position + Info.Range * transform.forward);

        // HELPERS
        private void attack() {
            // Get a random Ray within the accuracy cone
            float z = U.Random.Range(Mathf.Cos(Mathf.Deg2Rad * AccuracyConeHalfAngle), 1f);
            float theta = U.Random.Range(0f, 2 * Mathf.PI);
            float sqrtPart = Mathf.Sqrt(1 - z * z);
            var dir = new Vector3(sqrtPart * Mathf.Cos(theta), sqrtPart * Mathf.Sin(theta), z);
            var ray = new Ray(transform.position, transform.TransformDirection(dir));

            // Raycast into the scene with the given LayerMask
            // Raise the Attacked event, allowing other components to select which components to affect
            var hits = new RaycastHit[0];
            if (Info.AttackAllInRange || Info.MaxAttacks > 1) {
                RaycastHit[] allHits = Physics.RaycastAll(ray, Info.Range, Info.AttackLayerMask);
                hits = allHits;
            }
            else if (Info.MaxAttacks == 1) {
                bool somethingHit = Physics.Raycast(ray, out RaycastHit hit, Info.Range, Info.AttackLayerMask);
                if (somethingHit)
                    hits = new[] { hit };
            }
            hits = hits.OrderBy(h => h.distance).Take((int)Info.MaxAttacks).ToArray();
            Attacked.Invoke(ray, hits);

            // Adjust accuracy for the next attack, assuming the base Tool is automatic
            _accuracyLerpT = (Info.AccuracyLerpTime == 0 ? 1f : _accuracyLerpT + (1f / _tool.Info.AutomaticUseRate) / Info.AccuracyLerpTime);
        }

    }

}
