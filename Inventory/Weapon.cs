using UnityEngine;
using U = UnityEngine;
using UnityEngine.Events;
using UnityEngine.Assertions;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Danware.Unity.Inventory {

    [RequireComponent(typeof(Tool))]
    public class Weapon : MonoBehaviour {
        // ABSTRACT DATA TYPES
        [Serializable]
        public class AttackEvent : UnityEvent<Vector3, RaycastHit[]> { }

        // HIDDEN FIELDS
        private Tool _tool;
        private float _accuracyLerpT = 0f;

        // INSPECTOR FIELDS
        [Space]
        public LayerMask AttackLayerMask;
        public float Range;

        [Header("Accuracy")]
        [Range(0f, 90f)]
        public float InitialConeHalfAngle = 0f;
        [Range(0f, 90f)]
        public float FinalConeHalfAngle = 0f;
        [Tooltip("For automatic Weapons, the accuracy cone's half angle will lerp from the initial to the final value in this many seconds")]
        public float AccuracyLerpTime = 1f;
        [Tooltip("If true, then all colliders within Range and on the AttackLayerMask will be attacked, using the relatively expensive Physics.RaycastAll() method.  If false, only the closest collider will be attacked, using the cheaper Physics.Raycast() method.")]
        public bool RaycastAll = false;

        // API INTERFACE
        public AttackEvent Attacked = new AttackEvent();
        public float AccuracyConeHalfAngle => Mathf.LerpAngle(InitialConeHalfAngle, FinalConeHalfAngle, _accuracyLerpT);

        // EVENT HANDLERS
        private void Awake() {
            // Register Tool events
            _tool = GetComponent<Tool>();
            _tool.Using.AddListener(() =>
                _accuracyLerpT = 0f);
            _tool.Used.AddListener(attack);
        }
        private void OnDrawGizmos() =>
            Gizmos.DrawLine(transform.position, transform.position + Range * transform.forward);

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
            if (RaycastAll)
                hits = Physics.RaycastAll(ray, Range, AttackLayerMask);
            else {
                bool somethingHit = Physics.Raycast(ray, out RaycastHit hitInfo, Range, AttackLayerMask);
                hits = somethingHit ? new[] { hitInfo } : new RaycastHit[0];
            }
            Attacked.Invoke(ray.direction, hits);

            // Adjust accuracy for the next attack, assuming the base Tool is automatic
            _accuracyLerpT = (AccuracyLerpTime == 0 ? 1f : _accuracyLerpT + (1f / _tool.AutomaticUseRate) / AccuracyLerpTime);
        }

    }

}
