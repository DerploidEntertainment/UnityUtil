namespace UnityEngine.Inventories;

public enum AutomaticMode
{
    SingleAction,
    SemiAutomatic,
    FullyAutomatic,
}

[CreateAssetMenu(fileName = "tool", menuName = $"{nameof(UnityUtil)}/{nameof(UnityEngine.Inventories)}/{nameof(UnityEngine.Inventories.ToolInfo)}")]
public class ToolInfo : ScriptableObject
{
    public AutomaticMode AutomaticMode;

    [Tooltip($"The {nameof(Tool.UseInput)} must be maintained for this many seconds before the {nameof(UnityEngine.Inventories.Tool)} will perform its first use.")]
    public float TimeToCharge = 0f;

    [Tooltip(
        $"Once a (semi-)automatic {nameof(UnityEngine.Inventories.Tool)} starts being used, it performs this many uses per second while " +
        $"{nameof(Tool.UseInput)} is maintained. Ignored if {nameof(ToolInfo.AutomaticMode)} is {nameof(AutomaticMode.SingleAction)}."
    )]
    public float AutomaticUseRate = 1f;

    [Tooltip(
        $"A semi-automatic {nameof(UnityEngine.Inventories.Tool)} performs this many uses once the {nameof(Tool.UseInput)} is started. " +
        $"The {nameof(Tool.UseInput)} will be ignored until these uses have all completed. " +
        $"This value is ignored if {nameof(ToolInfo.AutomaticMode)} is not set to {nameof(AutomaticMode.SemiAutomatic)}."
    )]
    public int SemiAutomaticUses = 3;

    [Tooltip(
        $"After the previous use, this many seconds must pass before the {nameof(UnityEngine.Inventories.Tool)} will " +
        $"respond to {nameof(Tool.UseInput)} again."
    )]
    public float RefactoryPeriod = 0f;

    [Tooltip(
        $"For (semi-)automatic {nameof(UnityEngine.Inventories.Tool)}s, if this value is true, then {nameof(ToolInfo.AutomaticUseRate)} will be ignored, " +
        $"and the {nameof(UnityEngine.Inventories.Tool)} will recharge before every use while {nameof(Tool.UseInput)} is maintained."
    )]
    public bool RechargeEveryUse = false;

}
