using System;

namespace UnityUtil.Math;

[Serializable]
public sealed class RandomConfig
{
    public string Seed { get; set; } = "";
}

public sealed class RandomAdapter(RandomConfig config) : IRandomAdapter
{
    private readonly RandomConfig _config = config;

    private int? _seed;
    private Random? _rand;

    /// <inheritdoc/>
    public int Seed => _seed ??= getOrGenerateSeed();

    /// <inheritdoc/>
    public Random Rand => _rand ??= new Random(Seed);

    private int getOrGenerateSeed() => 
        _config.Seed == ""
            ? (int)DateTime.Now.Ticks
        : int.TryParse(_config.Seed, out int intVal)
            ? intVal
        : _config.Seed.GetHashCode(StringComparison.Ordinal);

}
