using UnityEngine;
using U = UnityEngine;

using System;

namespace Danware.Unity {

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
            _previous.name += string.Format("_{0}", _count);
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
                    return onUnitCone(ConeHalfAngle, false);

                case SpawnerSpawnType.ConeBoundary:
                    return onUnitCone(ConeHalfAngle, true);

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
        private Vector3 onUnitCone(float halfAngle, bool onlyBoundary) {
            float minZ = Mathf.Cos(Mathf.Deg2Rad * halfAngle);
            float z = U.Random.Range(minZ, onlyBoundary ? minZ : 1f);
            float theta = U.Random.Range(0f, 2 * Mathf.PI);
            float sqrtPart = Mathf.Sqrt(1 - z * z);

            Vector3 dir = new Vector3(sqrtPart * Mathf.Cos(theta), sqrtPart * Mathf.Sin(theta), z);
            return transform.TransformDirection(dir);
        }
    }

}
