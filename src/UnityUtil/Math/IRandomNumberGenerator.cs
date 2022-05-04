using System;
using System.Diagnostics.CodeAnalysis;
using S = System;

namespace UnityUtil.Math;

[SuppressMessage("Security", "CA5394:Do not use insecure randomness", Justification = "Interface is not intended to be secure")]
public interface IRandomNumberGenerator
{
    string Seed { get; }

    S.Random? SystemRand { get; }

    /// <summary>
    /// Returns a non-negative random integer.
    /// </summary>
    /// <returns>A 32-bit signed integer that is greater than or equal to 0 and less than <see cref="int.MaxValue"/>.</returns>
    /// <exception cref="InvalidOperationException"><see cref="SystemRand"/> is <see langword="null"/>.</exception>
    int NextInt() => SystemRand?.Next() ?? throw new InvalidOperationException($"{nameof(SystemRand)} must be non-null before calling {nameof(NextInt)}.");

    /// <summary>
    /// Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0.
    /// </summary>
    /// <returns>A double-precision floating point number that is greater than or equal to 0.0, and less than 1.0.</returns>
    /// <exception cref="InvalidOperationException"><see cref="SystemRand"/> is <see langword="null"/>.</exception>
    double NextDouble() => SystemRand?.NextDouble() ?? throw new InvalidOperationException($"{nameof(SystemRand)} must be non-null before calling {nameof(NextDouble)}.");

    /// <summary>
    /// Returns a random integer that is within a specified range.
    /// </summary>
    /// <param name="inclusiveMin">The inclusive lower bound of the random number returned.</param>
    /// <param name="exclusiveMax">The exclusive upper bound of the random number returned. <paramref name="exclusiveMax"/> must be greater than or equal to <paramref name="inclusiveMin"/>.</param>
    /// <returns>
    /// A 32-bit signed integer greater than or equal to <paramref name="inclusiveMin"/> and less than <paramref name="exclusiveMax"/>;
    /// that is, the range of return values includes <paramref name="inclusiveMin"/> but not <paramref name="exclusiveMax"/>.
    /// If <paramref name="inclusiveMin"/> equals <paramref name="exclusiveMax"/>, then <paramref name="inclusiveMin"/> is returned.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="inclusiveMin"/> is greater than <paramref name="exclusiveMax"/>.</exception>
    /// <exception cref="InvalidOperationException"><see cref="SystemRand"/> is <see langword="null"/>.</exception>
    int Range(int inclusiveMin, int exclusiveMax) =>
        SystemRand?.Next(inclusiveMin, exclusiveMax)
            ?? throw new InvalidOperationException($"{nameof(SystemRand)} must be non-null before calling {nameof(NextInt)}.");

    /// <summary>
    /// Returns a random floating-point number that is within a specified range.
    /// </summary>
    /// <param name="inclusiveMin">The inclusive lower bound of the random number returned.</param>
    /// <param name="exclusiveMax">The exclusive upper bound of the random number returned.</param>
    /// <returns>
    /// A floating point number greater than or equal to <paramref name="inclusiveMin"/> and less than <paramref name="exclusiveMax"/>;
    /// that is, the range of return values includes <paramref name="inclusiveMin"/> but not <paramref name="exclusiveMax"/>.
    /// If <paramref name="inclusiveMin"/> equals <paramref name="exclusiveMax"/>, then <paramref name="inclusiveMin"/> is returned.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="inclusiveMin"/> is greater than <paramref name="exclusiveMax"/>.</exception>
    /// <exception cref="InvalidOperationException"><see cref="SystemRand"/> is <see langword="null"/>.</exception>
    float Range(float inclusiveMin, float exclusiveMax) =>
        SystemRand is null
            ? throw new InvalidOperationException($"{nameof(SystemRand)} must be non-null before calling {nameof(NextInt)}.")
        : inclusiveMin > exclusiveMax
            ? throw new InvalidOperationException($"{nameof(inclusiveMin)} must be less than or equal to {nameof(exclusiveMax)}.")
        : (float)(SystemRand.NextDouble() * (exclusiveMax - inclusiveMin) + inclusiveMin);

    /// <summary>
    /// Returns a random floating-point number that is within a specified range.
    /// </summary>
    /// <param name="inclusiveMin">The inclusive lower bound of the random number returned.</param>
    /// <param name="exclusiveMax">The exclusive upper bound of the random number returned.</param>
    /// <returns>
    /// A floating point number greater than or equal to <paramref name="inclusiveMin"/> and less than <paramref name="exclusiveMax"/>;
    /// that is, the range of return values includes <paramref name="inclusiveMin"/> but not <paramref name="exclusiveMax"/>.
    /// If <paramref name="inclusiveMin"/> equals <paramref name="exclusiveMax"/>, then <paramref name="inclusiveMin"/> is returned.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="inclusiveMin"/> is greater than <paramref name="exclusiveMax"/>.</exception>
    /// <exception cref="InvalidOperationException"><see cref="SystemRand"/> is <see langword="null"/>.</exception>
    double Range(double inclusiveMin, double exclusiveMax) =>
        SystemRand is null
            ? throw new InvalidOperationException($"{nameof(SystemRand)} must be non-null before calling {nameof(NextInt)}.")
        : inclusiveMin > exclusiveMax
            ? throw new InvalidOperationException($"{nameof(inclusiveMin)} must be less than or equal to {nameof(exclusiveMax)}.")
        : SystemRand.NextDouble() * (exclusiveMax - inclusiveMin) + inclusiveMin;
}
