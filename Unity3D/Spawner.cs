using UnityEngine;
using U = UnityEngine;
using System;

namespace Danware.Unity3D {

    public class Spawner : MonoBehaviour {
        // ABSTRACT DATA TYPES
        public enum SpawnerSpawnType {
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
        public bool DestroyPrevious;
        public float RigidbodySpeed = 10f;
        public float MinSpeed = 0f;
        public float MaxSpeed = 10f;
        public bool UseRandomSpeed = false;
        public SpawnerSpawnType SpawnType = SpawnerSpawnType.Straight;
        public float ConeHalfAngle = 30f;

        // API INTERFACE
        public void Spawn() {
            // Destroy any previously spawned GameObjects, if requested
            if (_previous != null && DestroyPrevious)
                Destroy(_previous);

            // Instantiating a Prefab can sometimes give a GameObject or a Transform...we want the GameObject
            U.Object obj = Instantiate(Prefab, transform.position, transform.rotation);
            _previous = (obj is GameObject) ? obj as GameObject : (obj as Transform).gameObject;
            _previous.name += String.Format("_{0}", _count);
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

            Debug.LogFormat("{0} spawned at {1} in frame {2}", _previous.name, transform.position, Time.frameCount);
        }

        // HELPER FUNCTIONS
        private Vector3 getSpawnDirection() {
            switch (SpawnType) {
                case SpawnerSpawnType.Straight:
                    return transform.forward;

                case SpawnerSpawnType.ConeRandom:
                    return onUnitCone(transform.forward, ConeHalfAngle, false);

                case SpawnerSpawnType.ConeBoundary:
                    return onUnitCone(transform.forward, ConeHalfAngle, true);

                case SpawnerSpawnType.SphereRandom:
                    return U.Random.onUnitSphere;

                default:
                    throw new NotImplementedException();
            }
        }
        private float getSpeed() {
            float speed = UseRandomSpeed ? U.Random.Range(MinSpeed, MaxSpeed) : RigidbodySpeed;
            return speed;
        }
        private Vector3 onUnitCone(Vector3 unitAxis, float halfAngle, bool onlyBoundary) {
            // Get a random cone-vector, assuming that the cone is centered on the z-axis
            float minZ = Mathf.Cos(halfAngle);
            float z = onlyBoundary ? minZ : U.Random.Range(minZ, 1f);
            float theta = U.Random.Range(0f, 2 * Mathf.PI);
            float r = Mathf.Sqrt(1f - z * z);
            Vector3 basis = new Vector3(r * Mathf.Cos(theta), r * Mathf.Sin(theta), z);

            // Now rotate that vector to the actual cone's direction
            Vector3 rotAxis = Vector3.Cross(unitAxis, Vector3.forward);
            float rotAngle = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(unitAxis, Vector3.forward));

            return Vector3.one;
        }
    }

}
