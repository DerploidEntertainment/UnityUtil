using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityUtil.Triggers;

public class RandomTrigger : MonoBehaviour
{
    public SimpleTrigger[] Triggers = [];

    [Button]
    public void Trigger()
    {
        int t = Random.Range(0, Triggers.Length);
        Triggers[t].Trigger();
    }

}
