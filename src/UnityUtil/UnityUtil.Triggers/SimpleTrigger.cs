using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace UnityUtil.Triggers;

public class SimpleTrigger : MonoBehaviour
{
    private int _lastTriggerFrame;

    [Tooltip("If true, then the Trigger event can only be raised once per frame, even if there are multiple calls to Trigger() in a single frame.")]
    public bool OnlyOncePerFrame;
    public UnityEvent Triggered = new();

    [Button]
    public void Trigger()
    {
        // Make sure we only get triggered once per frame
        int currFrame = Time.frameCount;
        if (!OnlyOncePerFrame || currFrame != _lastTriggerFrame) {
            _lastTriggerFrame = currFrame;
            Triggered.Invoke();
        }
    }
}

