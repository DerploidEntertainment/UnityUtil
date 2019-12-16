using System;
using S = System;

namespace UnityEngine {

    public sealed class RandomNumberGenerator : Configurable {

        [Tooltip("Type any string to seed the random number generator, or leave this field blank to use a time-dependent default seed value.")]
        public string Seed;

        protected override void OnAwake() {
            (int seed, bool generated) = GetOrGenerateSeed(Seed);
            if (generated) {
                Seed = seed.ToString();
                Logger.Log($"Generated time-dependent seed {seed}");
            }
            else
                Logger.Log($"Using configured seed {seed}");

            Random.InitState(seed);
            SystemRand = new S.Random(seed);
        }

        public S.Random SystemRand { get; private set; }

        internal (int seed, bool generated) GetOrGenerateSeed(string configSeed) {
            int seed;
            bool generated;

            if (string.IsNullOrEmpty(configSeed)) {
                seed = DateTime.Now.GetHashCode();
                generated = true;
            }
            else {
                bool isInt = int.TryParse(Seed, out seed);
                if (!isInt)
                    seed = Seed.GetHashCode();
                generated = false;
            }

            return (seed, generated);
        }

    }

}
