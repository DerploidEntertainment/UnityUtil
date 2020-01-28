namespace UnityEngine.Inventory {

    [CreateAssetMenu(fileName = "overheat-tool", menuName = "UnityUtil" + "/" + nameof(UnityEngine.Inventory) + "/" + nameof(UnityEngine.Inventory.OverheatToolInfo))]
    public class OverheatToolInfo : ScriptableObject {

        [Tooltip("The maximum amount of heat that can be generated before the " + nameof(UnityEngine.Inventory.Tool) + " becomes unusable (overheats).")]
        public float MaxHeat = 100f;
        [Tooltip("The amount of heat generated per use.")]
        public float HeatGeneratedPerUse = 50f;
        [Tooltip("If true, then the " + nameof(UnityEngine.Inventory.Tool) + " cools by an absolute amount per second.  If false, then the " + nameof(UnityEngine.Inventory.Tool) + " cools by a fraction of " + nameof(OverheatToolInfo.MaxHeat) + " per second.")]
        public bool AbsoluteHeat = true;
        [Tooltip("The " + nameof(UnityEngine.Inventory.Tool) + " will cool by this many units (or this fraction of " + nameof(OverheatToolInfo.MaxHeat) + ") per second, unless overheated.")]
        public float CoolRate = 75f;
        [Tooltip("Once overheated, this many seconds must pass before the " + nameof(UnityEngine.Inventory.Tool) + " will start cooling again.")]
        public float OverheatDuration = 2f;
        [Tooltip("The amount of heat that this " + nameof(UnityEngine.Inventory.Tool) + " has when instantiated.  Must be <= " + nameof(OverheatToolInfo.MaxHeat) + ".")]
        public int StartingHeat;

    }

}
