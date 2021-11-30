using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TestTools;
using UnityEngine.Triggers;

namespace UnityUtil.Test.PlayMode {

    public class ColliderTriggerTest {

        [UnityTest]
        public IEnumerator TriggerEnterCanTrigger() {
            PlayModeTestHelpers.ResetScene();

            int numTriggers = 0;
            ColliderEnterTrigger trigger = getTriggerObject<ColliderEnterTrigger>(isTrigger: true, () => ++numTriggers);
            Collider testCollider = getCollidingObject();
            Rigidbody collidingRb = testCollider.GetComponent<Rigidbody>();

            // Position test object near trigger
            collidingRb.position = 3f * Vector3.up;
            yield return new WaitForFixedUpdate();
            Assert.That(numTriggers, Is.Zero);

            // Move test object into trigger
            collidingRb.position = trigger.AttachedCollider!.attachedRigidbody.position;
            yield return new WaitForFixedUpdate();
            Assert.That(numTriggers, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator CollisionCanTrigger() {
            PlayModeTestHelpers.ResetScene();

            int numTriggers = 0;
            ColliderEnterTrigger trigger = getTriggerObject<ColliderEnterTrigger>(isTrigger: false, () => ++numTriggers);
            Collider testCollider = getCollidingObject();
            Rigidbody collidingRb = testCollider.GetComponent<Rigidbody>();

            // Position test object near trigger-collider
            collidingRb.position = 3f * Vector3.up;
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
        public IEnumerator TriggerExitCanTrigger() {
            PlayModeTestHelpers.ResetScene();

            int numTriggers = 0;
            ColliderExitTrigger trigger = getTriggerObject<ColliderExitTrigger>(isTrigger: true, () => ++numTriggers);
            Collider testCollider = getCollidingObject();
            Rigidbody collidingRb = testCollider.GetComponent<Rigidbody>();

            // Position test object inside trigger
            collidingRb.position = trigger.AttachedCollider!.attachedRigidbody.position;
            yield return new WaitForFixedUpdate();
            Assert.That(numTriggers, Is.Zero);

            // Move test object out of trigger
            collidingRb.position = 3f * Vector3.up;
            yield return new WaitForFixedUpdate();
            Assert.That(numTriggers, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator CollisionStopCanTrigger() {
            PlayModeTestHelpers.ResetScene();

            int numTriggers = 0;
            ColliderExitTrigger trigger = getTriggerObject<ColliderExitTrigger>(isTrigger: false, () => ++numTriggers);
            Collider testCollider = getCollidingObject();
            Rigidbody collidingRb = testCollider.GetComponent<Rigidbody>();

            // Position test object near trigger-collider (not directly above)
            collidingRb.position = new Vector3(0.25f, 1.1f);
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
        public IEnumerator CanFilterWithWhiteList() {
            PlayModeTestHelpers.ResetScene();

            int numTriggers = 0;
            ColliderEnterTrigger trigger = getTriggerObject<ColliderEnterTrigger>(isTrigger: true, () => ++numTriggers, "test");
            Collider testCollider = getCollidingObject();
            Rigidbody collidingRb = testCollider.GetComponent<Rigidbody>();
            collidingRb.tag = "not-test";

            // Position test object near trigger
            collidingRb.position = 3f * Vector3.up;
            yield return new WaitForFixedUpdate();
            Assert.That(numTriggers, Is.Zero);

            // Move test object into trigger
            collidingRb.position = trigger.AttachedCollider!.attachedRigidbody.position;
            yield return new WaitForFixedUpdate();
            Assert.That(numTriggers, Is.Zero);

            // Move test object back out and back into trigger, now with a whitelisted tag
            collidingRb.position = 3f * Vector3.up;
            collidingRb.tag = "test";
            yield return new WaitForFixedUpdate();
            Assert.That(numTriggers, Is.Zero);
            collidingRb.position = trigger.AttachedCollider.attachedRigidbody.position;
            yield return new WaitForFixedUpdate();
            Assert.That(numTriggers, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator CanFilterWithBlackList() {
            PlayModeTestHelpers.ResetScene();

            int numTriggers = 0;
            ColliderEnterTrigger trigger = getTriggerObject<ColliderEnterTrigger>(isTrigger: true, () => ++numTriggers, "test", filterIsBlacklist: true);
            Collider testCollider = getCollidingObject();
            Rigidbody collidingRb = testCollider.GetComponent<Rigidbody>();
            collidingRb.tag = "not-test";

            // Position test object near trigger
            collidingRb.position = 3f * Vector3.up;
            yield return new WaitForFixedUpdate();
            Assert.That(numTriggers, Is.Zero);

            // Move test object into trigger
            collidingRb.position = trigger.AttachedCollider!.attachedRigidbody.position;
            yield return new WaitForFixedUpdate();
            Assert.That(numTriggers, Is.EqualTo(1));

            // Move test object back out and back into trigger, now with a blacklisted tag
            collidingRb.position = 3f * Vector3.up;
            collidingRb.tag = "test";
            yield return new WaitForFixedUpdate();
            Assert.That(numTriggers, Is.EqualTo(1));
            collidingRb.position = trigger.AttachedCollider.attachedRigidbody.position;
            yield return new WaitForFixedUpdate();
            Assert.That(numTriggers, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator NoRigidbodyTriggerEnterCanTrigger() {
            PlayModeTestHelpers.ResetScene();

            int numTriggers = 0;
            ColliderEnterTrigger trigger = getTriggerObject<ColliderEnterTrigger>(isTrigger: true, () => ++numTriggers);
            Collider testCollider = getCollidingObject(hasRigidbody: false);

            // Position test object near trigger
            testCollider.transform.position = 3f * Vector3.up;
            yield return new WaitForFixedUpdate();
            Assert.That(numTriggers, Is.Zero);

            // Move test object into trigger
            testCollider.transform.position = trigger.AttachedCollider!.attachedRigidbody.position;
            yield return new WaitForFixedUpdate();
            Assert.That(numTriggers, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator NoRigidbodyCollisionCanTrigger() {
            PlayModeTestHelpers.ResetScene();

            int numTriggers = 0;
            ColliderEnterTrigger trigger = getTriggerObject<ColliderEnterTrigger>(isTrigger: false, () => ++numTriggers, useGravity: true);
            Collider testCollider = getCollidingObject(hasRigidbody: false);

            // Position test object below trigger-collider
            testCollider.transform.position = -3f * Vector3.up;
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
        public IEnumerator NoRigidbodyTriggersWhenBlackListed() {
            PlayModeTestHelpers.ResetScene();

            int numTriggers = 0;
            ColliderEnterTrigger trigger = getTriggerObject<ColliderEnterTrigger>(isTrigger: true, () => ++numTriggers, "test", filterIsBlacklist: true);
            Collider testCollider = getCollidingObject(hasRigidbody: false);
            testCollider.tag = "test";

            // Position test object near trigger
            testCollider.transform.position = 3f * Vector3.up;
            yield return new WaitForFixedUpdate();
            Assert.That(numTriggers, Is.Zero);

            // Move test object into trigger
            testCollider.transform.position = trigger.AttachedCollider!.attachedRigidbody.position;
            yield return new WaitForFixedUpdate();
            Assert.That(numTriggers, Is.EqualTo(1));
        }

        private static Collider getCollidingObject(bool hasRigidbody = true) {
            var obj = new GameObject($"test-collider");
            Collider collider = obj.AddComponent<SphereCollider>();
            if (hasRigidbody)
                obj.AddComponent<Rigidbody>();
            return collider;
        }
        private static T getTriggerObject<T>(
            bool isTrigger,
            UnityAction? listener = null,
            string? tagFilter = null,
            bool filterIsBlacklist = false,
            bool useGravity = false
        ) where T : ColliderTriggerBase {
            var obj = new GameObject("test-trigger");

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

    }

}
