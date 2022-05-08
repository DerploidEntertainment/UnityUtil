using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Scripting;

namespace UnityUtil.Math;

public class MaxRepeatsRandomishOptionState
{
    /// <summary>
    /// All <see cref="OptionProbabilities"/> must sum to 1, within this tolerance.
    /// This accounts for probabilities that cannot be accurately represented with floating point numbers (e.g., 9 options of uniform probability).
    /// </summary>
    public const float ProbabilitySumTolerance = 0.000001f;

    private readonly float[] _probabilities;
    private readonly int[] _repeats;
    private readonly int[] _repeatRingBuffer;
    private int _repeatRingBufferIndex = -1;

    public int MaxRepeats { get; private set; }

    public int OptionCount => _probabilities.Length;
    public IReadOnlyList<int> OptionRepeats => _repeats;
    public IReadOnlyList<float> OptionProbabilities => _probabilities;
    public float TotalProbability { get; private set; } = 1f;

    /// <summary>
    /// Creates a new instance of <see cref="MaxRepeatsRandomishOptionState"/>.
    /// </summary>
    /// <param name="maxRepeats">The maximum number of times any option can be chosen in a row.</param>
    /// <param name="optionProbabilities">The probabilities of each option. Must sum to 1.</param>
    /// <exception cref="ArgumentException"><paramref name="optionProbabilities"/> is empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxRepeats"/> is less than 1.</exception>
    /// <exception cref="InvalidOperationException">The sum of <paramref name="optionProbabilities"/> is not 1.</exception>
    public MaxRepeatsRandomishOptionState(int maxRepeats, params float[] optionProbabilities)
    {
        if (maxRepeats < 1)
            throw new ArgumentOutOfRangeException(nameof(maxRepeats), $"{nameof(maxRepeats)} must be greater than or equal to 1.");
        if (optionProbabilities.Length == 0)
            throw new ArgumentException($"{nameof(optionProbabilities)} must have at least one element.", nameof(optionProbabilities));

        float sum = 0f;
        for (int x = 0; x < optionProbabilities.Length; ++x) {
            float weight = optionProbabilities[x];
            if (weight < 0f || weight > 1f)
                throw new InvalidOperationException($"All {nameof(optionProbabilities)} must be >= 0 and <= 1. Index {x} was {weight}.");
            sum += weight;
        }

        if (MathF.Abs(sum - 1f) > ProbabilitySumTolerance)
            throw new InvalidOperationException($"The sum of all {nameof(optionProbabilities)} must equal 1 (± {ProbabilitySumTolerance}).");

        MaxRepeats = maxRepeats;
        _probabilities = optionProbabilities;
        _repeats = new int[optionProbabilities.Length];
        _repeatRingBuffer = Enumerable.Repeat(-1, maxRepeats).ToArray();
    }

    /// <summary>
    /// Adjust state after the option at <paramref name="index"/> is chosen.
    /// </summary>
    /// <param name="index">Index of the option that was chosen.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is out of bounds of the <see cref="OptionRepeats"/> list.</exception>
    /// <exception cref="InvalidOperationException">Option at <paramref name="index"/> has already been repeated the max of <see cref="MaxRepeats"/> times in a row.</exception>
    public void UseOption(int index)
    {
        if (index < 0 || index >= OptionCount)
            throw new ArgumentOutOfRangeException(nameof(index), $"{nameof(index)} must be between 0 (inclusive) and {nameof(OptionCount)} (exclusive).");
        if (OptionCount == 1) {
            _repeats[0] = 1;
            return;
        }
        if (_repeats[index] == MaxRepeats)
            throw new InvalidOperationException($"Option at {nameof(index)} has already been repeated the max of {MaxRepeats} times in a row.");

        ++_repeats[index];
        if (_repeats[index] == MaxRepeats)
            TotalProbability -= _probabilities[index];

        if (_repeatRingBufferIndex == -1)
            _repeatRingBufferIndex = 0;
        else {
            int oldestRepeatIndex = _repeatRingBuffer[_repeatRingBufferIndex];
            if (oldestRepeatIndex > -1 && _repeats[oldestRepeatIndex] > 0) {
                --_repeats[oldestRepeatIndex];
                if (_repeats[oldestRepeatIndex] == MaxRepeats - 1)
                    TotalProbability += _probabilities[oldestRepeatIndex];
            }
        }
        _repeatRingBuffer[_repeatRingBufferIndex] = index;
        _repeatRingBufferIndex = (_repeatRingBufferIndex + 1) % MaxRepeats;
    }
}

/// <summary>
/// Chooses between options in a "randomish" way, i.e., a way that FEELS random to humans over time but is not truly random.
/// Achieved by ensuring that options are chosen at most <see cref="MaxRepeats"/> times in a row.
/// </summary>
public class MaxRepeatsRandomishOptionChooser : IRandomishOptionChooser<MaxRepeatsRandomishOptionState>
{
    private readonly IRandomNumberGenerator _randomNumberGenerator;

    [Preserve]
    public MaxRepeatsRandomishOptionChooser(IRandomNumberGenerator randomNumberGenerator)
    {
        _randomNumberGenerator = randomNumberGenerator;
    }

    /// <summary>
    /// Chooses an option index using "randomish" logic, while updating <paramref name="state"/>.
    /// This is an O(n) operation, where n is the number options in <paramref name="state"/>.
    /// </summary>
    /// <param name="state">Option choosing state, presumably carried over from the last choice.</param>
    /// <param name="randomNumberGenerator">The object used to generate pseudorandom numbers.</param>
    /// <returns>
    /// An index between 0 (inclusive) and <paramref name="state"/>'s <see cref="MaxRepeatsRandomishOptionState.OptionCount"/> (exclusive).
    /// </returns>
    /// <remarks>
    /// <para>
    /// Indices are chosen using <paramref name="state"/>'s <see cref="MaxRepeatsRandomishOptionState.OptionProbabilities"/>
    /// while ensuring that no option is returned more than <see cref="MaxRepeatsRandomishOptionState.MaxRepeats"/> times in a row.
    /// For example, suppose [0.3, 0.4, 0.3] are the specified probabilities, with a max repeat of 3.
    /// Initially, there is a 30% chance of returning 0, 40% chance of returning 1, and 30% chance of returning 2.
    /// If index 1 is returned 3 times in a row, then on the next call, there will instead be a 50% chance of returning 0 and a 50% chance of returning 2
    /// (maintaining the relative probabilities of those options, while preventing 1 from being returned again).
    /// On the next call, index 1 has no longer been returned 3 times in a row, so the probabilties go back to their initial values.
    /// </para>
    /// <para>
    /// To understand this method's logic, picture a set of ranges between 0 and 1, with sizes determined by the <see cref="MaxRepeatsRandomishOptionState.OptionProbabilities"/>.
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
    /// Therefore, the index at which the cumulative probability of <see cref="MaxRepeatsRandomishOptionState.OptionProbabilities"/> is greater than R is the "chosen" index.
    /// </para>
    /// <para>
    /// <paramref name="state"/> maintains a collection of the last <see cref="MaxRepeatsRandomishOptionState.MaxRepeats"/> indices returned.
    /// When an option has been chosen more than the max allowed times in a row, its probability is temporarily zero.
    /// In the above visualization, this is equivalent to removing the interval for that option, and then restricting R
    /// to the range between 0 and (1 - that option's probability).
    /// </para>
    /// </remarks>
    public int GetWeightedOptionIndex(MaxRepeatsRandomishOptionState state)
    {
        float val = (float)_randomNumberGenerator.Range(0f, state.TotalProbability);

        int index = -1;
        float sum = 0f;
        for (int x = 0; x < state.OptionCount; ++x) {
            sum += (state.OptionRepeats[x] < state.MaxRepeats ? state.OptionProbabilities[x] : 0f);
            if (index == -1 && sum > val)
                index = x;
        }

        int resultIndex = index == -1 ? state.OptionCount - 1 : index;
        state.UseOption(resultIndex);

        return resultIndex;
    }
}
