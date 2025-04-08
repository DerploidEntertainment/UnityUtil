using NUnit.Framework;
using UnityEngine;
using UnityUtil.Editor.Tests;

namespace UnityUtil.Triggers.Tests.Editor;

public class ToggleTriggerTest : BaseEditModeTestFixture
{
    [Test]
    public void CanToggle()
    {
        ToggleTrigger trigger = getToggleTrigger();

        trigger.TurnOn();
        Assert.That(trigger.IsConditionMet(), Is.True);

        trigger.TurnOff();
        Assert.That(trigger.IsConditionMet(), Is.False);

        trigger.TurnOn();
        Assert.That(trigger.IsConditionMet(), Is.True);
    }

    [Test]
    public void TogglingRaisesCorrectEvent()
    {
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
    public void RepeatedToggleDoesNotRaiseEvent()
    {
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
    public void RepeatedToggleRaisesStillEvent()
    {
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

    private static ToggleTrigger getToggleTrigger() => new GameObject().AddComponent<ToggleTrigger>();
}
