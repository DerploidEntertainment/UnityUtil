using System;

namespace UnityUtil.Math;

/// <summary>
/// Provides an interface around <see cref="Random"/> to allow for easier testing and dependency injection.
/// </summary>
public interface IRandomAdapter
{
    /// <summary>
    /// The seed used to initialize <see cref="Rand"/>
    /// </summary>
    int Seed { get; }

    /// <summary>
    /// The adapted <see cref="Random"/> instance
    /// </summary>
    Random Rand { get; }

    /// <summary>
    /// Returns a non-negative random integer.
    /// </summary>
    /// <returns>A 32-bit signed integer that is greater than or equal to 0 and less than <see cref="int.MaxValue"/>.</returns>
    /// <exception cref="InvalidOperationException"><see cref="Rand"/> is <see langword="null"/>.</exception>
    int NextInt() => Rand.Next();

    /// <summary>
    /// Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0.
    /// </summary>
    /// <returns>A double-precision floating point number that is greater than or equal to 0.0, and less than 1.0.</returns>
    /// <exception cref="InvalidOperationException"><see cref="Rand"/> is <see langword="null"/>.</exception>
    double NextDouble() => Rand.NextDouble();

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
    /// <exception cref="InvalidOperationException"><see cref="Rand"/> is <see langword="null"/>.</exception>
    int Range(int inclusiveMin, int exclusiveMax) => Rand.Next(inclusiveMin, exclusiveMax);

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
    /// <exception cref="InvalidOperationException"><see cref="Rand"/> is <see langword="null"/>.</exception>
    float Range(float inclusiveMin, float exclusiveMax) =>
        inclusiveMin > exclusiveMax
            ? throw new InvalidOperationException($"{nameof(inclusiveMin)} must be less than or equal to {nameof(exclusiveMax)}.")
        : (float)(Rand.NextDouble() * (exclusiveMax - inclusiveMin) + inclusiveMin);

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
    /// <exception cref="InvalidOperationException"><see cref="Rand"/> is <see langword="null"/>.</exception>
    double Range(double inclusiveMin, double exclusiveMax) =>
        inclusiveMin > exclusiveMax
            ? throw new InvalidOperationException($"{nameof(inclusiveMin)} must be less than or equal to {nameof(exclusiveMax)}.")
        : Rand.NextDouble() * (exclusiveMax - inclusiveMin) + inclusiveMin;
}
