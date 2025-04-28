using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityUtil.Math;

public class MaxRepeatsRandomishOptionChooserConfig
{
    public int MaxRepeats { get; set; } = 1;
    public IReadOnlyList<float> OptionProbabilities { get; set; } = [1f];
}

/// <summary>
/// <para>
/// Chooses between options in a "randomish" way, i.e., a way that <em>feels</em> random to humans over time but is not truly random.
/// Achieved by repeating options some max number of times in a row.
/// </para>
/// <para>
/// This type is not thread safe. If randomish option indices must be accessed by multiple threads, you will need to implement your own synchronization mechanism.
/// </para>
/// </summary>
public class MaxRepeatsRandomishOptionChooser : IRandomishOptionChooser
{
    /// <summary>
    /// All <see cref="MaxRepeatsRandomishOptionChooserConfig.OptionProbabilities"/> must sum to 1, within this tolerance.
    /// This accounts for probabilities that cannot be accurately represented with floating point numbers (e.g., 1/9).
    /// </summary>
    public const float ProbabilitySumTolerance = 0.000001f;

    private readonly IRandomAdapter _randomAdapter;
    private readonly MaxRepeatsRandomishOptionChooserConfig _config;

    private readonly int[] _repeats;
    private readonly int[] _repeatRingBuffer;
    private int _repeatRingBufferIndex = -1;

    public MaxRepeatsRandomishOptionChooser(IRandomAdapter randomAdapter, MaxRepeatsRandomishOptionChooserConfig config)
    {
        _randomAdapter = randomAdapter;
        _config = config;

        if (_config.MaxRepeats < 1)
            throw new ArgumentException($"{nameof(config)} must have {nameof(config.MaxRepeats)} greater than or equal to 1.", nameof(config));
        if (_config.OptionProbabilities.Count == 0)
            throw new ArgumentException($"{nameof(config)} must have {nameof(config.OptionProbabilities)} with at least one element.", nameof(config));

        float sum = 0f;
        for (int x = 0; x < _config.OptionProbabilities.Count; ++x) {
            float weight = _config.OptionProbabilities[x];
            if (weight < 0f || weight > 1f)
                throw new InvalidOperationException($"All {nameof(_config.OptionProbabilities)} must be between 0 and 1, inclusive. Index {x} was {weight}.");
            sum += weight;
        }

        if (System.Math.Abs(sum - 1f) > ProbabilitySumTolerance)
            throw new InvalidOperationException($"The sum of all {nameof(_config.OptionProbabilities)} must equal 1 (± {ProbabilitySumTolerance}).");

        _repeats = new int[_config.OptionProbabilities.Count];
        _repeatRingBuffer = [.. Enumerable.Repeat(-1, _config.MaxRepeats)];
    }

    /// <summary>
    /// Number of times the option at each index has been repeated during the last <see cref="MaxRepeatsRandomishOptionChooserConfig.MaxRepeats"/> calls to <see cref="GetOptionIndex"/>.
    /// </summary>
    public IReadOnlyList<int> OptionRepeats => _repeats;
    internal float TotalProbability { get; private set; } = 1f;

    /// <summary>
    /// Chooses an option index using "randomish" logic.
    /// This is an O(n) operation, where n is the number of options.
    /// </summary>
    /// <returns>
    /// An index between 0 (inclusive) and the number of options (exclusive).
    /// </returns>
    /// <remarks>
    /// <para>
    /// Indices are chosen using the configured <see cref="MaxRepeatsRandomishOptionChooserConfig.OptionProbabilities"/>
    /// while ensuring that no option is returned more than <see cref="MaxRepeatsRandomishOptionChooserConfig.MaxRepeats"/> times in a row.
    /// For example, suppose [0.3, 0.4, 0.3] are the specified probabilities, with a max repeat of 3.
    /// Initially, there is a 30% chance of returning 0, 40% chance of returning 1, and 30% chance of returning 2.
    /// If index 1 is returned 3 times in a row, then on the next call, there will instead be a 50% chance of returning 0 and a 50% chance of returning 2
    /// (maintaining the relative probabilities of those options, while preventing 1 from being returned again).
    /// On the next call, index 1 has no longer been returned 3 times in a row, so the probabilties go back to their initial values.
    /// </para>
    /// <para>
    /// To understand this method's logic, picture a set of ranges between 0 and 1, with sizes determined by the <see cref="MaxRepeatsRandomishOptionChooserConfig.OptionProbabilities"/>.
    /// </para>
    /// <code>
    /// |-------|----------------------|---|--------|
    /// |------------------^------------------------|
    /// </code>
    /// <para>
    /// The caret represents a random value, R, between 0 and 1 (inclusive).
    /// The probability of choosing option i (0-based), according to the specified probabilities,
    /// equals the probability of R falling within the (i+1)th range (where each range includes its left bound).
    /// E.g., the probability of choosing index 1 equals the probability of R falling within the 2nd range.
    /// Therefore, the index at which the cumulative probability of <see cref="MaxRepeatsRandomishOptionChooserConfig.OptionProbabilities"/> is greater than R is the "chosen" index.
    /// </para>
    /// <para>
    /// <see cref="MaxRepeatsRandomishOptionChooser"/> maintains a collection of the last <see cref="MaxRepeatsRandomishOptionChooserConfig.MaxRepeats"/> indices returned.
    /// When an option has been chosen more than the max allowed times in a row, its probability is temporarily zero.
    /// In the above visualization, this is equivalent to collapsing that option's interval to zero width, and then restricting R
    /// to the range between 0 and (1 - that option's probability).
    /// </para>
    /// </remarks>
    public int GetOptionIndex()
    {
        float val = _randomAdapter.Range(0f, TotalProbability);

        int index = -1;
        float sum = 0f;
        for (int x = 0; x < _config.OptionProbabilities.Count; ++x) {
            sum += (OptionRepeats[x] < _config.MaxRepeats ? _config.OptionProbabilities[x] : 0f);
            if (index == -1 && sum > val)
                index = x;
        }

        int resultIndex = index == -1 ? _config.OptionProbabilities.Count - 1 : index;
        UseOption(resultIndex);

        return resultIndex;
    }

    /// <summary>
    /// Adjust state after the option at <paramref name="index"/> is chosen.
    /// </summary>
    /// <param name="index">Index of the option that was chosen.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is out of bounds of the <see cref="OptionRepeats"/> list.</exception>
    /// <exception cref="InvalidOperationException">Option at <paramref name="index"/> has already been repeated the max of <see cref="MaxRepeatsRandomishOptionChooserConfig.MaxRepeats"/> times in a row.</exception>
    internal void UseOption(int index)
    {
        if (index < 0 || index >= _config.OptionProbabilities.Count)
            throw new ArgumentOutOfRangeException(nameof(index), $"{nameof(index)} must be between 0 (inclusive) and the number of available options (exclusive).");
        if (_config.OptionProbabilities.Count == 1) {
            _repeats[0] = 1;
            return;
        }
        if (_repeats[index] == _config.MaxRepeats)
            throw new InvalidOperationException($"Option at {nameof(index)} has already been repeated the max of {_config.MaxRepeats} times in a row.");

        ++_repeats[index];
        if (_repeats[index] == _config.MaxRepeats)
            TotalProbability -= _config.OptionProbabilities[index];

        if (_repeatRingBufferIndex == -1)
            _repeatRingBufferIndex = 0;
        else {
            int oldestRepeatIndex = _repeatRingBuffer[_repeatRingBufferIndex];
            if (oldestRepeatIndex > -1 && _repeats[oldestRepeatIndex] > 0) {
                --_repeats[oldestRepeatIndex];
                if (_repeats[oldestRepeatIndex] == _config.MaxRepeats - 1)
                    TotalProbability += _config.OptionProbabilities[oldestRepeatIndex];
            }
        }
        _repeatRingBuffer[_repeatRingBufferIndex] = index;
        _repeatRingBufferIndex = (_repeatRingBufferIndex + 1) % _config.MaxRepeats;
    }
}
