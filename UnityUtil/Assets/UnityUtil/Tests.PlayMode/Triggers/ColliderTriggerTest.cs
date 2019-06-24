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
            int numTriggers = 0;
            ColliderEnterTrigger trigger = getTriggerObject<ColliderEnterTrigger>(isTrigger: true, () => ++numTriggers);
            Rigidbody collidingRb = getCollidingObject();

            // Position test object above trigger
            collidingRb.position = 3 * Vector3.up;
            yield return new WaitForFixedUpdate();
            Assert.That(numTriggers, Is.Zero);

            // Move test object into trigger
            collidingRb.position = trigger.AttachedCollider.attachedRigidbody.position;
            yield return new WaitForFixedUpdate();
            Assert.That(numTriggers, Is.EqualTo(1));

            PlayModeTestHelpers.ResetScene();
        }

        [UnityTest]
        public IEnumerator CollisionCanTrigger() {
            int numTriggers = 0;
            ColliderEnterTrigger trigger = getTriggerObject<ColliderEnterTrigger>(isTrigger: false, () => ++numTriggers);
            Rigidbody collidingRb = getCollidingObject();

            // Position test object above trigger
            collidingRb.position = 3 * Vector3.up;
            yield return new WaitForFixedUpdate();
            Assert.That(numTriggers, Is.Zero);

            // Let test object fall for a fixed frame (no collision yet)
            yield return new WaitForFixedUpdate();
            Assert.That(numTriggers, Is.Zero);

            // Give test object enough time to fall and collide with trigger
            yield return new WaitForSeconds(1.5f);
            Assert.That(numTriggers, Is.EqualTo(1));

            PlayModeTestHelpers.ResetScene();
        }

        [Test(TestOf = typeof(SequenceTrigger))]
        public void ExitCanTrigger() {
            ColliderExitTrigger trigger = getTriggerObject<ColliderExitTrigger>(isTrigger: true);
            Rigidbody collidingRb = getCollidingObject();

            PlayModeTestHelpers.ResetScene();
        }

        private Rigidbody getCollidingObject() {
            var obj = new GameObject("test-collider", typeof(SphereCollider));
            Rigidbody rb = obj.AddComponent<Rigidbody>();
            return rb;
        }
        private T getTriggerObject<T>(bool isTrigger, UnityAction listener = null, string tagFilter = null, bool filterIsBlacklist = false) where T : ColliderTriggerBase {
            var obj = new GameObject("test-trigger");

            Rigidbody rb = obj.AddComponent<Rigidbody>();
            rb.useGravity = false;

            Collider collider = obj.AddComponent<SphereCollider>();
            collider.isTrigger = isTrigger;

            T trigger = obj.AddComponent<T>();
            trigger.AttachedRigidbodyTagFilter = tagFilter;
            trigger.FilterIsBlacklist = filterIsBlacklist;
            if (listener != null)
                trigger.Triggered.AddListener(listener);

            return trigger;
        }

    }

}
