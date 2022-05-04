using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityUtil.Editor;
using UnityUtil.Math;

namespace UnityUtil.Test.EditMode.Math
{
    public class MaxRepeatsRandomishOptionChooserTest
    {
        #region RandomishOptionState constructor

        [Test]
        public void RandomishOptionState_CannotConstruct_NoProbabilities()
        {
            EditModeTestHelpers.ResetScene();

            Assert.Throws<ArgumentException>(() =>
                new MaxRepeatsRandomishOptionState(maxRepeats: 1, Array.Empty<float>())
            );
        }

        [Test]
        [TestCase(new[] { 0f })]
        [TestCase(new[] { 1.01f })]
        [TestCase(new[] { 200f })]
        [TestCase(new[] { 1f, 1f })]
        [TestCase(new[] { 0.5f, 0f, 0.5f, 0.5f })]
        [TestCase(new[] { 0.25f, 0.85f })]
        [TestCase(new[] { 0.1f, 0.1f })]
        [TestCase(new[] { 0.2f, 0.5f })]
        public void RandomishOptionState_CannotConstruct_ProbabilitiesDontSumToOne(float[] optionProbabilities)
        {
            EditModeTestHelpers.ResetScene();

            Debug.Log($"Option probabilities: {string.Join(',', optionProbabilities)}");
            Assert.Throws<InvalidOperationException>(() =>
                new MaxRepeatsRandomishOptionState(maxRepeats: 1, optionProbabilities)
            );
        }

        [Test]
        [TestCase(new[] { -1f })]
        [TestCase(new[] { -1f, 2f })]
        [TestCase(new[] { -0.5f, -0.5f })]
        [TestCase(new[] { -0.5f, 0.5f })]
        [TestCase(new[] { 0.5f, -0.25f, 0.75f })]
        public void RandomishOptionState_CannotConstruct_NegativeProbabilities(float[] optionProbabilities)
        {
            EditModeTestHelpers.ResetScene();

            Debug.Log($"Option probabilities: {string.Join(',', optionProbabilities)}");
            Assert.Throws<InvalidOperationException>(() =>
                new MaxRepeatsRandomishOptionState(maxRepeats: 1, optionProbabilities)
            );
        }

        [Test]
        [TestCase(0)]
        [TestCase(-1)]
        public void RandomishOptionState_CannotConstruct_RepeatFactorLessThanOne(int repeatFactor)
        {
            EditModeTestHelpers.ResetScene();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new MaxRepeatsRandomishOptionState(repeatFactor, new[] { 1f })
            );
        }

        #endregion

        #region RandomishOptionState UseOption

        [Test]
        [TestCase(-1, new[] { 1f })]
        [TestCase(1, new[] { 1f })]
        [TestCase(-1, new[] { 0.5f, 0.5f })]
        [TestCase(2, new[] { 0.5f, 0.5f })]
        public void RandomishOptionState_UseOption_IndexMustBeInRange(int occurenceIndex, float[] optionProbabilities)
        {
            EditModeTestHelpers.ResetScene();

            Debug.Log($"Option probabilities: {string.Join(',', optionProbabilities)}");
            MaxRepeatsRandomishOptionState state = new(maxRepeats: 1, new[] { 1f });
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                state.UseOption(occurenceIndex)
            );
        }

        [Test]
        public void RandomishOptionState_UseOption_IncrementsAndDecrementsCounts()
        {
            EditModeTestHelpers.ResetScene();

            MaxRepeatsRandomishOptionState state = new(maxRepeats: 2, new[] { 0.5f, 0.5f });

            Assert.That(state.OptionRepeats[0], Is.Zero);
            Assert.That(state.OptionRepeats[1], Is.Zero);

            state.UseOption(0);
            Assert.That(state.OptionRepeats[0], Is.EqualTo(1));
            Assert.That(state.OptionRepeats[1], Is.EqualTo(0));

            state.UseOption(0);
            Assert.That(state.OptionRepeats[0], Is.EqualTo(2));
            Assert.That(state.OptionRepeats[1], Is.EqualTo(0));

            state.UseOption(1);
            Assert.That(state.OptionRepeats[0], Is.EqualTo(1));
            Assert.That(state.OptionRepeats[1], Is.EqualTo(1));

            state.UseOption(1);
            Assert.That(state.OptionRepeats[0], Is.EqualTo(0));
            Assert.That(state.OptionRepeats[1], Is.EqualTo(2));
        }

        [Test]
        [TestCase(2)]
        [TestCase(3)]
        public void RandomishOptionState_UseOption_DoesNotIncrementCounts_AboveMaxRepeats(int maxRepeats)
        {
            EditModeTestHelpers.ResetScene();

            MaxRepeatsRandomishOptionState state = new(maxRepeats, new[] { 0.5f, 0.5f });

            Assert.That(state.OptionRepeats[0], Is.Zero);
            Assert.That(state.OptionRepeats[1], Is.Zero);

            for (int x = 0; x < maxRepeats; ++x)
                state.UseOption(0);
            Assert.That(state.OptionRepeats[0], Is.EqualTo(maxRepeats));
            Assert.That(state.OptionRepeats[1], Is.EqualTo(0));

            state.UseOption(1);
            state.UseOption(0);
            Assert.That(state.OptionRepeats[0], Is.EqualTo(maxRepeats - 1));
            Assert.That(state.OptionRepeats[1], Is.EqualTo(1));
        }

        [Test]
        [TestCase(2)]
        [TestCase(3)]
        public void RandomishOptionState_UseOption_CannotHaveMoreThanMaxRepeatsInRow(int maxRepeats)
        {
            EditModeTestHelpers.ResetScene();

            MaxRepeatsRandomishOptionState state = new(maxRepeats, new[] { 0.5f, 0.5f });

            Assert.That(state.OptionRepeats[0], Is.Zero);
            Assert.That(state.OptionRepeats[1], Is.Zero);

            for (int x = 0; x < maxRepeats; ++x)
                state.UseOption(0);
            Assert.That(state.OptionRepeats[0], Is.EqualTo(maxRepeats));

            Assert.Throws<InvalidOperationException>(() => state.UseOption(0));
        }

        [Test]
        [TestCase(2)]
        [TestCase(3)]
        public void RandomishOptionState_UseOption_RemovesAndAddsProbability(int maxRepeats)
        {
            EditModeTestHelpers.ResetScene();

            MaxRepeatsRandomishOptionState state = new(maxRepeats, new[] { 0.5f, 0.5f });

            Assert.That(state.TotalProbability, Is.EqualTo(1f));

            for (int x = 0; x < maxRepeats - 1; ++x)
                state.UseOption(0);
            Assert.That(state.TotalProbability, Is.EqualTo(1f));

            state.UseOption(0);
            Assert.That(state.TotalProbability, Is.EqualTo(1f - state.OptionProbabilities[0]));

            state.UseOption(1);
            Assert.That(state.TotalProbability, Is.EqualTo(1f));
        }

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        public void RandomishOptionState_UseOption_CanReturnSingleOptionForever(int maxRepeats)
        {
            EditModeTestHelpers.ResetScene();

            MaxRepeatsRandomishOptionState state = new(maxRepeats, new[] { 1f });

            for (int x = 0; x < 10; ++x) {
                state.UseOption(0);
                Assert.That(state.OptionRepeats[0], Is.EqualTo(1));
                Assert.That(state.TotalProbability, Is.EqualTo(1f));
            }
        }

        [Test]
        public void RandomishOptionState_UseOption_RemovesOptionsAfterMaxRepeats()
        {
            EditModeTestHelpers.ResetScene();

            MaxRepeatsRandomishOptionState state = new(maxRepeats: 2, new[] { 0.5f, 0.5f });

            Assert.That(state.TotalProbability, Is.EqualTo(1f));
            Assert.That(state.OptionRepeats[0], Is.EqualTo(0));

            state.UseOption(0);
            Assert.That(state.TotalProbability, Is.EqualTo(1f));
            Assert.That(state.OptionRepeats[0], Is.EqualTo(1));

            state.UseOption(0);
            Assert.That(state.TotalProbability, Is.EqualTo(0.5f));
            Assert.That(state.OptionRepeats[0], Is.EqualTo(2));
        }

        #endregion

        #region GetWeightedOptionIndex

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        public void GetWeightedOptionIndex_CanReturnSingleOptionForever(int maxRepeats)
        {
            EditModeTestHelpers.ResetScene();

            MaxRepeatsRandomishOptionState state = new(maxRepeats, new[] { 1f });
            MaxRepeatsRandomishOptionChooser randomishOptionChooser = getRandomishOptionChooser();

            for (int x = 0; x < 10; ++x) {
                int index = randomishOptionChooser.GetWeightedOptionIndex(state);
                Assert.That(index, Is.EqualTo(0));
            }
        }

        [Test]
        public void GetWeightedOptionIndex_UsesCorrectRandomRange()
        {
            EditModeTestHelpers.ResetScene();

            var randomNumberGenerator = new Mock<IRandomNumberGenerator>();
            MaxRepeatsRandomishOptionChooser randomishOptionChooser = getRandomishOptionChooser(randomNumberGenerator.Object);
            randomNumberGenerator.SetupSequence(x => x.Range(0f, It.IsAny<float>())).Returns(0);

            // Uniform distribution, 1 max repeat
            randomNumberGenerator.Invocations.Clear();
            MaxRepeatsRandomishOptionState state = new(maxRepeats: 1, new[] { 0.5f, 0.5f });

            randomishOptionChooser.GetWeightedOptionIndex(state);
            randomNumberGenerator.Verify(x => x.Range(0f, 1.0f), Times.Exactly(1));
            randomNumberGenerator.Verify(x => x.Range(0f, 0.5f), Times.Exactly(0));

            randomishOptionChooser.GetWeightedOptionIndex(state);
            randomNumberGenerator.Verify(x => x.Range(0f, 1.0f), Times.Exactly(1));
            randomNumberGenerator.Verify(x => x.Range(0f, 0.5f), Times.Exactly(1));

            // Uniform distribution, >1 max repeat
            randomNumberGenerator.Invocations.Clear();
            state = new(maxRepeats: 2, new[] { 0.5f, 0.5f });

            randomishOptionChooser.GetWeightedOptionIndex(state);
            randomNumberGenerator.Verify(x => x.Range(0f, 1.0f), Times.Exactly(1));
            randomNumberGenerator.Verify(x => x.Range(0f, 0.5f), Times.Exactly(0));

            randomishOptionChooser.GetWeightedOptionIndex(state);
            randomNumberGenerator.Verify(x => x.Range(0f, 1.0f), Times.Exactly(2));
            randomNumberGenerator.Verify(x => x.Range(0f, 0.5f), Times.Exactly(0));

            randomishOptionChooser.GetWeightedOptionIndex(state);
            randomNumberGenerator.Verify(x => x.Range(0f, 1.0f), Times.Exactly(2));
            randomNumberGenerator.Verify(x => x.Range(0f, 0.5f), Times.Exactly(1));

            // Non-uniform distribution, 1 max repeat
            randomNumberGenerator.Invocations.Clear();
            state = new(maxRepeats: 1, new[] { 0.75f, 0.25f });

            randomishOptionChooser.GetWeightedOptionIndex(state);
            randomNumberGenerator.Verify(x => x.Range(0f, 1.0f), Times.Exactly(1));
            randomNumberGenerator.Verify(x => x.Range(0f, 0.25f), Times.Exactly(0));

            randomishOptionChooser.GetWeightedOptionIndex(state);
            randomNumberGenerator.Verify(x => x.Range(0f, 1.0f), Times.Exactly(1));
            randomNumberGenerator.Verify(x => x.Range(0f, 0.25f), Times.Exactly(1));

            // Non-uniform distribution, >1 max repeat
            randomNumberGenerator.Invocations.Clear();
            state = new(maxRepeats: 2, new[] { 0.75f, 0.25f });

            randomishOptionChooser.GetWeightedOptionIndex(state);
            randomNumberGenerator.Verify(x => x.Range(0f, 1.0f), Times.Exactly(1));
            randomNumberGenerator.Verify(x => x.Range(0f, 0.25f), Times.Exactly(0));

            randomishOptionChooser.GetWeightedOptionIndex(state);
            randomNumberGenerator.Verify(x => x.Range(0f, 1.0f), Times.Exactly(2));
            randomNumberGenerator.Verify(x => x.Range(0f, 0.25f), Times.Exactly(0));

            randomishOptionChooser.GetWeightedOptionIndex(state);
            randomNumberGenerator.Verify(x => x.Range(0f, 1.0f), Times.Exactly(2));
            randomNumberGenerator.Verify(x => x.Range(0f, 0.25f), Times.Exactly(1));
        }

        private static IEnumerable<TestCaseData> yieldCorrectIndexTestCases()
        {
            float[] oneProbs = new[] { 1f };
            yield return new(0.0d, oneProbs, 0);
            yield return new(0.3d, oneProbs, 0);
            yield return new(0.5d, oneProbs, 0);
            yield return new(0.6d, oneProbs, 0);
            yield return new(1.0d, oneProbs, 0);

            float[] twoProbs = new[] { 0.5f, 0.5f };
            yield return new(0.0d, twoProbs, 0);
            yield return new(0.3d, twoProbs, 0);
            yield return new(0.5d, twoProbs, 1);
            yield return new(0.6d, twoProbs, 1);
            yield return new(1.0d, twoProbs, 1);

            float[] threeProbs = new[] { 0.2f, 0.3f, 0.5f };
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
        public void GetWeightedOptionIndex_ReturnsCorrectIndex(double randomValue, float[] optionProbabilities, int expectedIndex)
        {
            EditModeTestHelpers.ResetScene();

            Debug.Log($"Index weights: {string.Join(',', optionProbabilities)}");
            IRandomNumberGenerator randomNumberGenerator = Mock.Of<IRandomNumberGenerator>(x => x.Range(0f, It.IsAny<float>()) == randomValue);
            MaxRepeatsRandomishOptionState state = new(maxRepeats: 1, optionProbabilities);
            MaxRepeatsRandomishOptionChooser randomishOptionChooser = getRandomishOptionChooser(randomNumberGenerator);

            int index = randomishOptionChooser.GetWeightedOptionIndex(state);

            Assert.That(index, Is.EqualTo(expectedIndex));
        }

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        public void GetWeightedOptionIndex_CorrectlyRepeatsIndices_UniformDistro(int maxRepeats)
        {
            EditModeTestHelpers.ResetScene();

            MaxRepeatsRandomishOptionState state = new(maxRepeats, new[] { 0.25f, 0.25f, 0.25f, 0.25f });
            IRandomNumberGenerator randomNumberGenerator = Mock.Of<IRandomNumberGenerator>(x => x.Range(It.IsAny<float>(), It.IsAny<float>()) == 0f);
            MaxRepeatsRandomishOptionChooser randomishOptionChooser = getRandomishOptionChooser(randomNumberGenerator);

            List<int> chosenIndices = new();

            for (int x = 0; x < state.MaxRepeats + 2; ++x)
                chosenIndices.Add(randomishOptionChooser.GetWeightedOptionIndex(state));
            for (int x = 0; x < state.MaxRepeats; ++x)
                Assert.That(chosenIndices[x], Is.EqualTo(0));
            Assert.That(chosenIndices[state.MaxRepeats], Is.EqualTo(1));        // First option already repeated max times
            Assert.That(chosenIndices[state.MaxRepeats + 1], Is.EqualTo(0));    // First option available again
        }

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        public void GetWeightedOptionIndex_CorrectlyRepeatsIndices_NonUniformDistro(int maxRepeats)
        {
            EditModeTestHelpers.ResetScene();

            MaxRepeatsRandomishOptionState state = new(maxRepeats, new[] { 0.75f, 0.25f });
            IRandomNumberGenerator randomNumberGenerator = Mock.Of<IRandomNumberGenerator>(x => x.Range(It.IsAny<float>(), It.IsAny<float>()) == 0f);
            MaxRepeatsRandomishOptionChooser randomishOptionChooser = getRandomishOptionChooser(randomNumberGenerator);

            List<int> chosenIndices = new();

            for (int x = 0; x < state.MaxRepeats + 2; ++x)
                chosenIndices.Add(randomishOptionChooser.GetWeightedOptionIndex(state));
            for (int x = 0; x < state.MaxRepeats; ++x)
                Assert.That(chosenIndices[x], Is.EqualTo(0));
            Assert.That(chosenIndices[state.MaxRepeats], Is.EqualTo(1));        // First option already repeated max times
            Assert.That(chosenIndices[state.MaxRepeats + 1], Is.EqualTo(0));    // First option available again
        }

        [Test]
        [TestCase(1, new[] { 0.5f, 0.5f })]
        [TestCase(2, new[] { 0.5f, 0.5f })]
        [TestCase(5, new[] { 0.5f, 0.5f })]
        [TestCase(1, new[] { 0.7f, 0.2f, 0.1f })]
        [TestCase(2, new[] { 0.7f, 0.2f, 0.1f })]
        [TestCase(5, new[] { 0.7f, 0.2f, 0.1f })]
        public void GetWeightedOptionIndex_DoesNotRepeatOverMax(int maxRepeats, float[] optionProbabilities)
        {
            EditModeTestHelpers.ResetScene();

            // ARRANGE
            MaxRepeatsRandomishOptionState state = new(maxRepeats, optionProbabilities);
            MaxRepeatsRandomishOptionChooser randomishOptionChooser = getRandomishOptionChooser();
            List<int> chosenIndices = new();

            // ACT
            const int NUM_ITERATIONS = 100;
            for (int x = 0; x < NUM_ITERATIONS; ++x)
                chosenIndices.Add(randomishOptionChooser.GetWeightedOptionIndex(state));

            // ASSERT
            int currIndex = -1;
            int currRepeatCount = 0;
            for (int x = 0; x < NUM_ITERATIONS; ++x) {
                Assert.That(chosenIndices[x], Is.InRange(0, state.OptionCount - 1));
                if (chosenIndices[x] == currIndex)
                    Assert.That(++currRepeatCount, Is.LessThanOrEqualTo(state.MaxRepeats));
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
        public void GetWeightedOptionIndex_RepeatsSingleOptionForever(int maxRepeats)
        {
            EditModeTestHelpers.ResetScene();

            MaxRepeatsRandomishOptionState state = new(maxRepeats, new[] { 1f });
            MaxRepeatsRandomishOptionChooser randomishOptionChooser = getRandomishOptionChooser();
            List<int> chosenIndices = new();

            const int NUM_ITERATIONS = 100;
            for (int x = 0; x < NUM_ITERATIONS; ++x)
                chosenIndices.Add(randomishOptionChooser.GetWeightedOptionIndex(state));

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
        public void GetWeightedOptionIndex_CorrectlyDistributesOptions(int maxRepeats, float[] optionProbabilities)
        {
            EditModeTestHelpers.ResetScene();

            // ARRANGE
            MaxRepeatsRandomishOptionState state = new(maxRepeats, optionProbabilities);
            MaxRepeatsRandomishOptionChooser randomishOptionChooser = getRandomishOptionChooser();
            List<int> chosenIndices = new();

            // ACT
            const int NUM_ITERATIONS = 10_000;
            for (int x = 0; x < NUM_ITERATIONS; ++x)
                chosenIndices.Add(randomishOptionChooser.GetWeightedOptionIndex(state));

            // ASSERT
            // TODO: Assert that chosenIndices has correct distribution of option indices
            // using Chi-square goodness-of-fit test (see https://www.jmp.com/en_sg/statistics-knowledge-portal/chi-square-test/chi-square-goodness-of-fit-test.html)
        }

        #endregion

        private static IRandomNumberGenerator getRandomNumberGenerator() => new TestRandomNumberGenerator(123456789);    // Hard-coded seed so tests are stable
        private static MaxRepeatsRandomishOptionChooser getRandomishOptionChooser(IRandomNumberGenerator? randomNumberGenerator = null) =>
            new(randomNumberGenerator ?? getRandomNumberGenerator());

    }
}
