using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using U = UnityEngine;

namespace UnityUtil.Physics2D;

[RequireComponent(typeof(Detonator2D))]
public class PushDetonator2D : MonoBehaviour
{
    private Detonator2D? _detonator;

    public float ExplosionForce = 10f;
    public float ExplosionUpwardsModifier = 2f;

    [Tooltip(
        "This rigidbody is 'safe' from pushing. " +
        "Useful if this detonator has a relationship with a rigidbody such that the rigidbody should not be pushed."
    )]
    public Rigidbody2D? SafeRigidbody;

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void Awake()
    {
        _detonator = GetComponent<Detonator2D>();
        _detonator.Detonated.AddListener(pushAll);
    }

    private void pushAll(Collider2D[] colliders)
    {
        // Apply an explosion force to all unique Rigidbodies among these Colliders
        // Upwards modifier adjusts to gravity
        Rigidbody2D[] rigidbodies = [.. colliders
            .Select(c => c.attachedRigidbody)
            .Where(rb => rb != null && rb != SafeRigidbody)
            .Distinct()
        ];
        for (int rb = 0; rb < rigidbodies.Length; ++rb) {
            Rigidbody2D rigidbody = rigidbodies[rb];
            Vector2 explosionPos = (Vector2)_detonator!.transform.position + ExplosionUpwardsModifier * U.Physics2D.gravity.normalized;
            rigidbody.AddExplosionForce(ExplosionForce, explosionPos, _detonator.ExplosionRadius, 0f, ForceMode2D.Impulse);
        }
    }

}
