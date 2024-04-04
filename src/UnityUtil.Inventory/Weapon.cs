using Microsoft.Extensions.Logging;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityUtil.DependencyInjection;
using UnityUtil.Math;
using U = UnityEngine;

namespace UnityUtil.Inventory;

[Serializable]
public class AttackEvent : UnityEvent<Ray, RaycastHit[]> { }

[RequireComponent(typeof(Tool))]
public class Weapon : MonoBehaviour
{
    private InventoriesLogger<Weapon>? _logger;
    private Tool? _tool;
    private float _accuracyLerpT;

    [RequiredIn(PrefabKind.NonPrefabInstance)]
    public WeaponInfo? Info;

    public AttackEvent Attacked = new();

    public float AccuracyConeHalfAngle => Mathf.LerpAngle(Info!.InitialConeHalfAngle, Info.FinalConeHalfAngle, _accuracyLerpT);

    public void Inject(ILoggerFactory loggerFactory) => _logger = new(loggerFactory, context: this);

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void Awake()
    {
        DependencyInjector.Instance.ResolveDependenciesOf(this);

        // Register Tool events
        _tool = GetComponent<Tool>();
        _tool.Using.AddListener(() => _accuracyLerpT = 0f);
        _tool.Used.AddListener(attack);
    }
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void OnDrawGizmos()
    {
        switch (Info!.PhysicsCastShape) {
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

            default:
                _logger!.WeaponGizmosUnknownPhysicsCastShape(Info.PhysicsCastShape);
                break;
        }
    }

    private void attack()
    {

        // Get a random Ray within the accuracy cone
        float z = U.Random.Range(Mathf.Cos(Mathf.Deg2Rad * AccuracyConeHalfAngle), 1f);
        float theta = U.Random.Range(0f, MoreMath.TwoPi);
        float sqrtPart = Mathf.Sqrt(1 - z * z);
        var dir = new Vector3(sqrtPart * Mathf.Cos(theta), sqrtPart * Mathf.Sin(theta), z);
        var ray = new Ray(transform.position, transform.TransformDirection(dir));

        // Cast into the scene for hits along this ray, using the specified cast shape
        RaycastHit[] hits = Info!.PhysicsCastShape switch {
            PhysicsCastShape.Ray => rayAttackHits(),
            PhysicsCastShape.Box => boxAttackHits(),
            PhysicsCastShape.Sphere => sphereAttackHits(),
            PhysicsCastShape.Capsule => capsuleAttackHits(),
            _ => throw UnityObjectExtensions.SwitchDefaultException(Info.PhysicsCastShape),
        };

        // Sort hits by increasing distance, and raise the Attacked event so that other components can select which components to affect
        IEnumerable<RaycastHit> orderedHits = hits.OrderBy(h => h.distance);
        if (!Info.AttackAllInRange)
            orderedHits = orderedHits.Take((int)Info.MaxAttacks);
        hits = orderedHits.ToArray();
        Attacked.Invoke(ray, hits);

        // Adjust accuracy for the next attack, assuming the base Tool is automatic
        _accuracyLerpT = (Info.AccuracyLerpTime == 0 ? 1f : _accuracyLerpT + (1f / _tool!.Info!.AutomaticUseRate) / Info.AccuracyLerpTime);
        RaycastHit[] rayAttackHits()
        {
            RaycastHit[] rayHits = [];

            // Raycast into the scene with the given LayerMask, collecting the desired number of hitInfos
            if (Info.AttackAllInRange || Info.MaxAttacks > 1) {
                RaycastHit[] allHits = U.Physics.RaycastAll(ray.origin, ray.direction, Info.Range, Info.AttackLayerMask);
                rayHits = allHits;
            }
            else if (Info.MaxAttacks == 1) {
                bool somethingHit = U.Physics.Raycast(ray.origin, ray.direction, out RaycastHit hit, Info.Range, Info.AttackLayerMask);
                if (somethingHit)
                    rayHits = [hit];
            }

            return rayHits;
        }
        RaycastHit[] boxAttackHits()
        {
            RaycastHit[] boxHits = [];

            // Boxcast into the scene with the given LayerMask, collecting the desired number of hitInfos
            if (Info.AttackAllInRange || Info.MaxAttacks > 1) {
                RaycastHit[] allHits = U.Physics.BoxCastAll(ray.origin, Info.HalfExtents, ray.direction, Info.Orientation, Info.Range, Info.AttackLayerMask);
                boxHits = allHits;
            }
            else if (Info.MaxAttacks == 1) {
                bool somethingHit = U.Physics.BoxCast(ray.origin, Info.HalfExtents, ray.direction, out RaycastHit hit, Info.Orientation, Info.Range, Info.AttackLayerMask);
                if (somethingHit)
                    boxHits = [hit];
            }

            return boxHits;
        }
        RaycastHit[] sphereAttackHits()
        {
            RaycastHit[] sphereHits = [];

            // Spherecast into the scene with the given LayerMask, collecting the desired number of hitInfos
            if (Info.AttackAllInRange || Info.MaxAttacks > 1) {
                RaycastHit[] allHits = U.Physics.SphereCastAll(ray.origin, Info.Radius, ray.direction, Info.Range, Info.AttackLayerMask);
                sphereHits = allHits;
            }
            else if (Info.MaxAttacks == 1) {
                bool somethingHit = U.Physics.SphereCast(ray.origin, Info.Radius, ray.direction, out RaycastHit hit, Info.Range, Info.AttackLayerMask);
                if (somethingHit)
                    sphereHits = [hit];
            }

            return sphereHits;
        }
        RaycastHit[] capsuleAttackHits()
        {
            RaycastHit[] capsuleHits = [];

            // Capsulecast into the scene with the given LayerMask, collecting the desired number of hitInfos
            Vector3 p1 = ray.origin + Info.Point1;
            Vector3 p2 = ray.origin + Info.Point2;
            if (Info.AttackAllInRange || Info.MaxAttacks > 1) {
                RaycastHit[] allHits = U.Physics.CapsuleCastAll(p1, p2, Info.Radius, ray.direction, Info.Range, Info.AttackLayerMask);
                capsuleHits = allHits;
            }
            else if (Info.MaxAttacks == 1) {
                bool somethingHit = U.Physics.CapsuleCast(p1, p2, Info.Radius, ray.direction, out RaycastHit hit, Info.Range, Info.AttackLayerMask);
                if (somethingHit)
                    capsuleHits = [hit];
            }

            return capsuleHits;
        }
    }

}
