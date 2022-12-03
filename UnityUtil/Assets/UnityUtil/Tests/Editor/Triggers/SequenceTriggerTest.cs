using NUnit.Framework;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityUtil.Editor.Tests.Logging;
using UnityUtil.Triggers;

namespace UnityUtil.Editor.Tests.Triggers
{

    public class SequenceTriggerTest : BaseEditModeTestFixture
    {

        [Test]
        public void CanStep()
        {
            SequenceTrigger trigger = getSequenceTrigger(2);
            int origStep = trigger.CurrentStep;

            trigger.Step();

            Assert.That(trigger.CurrentStep, Is.EqualTo(origStep + 1));
        }

        [Test]
        public void CanStepForwardMultiple()
        {
            SequenceTrigger trigger = getSequenceTrigger(10);
            int origStep = trigger.CurrentStep;

            trigger.Step(1);
            Assert.That(trigger.CurrentStep, Is.EqualTo(origStep + 1));

            trigger.CurrentStep = origStep;
            trigger.Step(2);
            Assert.That(trigger.CurrentStep, Is.EqualTo(origStep + 2));
        }

        [Test]
        public void CanStepBackwardMultiple()
        {
            SequenceTrigger trigger = getSequenceTrigger(10);
            int origStep = 5;
            trigger.CurrentStep = origStep;

            trigger.Step(-1);
            Assert.That(trigger.CurrentStep, Is.EqualTo(origStep - 1));

            trigger.CurrentStep = origStep;
            trigger.Step(-2);
            Assert.That(trigger.CurrentStep, Is.EqualTo(origStep - 2));
        }

        [Test]
        public void StepForwardCanClamp()
        {
            int numSteps = 2;
            SequenceTrigger trigger = getSequenceTrigger(numSteps);
            int origStep = 0;
            trigger.CurrentStep = origStep;

            trigger.Step(numSteps);
            Assert.That(trigger.CurrentStep, Is.EqualTo(numSteps - 1));

            trigger.CurrentStep = trigger.StepTriggers.Length - 1;
            trigger.Step(numSteps + 100);
            Assert.That(trigger.CurrentStep, Is.EqualTo(numSteps - 1));
        }

        [Test]
        public void StepBackwardCanClamp()
        {
            int numSteps = 2;
            SequenceTrigger trigger = getSequenceTrigger(numSteps);
            int origStep = numSteps - 1;
            trigger.CurrentStep = origStep;

            trigger.Step(-numSteps);
            Assert.That(trigger.CurrentStep, Is.EqualTo(0));

            trigger.CurrentStep = 0;
            trigger.Step(-numSteps - 100);
            Assert.That(trigger.CurrentStep, Is.EqualTo(0));
        }

        [Test]
        public void StepForwardCanCycle()
        {
            int numSteps = 2;
            SequenceTrigger trigger = getSequenceTrigger(numSteps, cycle: true);
            trigger.CurrentStep = trigger.StepTriggers.Length - 1;

            trigger.Step(1);
            Assert.That(trigger.CurrentStep, Is.EqualTo(0));

            trigger.Step(numSteps);
            Assert.That(trigger.CurrentStep, Is.EqualTo(0));

            trigger.Step(10 * numSteps + 1);
            Assert.That(trigger.CurrentStep, Is.EqualTo(1));
        }

        [Test]
        public void StepBackwardCanCycle()
        {
            int numSteps = 2;
            SequenceTrigger trigger = getSequenceTrigger(numSteps, cycle: true);
            trigger.CurrentStep = 1;

            trigger.Step(-1);
            Assert.That(trigger.CurrentStep, Is.EqualTo(0));

            trigger.Step(-numSteps);
            Assert.That(trigger.CurrentStep, Is.EqualTo(0));

            trigger.Step(-10 * numSteps - 1);
            Assert.That(trigger.CurrentStep, Is.EqualTo(1));
        }

        [Test]
        public void CanTrigger()
        {
            string affectedTxt = "";
            SequenceTrigger trigger = getSequenceTrigger(2);
            trigger.CurrentStep = 0;
            var unityEvent = new UnityEvent();
            unityEvent.AddListener(() => affectedTxt = $"Triggered");
            trigger.StepTriggers[0] = unityEvent;

            trigger.Trigger();

            Assert.That(affectedTxt, Is.EqualTo("Triggered"));
        }

        [Test]
        public void CanTriggerMultipleTimes()
        {
            int affectedNum = 0;
            SequenceTrigger trigger = getSequenceTrigger(1);
            trigger.CurrentStep = 0;
            var unityEvent = new UnityEvent();
            unityEvent.AddListener(() => ++affectedNum);
            trigger.StepTriggers[0] = unityEvent;

            trigger.Trigger();
            Assert.That(affectedNum, Is.EqualTo(1));

            trigger.Trigger();
            Assert.That(affectedNum, Is.EqualTo(2));
        }

        [Test]
        public void CanStepAndTrigger()
        {
            string affectedTxt = "";
            SequenceTrigger trigger = getSequenceTrigger(2);
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

        [Test]
        public void TriggerHandlesNullEvents()
        {
            SequenceTrigger trigger = getSequenceTrigger(1);

            Assert.DoesNotThrow(trigger.Trigger);
        }

        private static SequenceTrigger getSequenceTrigger(int numSteps, bool cycle = false)
        {
            var obj = new GameObject("TestTrigger");
            SequenceTrigger trigger = obj.AddComponent<SequenceTrigger>();
            trigger.Inject(new TestLoggerProvider());
            trigger.StepTriggers = new UnityEvent[numSteps];
            trigger.Cycle = cycle;

            return trigger;
        }

    }

}
