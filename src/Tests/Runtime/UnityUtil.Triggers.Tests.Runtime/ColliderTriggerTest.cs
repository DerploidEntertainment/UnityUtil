using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TestTools;
using UnityUtil.Triggers;

namespace UnityUtil.Tests
{

    public class ColliderTriggerTest : BasePlayModeTestFixture
    {

        [UnityTest]
        public IEnumerator TriggerEnterCanTrigger()
        {
            int numTriggers = 0;
            ColliderEnterTrigger trigger = getTriggerObject<ColliderEnterTrigger>(isTrigger: true, () => ++numTriggers);
            Collider testCollider = getCollidingObject();
            Rigidbody collidingRb = testCollider.GetComponent<Rigidbody>();

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
            ColliderEnterTrigger trigger = getTriggerObject<ColliderEnterTrigger>(isTrigger: false, () => ++numTriggers);
            Collider testCollider = getCollidingObject();
            Rigidbody collidingRb = testCollider.GetComponent<Rigidbody>();

            // Position test object near trigger-collider
            collidingRb.position = new(collidingRb.position.x, 3f);
            yield return new WaitForFixedUpdate();
            Assert.That(numTriggers, Is.Zero);

            // Let test object fall for a fixed frame (no collision yet)
            yield return new WaitForFixedUpdate();
            Assert.That(numTriggers, Is.Zero);

            // Give test object enough time to fall and collide with trigger-collider
            yield return new WaitForSeconds(1f);
            Assert.That(numTriggers, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator TriggerExitCanTrigger()
        {
            int numTriggers = 0;
            ColliderExitTrigger trigger = getTriggerObject<ColliderExitTrigger>(isTrigger: true, () => ++numTriggers);
            Collider testCollider = getCollidingObject();
            Rigidbody collidingRb = testCollider.GetComponent<Rigidbody>();

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
            ColliderExitTrigger trigger = getTriggerObject<ColliderExitTrigger>(isTrigger: false, () => ++numTriggers);
            Collider testCollider = getCollidingObject();
            Rigidbody collidingRb = testCollider.GetComponent<Rigidbody>();

            // Position test object near trigger-collider (not directly above)
            collidingRb.position = new Vector3(collidingRb.position.x + 0.25f, collidingRb.position.y + 1.1f);
            yield return new WaitForFixedUpdate();
            Assert.That(numTriggers, Is.Zero);

            // Let test object fall for a fixed frame (no collision yet)
            yield return new WaitForFixedUpdate();
            Assert.That(numTriggers, Is.Zero);

            // Give test object enough time to fall onto trigger-collider and roll off
            yield return new WaitForSeconds(1.5f);
            Assert.That(numTriggers, Is.GreaterThan(0));
        }

        [UnityTest]
        public IEnumerator CanFilterWithWhiteList()
        {
            int numTriggers = 0;
            ColliderEnterTrigger trigger = getTriggerObject<ColliderEnterTrigger>(isTrigger: true, () => ++numTriggers, "test");
            Collider testCollider = getCollidingObject();
            Rigidbody collidingRb = testCollider.GetComponent<Rigidbody>();
            collidingRb.tag = "not-test";

            // Position test object near trigger
            testCollider.transform.position = new(testCollider.transform.position.x, 3f);
            yield return new WaitForFixedUpdate();
            Assert.That(numTriggers, Is.Zero);

            // Move test object into trigger
            collidingRb.position = trigger.AttachedCollider!.attachedRigidbody.position;
            yield return new WaitForFixedUpdate();
            Assert.That(numTriggers, Is.Zero);

            // Move test object back out and back into trigger, now with a whitelisted tag
            testCollider.transform.position = new(testCollider.transform.position.x, 3f);
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
            ColliderEnterTrigger trigger = getTriggerObject<ColliderEnterTrigger>(isTrigger: true, () => ++numTriggers, "test", filterIsBlacklist: true);
            Collider testCollider = getCollidingObject();
            Rigidbody collidingRb = testCollider.GetComponent<Rigidbody>();
            collidingRb.tag = "not-test";

            // Position test object near trigger
            testCollider.transform.position = new(testCollider.transform.position.x, 3f);
            yield return new WaitForFixedUpdate();
            Assert.That(numTriggers, Is.Zero);

            // Move test object into trigger
            collidingRb.position = trigger.AttachedCollider!.attachedRigidbody.position;
            yield return new WaitForFixedUpdate();
            Assert.That(numTriggers, Is.EqualTo(1));

            // Move test object back out and back into trigger, now with a blacklisted tag
            testCollider.transform.position = new(testCollider.transform.position.x, 3f);
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
            ColliderEnterTrigger trigger = getTriggerObject<ColliderEnterTrigger>(isTrigger: true, () => ++numTriggers);
            Collider testCollider = getCollidingObject(hasRigidbody: false);

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
            ColliderEnterTrigger trigger = getTriggerObject<ColliderEnterTrigger>(isTrigger: false, () => ++numTriggers, useGravity: true);
            Collider testCollider = getCollidingObject(hasRigidbody: false);

            // Position test object below trigger-collider
            testCollider.transform.position = new(testCollider.transform.position.x, -3f);
            yield return new WaitForFixedUpdate();
            Assert.That(numTriggers, Is.Zero);

            // Let trigger fall for a fixed frame (no collision yet)
            yield return new WaitForFixedUpdate();
            Assert.That(numTriggers, Is.Zero);

            // Give trigger enough time to fall and collide with test collider
            yield return new WaitForSeconds(1f);
            Assert.That(numTriggers, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator NoRigidbodyTriggersWhenBlackListed()
        {
            int numTriggers = 0;
            ColliderEnterTrigger trigger = getTriggerObject<ColliderEnterTrigger>(isTrigger: true, () => ++numTriggers, "test", filterIsBlacklist: true);
            Collider testCollider = getCollidingObject(hasRigidbody: false);
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
        ) where T : ColliderTriggerBase
        {
            var obj = new GameObject("test-trigger");

            // Scene is only reset once per test run, so make sure trigger objects don't bump into each other
            // by shifting the objects for each test by a little bit.
            obj.transform.position = new(s_currentPosX += TEST_DELTA_X, 0f);

            Rigidbody rb = obj.AddComponent<Rigidbody>();
            rb.useGravity = useGravity;
            rb.constraints = useGravity ? RigidbodyConstraints.None : RigidbodyConstraints.FreezeAll;

            Collider collider = obj.AddComponent<SphereCollider>();
            collider.isTrigger = isTrigger;

            T trigger = obj.AddComponent<T>();
            trigger.AttachedRigidbodyTagFilter = tagFilter;
            trigger.FilterIsBlacklist = filterIsBlacklist;
            if (listener is not null)
                trigger.Triggered.AddListener(listener);

            return trigger;
        }

        private static Collider getCollidingObject(bool hasRigidbody = true)
        {
            var obj = new GameObject("test-collider");
            obj.transform.position = new(s_currentPosX, 0f);

            Collider collider = obj.AddComponent<SphereCollider>();
            if (hasRigidbody)
                obj.AddComponent<Rigidbody>();

            return collider;
        }

    }

}
