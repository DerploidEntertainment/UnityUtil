using NUnit.Framework;
using UnityEngine;
using UnityEngine.Triggers;

namespace UnityUtil.Test.EditMode.Triggers {
    public class AndTriggerTest
    {

        [Test]
        public void SetsConditionCorrectly_1Condition()
        {
            MockConditionalTrigger condition = getTrigger();
            AndTrigger andTrigger = getAndTrigger(condition);

            condition.State = false;
            Assert.IsFalse(andTrigger.IsConditionMet());

            condition.State = true;
            Assert.IsTrue(andTrigger.IsConditionMet());
        }

        [Test]
        public void SetsConditionCorrectly_2Conditions()
        {
            MockConditionalTrigger condition0 = getTrigger();
            MockConditionalTrigger condition1 = getTrigger();
            AndTrigger andTrigger = getAndTrigger(condition0, condition1);

            condition0.State = false;
            condition1.State = false;
            Assert.IsFalse(andTrigger.IsConditionMet());

            condition0.State = true;
            condition1.State = false;
            Assert.IsFalse(andTrigger.IsConditionMet());

            condition0.State = false;
            condition1.State = true;
            Assert.IsFalse(andTrigger.IsConditionMet());

            condition0.State = true;
            condition1.State = true;
            Assert.IsTrue(andTrigger.IsConditionMet());
        }

        [Test]
        public void CorrectlyRaises_NoEvents()
        {
            // Arrange
            MockConditionalTrigger condition0 = getTrigger(); condition0.State = false;
            MockConditionalTrigger condition1 = getTrigger(); condition1.State = false;
            AndTrigger andTrigger = getAndTrigger(condition0, condition1);
            andTrigger.RaiseBecameEvents = false;
            andTrigger.RaiseStillEvents = false;
            var orTriggerWrapper = new ConditionalTriggerWrapper<AndTrigger>(andTrigger);

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
            AndTrigger andTrigger = getAndTrigger(condition0, condition1);
            andTrigger.RaiseBecameEvents = true;
            andTrigger.RaiseStillEvents = false;
            var orTriggerWrapper = new ConditionalTriggerWrapper<AndTrigger>(andTrigger);

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
            orTriggerWrapper.AssertTriggerCounts(1, 0, 0, 0);

            condition0.StillTrue.Invoke();
            condition1.StillTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(1, 0, 0, 0);

            condition1.State = false;
            condition1.BecameFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(1, 1, 0, 0);

            condition1.State = true;
            condition1.BecameTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(2, 1, 0, 0);

            condition0.State = false;
            condition0.BecameFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(2, 2, 0, 0);

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
            AndTrigger andTrigger = getAndTrigger(condition0, condition1);
            andTrigger.RaiseBecameEvents = false;
            andTrigger.RaiseStillEvents = true;
            var orTriggerWrapper = new ConditionalTriggerWrapper<AndTrigger>(andTrigger);

            // Act/assert
            condition0.StillFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 0, 1);

            condition1.StillFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 0, 2);

            condition0.BecameTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 0, 3);

            condition0.StillTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 0, 4);

            condition0.State = false;
            condition0.BecameFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 0, 5);

            condition1.State = true;
            condition1.BecameTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 0, 6);

            condition1.StillTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 0, 7);

            condition0.State = true;
            condition0.BecameTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 0, 7);

            condition0.StillTrue.Invoke();
            condition1.StillTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 2, 7);

            condition1.State = false;
            condition1.BecameFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 2, 7);

            condition1.State = true;
            condition1.BecameTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 2, 7);

            condition0.State = false;
            condition0.BecameFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 2, 7);

            condition1.State = false;
            condition1.BecameFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 2, 8);
        }

        [Test]
        public void CorrectlyRaises_AllEvents()
        {
            // Arrange
            MockConditionalTrigger condition0 = getTrigger(); condition0.State = false;
            MockConditionalTrigger condition1 = getTrigger(); condition1.State = false;
            AndTrigger andTrigger = getAndTrigger(condition0, condition1);
            andTrigger.RaiseBecameEvents = true;
            andTrigger.RaiseStillEvents = true;
            var orTriggerWrapper = new ConditionalTriggerWrapper<AndTrigger>(andTrigger);

            // Act/assert
            condition0.StillFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 0, 1);

            condition1.StillFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 0, 2);

            condition0.BecameTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 0, 3);

            condition0.StillTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 0, 4);

            condition0.State = false;
            condition0.BecameFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 0, 5);

            condition1.State = true;
            condition1.BecameTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 0, 6);

            condition1.StillTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(0, 0, 0, 7);

            condition0.State = true;
            condition0.BecameTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(1, 0, 0, 7);

            condition0.StillTrue.Invoke();
            condition1.StillTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(1, 0, 2, 7);

            condition1.State = false;
            condition1.BecameFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(1, 1, 2, 7);

            condition1.State = true;
            condition1.BecameTrue.Invoke();
            orTriggerWrapper.AssertTriggerCounts(2, 1, 2, 7);

            condition0.State = false;
            condition0.BecameFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(2, 2, 2, 7);

            condition1.State = false;
            condition1.BecameFalse.Invoke();
            orTriggerWrapper.AssertTriggerCounts(2, 2, 2, 8);
        }

        private static MockConditionalTrigger getTrigger(bool state = false)
        {
            MockConditionalTrigger trigger = new GameObject().AddComponent<MockConditionalTrigger>();
            trigger.State = state;

            return trigger;
        }
        private static AndTrigger getAndTrigger(params ConditionalTrigger[] conditions)
        {
            AndTrigger trigger = new GameObject().AddComponent<AndTrigger>();
            trigger.TriggerWhenConditionsChanged = true;
            trigger.TriggerWhenConditionsMaintained = true;

            trigger.Conditions = conditions;
            trigger.ResetEventListeners();

            return trigger;
        }
    }
}
