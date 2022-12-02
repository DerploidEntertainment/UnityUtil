using NUnit.Framework;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityUtil.Triggers;

namespace UnityUtil.Test.EditMode.Triggers
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated when added to test Game Objects")]
    internal class MockConditionalTrigger : ConditionalTrigger
    {
        public bool State;
        public override bool IsConditionMet() => State;
    }

    public class ConditionalTriggerTest : BaseEditModeTestFixture
    {

        [Test]
        public void TriggersStateCorrectly()
        {
            MockConditionalTrigger trigger = getConditionalTrigger();
            int falseTriggerCount = 0, trueTriggerCount = 0;
            trigger.StillFalse.AddListener(() => ++falseTriggerCount);
            trigger.StillTrue.AddListener(() => ++trueTriggerCount);

            trigger.State = false;
            trigger.TriggerState();
            Assert.That(falseTriggerCount, Is.EqualTo(1));
            Assert.That(trueTriggerCount, Is.EqualTo(0));

            trigger.State = true;
            trigger.TriggerState();
            Assert.That(falseTriggerCount, Is.EqualTo(1));
            Assert.That(trueTriggerCount, Is.EqualTo(1));
        }

        [Test]
        public void TriggersStateDoesNothingIfTurnedOff()
        {
            MockConditionalTrigger trigger = getConditionalTrigger();
            int falseTriggerCount = 0, trueTriggerCount = 0;
            trigger.StillFalse.AddListener(() => ++falseTriggerCount);
            trigger.StillTrue.AddListener(() => ++trueTriggerCount);

            // Raising "still" events is off
            trigger.RaiseStillEvents = false;
            trigger.State = false;
            trigger.TriggerState();
            Assert.That(falseTriggerCount, Is.EqualTo(0));
            Assert.That(trueTriggerCount, Is.EqualTo(0));

            trigger.State = true;
            trigger.TriggerState();
            Assert.That(falseTriggerCount, Is.EqualTo(0));
            Assert.That(trueTriggerCount, Is.EqualTo(0));

            // Raising "still" events is back on
            trigger.RaiseStillEvents = true;
            trigger.State = false;
            trigger.TriggerState();
            Assert.That(falseTriggerCount, Is.EqualTo(1));
            Assert.That(trueTriggerCount, Is.EqualTo(0));
        }

        private static MockConditionalTrigger getConditionalTrigger() => new GameObject().AddComponent<MockConditionalTrigger>();

    }
}
