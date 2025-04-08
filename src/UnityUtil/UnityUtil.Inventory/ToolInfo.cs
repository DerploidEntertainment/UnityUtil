using UnityEngine;

namespace UnityUtil.Inventory;

public enum AutomaticMode
{
    SingleAction,
    SemiAutomatic,
    FullyAutomatic,
}

[CreateAssetMenu(fileName = "tool", menuName = $"{nameof(UnityUtil)}/{nameof(UnityUtil.Inventory)}/{nameof(ToolInfo)}")]
public class ToolInfo : ScriptableObject
{
    public AutomaticMode AutomaticMode;

    [Tooltip($"The {nameof(Tool.UseInput)} must be maintained for this many seconds before the {nameof(Tool)} will perform its first use.")]
    public float TimeToCharge = 0f;

    [Tooltip(
        $"Once a (semi-)automatic {nameof(Tool)} starts being used, it performs this many uses per second while " +
        $"{nameof(Tool.UseInput)} is maintained. Ignored if {nameof(AutomaticMode)} is {nameof(AutomaticMode.SingleAction)}."
    )]
    public float AutomaticUseRate = 1f;

    [Tooltip(
        $"A semi-automatic {nameof(Tool)} performs this many uses once the {nameof(Tool.UseInput)} is started. " +
        $"The {nameof(Tool.UseInput)} will be ignored until these uses have all completed. " +
        $"This value is ignored if {nameof(AutomaticMode)} is not set to {nameof(AutomaticMode.SemiAutomatic)}."
    )]
    public int SemiAutomaticUses = 3;

    [Tooltip(
        $"After the previous use, this many seconds must pass before the {nameof(Tool)} will " +
        $"respond to {nameof(Tool.UseInput)} again."
    )]
    public float RefactoryPeriod = 0f;

    [Tooltip(
        $"For (semi-)automatic {nameof(Tool)}s, if this value is true, then {nameof(AutomaticUseRate)} will be ignored, " +
        $"and the {nameof(Tool)} will recharge before every use while {nameof(Tool.UseInput)} is maintained."
    )]
    public bool RechargeEveryUse = false;

}
