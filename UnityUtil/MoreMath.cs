﻿using System;
using U = UnityEngine;

namespace UnityEngine {

    public static class MoreMath {

        public const float Sqrt2 = 1.41421f;
        public const float TwoPi = 2 * Mathf.PI;

        public static int RandomWeightedIndex(float[] indexWeights) {
            // Get a random float between 0 and 1 (inclusive)
            float val = Random.value;

            // Picture a set of ranges between 0 and 1, with sizes determined by the index weights
            // |-------|----------------------|---|--------|
            // |------------------^------------------------|
            // The carrot represents the random value
            // The probability of choosing index i according to the specified weights
            // is the same as that of the random value falling within the (i+1)th range (left-bound inclusive).
            // Therefore, the index at which the cumulative probability is greater than the random value is our "chosen" index
            float sum = indexWeights[0];
            int w;
            for (w = 0; w < indexWeights.Length - 1 && sum <= val; ++w)
                sum += indexWeights[w + 1];

            return w;
        }

        /// <summary>
        /// Returns a random unit vector within a cone of the provided half-angle around the provided <see cref="Transform"/>'s forward vector (uniformly distributed).
        /// </summary>
        /// <param name="transform">The <see cref="Transform"/> around whose forward vector the cone is centered</param>
        /// <param name="halfAngle">The half angle (in degrees) of the cone</param>
        /// <param name="onlyBoundary">If <see langword="true"/>, then the random unit vector will be constrained to the boundary of the cone.  If <see langword="false"/>, then the random unit vector may be anywhere within the cone.</param>
        /// <returns>A random unit vector within a cone of the provided half-angle around the provided <see cref="Transform"/>'s forward vector (uniformly distributed).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transform"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="halfAngle"/> is less then 0° or greater than or equal to 360°.</exception>
        public static Vector3 RandomConeVector(Transform transform, float halfAngle, bool onlyBoundary) {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));
            if (halfAngle < 0f || 360f <= halfAngle)
                throw new ArgumentOutOfRangeException(nameof(halfAngle), halfAngle, $"Cannot generate a random unit vector within a cone of half-angle {halfAngle}°");

            return randomConeVector(transform.forward, halfAngle, onlyBoundary);
        }
        /// <summary>
        /// Returns a random unit vector within a cone of the provided half-angle centered around the provided axis (uniformly distributed).
        /// </summary>
        /// <param name="axis">The center axis of the cone</param>
        /// <param name="halfAngle">The half angle (in degrees) of the cone</param>
        /// <param name="onlyBoundary">If <see langword="true"/>, then the random unit vector will be constrained to the boundary of the cone.  If <see langword="false"/>, then the random unit vector may be anywhere within the cone.</param>
        /// <returns>A random unit vector within a cone of the provided half-angle around the provided <see cref="Transform"/>'s forward vector (uniformly distributed).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="axis"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="halfAngle"/> is less then 0° or greater than or equal to 360°.</exception>
        public static Vector3 RandomConeVector(Vector3 axis, float halfAngle, bool onlyBoundary) {
            if (axis == Vector3.zero)
                throw new ArgumentOutOfRangeException(nameof(axis), axis, $"Cannot generate a random unit vector within a cone whose center axis is the zero vector");
            if (halfAngle < 0f || 360f <= halfAngle)
                throw new ArgumentOutOfRangeException(nameof(halfAngle), halfAngle, $"Cannot generate a random unit vector within a cone of half-angle {halfAngle}°");

            return randomConeVector(axis, halfAngle, onlyBoundary);
        }

        private static Vector3 randomConeVector(Vector3 axis, float halfAngle, bool onlyBoundary) {
            // Logic taken from @joriki's answer on the following StackExchange post: https://math.stackexchange.com/questions/56784/generate-a-random-direction-within-a-cone

            // Get random direction in cone centered around Vector3.forward
            float minZ = Mathf.Cos(Mathf.Deg2Rad * halfAngle);
            float z = U.Random.Range(minZ, onlyBoundary ? minZ : 1f);
            float phi = U.Random.Range(0f, TwoPi);
            float sqrtPart = Mathf.Sqrt(1f - z * z);
            var result = new Vector3(sqrtPart * Mathf.Cos(phi), sqrtPart * Mathf.Sin(phi), z);

            // Rotate direction so that it "came from" a cone centered on the provided axis
            if (axis == Vector3.forward)
                return result;
            else if (axis == Vector3.back)
                return -result;
            else {
                var rotVector = Vector3.Cross(axis, Vector3.forward);
                float rotDegrees = Vector3.Angle(axis, Vector3.forward);
                var rot = Quaternion.AngleAxis(rotDegrees, rotVector);
                return rot * result;
            }
        }

    }

}