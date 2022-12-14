using Microsoft.Extensions.Logging;
using UnityUtil.Logging;
using static Microsoft.Extensions.Logging.LogLevel;

namespace UnityUtil.Math;

/// <inheritdoc/>
internal class MathLogger<T> : BaseUnityUtilLogger<T>
{
    public MathLogger(ILoggerFactory loggerFactory, T context)
        : base(loggerFactory, context, eventIdOffset: 8000) { }

    #region Information

    public void SeedRngFromTime(int seed) =>
        Log(id: 0, nameof(SeedRngFromTime), Information, $"Generated PRNG {{{nameof(seed)}}} from current time", seed);

    public void SeedRngFromInspector(int seed) =>
        Log(id: 1, nameof(SeedRngFromInspector), Information, $"Using PRNG seed {{{nameof(seed)}}} from Inspector", seed);

    #endregion

    #region Warning

    #endregion

    #region Error

    #endregion

}
