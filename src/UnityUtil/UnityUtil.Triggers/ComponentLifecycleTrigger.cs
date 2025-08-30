using UnityEngine;
using UnityEngine.Events;

namespace UnityUtil.Triggers;

public class ComponentLifecycleTrigger : MonoBehaviour
{
    public UnityEvent Awoken = new();
    public UnityEvent Started = new();
    public UnityEvent Enabled = new();
    public UnityEvent Disabled = new();
    public UnityEvent Destroyed = new();

    private void Awake() => Awoken.Invoke();

    private void Start() => Started.Invoke();

    private void OnEnable() => Enabled.Invoke();

    private void OnDisable() => Disabled.Invoke();

    private void OnDestroy() => Destroyed.Invoke();

}
