using System;
using System.Collections.Generic;
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
        private void OnDrawGizmos() {
            switch (Info.PhysicsCastShape) {
                case PhysicsCastShape.Ray:
                case PhysicsCastShape.Capsule:  // No Gizmos.DrawCapsule method unfortunately :/
                    Gizmos.DrawLine(transform.position, transform.position + Info.Range * transform.forward);
                    break;

                case PhysicsCastShape.Box:
                    Gizmos.DrawWireCube(transform.position + Info.Range * transform.forward, 2f * Info.HalfExtents);
                    break;

                case PhysicsCastShape.Sphere:
                    Gizmos.DrawWireSphere(transform.position + Info.Range * transform.forward, Info.Radius);
                    break;

                default: this.LogWarning(" could not draw Gizmos.  " + BetterLogger.GetSwitchDefault(Info.PhysicsCastShape)); break;
            }
        }

        // HELPERS
        private void attack() {

            // Get a random Ray within the accuracy cone
            float z = U.Random.Range(Mathf.Cos(Mathf.Deg2Rad * AccuracyConeHalfAngle), 1f);
            float theta = U.Random.Range(0f, MoreMath.TwoPi);
            float sqrtPart = Mathf.Sqrt(1 - z * z);
            var dir = new Vector3(sqrtPart * Mathf.Cos(theta), sqrtPart * Mathf.Sin(theta), z);
            var ray = new Ray(transform.position, transform.TransformDirection(dir));

            // Cast into the scene for hits along this ray, using the specified cast shape
            var hits = new RaycastHit[0];
            switch (Info.PhysicsCastShape) {
                case PhysicsCastShape.Ray:     hits = rayAttackHits();     break;
                case PhysicsCastShape.Box:     hits = boxAttackHits();     break;
                case PhysicsCastShape.Sphere:  hits = sphereAttackHits();  break;
                case PhysicsCastShape.Capsule: hits = capsuleAttackHits(); break;
                default: throw new NotImplementedException(BetterLogger.GetSwitchDefault(Info.PhysicsCastShape));
            }

            // Sort hits by increasing distance, and raise the Attacked event so that other components can select which components to affect
            IEnumerable<RaycastHit> orderedHits = hits.OrderBy(h => h.distance);
            if (!Info.AttackAllInRange)
                orderedHits = orderedHits.Take((int)Info.MaxAttacks);
            hits = orderedHits.ToArray();
            Attacked.Invoke(ray, hits);

            // Adjust accuracy for the next attack, assuming the base Tool is automatic
            _accuracyLerpT = (Info.AccuracyLerpTime == 0 ? 1f : _accuracyLerpT + (1f / _tool.Info.AutomaticUseRate) / Info.AccuracyLerpTime);
            RaycastHit[] rayAttackHits() {
                var rayHits = new RaycastHit[0];

                // Raycast into the scene with the given LayerMask, collecting the desired number of hitInfos
                if (Info.AttackAllInRange || Info.MaxAttacks > 1) {
                    RaycastHit[] allHits = Physics.RaycastAll(ray.origin, ray.direction, Info.Range, Info.AttackLayerMask);
                    rayHits = allHits;
                }
                else if (Info.MaxAttacks == 1) {
                    bool somethingHit = Physics.Raycast(ray.origin, ray.direction, out RaycastHit hit, Info.Range, Info.AttackLayerMask);
                    if (somethingHit)
                        rayHits = new[] { hit };
                }

                return rayHits;
            }
            RaycastHit[] boxAttackHits() {
                var boxHits = new RaycastHit[0];

                // Boxcast into the scene with the given LayerMask, collecting the desired number of hitInfos
                if (Info.AttackAllInRange || Info.MaxAttacks > 1) {
                    RaycastHit[] allHits = Physics.BoxCastAll(ray.origin, Info.HalfExtents, ray.direction, Info.Orientation, Info.Range, Info.AttackLayerMask);
                    boxHits = allHits;
                }
                else if (Info.MaxAttacks == 1) {
                    bool somethingHit = Physics.BoxCast(ray.origin, Info.HalfExtents, ray.direction, out RaycastHit hit, Info.Orientation, Info.Range, Info.AttackLayerMask);
                    if (somethingHit)
                        boxHits = new[] { hit };
                }

                return boxHits;
            }
            RaycastHit[] sphereAttackHits() {
                var sphereHits = new RaycastHit[0];

                // Spherecast into the scene with the given LayerMask, collecting the desired number of hitInfos
                if (Info.AttackAllInRange || Info.MaxAttacks > 1) {
                    RaycastHit[] allHits = Physics.SphereCastAll(ray.origin, Info.Radius, ray.direction, Info.Range, Info.AttackLayerMask);
                    sphereHits = allHits;
                }
                else if (Info.MaxAttacks == 1) {
                    bool somethingHit = Physics.SphereCast(ray.origin, Info.Radius, ray.direction, out RaycastHit hit, Info.Range, Info.AttackLayerMask);
                    if (somethingHit)
                        sphereHits = new[] { hit };
                }

                return sphereHits;
            }
            RaycastHit[] capsuleAttackHits() {
                var capsuleHits = new RaycastHit[0];

                // Capsulecast into the scene with the given LayerMask, collecting the desired number of hitInfos
                Vector3 p1 = ray.origin + Info.Point1;
                Vector3 p2 = ray.origin + Info.Point2;
                if (Info.AttackAllInRange || Info.MaxAttacks > 1) {
                    RaycastHit[] allHits = Physics.CapsuleCastAll(p1, p2, Info.Radius, ray.direction, Info.Range, Info.AttackLayerMask);
                    capsuleHits = allHits;
                }
                else if (Info.MaxAttacks == 1) {
                    bool somethingHit = Physics.CapsuleCast(p1, p2, Info.Radius, ray.direction, out RaycastHit hit, Info.Range, Info.AttackLayerMask);
                    if (somethingHit)
                        capsuleHits = new[] { hit };
                }

                return capsuleHits;
            }
        }

    }

}
