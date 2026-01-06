using System.Linq;
using NUnit.Framework;
using Unity.Extensions.Logging;
using UnityEngine;
using UnityEngine.Events;
using UnityUtil.Editor.Tests;

namespace UnityUtil.Triggers.Tests.Editor;

public class SequenceTriggerTest : BaseEditModeTestFixture
{
    private readonly UnityDebugLoggerFactory _loggerFactory = new();
    private GameObject? _gameObject;

    [SetUp]
    public void Setup() =>
        _gameObject = _gameObject != null ? _gameObject : new GameObject(nameof(SequenceTriggerTest));

    [OneTimeTearDown]
    public void OneTimeTearDown() => _loggerFactory.Dispose();

    [Test]
    public void CanStep()
    {
        SequenceTrigger trigger = buildSequenceTrigger(2);
        int origStep = trigger.CurrentStep;

        trigger.StepByOne();

        Assert.That(trigger.CurrentStep, Is.EqualTo(origStep + 1));
    }

    [Test]
    public void CanStepForwardMultiple()
    {
        SequenceTrigger trigger = buildSequenceTrigger(10);
        int origStep = trigger.CurrentStep;

        trigger.StepByCount(1);
        Assert.That(trigger.CurrentStep, Is.EqualTo(origStep + 1));

        trigger.CurrentStep = origStep;
        trigger.StepByCount(2);
        Assert.That(trigger.CurrentStep, Is.EqualTo(origStep + 2));
    }

    [Test]
    public void CanStepBackwardMultiple()
    {
        SequenceTrigger trigger = buildSequenceTrigger(10);
        int origStep = 5;
        trigger.CurrentStep = origStep;

        trigger.StepByCount(-1);
        Assert.That(trigger.CurrentStep, Is.EqualTo(origStep - 1));

        trigger.CurrentStep = origStep;
        trigger.StepByCount(-2);
        Assert.That(trigger.CurrentStep, Is.EqualTo(origStep - 2));
    }

    [Test]
    public void StepForwardCanClamp()
    {
        int numSteps = 2;
        SequenceTrigger trigger = buildSequenceTrigger(numSteps);
        int origStep = 0;
        trigger.CurrentStep = origStep;

        trigger.StepByCount(numSteps);
        Assert.That(trigger.CurrentStep, Is.EqualTo(numSteps - 1));

        trigger.CurrentStep = trigger.StepTriggers.Length - 1;
        trigger.StepByCount(numSteps + 100);
        Assert.That(trigger.CurrentStep, Is.EqualTo(numSteps - 1));
    }

    [Test]
    public void StepBackwardCanClamp()
    {
        int numSteps = 2;
        SequenceTrigger trigger = buildSequenceTrigger(numSteps);
        int origStep = numSteps - 1;
        trigger.CurrentStep = origStep;

        trigger.StepByCount(-numSteps);
        Assert.That(trigger.CurrentStep, Is.EqualTo(0));

        trigger.CurrentStep = 0;
        trigger.StepByCount(-numSteps - 100);
        Assert.That(trigger.CurrentStep, Is.EqualTo(0));
    }

    [Test]
    public void StepForwardCanCycle()
    {
        int numSteps = 2;
        SequenceTrigger trigger = buildSequenceTrigger(numSteps, cycle: true);
        trigger.CurrentStep = trigger.StepTriggers.Length - 1;

        trigger.StepByCount(1);
        Assert.That(trigger.CurrentStep, Is.EqualTo(0));

        trigger.StepByCount(numSteps);
        Assert.That(trigger.CurrentStep, Is.EqualTo(0));

        trigger.StepByCount(10 * numSteps + 1);
        Assert.That(trigger.CurrentStep, Is.EqualTo(1));
    }

    [Test]
    public void StepBackwardCanCycle()
    {
        int numSteps = 2;
        SequenceTrigger trigger = buildSequenceTrigger(numSteps, cycle: true);
        trigger.CurrentStep = 1;

        trigger.StepByCount(-1);
        Assert.That(trigger.CurrentStep, Is.EqualTo(0));

        trigger.StepByCount(-numSteps);
        Assert.That(trigger.CurrentStep, Is.EqualTo(0));

        trigger.StepByCount(-10 * numSteps - 1);
        Assert.That(trigger.CurrentStep, Is.EqualTo(1));
    }

    [Test]
    public void CanTrigger()
    {
        string affectedTxt = "";
        SequenceTrigger trigger = buildSequenceTrigger(2);
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
        SequenceTrigger trigger = buildSequenceTrigger(1);
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
        SequenceTrigger trigger = buildSequenceTrigger(2);
        trigger.CurrentStep = 0;
        trigger.StepTriggers = [
            .. Enumerable.Range(0, 2).Select(e => {
                var unityEvent = new UnityEvent();
                unityEvent.AddListener(() => affectedTxt = $"Trigger {e}");
                return unityEvent;
            })
        ];

        trigger.StepByOne();
        trigger.Trigger();
        Assert.That(affectedTxt, Is.EqualTo("Trigger 1"));

        trigger.CurrentStep = 0;
        trigger.StepByOneAndTrigger();
        Assert.That(affectedTxt, Is.EqualTo("Trigger 1"));
    }

    [Test]
    public void CanSetStep()
    {
        SequenceTrigger trigger = buildSequenceTrigger(2);

        trigger.SetStep(1);
        Assert.That(trigger.CurrentStep, Is.EqualTo(1));

        trigger.SetStep(0);
        Assert.That(trigger.CurrentStep, Is.EqualTo(0));
    }

    [Test]
    public void CanSetStepAndTrigger()
    {
        // ARRANGE
        int step0TriggerCount = 0;
        int step1TriggerCount = 0;
        SequenceTrigger trigger = buildSequenceTrigger(2);
        trigger.StepTriggers[0] = new UnityEvent();
        trigger.StepTriggers[1] = new UnityEvent();
        trigger.StepTriggers[0].AddListener(() => ++step0TriggerCount);
        trigger.StepTriggers[1].AddListener(() => ++step1TriggerCount);

        // ACT / ASSERT
        Assert.That(step0TriggerCount, Is.Zero);
        Assert.That(step1TriggerCount, Is.Zero);

        trigger.SetStepAndTrigger(1);
        Assert.That(step0TriggerCount, Is.Zero);
        Assert.That(step1TriggerCount, Is.EqualTo(1));

        trigger.SetStepAndTrigger(0);
        Assert.That(step0TriggerCount, Is.EqualTo(1));
        Assert.That(step1TriggerCount, Is.EqualTo(1));
    }

    [Test]
    public void TriggerHandlesNullEvents()
    {
        SequenceTrigger trigger = buildSequenceTrigger(1);

        Assert.DoesNotThrow(trigger.Trigger);
    }

    private SequenceTrigger buildSequenceTrigger(int numSteps, bool cycle = false)
    {
        SequenceTrigger trigger = _gameObject!.AddComponent<SequenceTrigger>();
        trigger.Inject(_loggerFactory);
        trigger.StepTriggers = new UnityEvent[numSteps];
        trigger.Cycle = cycle;

        return trigger;
    }
}
