using Sirenix.OdinInspector;
using System;
using System.Globalization;
using UnityEngine;
using UnityUtil.Configuration;
using S = System;

namespace UnityUtil.Math;

public sealed class RandomNumberGenerator : Configurable, IRandomNumberGenerator
{
    private MathLogger<RandomNumberGenerator>? _logger;

    [field: Tooltip("Type any string to seed the random number generator, or leave this field blank to use a time-dependent default seed value.")]
    [field: SerializeField, LabelText(nameof(Seed))]
    public string Seed { get; private set; } = "";

    protected override void Awake()
    {
        base.Awake();

        _logger = new(LoggerFactory!, context: this);

        (int seed, bool generated) = GetOrGenerateSeed(Seed);
        if (generated) {
            Seed = seed.ToString(CultureInfo.InvariantCulture);
            _logger!.SeedRngFromTime(seed);
        }
        else
            _logger!.SeedRngFromInspector(seed);

        UnityEngine.Random.InitState(seed);
        SystemRand = new S.Random(seed);
    }

    public S.Random? SystemRand { get; private set; }

    internal (int seed, bool generated) GetOrGenerateSeed(string configSeed)
    {
        int seed;
        bool generated;

        if (string.IsNullOrEmpty(configSeed)) {
            seed = DateTime.Now.GetHashCode();
            generated = true;
        }
        else {
            bool isInt = int.TryParse(Seed, out seed);
            if (!isInt)
                seed = Seed.GetHashCode(StringComparison.Ordinal);
            generated = false;
        }

        return (seed, generated);
    }

}
