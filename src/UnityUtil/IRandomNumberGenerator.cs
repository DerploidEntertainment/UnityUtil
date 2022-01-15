using System;
using System.Diagnostics.CodeAnalysis;
using S = System;

namespace UnityEngine
{

    [SuppressMessage("Security", "CA5394:Do not use insecure randomness", Justification = "Interface is not intended to be secure")]
    public interface IRandomNumberGenerator
    {
        string Seed { get; }
        S.Random? SystemRand { get; }

        int NextInt() => SystemRand?.Next() ?? throw new InvalidOperationException($"{nameof(SystemRand)} must be non-null before calling {nameof(NextInt)}.");
        int Range(int inclusiveMin, int exclusiveMax) =>
            SystemRand?.Next(inclusiveMin, exclusiveMax) ?? throw new InvalidOperationException($"{nameof(SystemRand)} must be non-null before calling {nameof(NextInt)}.");

    }

}
