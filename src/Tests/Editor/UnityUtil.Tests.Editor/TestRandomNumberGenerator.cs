using UnityUtil.Math;

namespace UnityUtil.Editor.Tests;

public class TestRandomNumberGenerator : IRandomNumberGenerator
{
    public System.Random? SystemRand { get; }

    public string Seed { get; private set; }
    public TestRandomNumberGenerator(int? seed = null)
    {
        seed ??= 13190954;     // Use hard-coded seed by default, so tests are stable
        Seed = seed.ToString();
        SystemRand = new(seed.Value);

    }
}
