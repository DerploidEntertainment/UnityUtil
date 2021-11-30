using Sirenix.OdinInspector;
using System;
using UnityEngine.Logging;
using S = System;

namespace UnityEngine {

    public sealed class RandomNumberGenerator : Configurable, IRandomNumberGenerator {

        [field: Tooltip("Type any string to seed the random number generator, or leave this field blank to use a time-dependent default seed value.")]
        [field: SerializeField, LabelText(nameof(Seed))]
        public string Seed { get; private set; } = "";

        protected override void Awake() {
            base.Awake();

            (int seed, bool generated) = GetOrGenerateSeed(Seed);
            if (generated) {
                Seed = seed.ToString();
                Logger.Log($"Generated time-dependent seed {seed}", context: this);
            }
            else
                Logger.Log($"Using configured seed {seed}", context: this);

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
