using System;
using S = System;

namespace UnityEngine {

    public sealed class RandomNumberGenerator : MonoBehaviour {

        [Tooltip("Type any string to seed the random number generator, or leave this field blank to use a time-dependent default seed value.")]
        public string Seed;

        private void Awake() {
            int seed = string.IsNullOrEmpty(Seed) ? DateTime.Now.GetHashCode() : Seed.GetHashCode();
            Random.InitState(seed);
            SystemRand = new S.Random(seed);
        }

        public S.Random SystemRand { get; private set; }

    }

}
