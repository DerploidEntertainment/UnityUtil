using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TestTools;
using UnityUtil.Triggers;

namespace UnityUtil.Tests;

public class ColliderTriggerTest2D : BasePlayModeTestFixture
{
    [UnityTest]
    public IEnumerator TriggerEnterCanTrigger()
    {
        int numTriggers = 0;
        ColliderEnterTrigger2D trigger = getTriggerObject<ColliderEnterTrigger2D>(isTrigger: true, () => ++numTriggers);
        Collider2D testCollider = getCollidingObject();
        Rigidbody2D collidingRb = testCollider.GetComponent<Rigidbody2D>();

        // Position test object near trigger
        collidingRb.position = new(collidingRb.position.x, 3f);
        yield return new WaitForFixedUpdate();
        Assert.That(numTriggers, Is.Zero);

        // Move test object into trigger
        collidingRb.position = trigger.AttachedCollider!.attachedRigidbody.position;
        yield return new WaitForFixedUpdate();
        Assert.That(numTriggers, Is.EqualTo(1));
    }

    [UnityTest]
    public IEnumerator CollisionCanTrigger()
    {
        int numTriggers = 0;
        ColliderEnterTrigger2D trigger = getTriggerObject<ColliderEnterTrigger2D>(isTrigger: false, () => ++numTriggers);
        Collider2D testCollider = getCollidingObject();
        Rigidbody2D collidingRb = testCollider.GetComponent<Rigidbody2D>();

        // Position test object near trigger-Collider2D
        collidingRb.position = new(collidingRb.position.x, 3f);
        yield return new WaitForFixedUpdate();
        Assert.That(numTriggers, Is.Zero);

        // Let test object fall for a fixed frame (no collision yet)
        yield return new WaitForFixedUpdate();
        Assert.That(numTriggers, Is.Zero);

        // Give test object enough time to fall and collide with trigger-Collider2D
        yield return new WaitForSeconds(1f);
        Assert.That(numTriggers, Is.EqualTo(1));
    }

    [UnityTest]
    public IEnumerator TriggerExitCanTrigger()
    {
        int numTriggers = 0;
        ColliderExitTrigger2D trigger = getTriggerObject<ColliderExitTrigger2D>(isTrigger: true, () => ++numTriggers);
        Collider2D testCollider = getCollidingObject();
        Rigidbody2D collidingRb = testCollider.GetComponent<Rigidbody2D>();

        // Position test object inside trigger
        collidingRb.position = trigger.AttachedCollider!.attachedRigidbody.position;
        yield return new WaitForFixedUpdate();
        Assert.That(numTriggers, Is.Zero);

        // Move test object out of trigger
        collidingRb.position = new(collidingRb.position.x, 3f);
        yield return new WaitForFixedUpdate();
        Assert.That(numTriggers, Is.EqualTo(1));
    }

    [UnityTest]
    public IEnumerator CollisionStopCanTrigger()
    {
        int numTriggers = 0;
        ColliderExitTrigger2D trigger = getTriggerObject<ColliderExitTrigger2D>(isTrigger: false, () => ++numTriggers);
        Collider2D testCollider = getCollidingObject();
        Rigidbody2D collidingRb = testCollider.GetComponent<Rigidbody2D>();

        // Position test object near trigger-Collider2D (not directly above)
        collidingRb.position = new(collidingRb.position.x + 0.25f, collidingRb.position.y + 1.1f);
        yield return new WaitForFixedUpdate();
        Assert.That(numTriggers, Is.Zero);

        // Let test object fall for a fixed frame (no collision yet)
        yield return new WaitForFixedUpdate();
        Assert.That(numTriggers, Is.Zero);

        // Give test object enough time to fall onto trigger-Collider2D and roll off
        yield return new WaitForSeconds(1.5f);
        Assert.That(numTriggers, Is.GreaterThan(0));
    }

    [UnityTest]
    public IEnumerator CanFilterWithWhiteList()
    {
        int numTriggers = 0;
        ColliderEnterTrigger2D trigger = getTriggerObject<ColliderEnterTrigger2D>(isTrigger: true, () => ++numTriggers, "test");
        Collider2D testCollider = getCollidingObject();
        Rigidbody2D collidingRb = testCollider.GetComponent<Rigidbody2D>();
        collidingRb.tag = "not-test";

        // Position test object near trigger
        collidingRb.position = new(collidingRb.position.x, 3f);
        yield return new WaitForFixedUpdate();
        Assert.That(numTriggers, Is.Zero);

        // Move test object into trigger
        collidingRb.position = trigger.AttachedCollider!.attachedRigidbody.position;
        yield return new WaitForFixedUpdate();
        Assert.That(numTriggers, Is.Zero);

        // Move test object back out and back into trigger, now with a whitelisted tag
        collidingRb.position = new(collidingRb.position.x, 3f);
        collidingRb.tag = "test";
        yield return new WaitForFixedUpdate();
        Assert.That(numTriggers, Is.Zero);
        collidingRb.position = trigger.AttachedCollider.attachedRigidbody.position;
        yield return new WaitForFixedUpdate();
        Assert.That(numTriggers, Is.EqualTo(1));
    }

    [UnityTest]
    public IEnumerator CanFilterWithBlackList()
    {
        int numTriggers = 0;
        ColliderEnterTrigger2D trigger = getTriggerObject<ColliderEnterTrigger2D>(isTrigger: true, () => ++numTriggers, "test", filterIsBlacklist: true);
        Collider2D testCollider = getCollidingObject();
        Rigidbody2D collidingRb = testCollider.GetComponent<Rigidbody2D>();
        collidingRb.tag = "not-test";

        // Position test object near trigger
        collidingRb.position = new(collidingRb.position.x, 3f);
        yield return new WaitForFixedUpdate();
        Assert.That(numTriggers, Is.Zero);

        // Move test object into trigger
        collidingRb.position = trigger.AttachedCollider!.attachedRigidbody.position;
        yield return new WaitForFixedUpdate();
        Assert.That(numTriggers, Is.EqualTo(1));

        // Move test object back out and back into trigger, now with a blacklisted tag
        collidingRb.position = new(collidingRb.position.x, 3f);
        collidingRb.tag = "test";
        yield return new WaitForFixedUpdate();
        Assert.That(numTriggers, Is.EqualTo(1));
        collidingRb.position = trigger.AttachedCollider.attachedRigidbody.position;
        yield return new WaitForFixedUpdate();
        Assert.That(numTriggers, Is.EqualTo(1));
    }

    [UnityTest]
    public IEnumerator NoRigidbodyTriggerEnterCanTrigger()
    {
        int numTriggers = 0;
        ColliderEnterTrigger2D trigger = getTriggerObject<ColliderEnterTrigger2D>(isTrigger: true, () => ++numTriggers);
        Collider2D testCollider = getCollidingObject(hasRigidbody: false);

        // Position test object near trigger
        testCollider.transform.position = new(testCollider.transform.position.x, 3f);
        yield return new WaitForFixedUpdate();
        Assert.That(numTriggers, Is.Zero);

        // Move test object into trigger
        testCollider.transform.position = trigger.AttachedCollider!.attachedRigidbody.position;
        yield return new WaitForFixedUpdate();
        Assert.That(numTriggers, Is.EqualTo(1));
    }

    [UnityTest]
    public IEnumerator NoRigidbodyCollisionCanTrigger()
    {
        int numTriggers = 0;
        ColliderEnterTrigger2D trigger = getTriggerObject<ColliderEnterTrigger2D>(isTrigger: false, () => ++numTriggers, useGravity: true);
        Collider2D testCollider = getCollidingObject(hasRigidbody: false);

        // Position test object below trigger-Collider2D
        testCollider.transform.position = new(testCollider.transform.position.x, -3f);
        yield return new WaitForFixedUpdate();
        Assert.That(numTriggers, Is.Zero);

        // Let trigger fall for a fixed frame (no collision yet)
        yield return new WaitForFixedUpdate();
        Assert.That(numTriggers, Is.Zero);

        // Give trigger enough time to fall and collide with test Collider2D
        yield return new WaitForSeconds(1f);
        Assert.That(numTriggers, Is.EqualTo(1));
    }

    [UnityTest]
    public IEnumerator NoRigidbodyTriggersWhenBlackListed()
    {
        int numTriggers = 0;
        ColliderEnterTrigger2D trigger = getTriggerObject<ColliderEnterTrigger2D>(isTrigger: true, () => ++numTriggers, "test", filterIsBlacklist: true);
        Collider2D testCollider = getCollidingObject(hasRigidbody: false);
        testCollider.tag = "test";

        // Position test object near trigger
        testCollider.transform.position = new(testCollider.transform.position.x, 3f);
        yield return new WaitForFixedUpdate();
        Assert.That(numTriggers, Is.Zero);

        // Move test object into trigger
        testCollider.transform.position = trigger.AttachedCollider!.attachedRigidbody.position;
        yield return new WaitForFixedUpdate();
        Assert.That(numTriggers, Is.EqualTo(1));
    }

    private static float s_currentPosX = 0f;
    private const float TEST_DELTA_X = 5f;

    private static T getTriggerObject<T>(
        bool isTrigger,
        UnityAction? listener = null,
        string? tagFilter = null,
        bool filterIsBlacklist = false,
        bool useGravity = false
    ) where T : ColliderTriggerBase2D
    {
        var obj = new GameObject("test-trigger");

        // Scene is only reset once per test run, so make sure trigger objects don't bump into each other
        // by shifting the objects for each test by a little bit.
        obj.transform.position = new(s_currentPosX += TEST_DELTA_X, 0f);

        Rigidbody2D rb = obj.AddComponent<Rigidbody2D>();
        rb.gravityScale = useGravity ? 1f : 0f;
        rb.constraints = useGravity ? RigidbodyConstraints2D.None : RigidbodyConstraints2D.FreezeAll;

        Collider2D collider = obj.AddComponent<CircleCollider2D>();
        collider.isTrigger = isTrigger;

        T trigger = obj.AddComponent<T>();
        trigger.AttachedRigidbodyTagFilter = tagFilter;
        trigger.FilterIsBlacklist = filterIsBlacklist;
        if (listener is not null)
            trigger.Triggered.AddListener(listener);

        return trigger;
    }

    private static Collider2D getCollidingObject(bool hasRigidbody = true)
    {
        var obj = new GameObject($"test-collider");
        obj.transform.position = new(s_currentPosX, 0f);

        Collider2D collider = obj.AddComponent<CircleCollider2D>();
        if (hasRigidbody)
            obj.AddComponent<Rigidbody2D>();

        return collider;
    }

}
