using NUnit.Framework;
using UnityEngine;
using UnityUtil.Triggers;

namespace UnityUtil.Test.EditMode.Triggers
{

    public class OrTriggerTest : BaseEditModeTestFixture
    {

        [Test]
        public void SetsConditionCorrectly_1Condition()
        {
            MockConditionalTrigger condition = getTrigger();
            OrTrigger orTrigger = getOrTrigger(condition);

            condition.State = false;
            Assert.That(orTrigger.IsConditionMet(), Is.False);

            condition.State = true;
            Assert.That(orTrigger.IsConditionMet(), Is.True);
        }

        [Test]
        public void SetsConditionCorrectly_2Conditions()
        {
            MockConditionalTrigger condition0 = getTrigger();
            MockConditionalTrigger condition1 = getTrigger();
            OrTrigger orTrigger = getOrTrigger(condition0, condition1);

            condition0.State = false;
            condition1.State = false;
            Assert.That(orTrigger.IsConditionMet(), Is.False);

            condition0.State = true;
            condition1.State = false;
            Assert.That(orTrigger.IsConditionMet(), Is.True);

            condition0.State = false;
            condition1.State = true;
            Assert.That(orTrigger.IsConditionMet(), Is.True);

            condition0.State = true;
            condition1.State = true;
            Assert.That(orTrigger.IsConditionMet(), Is.True);
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
            var orTriggerWrapper = new TestConditionalTriggerWrapper<OrTrigger>(orTrigger);

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
            var orTriggerWrapper = new TestConditionalTriggerWrapper<OrTrigger>(orTrigger);

            // Act/assert
            condition0.StillFalse.Invoke();
            orTrigger.TriggerState();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 0, 0);

            condition1.StillFalse.Invoke();
            orTrigger.TriggerState();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 0, 0);

            condition0.State = true;
            condition0.BecameTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(1, 0, 0, 0);

            condition0.StillTrue.Invoke();
            orTrigger.TriggerState();
            orTriggerWrapper.AssertTriggerCounts(1, 0, 0, 0);

            condition0.State = false;
            condition0.BecameFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(1, 1, 0, 0);

            condition1.State = true;
            condition1.BecameTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(2, 1, 0, 0);

            condition1.StillTrue.Invoke();
            orTrigger.TriggerState();
            orTriggerWrapper.AssertTriggerCounts(2, 1, 0, 0);

            condition0.State = true;
            condition0.BecameTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(2, 1, 0, 0);

            condition0.StillTrue.Invoke();
            condition1.StillTrue.Invoke();
            orTrigger.TriggerState();
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
            var orTriggerWrapper = new TestConditionalTriggerWrapper<OrTrigger>(orTrigger);

            // Act/assert
            condition0.StillFalse.Invoke();
            orTrigger.TriggerState();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 0, 1);

            condition1.StillFalse.Invoke();
            orTrigger.TriggerState();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 0, 2);

            condition0.State = true;
            condition0.BecameTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 0, 2);

            condition0.StillTrue.Invoke();
            orTrigger.TriggerState();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 1, 2);

            condition0.State = false;
            condition0.BecameFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 1, 2);

            condition1.State = true;
            condition1.BecameTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 1, 2);

            condition1.StillTrue.Invoke();
            orTrigger.TriggerState();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 2, 2);

            condition0.State = true;
            condition0.BecameTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 2, 2);

            condition0.StillTrue.Invoke();
            condition1.StillTrue.Invoke();
            orTrigger.TriggerState();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 3, 2);

            condition1.State = false;
            condition1.BecameFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 3, 2);

            condition1.State = true;
            condition1.BecameTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 3, 2);

            condition0.State = false;
            condition0.BecameFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 3, 2);

            condition1.State = false;
            condition1.BecameFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 3, 2);
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
            var orTriggerWrapper = new TestConditionalTriggerWrapper<OrTrigger>(orTrigger);

            // Act/assert
            condition0.StillFalse.Invoke();
            orTrigger.TriggerState();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 0, 1);

            condition1.StillFalse.Invoke();
            orTrigger.TriggerState();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 0, 2);

            condition0.State = true;
            condition0.BecameTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(1, 0, 0, 2);

            condition0.StillTrue.Invoke();
            orTrigger.TriggerState();
            orTriggerWrapper.AssertTriggerCounts(1, 0, 1, 2);

            condition0.State = false;
            condition0.BecameFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(1, 1, 1, 2);

            condition1.State = true;
            condition1.BecameTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(2, 1, 1, 2);

            condition1.StillTrue.Invoke();
            orTrigger.TriggerState();
            orTriggerWrapper.AssertTriggerCounts(2, 1, 2, 2);

            condition0.State = true;
            condition0.BecameTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(2, 1, 2, 2);

            condition0.StillTrue.Invoke();
            condition1.StillTrue.Invoke();
            orTrigger.TriggerState();
            orTriggerWrapper.AssertTriggerCounts(2, 1, 3, 2);

            condition1.State = false;
            condition1.BecameFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(2, 1, 3, 2);

            condition1.State = true;
            condition1.BecameTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(2, 1, 3, 2);

            condition0.State = false;
            condition0.BecameFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(2, 1, 3, 2);

            condition1.State = false;
            condition1.BecameFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(2, 2, 3, 2);
        }

        private static MockConditionalTrigger getTrigger(bool state = false)
        {
            MockConditionalTrigger trigger = new GameObject().AddComponent<MockConditionalTrigger>();
            trigger.State = state;

            return trigger;
        }
        private static OrTrigger getOrTrigger(params ConditionalTrigger[] conditions)
        {
            OrTrigger trigger = new GameObject().AddComponent<OrTrigger>();
            trigger.RaiseBecameEvents = true;
            trigger.RaiseStillEvents = true;

            trigger.Conditions = conditions;
            trigger.ResetBecameEventListeners();

            return trigger;
        }

    }
}
