﻿using System;
using UnityEngine;
using UnityEngine.Assertions;
using U = UnityEngine;

namespace UnityUtil {

    /// <summary>
    /// Determines the direction in which <see cref="Spawner.Prefab"/> instances spawned by a <see cref="UnityUtil.Spawner"/> are launched.
    /// </summary>
    public enum SpawnDirection {
        /// <summary>
        /// Spawned <see cref="Spawner.Prefab"/> instances will be launched along the <see cref="UnityUtil.Spawner"/>'s forward vector.
        /// </summary>
        Straight,

        /// <summary>
        /// Spawned <see cref="Spawner.Prefab"/> instances will be launched along a random vector within a cone centered around the <see cref="UnityUtil.Spawner"/>'s forward vector.
        /// </summary>
        ConeRandom,

        /// <summary>
        /// Spawned <see cref="Spawner.Prefab"/> instances will be launched along a random vector on the boundary of a cone centered around the <see cref="UnityUtil.Spawner"/>'s forward vector.
        /// </summary>
        ConeBoundary,

        /// <summary>
        /// Spawned <see cref="Spawner.Prefab"/> instances will be launched along any random vector.
        /// </summary>
        AnyDirection,
    }

    public class Spawner : MonoBehaviour {

        // HIDDEN FIELDS
        private GameObject _previous;
        private long _count = 0;

        // INSPECTOR FIELDS
        [Tooltip("The actual Unity prefab to spawn.  We highly recommend using a PREFAB, as opposed to an existing GameObject in the Scene, though either will technically work.")]
        public GameObject Prefab;
        [Tooltip("All spawned instances of " + nameof(Spawner.Prefab) + " will be parented to this Transform.")]
        public Transform SpawnParent;
        [Tooltip("All spawned " + nameof(Spawner.Prefab) + " instances will be given this name, along with a numeric suffix.  If " + nameof(Spawner.DestroyPrevious) + " is true, then the numeric suffix will not be added.")]
        public string BaseName = "Object";
        [Tooltip("If true, then previously spawned " + nameof(Spawner.Prefab) + " instances will be destroyed before the next instance is spawned, so there will only ever be one spawned instance in existence.  If false, then multiple instances may be spawned.")]
        public bool DestroyPrevious;
        [Tooltip("All spawned " + nameof(Spawner.Prefab) + " instances will be launched in the " + nameof(Spawner.SpawnDirection) + ", with at least this speed.  Setting both " + nameof(Spawner.MinSpeed) + " and " + nameof(Spawner.MaxSpeed) + " to zero will spawn instances right at this " + nameof(UnityUtil.Spawner) + "'s position, without any launching.")]
        public float MinSpeed = 0f;
        [Tooltip("All spawned " + nameof(Spawner.Prefab) + " instances will be launched in the " + nameof(Spawner.SpawnDirection) + ", with at most this speed.  Setting both " + nameof(Spawner.MinSpeed) + " and " + nameof(Spawner.MaxSpeed) + " to zero will spawn instances right at this " + nameof(UnityUtil.Spawner) + "'s position, without any launching.")]
        public float MaxSpeed = 10f;
        [Tooltip("This property defines the direction in which spawned " + nameof(Spawner.Prefab) + " instances will be  launched.")]
        public SpawnDirection SpawnDirection = SpawnDirection.Straight;
        [Tooltip("If " + nameof(Spawner.SpawnDirection) + " is set to " + nameof(SpawnDirection.ConeRandom) + " or " + nameof(SpawnDirection.ConeBoundary) + ", then this value determines the half-angle of that cone.  Otherwise, this value is ignored.")]
        [Range(0f, 90f)]
        public float ConeHalfAngle = 30f;

        private void Awake() => 
            Assert.IsNotNull(Prefab, this.GetAssociationAssertion(nameof(this.Prefab)));

        // API INTERFACE
        public void Spawn() {
            // Destroy any previously spawned GameObjects, if requested
            if (_previous != null && DestroyPrevious)
                Destroy(_previous);

            string newName = $"{BaseName}{(DestroyPrevious ? "" : "_" + _count)}";
            this.Log($" spawning {newName}");

            // Instantiating a Prefab can sometimes give a GameObject or a Transform...we want the GameObject
            GameObject obj = (SpawnParent == null) ?
                Instantiate(Prefab, transform.position, transform.rotation) :
                Instantiate(Prefab, transform.position, transform.rotation, SpawnParent);
            obj.name = newName;
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
                default: throw new NotImplementedException(ConditionalLogger.GetSwitchDefault(SpawnDirection));
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
