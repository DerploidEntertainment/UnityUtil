using Sirenix.OdinInspector;
using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.Events;

namespace UnityEngine.Triggers {

    [Serializable]
    public class JointEvent : UnityEvent<Joint> { }

    [RequireComponent(typeof(Joint))]
    public class JointBreakTrigger : MonoBehaviour
    {
        [Required]
        public Joint? Joint { get; private set; }

        public void Break() => Destroy(Joint);
        public JointEvent Broken = new();

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Unity message")]
        private void OnJointBreak(float breakForce) => Broken.Invoke(Joint!);

    }

}