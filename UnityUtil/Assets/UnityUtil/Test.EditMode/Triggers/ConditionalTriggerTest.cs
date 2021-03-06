﻿using NUnit.Framework;
using UnityEngine;
using UnityEngine.Triggers;

namespace UnityUtil.Test.EditMode.Triggers {

    internal class MockConditionalTrigger : ConditionalTrigger {
        public bool State = false;
        public override bool IsConditionMet() => State;
    }

    public class ConditionalTriggerTest {

        [Test]
        public void TriggersStateCorrectly() {
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
        public void TriggersStateDoesNothingIfTurnedOff() {
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

        private MockConditionalTrigger getConditionalTrigger() {
            MockConditionalTrigger trigger = new GameObject().AddComponent<MockConditionalTrigger>();

            return trigger;
        }

    }
}
