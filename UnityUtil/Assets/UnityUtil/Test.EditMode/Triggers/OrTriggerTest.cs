using NUnit.Framework;
using UnityEngine;
using UnityEngine.Triggers;

namespace UnityUtil.Test.EditMode.Triggers {

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

        public int BecameTrueCount { get; private set; } = 0;
        public int BecameFalseCount { get; private set; } = 0;
        public int StillTrueCount { get; private set; } = 0;
        public int StillFalseCount { get; private set; } = 0;

        public void AssertTriggerCounts(int expectedBecameTrue, int expectedBecameFalse, int expectedStillTrue, int expectedStillFalse)
        {
            Assert.That(BecameTrueCount, Is.EqualTo(expectedBecameTrue));
            Assert.That(BecameFalseCount, Is.EqualTo(expectedBecameFalse));
            Assert.That(StillTrueCount, Is.EqualTo(expectedStillTrue));
            Assert.That(StillFalseCount, Is.EqualTo(expectedStillFalse));
        }
    }

    public class OrTriggerTest
    {

        [Test]
        public void SetsConditionCorrectly_1Condition()
        {
            MockConditionalTrigger condition = getTrigger();
            OrTrigger orTrigger = getOrTrigger(condition);

            condition.State = false;
            Assert.IsFalse(orTrigger.IsConditionMet());

            condition.State = true;
            Assert.IsTrue(orTrigger.IsConditionMet());
        }

        [Test]
        public void SetsConditionCorrectly_2Conditions() {
            MockConditionalTrigger condition0 = getTrigger();
            MockConditionalTrigger condition1 = getTrigger();
            OrTrigger orTrigger = getOrTrigger(condition0, condition1);

            condition0.State = false;
            condition1.State = false;
            Assert.IsFalse(orTrigger.IsConditionMet());

            condition0.State = true;
            condition1.State = false;
            Assert.IsTrue(orTrigger.IsConditionMet());

            condition0.State = false;
            condition1.State = true;
            Assert.IsTrue(orTrigger.IsConditionMet());

            condition0.State = true;
            condition1.State = true;
            Assert.IsTrue(orTrigger.IsConditionMet());
        }

        [Test]
        public void CorrectlyRaises_NoEvents()
        {
            // Arrange
            MockConditionalTrigger condition0 = getTrigger(); condition0.State = false;
            MockConditionalTrigger condition1 = getTrigger(); condition1.State = false;
            OrTrigger orTrigger = getOrTrigger(condition0, condition1);
            orTrigger.RaiseBecameEvents = false;
            orTrigger.RaiseStillEvents = false;
            var orTriggerWrapper = new ConditionalTriggerWrapper<OrTrigger>(orTrigger);

            // Act/assert
            condition0.StillFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 0, 0);

            condition1.StillFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 0, 0);

            condition0.BecameTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 0, 0);

            condition0.StillTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 0, 0);

            condition0.State = false;
            condition0.BecameFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 0, 0);

            condition1.State = true;
            condition1.BecameTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 0, 0);

            condition1.StillTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 0, 0);

            condition0.State = true;
            condition0.BecameTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 0, 0);

            condition0.StillTrue.Invoke();
            condition1.StillTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 0, 0);

            condition1.State = false;
            condition1.BecameFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 0, 0);

            condition1.State = true;
            condition1.BecameTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 0, 0);

            condition0.State = false;
            condition0.BecameFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 0, 0);

            condition1.State = false;
            condition1.BecameFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 0, 0);
        }

        [Test]
        public void CorrectlyRaises_BecameEvents()
        {
            // Arrange
            MockConditionalTrigger condition0 = getTrigger(); condition0.State = false;
            MockConditionalTrigger condition1 = getTrigger(); condition1.State = false;
            OrTrigger orTrigger = getOrTrigger(condition0, condition1);
            orTrigger.RaiseBecameEvents = true;
            orTrigger.RaiseStillEvents = false;
            var orTriggerWrapper = new ConditionalTriggerWrapper<OrTrigger>(orTrigger);

            // Act/assert
            condition0.StillFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 0, 0);

            condition1.StillFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 0, 0);

            condition0.BecameTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(1, 0, 0, 0);

            condition0.StillTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(1, 0, 0, 0);

            condition0.State = false;
            condition0.BecameFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(1, 1, 0, 0);

            condition1.State = true;
            condition1.BecameTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(2, 1, 0, 0);

            condition1.StillTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(2, 1, 0, 0);

            condition0.State = true;
            condition0.BecameTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(2, 1, 0, 0);

            condition0.StillTrue.Invoke();
            condition1.StillTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(2, 1, 0, 0);

            condition1.State = false;
            condition1.BecameFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(2, 1, 0, 0);

            condition1.State = true;
            condition1.BecameTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(2, 1, 0, 0);

            condition0.State = false;
            condition0.BecameFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(2, 1, 0, 0);

            condition1.State = false;
            condition1.BecameFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(2, 2, 0, 0);
        }

        [Test]
        public void CorrectlyRaises_StillEvents()
        {
            // Arrange
            MockConditionalTrigger condition0 = getTrigger(); condition0.State = false;
            MockConditionalTrigger condition1 = getTrigger(); condition1.State = false;
            OrTrigger orTrigger = getOrTrigger(condition0, condition1);
            orTrigger.RaiseBecameEvents = false;
            orTrigger.RaiseStillEvents = true;
            var orTriggerWrapper = new ConditionalTriggerWrapper<OrTrigger>(orTrigger);

            // Act/assert
            condition0.StillFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 0, 1);

            condition1.StillFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 0, 2);

            condition0.BecameTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 0, 2);

            condition0.StillTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 1, 2);

            condition0.State = false;
            condition0.BecameFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 1, 2);

            condition1.State = true;
            condition1.BecameTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 1, 2);

            condition1.StillTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 2, 2);

            condition0.State = true;
            condition0.BecameTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 3, 2);

            condition0.StillTrue.Invoke();
            condition1.StillTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 5, 2);

            condition1.State = false;
            condition1.BecameFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 6, 2);

            condition1.State = true;
            condition1.BecameTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 7, 2);

            condition0.State = false;
            condition0.BecameFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 8, 2);

            condition1.State = false;
            condition1.BecameFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 8, 2);
        }

        [Test]
        public void CorrectlyRaises_AllEvents()
        {
            // Arrange
            MockConditionalTrigger condition0 = getTrigger(); condition0.State = false;
            MockConditionalTrigger condition1 = getTrigger(); condition1.State = false;
            OrTrigger orTrigger = getOrTrigger(condition0, condition1);
            orTrigger.RaiseBecameEvents = true;
            orTrigger.RaiseStillEvents = true;
            var orTriggerWrapper = new ConditionalTriggerWrapper<OrTrigger>(orTrigger);

            // Act/assert
            condition0.StillFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 0, 1);

            condition1.StillFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 0, 2);

            condition0.BecameTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(1, 0, 0, 2);

            condition0.StillTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(1, 0, 1, 2);

            condition0.State = false;
            condition0.BecameFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(1, 1, 1, 2);

            condition1.State = true;
            condition1.BecameTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(2, 1, 1, 2);

            condition1.StillTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(2, 1, 2, 2);

            condition0.State = true;
            condition0.BecameTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(2, 1, 3, 2);

            condition0.StillTrue.Invoke();
            condition1.StillTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(2, 1, 5, 2);

            condition1.State = false;
            condition1.BecameFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(2, 1, 6, 2);

            condition1.State = true;
            condition1.BecameTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(2, 1, 7, 2);

            condition0.State = false;
            condition0.BecameFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(2, 1, 8, 2);

            condition1.State = false;
            condition1.BecameFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(2, 2, 8, 2);
        }

        private MockConditionalTrigger getTrigger(bool state = false)
        {
            MockConditionalTrigger trigger = new GameObject().AddComponent<MockConditionalTrigger>();
            trigger.State = state;

            return trigger;
        }
        private OrTrigger getOrTrigger(params ConditionalTrigger[] conditions) {
            OrTrigger trigger = new GameObject().AddComponent<OrTrigger>();
            trigger.TriggerWhenConditionsChanged = true;
            trigger.TriggerWhenConditionsMaintained = true;

            trigger.Conditions = conditions;
            trigger.ResetEventListeners();

            return trigger;
        }

    }
}
