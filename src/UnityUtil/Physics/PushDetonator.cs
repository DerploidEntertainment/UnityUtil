using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace UnityEngine
{

    [RequireComponent(typeof(Detonator))]
    public class PushDetonator : MonoBehaviour {

        private Detonator _detonator;

        // INSPECTOR FIELDS
        public float ExplosionForce = 10f;
        public float ExplosionUpwardsModifier = 2f;

        [Tooltip("This rigidbody is 'safe' from pushing. Useful if this detonator has a relationship with a rigidbody such that the rigidbody should not be pushed.")]
        public Rigidbody SafeRigidbody;

        // EVENT HANDLERS
        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
        private void Awake() {
            _detonator = GetComponent<Detonator>();
            _detonator.Detonated.AddListener(pushAll);
        }

        // HELPER FUNCTIONS
        private void pushAll(Collider[] colliders) {
            // Apply an explosion force to all unique Rigidbodies among these Colliders
            // Upwards modifier adjusts to gravity
            Rigidbody[] rigidbodies = colliders
                .Select(c => c.attachedRigidbody)
                .Where(rb => rb != null && rb != SafeRigidbody)
                .Distinct()
                .ToArray();
            for (int rb = 0; rb < rigidbodies.Length; ++rb) {
                Rigidbody rigidbody = rigidbodies[rb];
                Vector3 explosionPos = _detonator.transform.position + ExplosionUpwardsModifier * Physics.gravity.normalized;
                rigidbody.AddExplosionForce(ExplosionForce, explosionPos, _detonator.ExplosionRadius, 0f, ForceMode.Impulse);
            }
        }

    }

}
