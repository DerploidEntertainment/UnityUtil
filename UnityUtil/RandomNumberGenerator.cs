using System;
using S = System;

namespace UnityEngine {

    public sealed class RandomNumberGenerator : Configurable {

        [Tooltip("Type any string to seed the random number generator, or leave this field blank to use a time-dependent default seed value.")]
        public string Seed;

        protected override void OnAwake() {
            int seed;
            if (string.IsNullOrEmpty(Seed)) {
                seed = DateTime.Now.GetHashCode();
                Seed = seed.ToString();
            }
            else {
                bool isInt = int.TryParse(Seed, out seed);
                if (!isInt)
                    seed = Seed.GetHashCode();
            }

            Random.InitState(seed);
            SystemRand = new S.Random(seed);
        }

        public S.Random SystemRand { get; private set; }

    }

}
