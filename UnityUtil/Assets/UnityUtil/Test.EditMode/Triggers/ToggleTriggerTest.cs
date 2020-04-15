using NUnit.Framework;
using UnityEngine;
using UnityEngine.Triggers;

namespace UnityUtil.Test.EditMode.Triggers {
    public class ToggleTriggerTest {

        [Test]
        public void CanToggle() {
            ToggleTrigger trigger = getToggleTrigger();

            trigger.TurnOn();
            Assert.IsTrue(trigger.IsConditionMet());

            trigger.TurnOff();
            Assert.IsFalse(trigger.IsConditionMet());

            trigger.TurnOn();
            Assert.IsTrue(trigger.IsConditionMet());
        }

        [Test]
        public void TogglingRaisesCorrectEvent() {
            ToggleTrigger trigger = getToggleTrigger();
            int numFalseTriggers = 0, numTrueTriggers = 0;
            trigger.BecameFalse.AddListener(() => ++numFalseTriggers);
            trigger.BecameTrue.AddListener(() => ++numTrueTriggers);

            trigger.TurnOn();
            Assert.That(numFalseTriggers, Is.EqualTo(0));
            Assert.That(numTrueTriggers, Is.EqualTo(1));

            trigger.TurnOff();
            Assert.That(numFalseTriggers, Is.EqualTo(1));
            Assert.That(numTrueTriggers, Is.EqualTo(1));

            trigger.TurnOn();
            Assert.That(numFalseTriggers, Is.EqualTo(1));
            Assert.That(numTrueTriggers, Is.EqualTo(2));
        }

        [Test]
        public void RepeatedToggleDoesNotRaiseEvent() {
            ToggleTrigger trigger = getToggleTrigger();
            int numFalseTriggers = 0, numTrueTriggers = 0;
            trigger.BecameFalse.AddListener(() => ++numFalseTriggers);
            trigger.BecameTrue.AddListener(() => ++numTrueTriggers);

            // Multiple toggles to true
            trigger.TurnOn();
            Assert.That(numFalseTriggers, Is.EqualTo(0));
            Assert.That(numTrueTriggers, Is.EqualTo(1));

            trigger.TurnOn();
            Assert.That(numFalseTriggers, Is.EqualTo(0));
            Assert.That(numTrueTriggers, Is.EqualTo(1));

            // Multiple toggles to false
            trigger.TurnOff();
            Assert.That(numFalseTriggers, Is.EqualTo(1));
            Assert.That(numTrueTriggers, Is.EqualTo(1));

            trigger.TurnOff();
            Assert.That(numFalseTriggers, Is.EqualTo(1));
            Assert.That(numTrueTriggers, Is.EqualTo(1));
        }

        [Test]
        public void RepeatedToggleRaisesStillEvent() {
            ToggleTrigger trigger = getToggleTrigger();
            int numFalseTriggers = 0, numTrueTriggers = 0;
            trigger.StillFalse.AddListener(() => ++numFalseTriggers);
            trigger.StillTrue.AddListener(() => ++numTrueTriggers);

            // Multiple toggles to true
            trigger.TurnOn();
            Assert.That(numFalseTriggers, Is.EqualTo(0));
            Assert.That(numTrueTriggers, Is.EqualTo(0));

            trigger.TurnOn();
            Assert.That(numFalseTriggers, Is.EqualTo(0));
            Assert.That(numTrueTriggers, Is.EqualTo(1));

            // Multiple toggles to false
            trigger.TurnOff();
            Assert.That(numFalseTriggers, Is.EqualTo(0));
            Assert.That(numTrueTriggers, Is.EqualTo(1));

            trigger.TurnOff();
            Assert.That(numFalseTriggers, Is.EqualTo(1));
            Assert.That(numTrueTriggers, Is.EqualTo(1));
        }

        private ToggleTrigger getToggleTrigger() => new GameObject().AddComponent<ToggleTrigger>();
    }
}
