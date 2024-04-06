using Microsoft.Extensions.Logging;
using Sirenix.OdinInspector;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using UnityEngine;
using UnityUtil.DependencyInjection;
using S = System;

namespace UnityUtil.Math;

[Serializable]
public sealed class RandomNumberGeneratorConfig
{
    public string Seed { get; set; } = "";
}

[CreateAssetMenu(menuName = $"{nameof(UnityUtil)}/{nameof(UnityUtil.Math)}/{nameof(RandomNumberGenerator)}", fileName = "random-number-generator")]
public sealed class RandomNumberGenerator : ScriptableObject, IRandomNumberGenerator
{
    [ReadOnly, ShowInInspector]
    private RandomNumberGeneratorConfig? _config;
    private MathLogger<RandomNumberGenerator>? _logger;

    public string Seed { get; private set; } = "";

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void Awake()
    {
        DependencyInjector.Instance.ResolveDependenciesOf(this);

        Seed = _config!.Seed;
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

    public void Inject(RandomNumberGeneratorConfig config, ILoggerFactory loggerFactory)
    {
        _config = config;
        _logger = new(loggerFactory, context: this);
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
