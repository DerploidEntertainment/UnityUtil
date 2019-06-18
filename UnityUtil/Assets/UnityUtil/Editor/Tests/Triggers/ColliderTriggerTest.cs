using NUnit.Framework;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Triggers;

namespace UnityUtil.Test {

    public class ColliderTriggerTest {

        [Parallelizable, Test(TestOf = typeof(SequenceTrigger))]
        public void EnterCanTrigger() {
            ColliderEnterTrigger trigger = getEnterTriggerObject(isTrigger: true);
            Rigidbody collidingRb = getCollidingObject();

            collidingRb.position = Vector3.up;
        }

        [Parallelizable, Test(TestOf = typeof(SequenceTrigger))]
        public void ExitCanTrigger() {
            ColliderExitTrigger trigger = getExitTriggerObject(isTrigger: true);
            Rigidbody collidingRb = getCollidingObject();
        }

        private Rigidbody getCollidingObject() {
            var obj = new GameObject("test-collider", typeof(SphereCollider));
            Rigidbody rb = obj.AddComponent<Rigidbody>();
            return rb;
        }
        private ColliderEnterTrigger getEnterTriggerObject(bool isTrigger, UnityAction listener = null, string tagFilter = null, bool filterIsBlacklist = false) {
            var obj = new GameObject("test-enter-trigger", typeof(Rigidbody));
            Collider collider = obj.AddComponent<SphereCollider>();
            ColliderEnterTrigger trigger = obj.AddComponent<ColliderEnterTrigger>();
            collider.isTrigger = isTrigger;
            trigger.AttachedRigidbodyTagFilter = tagFilter;
            trigger.FilterIsBlacklist = filterIsBlacklist;
            if (listener != null)
                trigger.ColliderEnterred.AddListener(listener);

            return trigger;
        }
        private ColliderExitTrigger getExitTriggerObject(bool isTrigger, UnityAction listener = null, string tagFilter = null, bool filterIsBlacklist = false) {
            var obj = new GameObject("test-exit-trigger", typeof(Rigidbody), typeof(SphereCollider));
            Collider collider = obj.AddComponent<SphereCollider>();
            ColliderExitTrigger trigger = obj.AddComponent<ColliderExitTrigger>();
            collider.isTrigger = isTrigger;
            trigger.AttachedRigidbodyTagFilter = tagFilter;
            trigger.FilterIsBlacklist = filterIsBlacklist;
            if (listener != null)
                trigger.ColliderExited.AddListener(listener);

            return trigger;
        }

    }

}
