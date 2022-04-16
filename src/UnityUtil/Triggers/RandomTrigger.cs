using Sirenix.OdinInspector;
using System;

namespace UnityEngine.Triggers;

public class RandomTrigger : MonoBehaviour
{
    public SimpleTrigger[] Triggers = Array.Empty<SimpleTrigger>();

    [Button]
    public void Trigger()
    {
        int t = Random.Range(0, Triggers.Length);
        Triggers[t].Trigger();
    }

}
