namespace UnityUtil.Math;

/// <summary>
/// Chooses between options in a "randomish" way, i.e., a way that <em>feels</em> random to humans over time but may not be truly (psuedo)random.
/// </summary>
public interface IRandomishOptionChooser
{
    /// <summary>
    /// Chooses an option index in a "randomish" way, i.e., a way that <em>feels</em> random to humans over time but may not be truly (psuedo)random.
    /// </summary>
    /// <returns>
    /// An index between 0 (inclusive) and the number of options (exclusive).
    /// </returns>
    int GetOptionIndex();
}
