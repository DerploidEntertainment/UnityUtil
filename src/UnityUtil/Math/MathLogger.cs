using Microsoft.Extensions.Logging;
using UnityUtil.Logging;

namespace UnityUtil.Math;

/// <inheritdoc/>
internal class MathLogger<T>(ILoggerFactory loggerFactory, T context)
    : BaseUnityUtilLogger<T>(loggerFactory, context, eventIdOffset: 8000)
{

    #region Information

    public void SeedRngFromTime(int seed) =>
        LogInformation(id: 0, nameof(SeedRngFromTime), $"Generated PRNG {{{nameof(seed)}}} from current time", seed);

    public void SeedRngFromInspector(int seed) =>
        LogInformation(id: 1, nameof(SeedRngFromInspector), $"Using PRNG seed {{{nameof(seed)}}} from Inspector", seed);

    #endregion
}
