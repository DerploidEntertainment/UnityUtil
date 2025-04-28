using System;
using UnityUtil.Math;

namespace UnityUtil.Tests.Editor;

public class TestRandomAdapter : IRandomAdapter
{
    public int Seed { get; private set; }
    public Random Rand { get; }

    public TestRandomAdapter(int? seed = null)
    {
        Seed = seed ?? 13190954;     // Use hard-coded seed by default, so tests are stable
        Rand = new(Seed);
    }
}
