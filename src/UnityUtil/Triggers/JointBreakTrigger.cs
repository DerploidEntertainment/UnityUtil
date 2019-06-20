using System;
using UnityEngine;
using UnityEngine.Events;

namespace UnityEngine.Triggers {

    [Serializable]
    public class JointEvent : UnityEvent<Joint> { }

    [RequireComponent(typeof(Joint))]
    public class JointBreakTrigger : MonoBehaviour {

        public Joint Joint { get; private set; }

        public void Break() => Destroy(Joint);
        public JointEvent Broken = new JointEvent();

        private void Awake() => Joint = GetComponent<Joint>();
        private void OnJointBreak(float breakForce) => Broken.Invoke(Joint);

    }

}