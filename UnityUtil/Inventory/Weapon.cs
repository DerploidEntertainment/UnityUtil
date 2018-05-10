using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using U = UnityEngine;

namespace UnityEngine.Inventory {

    [RequireComponent(typeof(Tool))]
    public class Weapon : MonoBehaviour {
        // ABSTRACT DATA TYPES
        [Serializable]
        public class AttackEvent : UnityEvent<Vector3, RaycastHit[]> { }

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
            RaycastHit[] hits;
            if (Info.RaycastAll)
                hits = Physics.RaycastAll(ray, Info.Range, Info.AttackLayerMask);
            else {
                bool somethingHit = Physics.Raycast(ray, out RaycastHit hitInfo, Info.Range, Info.AttackLayerMask);
                hits = somethingHit ? new[] { hitInfo } : new RaycastHit[0];
            }
            Attacked.Invoke(ray.direction, hits);

            // Adjust accuracy for the next attack, assuming the base Tool is automatic
            _accuracyLerpT = (Info.AccuracyLerpTime == 0 ? 1f : _accuracyLerpT + (1f / _tool.Info.AutomaticUseRate) / Info.AccuracyLerpTime);
        }

    }

}
