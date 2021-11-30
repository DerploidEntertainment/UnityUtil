using S = System;

namespace UnityEngine {

    public interface IRandomNumberGenerator {

        string Seed { get; }
        S.Random? SystemRand { get; }

    }

}
