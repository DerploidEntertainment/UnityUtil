using UnityEngine;

namespace UnityEngine.Inventory {

    [CreateAssetMenu(fileName = "overheat-tool", menuName = "UnityUtil/OverheatToolInfo")]
    public class OverheatToolInfo : ScriptableObject {

        [Tooltip("The maximum amount of heat that can be generated before the Weapon becomes unusable (overheats).")]
        public float MaxHeat = 100f;
        [Tooltip("The amount of heat generated per use.")]
        public float HeatGeneratedPerUse = 50f;
        [Tooltip("If true, then the Weapon cools by an absolute amount per second.  If false, then the Weapon cools by a fraction of MaxHeat per second.")]
        public bool AbsoluteHeat = true;
        [Tooltip("The Weapon will cool by this many units (or this fraction of MaxHeat) per second, unless overheated.")]
        public float CoolRate = 75f;
        [Tooltip("Once overheated, this many seconds must pass before the Weapon will start cooling again.")]
        public float OverheatDuration = 2f;
        [Tooltip("The amount of heat that this Tool has when instantiated.  Must be <= MaxHeat.")]
        public int StartingHeat;

    }

}
