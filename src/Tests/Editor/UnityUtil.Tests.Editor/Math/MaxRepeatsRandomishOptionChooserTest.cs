using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityUtil.Math;

namespace UnityUtil.Editor.Tests.Math
{
    public class MaxRepeatsRandomishOptionChooserTest : BaseEditModeTestFixture
    {
#pragma warning disable CA1861 // Avoid constant arrays as arguments

        #region Constructor

        [Test]
        public void CannotConstruct_NoProbabilities() =>
            Assert.Throws<ArgumentException>(() =>
                new MaxRepeatsRandomishOptionChooser(
                    Mock.Of<IRandomNumberGenerator>(),
                    new MaxRepeatsRandomishOptionChooserConfig { MaxRepeats = 1, OptionProbabilities = [] }
                )
            );

        [Test]
        [TestCase(new[] { 0f })]
        [TestCase(new[] { 1.01f })]
        [TestCase(new[] { 200f })]
        [TestCase(new[] { 1f, 1f })]
        [TestCase(new[] { 0.5f, 0f, 0.5f, 0.5f })]
        [TestCase(new[] { 0.25f, 0.85f })]
        [TestCase(new[] { 0.1f, 0.1f })]
        [TestCase(new[] { 0.2f, 0.5f })]
        public void CannotConstruct_ProbabilitiesDontSumToOne(float[] optionProbabilities)
        {
            Debug.Log($"Option probabilities: {string.Join(',', optionProbabilities)}");
            Assert.Throws<InvalidOperationException>(() =>
                new MaxRepeatsRandomishOptionChooser(
                    Mock.Of<IRandomNumberGenerator>(),
                    new MaxRepeatsRandomishOptionChooserConfig { MaxRepeats = 1, OptionProbabilities = optionProbabilities }
                )
            );
        }

        [Test]
        [TestCase(new[] { -1f })]
        [TestCase(new[] { -1f, 2f })]
        [TestCase(new[] { -0.5f, -0.5f })]
        [TestCase(new[] { -0.5f, 0.5f })]
        [TestCase(new[] { 0.5f, -0.25f, 0.75f })]
        public void CannotConstruct_NegativeProbabilities(float[] optionProbabilities)
        {
            Debug.Log($"Option probabilities: {string.Join(',', optionProbabilities)}");
            Assert.Throws<InvalidOperationException>(() =>
                new MaxRepeatsRandomishOptionChooser(
                    Mock.Of<IRandomNumberGenerator>(),
                    new MaxRepeatsRandomishOptionChooserConfig { MaxRepeats = 1, OptionProbabilities = optionProbabilities }
                )
            );
        }

        [Test]
        [TestCase(0)]
        [TestCase(-1)]
        public void CannotConstruct_MaxRepeatsLessThanOne(int maxRepeats) =>
            Assert.Throws<ArgumentException>(() =>
                new MaxRepeatsRandomishOptionChooser(
                    Mock.Of<IRandomNumberGenerator>(),
                    new MaxRepeatsRandomishOptionChooserConfig { MaxRepeats = maxRepeats, OptionProbabilities = [1f] }
                )
            );

        #endregion

        #region UseOption

        [Test]
        [TestCase(-1, new[] { 1f })]
        [TestCase(1, new[] { 1f })]
        [TestCase(-1, new[] { 0.5f, 0.5f })]
        [TestCase(2, new[] { 0.5f, 0.5f })]
        public void UseOption_IndexMustBeInRange(int occurenceIndex, float[] optionProbabilities)
        {
            Debug.Log($"Option probabilities: {string.Join(',', optionProbabilities)}");
            MaxRepeatsRandomishOptionChooser maxRepeatsRandomishOptionChooser = getRandomishOptionChooser(
                config: new MaxRepeatsRandomishOptionChooserConfig { MaxRepeats = 1, OptionProbabilities = [1f] }
            );

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                maxRepeatsRandomishOptionChooser.UseOption(occurenceIndex)
            );
        }

        [Test]
        public void UseOption_IncrementsAndDecrementsCounts()
        {
            MaxRepeatsRandomishOptionChooser maxRepeatsRandomishOptionChooser = getRandomishOptionChooser(
                config: new MaxRepeatsRandomishOptionChooserConfig { MaxRepeats = 2, OptionProbabilities = [0.5f, 0.5f] }
            );

            Assert.That(maxRepeatsRandomishOptionChooser.OptionRepeats[0], Is.Zero);
            Assert.That(maxRepeatsRandomishOptionChooser.OptionRepeats[1], Is.Zero);

            maxRepeatsRandomishOptionChooser.UseOption(0);
            Assert.That(maxRepeatsRandomishOptionChooser.OptionRepeats[0], Is.EqualTo(1));
            Assert.That(maxRepeatsRandomishOptionChooser.OptionRepeats[1], Is.EqualTo(0));

            maxRepeatsRandomishOptionChooser.UseOption(0);
            Assert.That(maxRepeatsRandomishOptionChooser.OptionRepeats[0], Is.EqualTo(2));
            Assert.That(maxRepeatsRandomishOptionChooser.OptionRepeats[1], Is.EqualTo(0));

            maxRepeatsRandomishOptionChooser.UseOption(1);
            Assert.That(maxRepeatsRandomishOptionChooser.OptionRepeats[0], Is.EqualTo(1));
            Assert.That(maxRepeatsRandomishOptionChooser.OptionRepeats[1], Is.EqualTo(1));

            maxRepeatsRandomishOptionChooser.UseOption(1);
            Assert.That(maxRepeatsRandomishOptionChooser.OptionRepeats[0], Is.EqualTo(0));
            Assert.That(maxRepeatsRandomishOptionChooser.OptionRepeats[1], Is.EqualTo(2));
        }

        [Test]
        [TestCase(2)]
        [TestCase(3)]
        public void UseOption_DoesNotIncrementCounts_AboveMaxRepeats(int maxRepeats)
        {
            MaxRepeatsRandomishOptionChooser maxRepeatsRandomishOptionChooser = getRandomishOptionChooser(
                config: new MaxRepeatsRandomishOptionChooserConfig { MaxRepeats = maxRepeats, OptionProbabilities = [0.5f, 0.5f] }
            );

            Assert.That(maxRepeatsRandomishOptionChooser.OptionRepeats[0], Is.Zero);
            Assert.That(maxRepeatsRandomishOptionChooser.OptionRepeats[1], Is.Zero);

            for (int x = 0; x < maxRepeats; ++x)
                maxRepeatsRandomishOptionChooser.UseOption(0);
            Assert.That(maxRepeatsRandomishOptionChooser.OptionRepeats[0], Is.EqualTo(maxRepeats));
            Assert.That(maxRepeatsRandomishOptionChooser.OptionRepeats[1], Is.EqualTo(0));

            maxRepeatsRandomishOptionChooser.UseOption(1);
            maxRepeatsRandomishOptionChooser.UseOption(0);
            Assert.That(maxRepeatsRandomishOptionChooser.OptionRepeats[0], Is.EqualTo(maxRepeats - 1));
            Assert.That(maxRepeatsRandomishOptionChooser.OptionRepeats[1], Is.EqualTo(1));
        }

        [Test]
        [TestCase(2)]
        [TestCase(3)]
        public void UseOption_CannotHaveMoreThanMaxRepeatsInRow(int maxRepeats)
        {
            MaxRepeatsRandomishOptionChooser maxRepeatsRandomishOptionChooser = getRandomishOptionChooser(
                config: new MaxRepeatsRandomishOptionChooserConfig { MaxRepeats = maxRepeats, OptionProbabilities = [0.5f, 0.5f] }
            );

            Assert.That(maxRepeatsRandomishOptionChooser.OptionRepeats[0], Is.Zero);
            Assert.That(maxRepeatsRandomishOptionChooser.OptionRepeats[1], Is.Zero);

            for (int x = 0; x < maxRepeats; ++x)
                maxRepeatsRandomishOptionChooser.UseOption(0);
            Assert.That(maxRepeatsRandomishOptionChooser.OptionRepeats[0], Is.EqualTo(maxRepeats));

            Assert.Throws<InvalidOperationException>(() => maxRepeatsRandomishOptionChooser.UseOption(0));
        }

        [Test]
        [TestCase(2)]
        [TestCase(3)]
        public void UseOption_RemovesAndAddsProbability(int maxRepeats)
        {
            MaxRepeatsRandomishOptionChooserConfig config = new() { MaxRepeats = maxRepeats, OptionProbabilities = [0.5f, 0.5f] };
            MaxRepeatsRandomishOptionChooser maxRepeatsRandomishOptionChooser = getRandomishOptionChooser(
                config: new MaxRepeatsRandomishOptionChooserConfig { MaxRepeats = maxRepeats, OptionProbabilities = [0.5f, 0.5f] }
            );

            Assert.That(maxRepeatsRandomishOptionChooser.TotalProbability, Is.EqualTo(1f));

            for (int x = 0; x < maxRepeats - 1; ++x)
                maxRepeatsRandomishOptionChooser.UseOption(0);
            Assert.That(maxRepeatsRandomishOptionChooser.TotalProbability, Is.EqualTo(1f));

            maxRepeatsRandomishOptionChooser.UseOption(0);
            Assert.That(maxRepeatsRandomishOptionChooser.TotalProbability, Is.EqualTo(1f - config.OptionProbabilities[0]));

            maxRepeatsRandomishOptionChooser.UseOption(1);
            Assert.That(maxRepeatsRandomishOptionChooser.TotalProbability, Is.EqualTo(1f));
        }

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        public void UseOption_CanReturnSingleOptionForever(int maxRepeats)
        {
            MaxRepeatsRandomishOptionChooser maxRepeatsRandomishOptionChooser = getRandomishOptionChooser(
                config: new MaxRepeatsRandomishOptionChooserConfig { MaxRepeats = maxRepeats, OptionProbabilities = [1f] }
            );

            for (int x = 0; x < 10; ++x) {
                maxRepeatsRandomishOptionChooser.UseOption(0);
                Assert.That(maxRepeatsRandomishOptionChooser.OptionRepeats[0], Is.EqualTo(1));
                Assert.That(maxRepeatsRandomishOptionChooser.TotalProbability, Is.EqualTo(1f));
            }
        }

        [Test]
        public void UseOption_RemovesOptionsAfterMaxRepeats()
        {
            MaxRepeatsRandomishOptionChooser maxRepeatsRandomishOptionChooser = getRandomishOptionChooser(
                config: new MaxRepeatsRandomishOptionChooserConfig { MaxRepeats = 2, OptionProbabilities = [0.5f, 0.5f] }
            );

            Assert.That(maxRepeatsRandomishOptionChooser.TotalProbability, Is.EqualTo(1f));
            Assert.That(maxRepeatsRandomishOptionChooser.OptionRepeats[0], Is.EqualTo(0));

            maxRepeatsRandomishOptionChooser.UseOption(0);
            Assert.That(maxRepeatsRandomishOptionChooser.TotalProbability, Is.EqualTo(1f));
            Assert.That(maxRepeatsRandomishOptionChooser.OptionRepeats[0], Is.EqualTo(1));

            maxRepeatsRandomishOptionChooser.UseOption(0);
            Assert.That(maxRepeatsRandomishOptionChooser.TotalProbability, Is.EqualTo(0.5f));
            Assert.That(maxRepeatsRandomishOptionChooser.OptionRepeats[0], Is.EqualTo(2));
        }

        #endregion

        #region GetOptionIndex

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        public void GetOptionIndex_CanReturnSingleOptionForever(int maxRepeats)
        {
            MaxRepeatsRandomishOptionChooser maxRepeatsRandomishOptionChooser = getRandomishOptionChooser(
                config: new MaxRepeatsRandomishOptionChooserConfig { MaxRepeats = maxRepeats, OptionProbabilities = [1f] }
            );

            MaxRepeatsRandomishOptionChooser randomishOptionChooser = getRandomishOptionChooser();

            for (int x = 0; x < 10; ++x) {
                int index = maxRepeatsRandomishOptionChooser.GetOptionIndex();
                Assert.That(index, Is.EqualTo(0));
            }
        }

        [Test]
        public void GetOptionIndex_UsesCorrectRandomRange()
        {
            MaxRepeatsRandomishOptionChooserConfig config;
            MaxRepeatsRandomishOptionChooser randomishOptionChooser;
            var randomNumberGenerator = new Mock<IRandomNumberGenerator>();
            randomNumberGenerator.SetupSequence(x => x.Range(0f, It.IsAny<float>())).Returns(0);

            // Uniform distribution, 1 max repeat
            randomNumberGenerator.Invocations.Clear();
            config = new() { MaxRepeats = 1, OptionProbabilities = [0.5f, 0.5f] };
            randomishOptionChooser = getRandomishOptionChooser(randomNumberGenerator.Object, config);

            randomishOptionChooser.GetOptionIndex();
            randomNumberGenerator.Verify(x => x.Range(0f, 1.0f), Times.Exactly(1));
            randomNumberGenerator.Verify(x => x.Range(0f, 0.5f), Times.Exactly(0));

            randomishOptionChooser.GetOptionIndex();
            randomNumberGenerator.Verify(x => x.Range(0f, 1.0f), Times.Exactly(1));
            randomNumberGenerator.Verify(x => x.Range(0f, 0.5f), Times.Exactly(1));

            // Uniform distribution, >1 max repeat
            randomNumberGenerator.Invocations.Clear();
            config = new() { MaxRepeats = 2, OptionProbabilities = [0.5f, 0.5f] };
            randomishOptionChooser = getRandomishOptionChooser(randomNumberGenerator.Object, config);

            randomishOptionChooser.GetOptionIndex();
            randomNumberGenerator.Verify(x => x.Range(0f, 1.0f), Times.Exactly(1));
            randomNumberGenerator.Verify(x => x.Range(0f, 0.5f), Times.Exactly(0));

            randomishOptionChooser.GetOptionIndex();
            randomNumberGenerator.Verify(x => x.Range(0f, 1.0f), Times.Exactly(2));
            randomNumberGenerator.Verify(x => x.Range(0f, 0.5f), Times.Exactly(0));

            randomishOptionChooser.GetOptionIndex();
            randomNumberGenerator.Verify(x => x.Range(0f, 1.0f), Times.Exactly(2));
            randomNumberGenerator.Verify(x => x.Range(0f, 0.5f), Times.Exactly(1));

            // Non-uniform distribution, 1 max repeat
            randomNumberGenerator.Invocations.Clear();
            config = new() { MaxRepeats = 1, OptionProbabilities = [0.75f, 0.25f] };
            randomishOptionChooser = getRandomishOptionChooser(randomNumberGenerator.Object, config);

            randomishOptionChooser.GetOptionIndex();
            randomNumberGenerator.Verify(x => x.Range(0f, 1.0f), Times.Exactly(1));
            randomNumberGenerator.Verify(x => x.Range(0f, 0.25f), Times.Exactly(0));

            randomishOptionChooser.GetOptionIndex();
            randomNumberGenerator.Verify(x => x.Range(0f, 1.0f), Times.Exactly(1));
            randomNumberGenerator.Verify(x => x.Range(0f, 0.25f), Times.Exactly(1));

            // Non-uniform distribution, >1 max repeat
            randomNumberGenerator.Invocations.Clear();
            config = new() { MaxRepeats = 2, OptionProbabilities = [0.75f, 0.25f] };
            randomishOptionChooser = getRandomishOptionChooser(randomNumberGenerator.Object, config);

            randomishOptionChooser.GetOptionIndex();
            randomNumberGenerator.Verify(x => x.Range(0f, 1.0f), Times.Exactly(1));
            randomNumberGenerator.Verify(x => x.Range(0f, 0.25f), Times.Exactly(0));

            randomishOptionChooser.GetOptionIndex();
            randomNumberGenerator.Verify(x => x.Range(0f, 1.0f), Times.Exactly(2));
            randomNumberGenerator.Verify(x => x.Range(0f, 0.25f), Times.Exactly(0));

            randomishOptionChooser.GetOptionIndex();
            randomNumberGenerator.Verify(x => x.Range(0f, 1.0f), Times.Exactly(2));
            randomNumberGenerator.Verify(x => x.Range(0f, 0.25f), Times.Exactly(1));
        }

        private static IEnumerable<TestCaseData> yieldCorrectIndexTestCases()
        {
            float[] oneProbs = [1f];
            yield return new(0.0d, oneProbs, 0);
            yield return new(0.3d, oneProbs, 0);
            yield return new(0.5d, oneProbs, 0);
            yield return new(0.6d, oneProbs, 0);
            yield return new(1.0d, oneProbs, 0);

            float[] twoProbs = [0.5f, 0.5f];
            yield return new(0.0d, twoProbs, 0);
            yield return new(0.3d, twoProbs, 0);
            yield return new(0.5d, twoProbs, 1);
            yield return new(0.6d, twoProbs, 1);
            yield return new(1.0d, twoProbs, 1);

            float[] threeProbs = [0.2f, 0.3f, 0.5f];
            yield return new(0.0d, threeProbs, 0);
            yield return new(0.1d, threeProbs, 0);
            yield return new(0.2d, threeProbs, 1);
            yield return new(0.3d, threeProbs, 1);
            yield return new(0.4d, threeProbs, 1);
            yield return new(0.5d, threeProbs, 2);
            yield return new(0.8d, threeProbs, 2);
            yield return new(1.0d, threeProbs, 2);
        }

        [Test]
        [TestCaseSource(nameof(yieldCorrectIndexTestCases))]
        public void GetOptionIndex_ReturnsCorrectIndex(double randomValue, float[] optionProbabilities, int expectedIndex)
        {
            Debug.Log($"Index weights: {string.Join(',', optionProbabilities)}");
            IRandomNumberGenerator randomNumberGenerator = Mock.Of<IRandomNumberGenerator>(x => x.Range(0f, It.IsAny<float>()) == randomValue);
            MaxRepeatsRandomishOptionChooserConfig config = new() { MaxRepeats = 1, OptionProbabilities = optionProbabilities };
            MaxRepeatsRandomishOptionChooser randomishOptionChooser = getRandomishOptionChooser(randomNumberGenerator, config);

            int index = randomishOptionChooser.GetOptionIndex();

            Assert.That(index, Is.EqualTo(expectedIndex));
        }

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        public void GetOptionIndex_CorrectlyRepeatsIndices_UniformDistro(int maxRepeats)
        {
            MaxRepeatsRandomishOptionChooserConfig config = new() { MaxRepeats = maxRepeats, OptionProbabilities = [0.25f, 0.25f, 0.25f, 0.25f] };
            IRandomNumberGenerator randomNumberGenerator = Mock.Of<IRandomNumberGenerator>(x => x.Range(It.IsAny<float>(), It.IsAny<float>()) == 0f);
            MaxRepeatsRandomishOptionChooser randomishOptionChooser = getRandomishOptionChooser(randomNumberGenerator, config);

            List<int> chosenIndices = [];

            for (int x = 0; x < config.MaxRepeats + 2; ++x)
                chosenIndices.Add(randomishOptionChooser.GetOptionIndex());
            for (int x = 0; x < config.MaxRepeats; ++x)
                Assert.That(chosenIndices[x], Is.EqualTo(0));
            Assert.That(chosenIndices[config.MaxRepeats], Is.EqualTo(1));        // First option already repeated max times
            Assert.That(chosenIndices[config.MaxRepeats + 1], Is.EqualTo(0));    // First option available again
        }

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        public void GetOptionIndex_CorrectlyRepeatsIndices_NonUniformDistro(int maxRepeats)
        {
            MaxRepeatsRandomishOptionChooserConfig config = new() { MaxRepeats = maxRepeats, OptionProbabilities = [0.75f, 0.25f] };
            IRandomNumberGenerator randomNumberGenerator = Mock.Of<IRandomNumberGenerator>(x => x.Range(It.IsAny<float>(), It.IsAny<float>()) == 0f);
            MaxRepeatsRandomishOptionChooser randomishOptionChooser = getRandomishOptionChooser(randomNumberGenerator, config);

            List<int> chosenIndices = [];

            for (int x = 0; x < config.MaxRepeats + 2; ++x)
                chosenIndices.Add(randomishOptionChooser.GetOptionIndex());
            for (int x = 0; x < config.MaxRepeats; ++x)
                Assert.That(chosenIndices[x], Is.EqualTo(0));
            Assert.That(chosenIndices[config.MaxRepeats], Is.EqualTo(1));        // First option already repeated max times
            Assert.That(chosenIndices[config.MaxRepeats + 1], Is.EqualTo(0));    // First option available again
        }

        [Test]
        [TestCase(1, new[] { 0.5f, 0.5f })]
        [TestCase(2, new[] { 0.5f, 0.5f })]
        [TestCase(5, new[] { 0.5f, 0.5f })]
        [TestCase(1, new[] { 0.7f, 0.2f, 0.1f })]
        [TestCase(2, new[] { 0.7f, 0.2f, 0.1f })]
        [TestCase(5, new[] { 0.7f, 0.2f, 0.1f })]
        public void GetOptionIndex_DoesNotRepeatOverMax(int maxRepeats, float[] optionProbabilities)
        {
            // ARRANGE
            MaxRepeatsRandomishOptionChooserConfig config = new() { MaxRepeats = maxRepeats, OptionProbabilities = optionProbabilities };
            MaxRepeatsRandomishOptionChooser randomishOptionChooser = getRandomishOptionChooser(config: config);
            List<int> chosenIndices = [];

            // ACT
            const int NUM_ITERATIONS = 100;
            for (int x = 0; x < NUM_ITERATIONS; ++x)
                chosenIndices.Add(randomishOptionChooser.GetOptionIndex());

            // ASSERT
            int currIndex = -1;
            int currRepeatCount = 0;
            for (int x = 0; x < NUM_ITERATIONS; ++x) {
                Assert.That(chosenIndices[x], Is.InRange(0, config.OptionProbabilities.Count - 1));
                if (chosenIndices[x] == currIndex)
                    Assert.That(++currRepeatCount, Is.LessThanOrEqualTo(config.MaxRepeats));
                else {
                    currIndex = chosenIndices[x];
                    currRepeatCount = 1;
                }

            }
        }

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        public void GetOptionIndex_RepeatsSingleOptionForever(int maxRepeats)
        {
            MaxRepeatsRandomishOptionChooserConfig config = new() { MaxRepeats = maxRepeats, OptionProbabilities = [1f] };
            MaxRepeatsRandomishOptionChooser randomishOptionChooser = getRandomishOptionChooser(config: config);
            List<int> chosenIndices = [];

            const int NUM_ITERATIONS = 100;
            for (int x = 0; x < NUM_ITERATIONS; ++x)
                chosenIndices.Add(randomishOptionChooser.GetOptionIndex());

            CollectionAssert.AreEqual(Enumerable.Repeat(0, NUM_ITERATIONS), chosenIndices);
        }

        [Test, Ignore("Haven't implemented a Chi-square goodness-of-fit test yet...")]
        [TestCase(1, new[] { 1f })]
        [TestCase(2, new[] { 1f })]
        [TestCase(5, new[] { 1f })]
        [TestCase(1, new[] { 0.5f, 0.5f })]
        [TestCase(2, new[] { 0.5f, 0.5f })]
        [TestCase(5, new[] { 0.5f, 0.5f })]
        [TestCase(1, new[] { 0.7f, 0.2f, 0.1f })]
        [TestCase(2, new[] { 0.7f, 0.2f, 0.1f })]
        [TestCase(5, new[] { 0.7f, 0.2f, 0.1f })]
        public void GetOptionIndex_CorrectlyDistributesOptions(int maxRepeats, float[] optionProbabilities)
        {
            // ARRANGE
            MaxRepeatsRandomishOptionChooserConfig config = new() { MaxRepeats = maxRepeats, OptionProbabilities = optionProbabilities };
            MaxRepeatsRandomishOptionChooser randomishOptionChooser = getRandomishOptionChooser(config: config);
            List<int> chosenIndices = [];

            // ACT
            const int NUM_ITERATIONS = 10_000;
            for (int x = 0; x < NUM_ITERATIONS; ++x)
                chosenIndices.Add(randomishOptionChooser.GetOptionIndex());

            // ASSERT
            // TODO: Assert that chosenIndices has correct distribution of option indices
            // using Chi-square goodness-of-fit test (see https://www.jmp.com/en_sg/statistics-knowledge-portal/chi-square-test/chi-square-goodness-of-fit-test.html)
        }

        #endregion

#pragma warning restore CA1861 // Avoid constant arrays as arguments

        private static TestRandomNumberGenerator getRandomNumberGenerator() => new(123456789);    // Hard-coded seed so tests are stable
        private static MaxRepeatsRandomishOptionChooser getRandomishOptionChooser(
            IRandomNumberGenerator? randomNumberGenerator = null,
            MaxRepeatsRandomishOptionChooserConfig? config = null
        ) =>
            new(
                randomNumberGenerator ?? getRandomNumberGenerator(),
                config ?? new MaxRepeatsRandomishOptionChooserConfig()
            );

    }
}
