namespace UnityUtil.Math;

/// <summary>
/// Chooses between options in a "randomish" way, i.e., a way that FEELS random to humans over time but may not be truly random.
/// </summary>
/// <typeparam name="TState">Represents state that persists between "randomish" option choices.</typeparam>
public interface IRandomishOptionChooser<TState>
{
    /// <summary>
    /// Chooses an option index in a "randomish" way, i.e., a way that FEELS random to humans over time but may not be truly random.
    /// </summary>
    /// <param name="state">Option choosing state, presumably carried over from the last choice.</param>
    /// <returns>An option index (0-based).</returns>
    int GetWeightedOptionIndex(TState state);
}
