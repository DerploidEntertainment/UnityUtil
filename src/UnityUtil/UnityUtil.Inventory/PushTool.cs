using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityUtil.Inventory;

[RequireComponent(typeof(Weapon))]
public class PushTool : MonoBehaviour
{
    private readonly HashSet<Rigidbody> _pushedRigidbodies = [];

    [RequiredIn(PrefabKind.PrefabInstanceAndNonPrefabInstance)]
    public PushToolInfo? Info;

    [Tooltip("If true, then any colliders attached to the same Rigidbodies as one of these colldiers will NOT be affected.  You might use this field to prevent pushing of any of the colliders that make up a player's vehicle, for example.")]
    public Collider[] IgnoreRigidbodiesAttachedTo = [];

    private void Awake()
    {
        Weapon? weapon = GetComponent<Weapon>();
        weapon.Attacked.AddListener(push);
    }
    private void push(Ray ray, RaycastHit[] hits)
    {
        // Determine those Rigidbodies, the attached Colliders of which cannot be pushed
        IEnumerable<Rigidbody> unpushableRbEnum = IgnoreRigidbodiesAttachedTo.Select(c => c.attachedRigidbody).Distinct();
        var unpushableRbs = new HashSet<Rigidbody>(unpushableRbEnum);

        // If we should only push the closest Rigidbody, then scan for the Rigidbody to push, otherwise push the Rigidbodies attached to all Colliders.
        // Ignore Colliders with the specified tags or attached to one of the specified Rigidbodies,
        // and be sure not to push Rigidbodies multiple times.
        _pushedRigidbodies.Clear();
        for (int h = 0; h < hits.Length; ++h) {
            RaycastHit hit = hits[h];
            Rigidbody rb = hit.collider.attachedRigidbody;
            bool push =
                rb != null &&
                !Info!.IgnoreColliderTags.Contains(hit.collider.tag) &&
                !unpushableRbs.Contains(rb) &&
                !_pushedRigidbodies.Contains(rb);
            if (push) {
                _pushedRigidbodies.Add(rb!);
                rb!.AddForceAtPosition(Info!.PushForce * ray.direction, hit.point, ForceMode.Impulse);
                if (Info.OnlyPushClosest && hits.Length > 0)
                    break;
            }
        }
    }

}
