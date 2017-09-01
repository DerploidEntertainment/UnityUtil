using UnityEngine;
using UnityEngine.Assertions;
using U = UnityEngine;

using System;

namespace Danware.Unity {

    public enum SpawnDirection {
        Straight,
        ConeRandom,
        ConeBoundary,
        AnyDirection,
    }

    public class Spawner : MonoBehaviour {

        // HIDDEN FIELDS
        private GameObject _previous;
        private long _count = 0;

        // INSPECTOR FIELDS
        public GameObject Prefab;
        public Transform SpawnParent;
        [Tooltip("All spawned instances of the Prefab will be given this name, along with a numeric suffix.  If DestroyPrevious is true, then the numeric suffix will not be added.")]
        public string BaseName = "Object";
        public bool DestroyPrevious;
        public float MinSpeed = 0f;
        public float MaxSpeed = 10f;
        [Tooltip("This property defines the direction in which spawned Prefab instances will move.")]
        public SpawnDirection SpawnDirection = SpawnDirection.Straight;
        [Range(0f, 90f)]
        public float ConeHalfAngle = 30f;

        private void Awake() =>
            Assert.IsNotNull(Prefab, $"{nameof(Spawner)} {transform.parent.name}.{name} must be associated with a {nameof(this.Prefab)}!");

        // API INTERFACE
        public void Spawn() {
            // Destroy any previously spawned GameObjects, if requested
            if (_previous != null && DestroyPrevious)
                Destroy(_previous);

            // Instantiating a Prefab can sometimes give a GameObject or a Transform...we want the GameObject
            GameObject obj = (SpawnParent == null) ?
                Instantiate(Prefab, transform.position, transform.rotation) :
                Instantiate(Prefab, transform.position, transform.rotation, SpawnParent);
            obj.name = $"{BaseName}{(DestroyPrevious ? "" : "_" + _count)}";
            if (!DestroyPrevious)
                ++_count;

            // If the Prefab has a Rigidbody, apply the requested velocity
#if DEBUG_2D
            Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
#else
            Rigidbody rb = obj.GetComponent<Rigidbody>();
#endif
            if (rb != null) {
#if DEBUG_2D
                Vector2 dir = getSpawnDirection();
#else
                Vector3 dir = getSpawnDirection();
#endif
                float speed = U.Random.Range(MinSpeed, MaxSpeed);
                if (rb.isKinematic)
                    rb.velocity = speed * dir;
                else
#if DEBUG_2D
                    rb.AddForce(speed * dir, ForceMode.VelocityChange);
#else
                    rb.AddForce(speed * dir, ForceMode.VelocityChange);
#endif
            }

            Debug.Log($"{nameof(Spawner)} {transform.parent.name}.{name} spawned {obj.name} in frame {Time.frameCount}");
            _previous = obj;
        }

        // HELPER FUNCTIONS
#if DEBUG_2D
        private Vector2 getSpawnDirection() {
#else
        private Vector3 getSpawnDirection() {
#endif
            switch (SpawnDirection) {
                case SpawnDirection.Straight: return transform.forward;
                case SpawnDirection.ConeRandom: return onUnitCone(ConeHalfAngle, false);
                case SpawnDirection.ConeBoundary: return onUnitCone(ConeHalfAngle, true);
#if DEBUG_2D
                case SpawnDirection.AnyDirection: return U.Random.insideUnitCircle.normalized;
#else
                case SpawnDirection.AnyDirection: return U.Random.onUnitSphere;
#endif
                default: throw new NotImplementedException($"Gah!  We haven't accounted for {nameof(Unity.SpawnDirection)} {SpawnDirection} yet!");
            }
        }
        private Vector3 onUnitCone(float halfAngle, bool onlyBoundary) {
            float minZ = Mathf.Cos(Mathf.Deg2Rad * halfAngle);
            float z = U.Random.Range(minZ, onlyBoundary ? minZ : 1f);
            float theta = U.Random.Range(0f, 2 * Mathf.PI);
            float sqrtPart = Mathf.Sqrt(1 - z * z);

            var dir = new Vector3(sqrtPart * Mathf.Cos(theta), sqrtPart * Mathf.Sin(theta), z);
            return transform.TransformDirection(dir);
        }
    }

}
