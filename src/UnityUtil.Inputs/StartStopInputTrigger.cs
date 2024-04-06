using Sirenix.OdinInspector;
using UnityEngine.Events;
using UnityUtil.Inputs;
using UnityUtil.Updating;

namespace UnityUtil.Triggers.Input;

public class StartStopInputTrigger : Updatable
{
    [RequiredIn(PrefabKind.NonPrefabInstance)]
    public StartStopInput? Input;
    public UnityEvent InputStarted = new();
    public UnityEvent InputStopped = new();

    protected override void Awake()
    {
        base.Awake();

        RegisterUpdate(checkInputs);
    }

    private void checkInputs(float deltaTime)
    {
        if (Input!.Started())
            InputStarted.Invoke();
        else if (Input.Stopped())
            InputStopped.Invoke();
    }

}
