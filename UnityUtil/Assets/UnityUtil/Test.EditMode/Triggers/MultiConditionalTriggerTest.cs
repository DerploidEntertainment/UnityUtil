using NUnit.Framework;
using System;
using UnityEngine;
using UnityEngine.Triggers;

namespace UnityUtil.Test.EditMode.Triggers {
    public class MultiConditionalTriggerTest
    {

        private class MockMultiConditionalTrigger : MultiConditionalTrigger {
            public override bool IsConditionMet() => throw new NotImplementedException();
            protected override void ConditionBecameFalseListener(ConditionalTrigger condition) => BecameFalse.Invoke();
            protected override void ConditionBecameTrueListener(ConditionalTrigger condition) => BecameTrue.Invoke();
            protected override void ConditionStillFalseListener(ConditionalTrigger condition) => StillFalse.Invoke();
            protected override void ConditionStillTrueListener(ConditionalTrigger condition) => StillTrue.Invoke();
        }

        [Test]
        public void CanResetEventListeners_NullConditions()
        {
            MultiConditionalTrigger trigger = getMultiTrigger();
            trigger.Conditions = null;

            Assert.DoesNotThrow(trigger.ResetEventListeners);
        }

        [Test]
        public void CanResetEventListeners_NullConditionElements()
        {
            MockConditionalTrigger condition = getTrigger();
            MockConditionalTrigger nullCondition = null;
            MultiConditionalTrigger trigger = getMultiTrigger(conditions: new[] { condition, nullCondition });

            Assert.DoesNotThrow(trigger.ResetEventListeners);
        }

        [Test]
        public void DoesNotListenFor_Changed_IfNotRequested() {
            MockConditionalTrigger condition0 = getTrigger();
            MockConditionalTrigger condition1 = getTrigger();
            MultiConditionalTrigger trigger = getMultiTrigger(triggerWhenConditionsChanged: false, conditions: new[] { condition0, condition1 });
            int numTrueTriggered = 0, numFalseTriggered = 0;
            trigger.BecameTrue.AddListener(() => ++numTrueTriggered);
            trigger.BecameFalse.AddListener(() => ++numFalseTriggered);

            condition0.BecameTrue.Invoke();
            Assert.That(numTrueTriggered, Is.EqualTo(0));
            Assert.That(numFalseTriggered, Is.EqualTo(0));

            condition0.BecameFalse.Invoke();
            Assert.That(numTrueTriggered, Is.EqualTo(0));
            Assert.That(numFalseTriggered, Is.EqualTo(0));

            condition1.BecameTrue.Invoke();
            Assert.That(numTrueTriggered, Is.EqualTo(0));
            Assert.That(numFalseTriggered, Is.EqualTo(0));

            condition1.BecameFalse.Invoke();
            Assert.That(numTrueTriggered, Is.EqualTo(0));
            Assert.That(numFalseTriggered, Is.EqualTo(0));
        }

        [Test]
        public void DoesNotListenFor_Maintained_IfNotRequested() {
            MockConditionalTrigger condition0 = getTrigger();
            MockConditionalTrigger condition1 = getTrigger();
            MultiConditionalTrigger trigger = getMultiTrigger(triggerWhenConditionsMaintained: false, conditions: new[] { condition0, condition1 });
            int numTrueTriggered = 0, numFalseTriggered = 0;
            trigger.StillTrue.AddListener(() => ++numTrueTriggered);
            trigger.StillFalse.AddListener(() => ++numFalseTriggered);

            condition0.StillTrue.Invoke();
            Assert.That(numTrueTriggered, Is.EqualTo(0));
            Assert.That(numFalseTriggered, Is.EqualTo(0));

            condition0.StillFalse.Invoke();
            Assert.That(numTrueTriggered, Is.EqualTo(0));
            Assert.That(numFalseTriggered, Is.EqualTo(0));

            condition1.StillTrue.Invoke();
            Assert.That(numTrueTriggered, Is.EqualTo(0));
            Assert.That(numFalseTriggered, Is.EqualTo(0));

            condition1.StillFalse.Invoke();
            Assert.That(numTrueTriggered, Is.EqualTo(0));
            Assert.That(numFalseTriggered, Is.EqualTo(0));
        }

        [Test]
        public void ListensFor_Changed_IfRequested() {
            MockConditionalTrigger condition0 = getTrigger();
            MockConditionalTrigger condition1 = getTrigger();
            MultiConditionalTrigger trigger = getMultiTrigger(triggerWhenConditionsChanged: true, conditions: new[] { condition0, condition1 });
            int numTrueTriggered = 0, numFalseTriggered = 0;
            trigger.BecameTrue.AddListener(() => ++numTrueTriggered);
            trigger.BecameFalse.AddListener(() => ++numFalseTriggered);

            condition0.BecameTrue.Invoke();
            Assert.That(numTrueTriggered, Is.EqualTo(1));
            Assert.That(numFalseTriggered, Is.EqualTo(0));

            condition0.BecameFalse.Invoke();
            Assert.That(numTrueTriggered, Is.EqualTo(1));
            Assert.That(numFalseTriggered, Is.EqualTo(1));

            condition1.BecameTrue.Invoke();
            Assert.That(numTrueTriggered, Is.EqualTo(2));
            Assert.That(numFalseTriggered, Is.EqualTo(1));

            condition1.BecameFalse.Invoke();
            Assert.That(numTrueTriggered, Is.EqualTo(2));
            Assert.That(numFalseTriggered, Is.EqualTo(2));
        }

        [Test]
        public void ListensFor_Maintained_IfRequested() {
            MockConditionalTrigger condition0 = getTrigger();
            MockConditionalTrigger condition1 = getTrigger();
            MultiConditionalTrigger trigger = getMultiTrigger(triggerWhenConditionsMaintained: true, conditions: new[] { condition0, condition1 });
            int numTrueTriggered = 0, numFalseTriggered = 0;
            trigger.StillTrue.AddListener(() => ++numTrueTriggered);
            trigger.StillFalse.AddListener(() => ++numFalseTriggered);

            condition0.StillTrue.Invoke();
            Assert.That(numTrueTriggered, Is.EqualTo(1));
            Assert.That(numFalseTriggered, Is.EqualTo(0));

            condition0.StillFalse.Invoke();
            Assert.That(numTrueTriggered, Is.EqualTo(1));
            Assert.That(numFalseTriggered, Is.EqualTo(1));

            condition1.StillTrue.Invoke();
            Assert.That(numTrueTriggered, Is.EqualTo(2));
            Assert.That(numFalseTriggered, Is.EqualTo(1));

            condition1.StillFalse.Invoke();
            Assert.That(numTrueTriggered, Is.EqualTo(2));
            Assert.That(numFalseTriggered, Is.EqualTo(2));
        }

        private static MockConditionalTrigger getTrigger() => new GameObject().AddComponent<MockConditionalTrigger>();
        private static MockMultiConditionalTrigger getMultiTrigger(
            bool triggerWhenConditionsChanged = true,
            bool triggerWhenConditionsMaintained = false,
            ConditionalTrigger[]? conditions = null
        ) {
            MockMultiConditionalTrigger trigger = new GameObject().AddComponent<MockMultiConditionalTrigger>();
            trigger.Conditions = conditions ?? Array.Empty<ConditionalTrigger>();
            trigger.TriggerWhenConditionsChanged = triggerWhenConditionsChanged;
            trigger.TriggerWhenConditionsMaintained = triggerWhenConditionsMaintained;
            trigger.ResetEventListeners();

            return trigger;
        }

    }
}
