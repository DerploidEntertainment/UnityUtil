using System.Linq;

namespace UnityEngine.Triggers;

public class OrTrigger : MultiConditionalTrigger
{
    private int _numTrue;

    protected override void Awake()
    {
        base.Awake();

        _numTrue = Conditions?.Count(c => c.IsConditionMet()) ?? 0;
    }

    public override bool IsConditionMet() => Conditions?.Any(c => c.IsConditionMet()) ?? false;

    protected override void ConditionBecameTrueListener(ConditionalTrigger condition) =>
        (++_numTrue == 1
            ? (RaiseBecameEvents ? BecameTrue : null)
            : (RaiseStillEvents ? StillTrue : null)
        )?.Invoke();
    protected override void ConditionBecameFalseListener(ConditionalTrigger condition) =>
        (--_numTrue == 0
            ? (RaiseBecameEvents ? BecameFalse : null)
            : (RaiseStillEvents ? StillTrue : null)
        )?.Invoke();
    protected override void ConditionStillTrueListener(ConditionalTrigger condition)
    {
        if (RaiseStillEvents)
            StillTrue.Invoke();
    }
    protected override void ConditionStillFalseListener(ConditionalTrigger condition)
    {
        if (RaiseStillEvents)
            ((_numTrue == 0) ? StillFalse : StillTrue).Invoke();
    }

}
