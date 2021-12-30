using NUnit.Framework;
using UnityEngine.Triggers;

namespace UnityUtil.Test.EditMode.Triggers
{

    internal class ConditionalTriggerWrapper<T> where T : ConditionalTrigger
    {
        public T Trigger { get; private set; }
        public ConditionalTriggerWrapper(T orTrigger)
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

        public void AssertTriggerCounts(int expectedBecameTrue, int expectedBecameFalse, int expectedStillTrue, int expectedStillFalse)
        {
            Assert.That(BecameTrueCount, Is.EqualTo(expectedBecameTrue));
            Assert.That(BecameFalseCount, Is.EqualTo(expectedBecameFalse));
            Assert.That(StillTrueCount, Is.EqualTo(expectedStillTrue));
            Assert.That(StillFalseCount, Is.EqualTo(expectedStillFalse));
        }
    }

}
