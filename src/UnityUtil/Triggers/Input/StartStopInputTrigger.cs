using Sirenix.OdinInspector;
using UnityEngine.Events;
using UnityEngine.Inputs;

namespace UnityEngine.Triggers.Input;

public class StartStopInputTrigger : Updatable
{
    [Required]
    public StartStopInput? Input;
    public UnityEvent InputStarted = new();
    public UnityEvent InputStopped = new();

    protected override void Awake()
    {
        base.Awake();

        RegisterUpdatesAutomatically = true;
        BetterUpdate = checkInputs;
    }

    private void checkInputs(float deltaTime)
    {
        if (Input!.Started())
            InputStarted.Invoke();
        else if (Input.Stopped())
            InputStopped.Invoke();
    }

}
