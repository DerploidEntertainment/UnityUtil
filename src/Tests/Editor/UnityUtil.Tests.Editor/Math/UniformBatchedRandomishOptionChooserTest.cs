using Moq;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityUtil.Math;

namespace UnityUtil.Editor.Tests.Math;

public class UniformBatchedRandomishOptionChooserTest : BaseEditModeTestFixture
{
    #region Constructor

    [Test]
    public void Constructs_DefaultConfig() =>
        Assert.DoesNotThrow(() =>
            new UniformBatchedRandomishOptionChooser(
                getRandomNumberGenerator(),
                new UniformBatchedRandomishOptionChooserConfig()
            )
        );

    [Test]
    [TestCase(0)]
    [TestCase(-1)]
    public void CannotConstruct_NoOptions(int optionCount)
    {
        UniformBatchedRandomishOptionChooserConfig config = new() { OptionCount = optionCount };
        ArgumentException e = Assert.Throws<ArgumentException>(() =>
            new UniformBatchedRandomishOptionChooser(getRandomNumberGenerator(), config)
        );
        Assert.That(e.Message, Contains.Substring(nameof(config.OptionCount)));
    }

    [Test]
    public void CannotConstruct_MinRunsPerBatchTooLow()
    {
        UniformBatchedRandomishOptionChooserConfig config = new() { MinRunsPerBatch = -1 };
        ArgumentException e = Assert.Throws<ArgumentException>(() =>
            new UniformBatchedRandomishOptionChooser(getRandomNumberGenerator(), config)
        );
        Assert.That(e.Message, Contains.Substring(nameof(config.MinRunsPerBatch)));
    }

    [Test]
    public void Constructs_ZeroMinRunsPerBatch() =>
        Assert.DoesNotThrow(() =>
            new UniformBatchedRandomishOptionChooser(
                getRandomNumberGenerator(),
                new UniformBatchedRandomishOptionChooserConfig { MinRunsPerBatch = 0 }
            )
        );

    [Test]
    public void CannotConstruct_MaxRunsPerBatchTooLow()
    {
        UniformBatchedRandomishOptionChooserConfig config = new() { MaxRunsPerBatch = -1 };
        ArgumentException e = Assert.Throws<ArgumentException>(() =>
            new UniformBatchedRandomishOptionChooser(getRandomNumberGenerator(), config)
        );
        Assert.That(e.Message, Contains.Substring(nameof(config.MaxRunsPerBatch)));
    }

    [Test]
    public void CannotConstruct_MaxRunsPerBatchLessThanMinRuns()
    {
        UniformBatchedRandomishOptionChooserConfig config = new() {
            MinRunsPerBatch = 2,
            MaxRunsPerBatch = 1,
        };
        ArgumentException e = Assert.Throws<ArgumentException>(() =>
            new UniformBatchedRandomishOptionChooser(getRandomNumberGenerator(), config)
        );
        Assert.That(e.Message, Contains.Substring(nameof(config.MaxRunsPerBatch)));
    }

    [Test]
    public void CannotConstruct_MaxRunsPerBatchGreaterThanOptionCount()
    {
        var config = new UniformBatchedRandomishOptionChooserConfig {
            OptionCount = 2,
            MaxRunsPerBatch = 3,
        };
        ArgumentException e = Assert.Throws<ArgumentException>(() =>
            new UniformBatchedRandomishOptionChooser(getRandomNumberGenerator(), config)
        );
        Assert.That(e.Message, Contains.Substring(nameof(config.MaxRunsPerBatch)));
    }

    [Test]
    public void Constructs_MaxRunsPerBatchEqualsMinRuns() =>
        Assert.DoesNotThrow(() =>
            new UniformBatchedRandomishOptionChooser(
                getRandomNumberGenerator(),
                new UniformBatchedRandomishOptionChooserConfig {
                    OptionCount = 3,
                    MinRunsPerBatch = 2,
                    MaxRunsPerBatch = 2,
                }
            )
        );

    [Test]
    public void Constructs_MaxRunsPerBatchEqualsOptionCount() =>
        Assert.DoesNotThrow(() =>
            new UniformBatchedRandomishOptionChooser(
                getRandomNumberGenerator(),
                new UniformBatchedRandomishOptionChooserConfig {
                    OptionCount = 2,
                    MaxRunsPerBatch = 2,
                }
            )
        );

    [Test]
    public void Constructs_MaxRunsPerBatchEqualsMinRunsEqualsOptionCount() =>
        Assert.DoesNotThrow(() =>
            new UniformBatchedRandomishOptionChooser(
                getRandomNumberGenerator(),
                new UniformBatchedRandomishOptionChooserConfig {
                    OptionCount = 2,
                    MinRunsPerBatch = 2,
                    MaxRunsPerBatch = 2,
                }
            )
        );

    [Test]
    [TestCase(-1)]
    [TestCase(0)]
    [TestCase(1)]
    public void CannotConstruct_MaxRepeatsPerRunTooLow(int maxRepeatsPerRun)
    {
        var config = new UniformBatchedRandomishOptionChooserConfig { MaxRepeatsPerRun = maxRepeatsPerRun };
        ArgumentException e = Assert.Throws<ArgumentException>(() =>
            new UniformBatchedRandomishOptionChooser(getRandomNumberGenerator(), config)
        );
        Assert.That(e.Message, Contains.Substring(nameof(config.MaxRepeatsPerRun)));
    }

    #endregion

    #region GetBatch

    [Test]
    [TestCase(1, 0, 2)]
    [TestCase(1, 1, 2)]
    [TestCase(3, 0, 2)]
    [TestCase(3, 2, 2)]
    [TestCase(3, 0, 3)]
    [TestCase(3, 2, 3)]
    [TestCase(3, 0, 4)]
    [TestCase(3, 2, 4)]
    [TestCase(9, 2, 4)]
    public void GetBatch_HasExpectedCount(int optionCount, int runCount, int maxRepeatsPerRun)
    {
        UniformBatchedRandomishOptionChooserConfig config = new() {
            OptionCount = optionCount,
            MinRunsPerBatch = runCount,
            MaxRunsPerBatch = runCount,
            MaxRepeatsPerRun = maxRepeatsPerRun,
        };
        UniformBatchedRandomishOptionChooser uniformBatchedRandomishOptionChooser = getRandomishOptionChooser(config: config);

        IReadOnlyList<int> batch = uniformBatchedRandomishOptionChooser.GetBatch();

        int nonRepeatedOptionCount = optionCount - runCount;
        int minCount = nonRepeatedOptionCount + runCount * 2;
        int maxCount = nonRepeatedOptionCount + runCount * config.MaxRepeatsPerRun;
        Assert.That(batch.Count, Is.InRange(minCount, maxCount));
    }

    private static TestCaseData[] yieldBatchConfigTestCases() =>
        [
            new(1, 0, 0, 2),
            new(1, 0, 1, 2),
            new(1, 1, 1, 2),
            new(3, 0, 0, 2),
            new(3, 1, 2, 2),
            new(3, 3, 3, 2),
            new(9, 0, 3, 3),
        ];

    [Test]
    [TestCaseSource(nameof(yieldBatchConfigTestCases))]
    public void GetBatch_IncludesAllOptions(int optionCount, int minRunsPerBatch, int maxRunsPerBatch, int maxRepeatsPerRun)
    {
        UniformBatchedRandomishOptionChooser uniformBatchedRandomishOptionChooser = getRandomishOptionChooser(
            config: new() {
                OptionCount = optionCount,
                MinRunsPerBatch = minRunsPerBatch,
                MaxRunsPerBatch = maxRunsPerBatch,
                MaxRepeatsPerRun = maxRepeatsPerRun,
            }
        );

        IReadOnlyList<int> batch = uniformBatchedRandomishOptionChooser.GetBatch();

        for (int x = 0; x < optionCount; ++x)
            Assert.That(batch, Contains.Item(x));
    }

    [Test]
    [TestCaseSource(nameof(yieldBatchConfigTestCases))]
    public void GetBatch_RandomlyPermutesIndices(int optionCount, int minRunsPerBatch, int maxRunsPerBatch, int maxRepeatsPerRun)
    {
        // ARRANGE
        UniformBatchedRandomishOptionChooser uniformBatchedRandomishOptionChooser = getRandomishOptionChooser(
            config: new() {
                OptionCount = optionCount,
                MinRunsPerBatch = minRunsPerBatch,
                MaxRunsPerBatch = maxRunsPerBatch,
                MaxRepeatsPerRun = maxRepeatsPerRun,
            }
        );

        // ACT
        int[] batch1 = (int[])uniformBatchedRandomishOptionChooser.GetBatch();
        uniformBatchedRandomishOptionChooser.LastOptionIndexOfPreviousBatch = -1;
        int[] batch2 = (int[])uniformBatchedRandomishOptionChooser.GetBatch();

        // ASSERT
        // We can't just loop over all options and assert that the first indices in the two batches of ONE of those options are unequal.
        // Consider if the two batches looked like this:
        //      1 1 0 2
        //      1 0 0 2
        // The first indices of option 0 are unequal (2 and 1, resp.), so the test would pass,
        // but it shouldn't because the batch permutations are still the same, they just randomly assigned runs to different options.
        // So, remove repeats first, then we CAN simply compare the permutation indices.
        static int[] getBatchWithoutRepeats(int optionCount, int[] batch)
        {
            int permutationIndex = 0;
            int prevOption = -1;
            int[] permutation = new int[optionCount];
            for (int x = 0; x < batch.Length; ++x) {
                int option = batch[x];
                if (option == prevOption)   // This assumes runs have sequential repeats, which is technically asserted in a separate test
                    continue;
                prevOption = option;
                permutation[permutationIndex++] = option;
            }
            return permutation;
        }
        int[] batch1WithoutRepeats = getBatchWithoutRepeats(optionCount, batch1);
        int[] batch2WithoutRepeats = getBatchWithoutRepeats(optionCount, batch2);
        if (optionCount == 1) {
            Assert.That(batch1WithoutRepeats[0], Is.Zero);
            Assert.That(batch2WithoutRepeats[0], Is.Zero);
        }
        else
            CollectionAssert.AreNotEqual(batch1WithoutRepeats, batch2WithoutRepeats);
    }

    [Test]
    [TestCaseSource(nameof(yieldBatchConfigTestCases))]
    public void GetBatch_GeneratesSequentialRunRepeats(int optionCount, int minRunsPerBatch, int maxRunsPerBatch, int maxRepeatsPerRun)
    {
        UniformBatchedRandomishOptionChooser uniformBatchedRandomishOptionChooser = getRandomishOptionChooser(
            config: new() {
                OptionCount = optionCount,
                MinRunsPerBatch = minRunsPerBatch,
                MaxRunsPerBatch = maxRunsPerBatch,
                MaxRepeatsPerRun = maxRepeatsPerRun,
            }
        );

        int[] batch = (int[])uniformBatchedRandomishOptionChooser.GetBatch();

        BitArray optionsPresentBitmask = new(optionCount, defaultValue: false);
        optionsPresentBitmask[batch[0]] = true;
        int badOption = -1;
        for (int x = 1; x < optionCount; ++x) {
            int option = batch[x];
            if (option != batch[x - 1] && optionsPresentBitmask[option]) {
                badOption = option;
                break;
            }
            optionsPresentBitmask[option] = true;
        }
        Assert.That(badOption, Is.EqualTo(-1), $"Option {badOption} was repeated outside of a run.");
    }

    [Test]
    [TestCase(1, 0, 0)]
    [TestCase(1, 0, 1)]
    [TestCase(1, 1, 1)]
    [TestCase(3, 0, 0)]
    [TestCase(3, 1, 2)]
    [TestCase(3, 3, 3)]
    [TestCase(9, 0, 3)]
    public void GetBatch_GeneratesCorrectRunCount(int optionCount, int minRunsPerBatch, int maxRunsPerBatch)
    {
        UniformBatchedRandomishOptionChooser uniformBatchedRandomishOptionChooser = getRandomishOptionChooser(
            config: new() {
                OptionCount = optionCount,
                MinRunsPerBatch = minRunsPerBatch,
                MaxRunsPerBatch = maxRunsPerBatch,
            }
        );

        int[] batch = (int[])uniformBatchedRandomishOptionChooser.GetBatch();

        bool inRun = false;
        int runCount = 0;
        for (int x = 1; x < batch.Length; ++x) {
            bool match = batch[x] == batch[x - 1];
            if (!inRun && match)
                ++runCount;
            inRun = match;
        }
        Assert.That(runCount, Is.InRange(minRunsPerBatch, maxRunsPerBatch));
    }

    [Test]
    [TestCase(1, 0, 2)]
    [TestCase(1, 1, 2)]
    [TestCase(3, 0, 2)]
    [TestCase(3, 2, 2)]
    [TestCase(3, 0, 3)]
    [TestCase(3, 2, 3)]
    [TestCase(3, 0, 4)]
    [TestCase(3, 2, 4)]
    [TestCase(9, 2, 4)]
    public void GetBatch_RunsHaveCorrectLength(int optionCount, int runCount, int maxRepeatsPerRun)
    {
        UniformBatchedRandomishOptionChooser uniformBatchedRandomishOptionChooser = getRandomishOptionChooser(
            config: new() {
                OptionCount = optionCount,
                MinRunsPerBatch = runCount,
                MaxRunsPerBatch = runCount,
                MaxRepeatsPerRun = maxRepeatsPerRun,
            }
        );

        int[] batch = (int[])uniformBatchedRandomishOptionChooser.GetBatch();

        int[] optionRunLengths = batch.GroupBy(x => x).Select(g => g.Count()).ToArray();
        Assert.That(optionRunLengths, Is.All.InRange(1, maxRepeatsPerRun));
    }

    [Test]
    public void GetBatch_CanGenerateZeroRuns_IfSoConfigured()
    {
        int optionCount = 5;
        UniformBatchedRandomishOptionChooser uniformBatchedRandomishOptionChooser = getRandomishOptionChooser(
            config: new() {
                OptionCount = optionCount,
                MinRunsPerBatch = 0,
                MaxRunsPerBatch = 0,
            }
        );

        int[] batch = (int[])uniformBatchedRandomishOptionChooser.GetBatch();

        Assert.That(batch.Length, Is.EqualTo(optionCount));
        Assert.That(batch.Distinct().Count(), Is.EqualTo(optionCount));     // No repeats, so no runs
    }

    [Test]
    public void GetBatch_CanGenerateRunsForAllOptions_IfSoConfigured()
    {
        int optionCount = 3;
        UniformBatchedRandomishOptionChooserConfig config = new() {
            OptionCount = optionCount,
            MinRunsPerBatch = optionCount,
            MaxRunsPerBatch = optionCount,
            MaxRepeatsPerRun = 2,
        };
        UniformBatchedRandomishOptionChooser uniformBatchedRandomishOptionChooser = getRandomishOptionChooser(config: config);

        IReadOnlyList<int> batch = uniformBatchedRandomishOptionChooser.GetBatch();

        Assert.That(batch.Count, Is.EqualTo(config.MaxRepeatsPerRun * optionCount));
        for (int opt = 0; opt < optionCount; ++opt)
            Assert.That(batch.Count(x => x == opt), Is.EqualTo(config.MaxRepeatsPerRun));
    }

    [Test]
    [TestCase(3, 0, 2)]
    [TestCase(3, 2, 2)]
    [TestCase(3, 0, 3)]
    [TestCase(3, 2, 3)]
    [TestCase(3, 0, 4)]
    [TestCase(3, 2, 4)]
    [TestCase(9, 2, 4)]
    public void GetBatch_CannotContinueRunBetweenBatches(int optionCount, int runCount, int maxRepeatsPerRun)
    {
        // ARRANGE
        // Mocked random number generator will return the hard-coded test "last option of a previous batch run".
        // Every random choice of option index for the batch will return this "forbidden" option.
        const int LAST_OPTION = 0;
        const int NUM_ATTEMPTS = 20;
        int numAttempts = 0;
#pragma warning disable CA2201 // Do not raise reserved exception types
        Mock<IRandomNumberGenerator> mockedRandomNumberGenerator = getPrngWithOptionIndexRandomization(optionCount, runCount, () =>
            (++numAttempts == NUM_ATTEMPTS) ? throw new Exception() : LAST_OPTION);
#pragma warning restore CA2201 // Do not raise reserved exception types
        UniformBatchedRandomishOptionChooser uniformBatchedRandomishOptionChooser = getRandomishOptionChooser(
            mockedRandomNumberGenerator.Object,
            new() {
                OptionCount = optionCount,
                MinRunsPerBatch = runCount,
                MaxRunsPerBatch = runCount,
                MaxRepeatsPerRun = maxRepeatsPerRun,
            }
        );
        uniformBatchedRandomishOptionChooser.LastOptionIndexOfPreviousBatch = LAST_OPTION;

        // ACT
        Assert.Throws<Exception>(() =>
            uniformBatchedRandomishOptionChooser.GetBatch()
        );

        // ASSERT
        Assert.That(numAttempts, Is.EqualTo(NUM_ATTEMPTS));
    }

    [Test]
    [TestCase(1, 0, 2)]
    [TestCase(1, 1, 2)]
    [TestCase(3, 0, 2)]
    [TestCase(3, 2, 2)]
    [TestCase(3, 0, 3)]
    [TestCase(3, 2, 3)]
    [TestCase(3, 0, 4)]
    [TestCase(3, 2, 4)]
    [TestCase(9, 2, 4)]
    public void GetBatch_SubsequentBatchIncludesAllOptions(int optionCount, int runCount, int maxRepeatsPerRun)
    {
        UniformBatchedRandomishOptionChooser uniformBatchedRandomishOptionChooser = getRandomishOptionChooser(
            config: new() {
                OptionCount = optionCount,
                MinRunsPerBatch = runCount,
                MaxRunsPerBatch = runCount,
                MaxRepeatsPerRun = maxRepeatsPerRun,
            }
        );

        uniformBatchedRandomishOptionChooser.GetBatch();
        IReadOnlyList<int> batch2 = uniformBatchedRandomishOptionChooser.GetBatch();

        // Assert that a subsequent batch still includes all options,
        // even though last option of previous batch isn't repeated (to prevent "extended" runs, asserted in other tests)
        for (int x = 0; x < optionCount; ++x)
            Assert.That(batch2, Contains.Item(x));
    }

    [Test]
    [TestCase(1, 0, 2)]
    [TestCase(1, 1, 2)]
    [TestCase(1, 1, 3)]
    public void GetBatch_CanContinueRunBetweenBatches_OnlyOneOption(int optionCount, int runCount, int maxRepeatsPerRun)
    {
        Mock<IRandomNumberGenerator> mockedRandomNumberGenerator = getPrngWithOptionIndexRandomization(optionCount, runCount, () => 0);
        UniformBatchedRandomishOptionChooser uniformBatchedRandomishOptionChooser = getRandomishOptionChooser(
            mockedRandomNumberGenerator.Object,
            new() {
                OptionCount = optionCount,
                MinRunsPerBatch = runCount,
                MaxRunsPerBatch = runCount,
                MaxRepeatsPerRun = maxRepeatsPerRun,
            }
        );
        uniformBatchedRandomishOptionChooser.LastOptionIndexOfPreviousBatch = 0;

        IReadOnlyList<int> batch = uniformBatchedRandomishOptionChooser.GetBatch();
        Assert.That(batch.Count, runCount == 0 ? Is.EqualTo(1) : Is.InRange(2, runCount * maxRepeatsPerRun));
        Assert.That(batch, Is.All.EqualTo(0));
    }

    #endregion

    #region GetOptionIndex

    [Test]
    public void GetOptionIndex_BatchesIndices_OnlyWhenBatchRunsOut()
    {
        int[] testBatch = [1, 0, 2, 2];
        UniformBatchedRandomishOptionChooser uniformBatchedRandomishOptionChooser = getRandomishOptionChooser(
            config: new() { OptionCount = 3 }
        );
        int numBatchesProvided = 0;
        uniformBatchedRandomishOptionChooser.BatchProviderDelegate = () => { ++numBatchesProvided; return testBatch; };

        for (int x = 0; x < testBatch.Length; ++x) {
            uniformBatchedRandomishOptionChooser.GetOptionIndex();
            Assert.That(numBatchesProvided, Is.EqualTo(1));
        }

        for (int x = 0; x < testBatch.Length; ++x) {
            uniformBatchedRandomishOptionChooser.GetOptionIndex();
            Assert.That(numBatchesProvided, Is.EqualTo(2));
        }
    }

    [Test]
    public void GetOptionIndex_IteratesBatch()
    {
        int[] testBatch = [1, 0, 2, 2];
        UniformBatchedRandomishOptionChooser uniformBatchedRandomishOptionChooser = getRandomishOptionChooser(
            config: new() { OptionCount = 3 }
        );
        int optionIndex;
        uniformBatchedRandomishOptionChooser.BatchProviderDelegate = () => testBatch;

        for (int x = 0; x < testBatch.Length; ++x) {
            optionIndex = uniformBatchedRandomishOptionChooser.GetOptionIndex();
            Assert.That(optionIndex, Is.EqualTo(testBatch[x]));
        }
    }

    #endregion

    private static TestRandomNumberGenerator getRandomNumberGenerator() => new(123456789);    // Hard-coded seed so tests are stable

    private static UniformBatchedRandomishOptionChooser getRandomishOptionChooser(
        IRandomNumberGenerator? randomNumberGenerator = null,
        UniformBatchedRandomishOptionChooserConfig? config = null
    ) =>
        new(
            randomNumberGenerator ?? getRandomNumberGenerator(),
            config ?? new UniformBatchedRandomishOptionChooserConfig()
        );

    /// <summary>
    /// Mock an <see name="IRandomNumberGenerator"/> to return the following "randomish" numbers:
    /// <list type="number">
    /// <item>Actual random numbers while logic is picking number/length of runs</item>
    /// <item>Thereafter, values provided by <paramref name="optionIndexProvider"/></item>
    /// </list>
    /// </summary>
    private static Mock<IRandomNumberGenerator> getPrngWithOptionIndexRandomization(int optionCount, int runCount, Func<int> optionIndexProvider)
    {
        Mock<IRandomNumberGenerator> mockedRandomNumberGenerator = new();

        // All "other" range calls will just return PRNG-generated values
#pragma warning disable CA1859 // Use concrete types when possible for improved performance; justification: var must be interface type to use default interface methods
        IRandomNumberGenerator randomNumberGenerator = getRandomNumberGenerator();
#pragma warning restore CA1859 // Use concrete types when possible for improved performance
        mockedRandomNumberGenerator.Setup(x => x.Range(It.IsAny<int>(), It.IsAny<int>()))
            .Returns<int, int>((inclusiveMin, exclusiveMax) => randomNumberGenerator.Range(inclusiveMin, exclusiveMax));

        // Range(0, optionCount) will first be called several times to decide which options will have runs,
        // so just return actual PRNG-generated values for those calls.
        int numAttempts = 0;
        mockedRandomNumberGenerator.Setup(x => x.Range(0, optionCount)) // Can't use SetupSequence cause it doesn't support "do this for all following invokes"
            .Returns(() => (numAttempts++ < runCount) ? randomNumberGenerator.Range(0, optionCount) : optionIndexProvider());

        return mockedRandomNumberGenerator;
    }

}
