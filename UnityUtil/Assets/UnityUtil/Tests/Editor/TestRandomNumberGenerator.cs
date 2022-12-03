using UnityUtil.Math;

namespace UnityUtil.Editor.Tests
{
    public class TestRandomNumberGenerator : IRandomNumberGenerator
    {
#pragma warning disable IDE0032 // Use auto property
        private readonly System.Random _rand;
#pragma warning restore IDE0032 // Use auto property

        public System.Random? SystemRand => _rand;

        public string Seed { get; private set; }
        public TestRandomNumberGenerator(int? seed = null)
        {
            seed ??= 13190954;     // Use hard-coded seed by default, so tests are stable
            Seed = seed.ToString();
            _rand = new(seed.Value);

        }
    }
}
