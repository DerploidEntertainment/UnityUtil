using Microsoft.Extensions.Logging;
using UnityUtil.Logging;
using static Microsoft.Extensions.Logging.LogLevel;

namespace UnityUtil.Triggers;

/// <inheritdoc/>
internal class TriggersLogger<T> : BaseUnityUtilLogger<T>
{
    public TriggersLogger(ILoggerFactory loggerFactory, T context)
        : base(loggerFactory, context, eventIdOffset: 13_000) { }

    #region Trace

    #endregion

    #region Debug

    #endregion

    #region Information

    #endregion

    #region Warning

    public void SequenceTriggerNullStep(int step) =>
        Log(id: 0, nameof(SequenceTriggerNullStep), Warning, $"Triggered at step {{{nameof(step)}}}, but the trigger was null", step);

    #endregion

    #region Error

    #endregion

    #region Critical

    #endregion
}
