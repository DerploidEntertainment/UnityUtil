using UnityEngine;

namespace UnityEngine.Inventory {

    public enum AutomaticMode {
        SingleAction,
        SemiAutomatic,
        FullyAutomatic,
    }

    [CreateAssetMenu(fileName = "tool", menuName = "UnityUtil/ToolInfo")]
    public class ToolInfo : ScriptableObject {

        public AutomaticMode AutomaticMode;
        [Tooltip("The UseInput must be maintained for this many seconds before the Tool will perform its first use.")]
        public float TimeToCharge = 0f;
        [Tooltip("Once a (semi-)automatic Tool starts being used, it performs this many uses per second while UseInput is maintained.  Ignored if AutomaticMode is SingleAction.")]
        public float AutomaticUseRate = 1f;
        [Tooltip("A semi-automatic Tool performs this many uses once UseInput is started.  UseInput will be ignored until these uses have all completed.  This value is ignored if AutomaticMode is not set to SemiAutomatic.")]
        public int SemiAutomaticUses = 3;
        [Tooltip("After the last use, this many seconds must pass before the Tool will respond to UseInput again.")]
        public float RefactoryPeriod = 0f;
        [Tooltip("For (semi-)automatic Tools, if this value is true, then AutomaticUseRate will be ignored, and the Tool will recharge before every use while UseInput is maintained.")]
        public bool RechargeEveryUse = false;

    }

}
