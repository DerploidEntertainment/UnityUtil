using NUnit.Framework;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Triggers;

namespace UnityUtil.Test.EditMode {

    public class SequenceTriggerTest {

        [Test(TestOf = typeof(SequenceTrigger))]
        public void CanStep() {
            SequenceTrigger trigger = getTriggerObject(2);
            int origStep = trigger.CurrentStep;

            trigger.Step();

            Assert.That(trigger.CurrentStep, Is.EqualTo(origStep + 1));
        }

        [Test(TestOf = typeof(SequenceTrigger))]
        public void CanStepForwardMultiple() {
            SequenceTrigger trigger = getTriggerObject(10);
            int origStep = trigger.CurrentStep;

            trigger.Step(1);
            Assert.That(trigger.CurrentStep, Is.EqualTo(origStep + 1));

            trigger.CurrentStep = origStep;
            trigger.Step(2);
            Assert.That(trigger.CurrentStep, Is.EqualTo(origStep + 2));
        }

        [Test(TestOf = typeof(SequenceTrigger))]
        public void CanStepBackwardMultiple() {
            SequenceTrigger trigger = getTriggerObject(10);
            int origStep = 5;
            trigger.CurrentStep = origStep;

            trigger.Step(-1);
            Assert.That(trigger.CurrentStep, Is.EqualTo(origStep - 1));

            trigger.CurrentStep = origStep;
            trigger.Step(-2);
            Assert.That(trigger.CurrentStep, Is.EqualTo(origStep - 2));
        }

        [Test(TestOf = typeof(SequenceTrigger))]
        public void StepForwardCanClamp() {
            int numSteps = 2;
            SequenceTrigger trigger = getTriggerObject(numSteps);
            int origStep = 0;
            trigger.CurrentStep = origStep;

            trigger.Step(numSteps);
            Assert.That(trigger.CurrentStep, Is.EqualTo(numSteps - 1));

            trigger.CurrentStep = trigger.StepTriggers.Length - 1;
            trigger.Step(numSteps + 100);
            Assert.That(trigger.CurrentStep, Is.EqualTo(numSteps - 1));
        }

        [Test(TestOf = typeof(SequenceTrigger))]
        public void StepBackwardCanClamp() {
            int numSteps = 2;
            SequenceTrigger trigger = getTriggerObject(numSteps);
            int origStep = numSteps - 1;
            trigger.CurrentStep = origStep;

            trigger.Step(-numSteps);
            Assert.That(trigger.CurrentStep, Is.EqualTo(0));

            trigger.CurrentStep = 0;
            trigger.Step(-numSteps - 100);
            Assert.That(trigger.CurrentStep, Is.EqualTo(0));
        }

        [Test(TestOf = typeof(SequenceTrigger))]
        public void StepForwardCanCycle() {
            int numSteps = 2;
            SequenceTrigger trigger = getTriggerObject(numSteps, cycle: true);
            trigger.CurrentStep = trigger.StepTriggers.Length - 1;

            trigger.Step(1);
            Assert.That(trigger.CurrentStep, Is.EqualTo(0));

            trigger.Step(numSteps);
            Assert.That(trigger.CurrentStep, Is.EqualTo(0));

            trigger.Step(10 * numSteps + 1);
            Assert.That(trigger.CurrentStep, Is.EqualTo(1));
        }

        [Test(TestOf = typeof(SequenceTrigger))]
        public void StepBackwardCanCycle() {
            int numSteps = 2;
            SequenceTrigger trigger = getTriggerObject(numSteps, cycle: true);
            trigger.CurrentStep = 1;

            trigger.Step(-1);
            Assert.That(trigger.CurrentStep, Is.EqualTo(0));

            trigger.Step(-numSteps);
            Assert.That(trigger.CurrentStep, Is.EqualTo(0));

            trigger.Step(-10 * numSteps - 1);
            Assert.That(trigger.CurrentStep, Is.EqualTo(1));
        }

        [Test(TestOf = typeof(SequenceTrigger))]
        public void CanTrigger() {
            string affectedTxt = "";
            SequenceTrigger trigger = getTriggerObject(2);
            trigger.CurrentStep = 0;
            var unityEvent = new UnityEvent();
            unityEvent.AddListener(() => affectedTxt = $"Triggered");
            trigger.StepTriggers[0] = unityEvent;

            trigger.Trigger();

            Assert.That(affectedTxt, Is.EqualTo("Triggered"));
        }

        [Test(TestOf = typeof(SequenceTrigger))]
        public void CanTriggerMultipleTimes() {
            int affectedNum = 0;
            SequenceTrigger trigger = getTriggerObject(1);
            trigger.CurrentStep = 0;
            var unityEvent = new UnityEvent();
            unityEvent.AddListener(() => ++affectedNum);
            trigger.StepTriggers[0] = unityEvent;

            trigger.Trigger();
            Assert.That(affectedNum, Is.EqualTo(1));

            trigger.Trigger();
            Assert.That(affectedNum, Is.EqualTo(2));
        }

        [Test(TestOf = typeof(SequenceTrigger))]
        public void CanStepAndTrigger() {
            string affectedTxt = "";
            SequenceTrigger trigger = getTriggerObject(2);
            trigger.CurrentStep = 0;
            trigger.StepTriggers = Enumerable.Range(0, 2).Select(e => {
                var unityEvent = new UnityEvent();
                unityEvent.AddListener(() => affectedTxt = $"Trigger {e}");
                return unityEvent;
            })
            .ToArray();

            trigger.Step();
            trigger.Trigger();
            Assert.That(affectedTxt, Is.EqualTo("Trigger 1"));

            trigger.CurrentStep = 0;
            trigger.StepAndTrigger();
            Assert.That(affectedTxt, Is.EqualTo("Trigger 1"));
        }

        [Test(TestOf = typeof(SequenceTrigger))]
        public void TriggerHandlesNullEvents() {
            SequenceTrigger trigger = getTriggerObject(1);

            Assert.DoesNotThrow(trigger.Trigger);
        }

        private SequenceTrigger getTriggerObject(int numSteps, bool cycle = false) {
            var obj = new GameObject("TestTrigger");
            SequenceTrigger trigger = obj.AddComponent<SequenceTrigger>();
            trigger.StepTriggers = new UnityEvent[numSteps];
            trigger.Cycle = cycle;

            return trigger;
        }

    }

}
