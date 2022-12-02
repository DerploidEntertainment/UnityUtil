namespace UnityUtil.Triggers;

public class AndTrigger : MultiConditionalTrigger
{

    public override bool IsConditionMet()
    {
        for (int c = 0; c < Conditions?.Length; c++) {
            if (!Conditions[c].IsConditionMet())
                return false;
        }
        return true;
    }

}
