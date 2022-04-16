using System;
using UnityEngine.Events;

namespace UnityEngine;

[Serializable]
public class CancellableUnityEvent : UnityEvent
{
    public bool Cancel { get; set; }
    public new void Invoke()
    {
        Cancel = false;
        base.Invoke();
    }
}
