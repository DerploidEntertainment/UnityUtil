using UnityEngine;
using UnityEngine.Assertions;
using U = UnityEngine;

using System;

namespace Danware.Unity {

    public class Spawner : MonoBehaviour {
        // ABSTRACT DATA TYPES
        public enum SpawnerSpawnType {
            Point,
            Straight,
            ConeRandom,
            ConeBoundary,
            SphereRandom
        }

        // HIDDEN FIELDS
        private GameObject _previous;
        private long _count = 0;

        // INSPECTOR FIELDS
        public Transform Prefab;
        public Transform SpawnParent;
        [Tooltip("All spawned instances of the Prefab will be given this name, along with a numeric suffix.  If DestroyPrevious is true, then the numeric suffix will not be added.")]
        public string BaseName = "Object";
        public bool DestroyPrevious;
        public float RigidbodySpeed = 10f;
        public float MinSpeed = 0f;
        public float MaxSpeed = 10f;
        public bool UseRandomSpeed = false;
        public SpawnerSpawnType SpawnType = SpawnerSpawnType.Straight;
        public float ConeHalfAngle = 30f;

        private void Awake() =>
            Assert.IsNotNull(Prefab, $"{nameof(Spawner)} {transform.parent.name}.{name} must be associated with a {nameof(this.Prefab)}!");

        // API INTERFACE
        public void Spawn() {
            // Destroy any previously spawned GameObjects, if requested
            if (_previous != null && DestroyPrevious)
                Destroy(_previous);

            // Instantiating a Prefab can sometimes give a GameObject or a Transform...we want the GameObject
            U.Object obj;
            if (SpawnParent == null)
                obj = Instantiate(Prefab, transform.position, transform.rotation);
            else
                obj = Instantiate(Prefab, transform.position, transform.rotation, SpawnParent);
            _previous = (obj is GameObject) ? obj as GameObject : (obj as Transform).gameObject;
            _previous.name = $"{BaseName}{(DestroyPrevious ? "" : "_" + _count)}";
            if (!DestroyPrevious)
                ++_count;

            // If the Prefab has a Rigidbody, apply the requested velocity
            Rigidbody rb = _previous.GetComponent<Rigidbody>();
            if (rb != null) {
                Vector3 dir = getSpawnDirection();
                float speed = getSpeed();
                if (rb.isKinematic)
                    rb.velocity = speed * dir;
                else
                    rb.AddForce(speed * dir, ForceMode.VelocityChange);
            }

            Debug.Log($"{nameof(Spawner)} {transform.parent.name}.{name} spawned {_previous.name} in frame {Time.frameCount}");
        }

        // HELPER FUNCTIONS
        private Vector3 getSpawnDirection() {
            switch (SpawnType) {
                case SpawnerSpawnType.Point: return Vector3.zero;
                case SpawnerSpawnType.Straight: return transform.forward;
                case SpawnerSpawnType.ConeRandom: return onUnitCone(ConeHalfAngle, false);
                case SpawnerSpawnType.ConeBoundary: return onUnitCone(ConeHalfAngle, true);
                case SpawnerSpawnType.SphereRandom: return U.Random.onUnitSphere;
                default: throw new NotImplementedException();
            }
        }
        private float getSpeed() {
            float speed = UseRandomSpeed ? U.Random.Range(MinSpeed, MaxSpeed) : RigidbodySpeed;
            return speed;
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
