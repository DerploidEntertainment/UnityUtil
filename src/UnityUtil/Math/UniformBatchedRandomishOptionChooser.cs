using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityUtil.Math;

public class UniformBatchedRandomishOptionChooserConfig
{
    /// <summary>
    /// Number of equally likely options that can be chosen. Must be 1 or greater.
    /// </summary>
    public int OptionCount { get; set; } = 1;

    /// <summary>
    /// Minimum number of runs (sequences of repeated options) allowed per batch. Must be 0 or greater and less than <see cref="MaxRunsPerBatch"/>.
    /// </summary>
    public int MinRunsPerBatch { get; set; }

    /// <summary>
    /// Maximum number of runs (sequences of repeated options) allowed per batch. Must be greater than or equal to <see cref="MinRunsPerBatch"/>.
    /// </summary>
    public int MaxRunsPerBatch { get; set; }

    /// <summary>
    /// Maximum number of times an option can be repeated in a run, i.e., the max "length" of a run.
    /// Minimum number of repeats per run is obviously 1, so <see cref="MaxRepeatsPerRun"/> must be greater than 1.
    /// </summary>
    public int MaxRepeatsPerRun { get; set; } = 2;
}

/// <summary>
/// <para>
/// Chooses between options in a "randomish" way, i.e., a way that <em>feels</em> random to humans over time but is not truly random.
/// Achieved by generating "batches" of options in advance with configurable numbers/lengths of runs.
/// All options are assumbed to be equally likely; they will have a "uniform" distribution over time.
/// </para>
/// <para>
/// This type is not thread safe. If randomish option indices must be accessed by multiple threads, you will need to implement your own synchronization mechanism.
/// </para>
/// </summary>
public class UniformBatchedRandomishOptionChooser : IRandomishOptionChooser
{
    /// <summary>
    /// All <see cref="OptionProbabilities"/> must sum to 1, within this tolerance.
    /// This accounts for probabilities that cannot be accurately represented with floating point numbers (e.g., 1/9).
    /// </summary>
    public const float ProbabilitySumTolerance = 0.000001f;

    private readonly IRandomNumberGenerator _randomNumberGenerator;
    private readonly UniformBatchedRandomishOptionChooserConfig _config;

    private IReadOnlyList<int>? _batch;
    private int _currBatchIndex;

    public UniformBatchedRandomishOptionChooser(IRandomNumberGenerator randomNumberGenerator, UniformBatchedRandomishOptionChooserConfig config)
    {
        _randomNumberGenerator = randomNumberGenerator;
        _config = config;

        if (_config.OptionCount < 1)
            throw new ArgumentException($"{nameof(_config.OptionCount)} must be 1 or greater", nameof(config));
        if (_config.MinRunsPerBatch < 0)
            throw new ArgumentException($"{nameof(_config.MinRunsPerBatch)} must be 0 or greater", nameof(config));
        if (_config.MaxRunsPerBatch < _config.MinRunsPerBatch || _config.MaxRunsPerBatch > _config.OptionCount)
            throw new ArgumentException($"{nameof(_config.MaxRunsPerBatch)} must be greater than or equal to {nameof(_config.MinRunsPerBatch)} and less than or equal to {nameof(_config.OptionCount)}", nameof(config));
        if (_config.MaxRepeatsPerRun <= 1)
            throw new ArgumentException($"{nameof(_config.MaxRepeatsPerRun)} must be greater than 1", nameof(config));

        BatchProviderDelegate = GetBatch;
    }

    /// <summary>
    /// The delegate that will provide the next batch of option indices. Defaults to <see cref="GetBatch"/>.
    /// <strong>This property should only be explicitly set in unit tests!</strong>
    /// </summary>
    internal Func<IReadOnlyList<int>> BatchProviderDelegate { get; set; }

    /// <summary>
    /// <see cref="GetBatch"/> will ensure that this index is not chosen at the start of a batch,
    /// to prevent accidentally "continuing" a run across batches.
    /// <strong>This property should only be explicitly set in unit tests!</strong>
    /// </summary>
    internal int LastOptionIndexOfPreviousBatch = -1;

    /// <inheritdoc/>
    public int GetOptionIndex()
    {
        if (_currBatchIndex == 0)
            _batch = BatchProviderDelegate();

        int nextOptionIndex = _batch![_currBatchIndex];

        _currBatchIndex = (++_currBatchIndex) % _batch.Count;

        return nextOptionIndex;
    }

    internal IReadOnlyList<int> GetBatch()
    {
        // Randomly pick unique indices to have runs in the batch, and randomly pick the lengths of those runs.
        // Assume the number of runs per batch will always be signifiantly less than the number of options.
        // Thus, to ensure uniqueness without allocating a separate collection of "available" indices (i.e., to avoid another O(n) of memory),
        // we just pick a random index, and then iterate up through indices (starting at the chosen one) until we find one not yet saved.
        // Time complexity is thus O(numRuns*numOptions) in the worst case, but should be O(numRuns) in the average case.
        Dictionary<int, int> extraOptionRepeats = new(_config.MaxRunsPerBatch);
        int batchCount = _config.OptionCount;
        int runCount = _randomNumberGenerator.Range(_config.MinRunsPerBatch, _config.MaxRunsPerBatch);
        for (int run = 0; run < runCount; ++run) {
            int extraRepeatsCount = _randomNumberGenerator.Range(1, _config.MaxRepeatsPerRun);    // E.g., max repeats of 2 means add at most 1 repeat (max is exclusive)
            int randOptionIndex = _randomNumberGenerator.Range(0, _config.OptionCount);
            while (extraOptionRepeats.ContainsKey(randOptionIndex))    // Will never infinite loop b/c constructor validates that # runs is <= # options
                randOptionIndex = (randOptionIndex + 1) % _config.OptionCount;
            extraOptionRepeats.Add(randOptionIndex, extraRepeatsCount);
            batchCount += extraRepeatsCount;
        }

        // Generate a random sequence of option indices with the runs determined above.
        // I tried doing this in a way that wouldn't require a separate collection of "available options";
        // unfortunately, it always required inserting into a List (an O(n) worst-case operation).
        // Doing this repeatedly is probably slower in the long run than just allocating one additional collection and pulling from it.
        int currBatchIndex = 0;
        int[] batch = new int[batchCount];
        int[] availableOptions = Enumerable.Range(0, _config.OptionCount).ToArray();
        for (int opt = 0; opt < _config.OptionCount; ++opt) {
            int randAvailableIndex = _randomNumberGenerator.Range(0, _config.OptionCount - opt);
            int randOpt = availableOptions[randAvailableIndex];
            if (opt == 0 && randOpt == LastOptionIndexOfPreviousBatch && _config.OptionCount > 1) { --opt; continue; }  // Make sure we don't accidentally "continue" last run of previous batch
            availableOptions[randAvailableIndex] = availableOptions[_config.OptionCount - opt - 1];

            int repeatCount = 1 + (extraOptionRepeats.TryGetValue(randOpt, out int extraRepeatCount) ? extraRepeatCount : 0);
            for (int x = 0; x < repeatCount; ++x)
                batch[currBatchIndex++] = randOpt;
        }

        LastOptionIndexOfPreviousBatch = batch[^1];

        return batch;
    }
}
