using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Assertions;
using System;

namespace Danware.Unity.Triggers {

    [Serializable]
    public class TriggerColliderEvent : UnityEvent<Collider> { }

    [RequireComponent(typeof(Collider))]
    public class TriggerEnterTrigger : MonoBehaviour {

        public Collider TriggerCollider { get; private set; }

        public TriggerColliderEvent ColliderEnterred = new TriggerColliderEvent();

        private void Awake() {
            TriggerCollider = GetComponent<Collider>();
            Assert.IsTrue(TriggerCollider.isTrigger, $"{nameof(TriggerEnterTrigger)} {transform.parent.name}.{name} is associated with a Collider, but the Collider is not a trigger!");
        }
        private void OnTriggerEnter(Collider other) => ColliderEnterred.Invoke(other);

    }

}
