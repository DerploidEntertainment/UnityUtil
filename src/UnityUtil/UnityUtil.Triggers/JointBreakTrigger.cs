using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace UnityUtil.Triggers;

[Serializable]
public class JointEvent : UnityEvent<Joint> { }

[RequireComponent(typeof(Joint))]
public class JointBreakTrigger : MonoBehaviour
{
    [RequiredIn(PrefabKind.PrefabInstanceAndNonPrefabInstance)]
    public Joint? Joint { get; private set; }

    public void Break() => Destroy(Joint);
    public JointEvent Broken = new();

    private void OnJointBreak(float breakForce) => Broken.Invoke(Joint!);

}
