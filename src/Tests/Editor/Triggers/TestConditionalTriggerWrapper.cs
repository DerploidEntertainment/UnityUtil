using NUnit.Framework;
using UnityUtil.Triggers;

namespace UnityUtil.Editor.Tests.Triggers
{

    internal class TestConditionalTriggerWrapper<T> where T : ConditionalTrigger
    {
        public T Trigger { get; private set; }
        public TestConditionalTriggerWrapper(T orTrigger)
        {
            Trigger = orTrigger;
            Trigger.BecameTrue.AddListener(() => ++BecameTrueCount);
            Trigger.BecameFalse.AddListener(() => ++BecameFalseCount);
            Trigger.StillTrue.AddListener(() => ++StillTrueCount);
            Trigger.StillFalse.AddListener(() => ++StillFalseCount);
        }

        public int BecameTrueCount { get; private set; }
        public int BecameFalseCount { get; private set; }
        public int StillTrueCount { get; private set; }
        public int StillFalseCount { get; private set; }

        public void AssertTriggerCounts(int expectedBecameTrueCount, int expectedBecameFalseCount, int expectedStillTrueCount, int expectedStillFalseCount)
        {
            Assert.That(BecameTrueCount, Is.EqualTo(expectedBecameTrueCount));
            Assert.That(BecameFalseCount, Is.EqualTo(expectedBecameFalseCount));
            Assert.That(StillTrueCount, Is.EqualTo(expectedStillTrueCount));
            Assert.That(StillFalseCount, Is.EqualTo(expectedStillFalseCount));
        }
    }

}
