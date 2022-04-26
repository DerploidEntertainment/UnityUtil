namespace UnityEngine.Triggers;

public class OrTrigger : MultiConditionalTrigger
{

    public override bool IsConditionMet()
    {
        for (int c = 0; c < Conditions?.Length; c++) {
            if (Conditions[c].IsConditionMet())
                return true;
        }
        return false;
    }

}
