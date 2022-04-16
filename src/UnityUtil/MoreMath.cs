using System;
using System.Collections.Generic;
using System.Linq;
using U = UnityEngine;

namespace UnityEngine;

public static class MoreMath
{

    public const float Sqrt2 = 1.41421f;
    public const float TwoPi = 2 * Mathf.PI;

    /// <summary>
    /// Gets a random index where each index has a provided weight. For example, if [0.3, 0.4, 0.3] is provided, then
    /// there is a 30% chance of returning 0, 40% chance of returning 1, and 30% chance of returning 2.
    /// </summary>
    /// <param name="indexWeights">The weights for each index. These must sum up to 1.</param>
    /// <returns>An index between 0 (inclusive) and the length of <paramref name="indexWeights"/> (exclusive)</returns>
    /// <exception cref="InvalidOperationException">The sum of <paramref name="indexWeights"/> is not 1</exception>
    /// <remarks>
    /// Picture a set of ranges between 0 and 1, with sizes determined by <paramref name="indexWeights"/>
    /// <code>
    /// |-------|----------------------|---|--------|
    /// |------------------^------------------------|
    /// </code>
    /// The caret represents a random value, R, between 0 and 1 (inclusive).
    /// The probability of choosing index i (0-based), according to the specified weights,
    /// equals the probability of R falling within the (i+1)th range (where each range includes its left bound).
    /// E.g., the probability of choosing index 1 equals the probability of R falling within the 2nd range.
    /// Therefore, the index at which the cumulative probability of <paramref name="indexWeights"/> is greater than R is our "chosen" index.
    /// </remarks>
    public static int RandomWeightedIndex(IReadOnlyList<float> indexWeights)
    {
        if (indexWeights.Sum() != 1f)
            throw new InvalidOperationException($"The sum of all {nameof(indexWeights)} must equal 1!");

        int w;
        float val = Random.value;   // Between 0 and 1 (inclusive)
        float sum = indexWeights[0];
        for (w = 0; w < indexWeights.Count - 1 && sum <= val; ++w)
            sum += indexWeights[w + 1];

        return w;
    }

    /// <summary>
    /// Returns a set of <paramref name="count"/> unique random indices into a collection with the provided <paramref name="sourceCount"/>.
    /// Order of the returned indices is unspecified. Note that this method allocates a block of memory that
    /// scales linearly (O(n)) with <paramref name="sourceCount"/>.
    /// </summary>
    /// <param name="count">The number of unique random indices to return.</param>
    /// <param name="sourceCount">The number of elements in the source collection into which we are returning random indices.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> or <paramref name="sourceCount"/> is less than zero.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="count"/> is greater than <paramref name="sourceCount"/>.</exception>
    public static int[] RandomUniqueIndices(int count, int sourceCount)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), count, $" must be greater than or equal to zero.");
        if (sourceCount < 0)
            throw new ArgumentOutOfRangeException(nameof(sourceCount), sourceCount, $" must be greater than or equal to zero.");
        if (count > sourceCount)
            throw new InvalidOperationException($"{nameof(count)} must be less than or equal to {nameof(sourceCount)}");

        int[] resultIndices = new int[count];

        int[] sourceIndices = Enumerable.Range(0, sourceCount).ToArray();
        for (int r = 0; r < count; ++r) {
            int s = U.Random.Range(0, sourceCount - r);
            resultIndices[r] = sourceIndices[s];
            int temp = sourceIndices[s];
            sourceIndices[s] = sourceIndices[sourceCount - r - 1];
            sourceIndices[sourceCount - r - 1] = temp;
        }

        return resultIndices;
    }

    /// <summary>
    /// Returns a random unit vector within a cone of the provided half-angle around the provided <see cref="Transform"/>'s forward vector (uniformly distributed).
    /// </summary>
    /// <param name="transform">The <see cref="Transform"/> around whose forward vector the cone is centered</param>
    /// <param name="halfAngle">The half angle (in degrees) of the cone</param>
    /// <param name="onlyBoundary">If <see langword="true"/>, then the random unit vector will be constrained to the boundary of the cone.  If <see langword="false"/>, then the random unit vector may be anywhere within the cone.</param>
    /// <returns>A random unit vector within a cone of the provided half-angle around the provided <see cref="Transform"/>'s forward vector (uniformly distributed).</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="halfAngle"/> is less then 0° or greater than or equal to 360°.</exception>
    public static Vector3 RandomConeVector(Transform transform, float halfAngle, bool onlyBoundary) =>
        halfAngle < 0f || 360f <= halfAngle
            ? throw new ArgumentOutOfRangeException(nameof(halfAngle), halfAngle, $"Cannot generate a random unit vector within a cone of half-angle {halfAngle}°")
            : randomConeVector(transform.forward, halfAngle, onlyBoundary);
    /// <summary>
    /// Returns a random unit vector within a cone of the provided half-angle centered around the provided axis (uniformly distributed).
    /// </summary>
    /// <param name="axis">The center axis of the cone</param>
    /// <param name="halfAngle">The half angle (in degrees) of the cone</param>
    /// <param name="onlyBoundary">If <see langword="true"/>, then the random unit vector will be constrained to the boundary of the cone.  If <see langword="false"/>, then the random unit vector may be anywhere within the cone.</param>
    /// <returns>A random unit vector within a cone of the provided half-angle around the provided <see cref="Transform"/>'s forward vector (uniformly distributed).</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="halfAngle"/> is less then 0° or greater than or equal to 360°.</exception>
    public static Vector3 RandomConeVector(Vector3 axis, float halfAngle, bool onlyBoundary) =>
        axis == Vector3.zero
            ? throw new ArgumentOutOfRangeException(nameof(axis), axis, $"Cannot generate a random unit vector within a cone whose center axis is the zero vector")
        : halfAngle < 0f || 360f <= halfAngle
            ? throw new ArgumentOutOfRangeException(nameof(halfAngle), halfAngle, $"Cannot generate a random unit vector within a cone of half-angle {halfAngle}°")
        : randomConeVector(axis, halfAngle, onlyBoundary);

    private static Vector3 randomConeVector(Vector3 axis, float halfAngle, bool onlyBoundary)
    {
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
