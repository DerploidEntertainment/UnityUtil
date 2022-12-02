using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace UnityUtil.Triggers;

public class RandomTrigger : MonoBehaviour
{
    public SimpleTrigger[] Triggers = Array.Empty<SimpleTrigger>();

    [Button]
    public void Trigger()
    {
        int t = UnityEngine.Random.Range(0, Triggers.Length);
        Triggers[t].Trigger();
    }

}
