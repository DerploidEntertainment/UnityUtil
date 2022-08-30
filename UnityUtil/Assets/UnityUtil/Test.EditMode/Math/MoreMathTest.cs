using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityUtil.Math;

namespace UnityUtil.Test.EditMode.Math
{
    public class MoreMathTest : BaseEditModeTestFixture
    {
        [Test]
        public void RandomWeightedIndex_Fails_NoWeights()
        {
            Assert.Throws<ArgumentException>(() =>
                MoreMath.RandomWeightedIndex(Array.Empty<float>(), getRandomNumberGenerator())
            );
        }

        [Test]
        [TestCase(new[] { 0f })]
        [TestCase(new[] { 1f, 1f })]
        [TestCase(new[] { 0.5f, 0f, 0.5f, 0.5f })]
        [TestCase(new[] { 0.25f, 0.85f })]
        [TestCase(new[] { 0.1f, 0.1f })]
        [TestCase(new[] { 0.2f, 0.5f })]
        public void RandomWeightedIndex_Fails_WeightsDontSumToOne(float[] indexWeights)
        {
            Debug.Log($"Index weights: {string.Join(',', indexWeights)}");
            Assert.Throws<InvalidOperationException>(() =>
                MoreMath.RandomWeightedIndex(indexWeights, getRandomNumberGenerator())
            );
        }

        [Test]
        [TestCase(new[] { -1f })]
        [TestCase(new[] { -0.5f, -0.5f })]
        [TestCase(new[] { -0.5f, 0.5f })]
        [TestCase(new[] { 0.5f, -0.25f, 0.75f })]
        public void RandomWeightedIndex_Fails_NegativeWeights(float[] indexWeights)
        {
            Debug.Log($"Index weights: {string.Join(',', indexWeights)}");
            Assert.Throws<InvalidOperationException>(() =>
                MoreMath.RandomWeightedIndex(indexWeights, getRandomNumberGenerator())
            );
        }

        [Test]
        [TestCase(new[] { 1.01f })]
        [TestCase(new[] { 200f })]
        [TestCase(new[] { -1f, 2f })]
        public void RandomWeightedIndex_Fails_WeightsOverOne(float[] indexWeights)
        {
            Debug.Log($"Index weights: {string.Join(',', indexWeights)}");
            Assert.Throws<InvalidOperationException>(() =>
                MoreMath.RandomWeightedIndex(indexWeights, getRandomNumberGenerator())
            );
        }

        private static IEnumerable<TestCaseData> yieldCorrectIndexTestCases()
        {
            yield return new(0.0d, new[] { 1f }, 0);
            yield return new(0.3d, new[] { 1f }, 0);
            yield return new(0.5d, new[] { 1f }, 0);
            yield return new(0.6d, new[] { 1f }, 0);
            yield return new(1.0d, new[] { 1f }, 0);

            yield return new(0.0d, new[] { 0.5f, 0.5f }, 0);
            yield return new(0.3d, new[] { 0.5f, 0.5f }, 0);
            yield return new(0.5d, new[] { 0.5f, 0.5f }, 1);
            yield return new(0.6d, new[] { 0.5f, 0.5f }, 1);
            yield return new(1.0d, new[] { 0.5f, 0.5f }, 1);

            yield return new(0.0d, new[] { 0.2f, 0.3f, 0.5f }, 0);
            yield return new(0.1d, new[] { 0.2f, 0.3f, 0.5f }, 0);
            yield return new(0.2d, new[] { 0.2f, 0.3f, 0.5f }, 1);
            yield return new(0.3d, new[] { 0.2f, 0.3f, 0.5f }, 1);
            yield return new(0.4d, new[] { 0.2f, 0.3f, 0.5f }, 1);
            yield return new(0.5d, new[] { 0.2f, 0.3f, 0.5f }, 2);
            yield return new(0.8d, new[] { 0.2f, 0.3f, 0.5f }, 2);
            yield return new(1.0d, new[] { 0.2f, 0.3f, 0.5f }, 2);
        }

        [Test]
        [TestCaseSource(nameof(yieldCorrectIndexTestCases))]
        public void RandomWeightedIndex_ReturnsCorrectIndex(double randomValue, float[] indexWeights, int expectedIndex)
        {
            Debug.Log($"Index weights: {string.Join(',', indexWeights)}");
            IRandomNumberGenerator randomNumberGenerator = Mock.Of<IRandomNumberGenerator>(x => x.NextDouble() == randomValue);
            int index = MoreMath.RandomWeightedIndex(indexWeights, randomNumberGenerator);
            Assert.That(index, Is.EqualTo(expectedIndex));
        }

        private static IRandomNumberGenerator getRandomNumberGenerator() => new TestRandomNumberGenerator(123456789);    // Hard-coded seed so tests are stable
    }
}
