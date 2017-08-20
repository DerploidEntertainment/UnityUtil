using UnityEngine;
using UnityEngine.Events;

namespace Danware.Unity.Triggers {

    [RequireComponent(typeof(Joint))]
    public class JointBreakTrigger : MonoBehaviour {

        public Joint Joint { get; private set; }

        public void Break() => Destroy(Joint);
        public UnityEvent Broken = new UnityEvent();

        private void Awake() => Joint = GetComponent<Joint>();
        private void OnJointBreak(float breakForce) => Broken.Invoke();

    }

}