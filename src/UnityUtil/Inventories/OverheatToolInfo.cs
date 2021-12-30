namespace UnityEngine.Inventories {

    [CreateAssetMenu(fileName = "overheat-tool", menuName = $"{nameof(UnityUtil)}/{nameof(UnityEngine.Inventories)}/{nameof(UnityEngine.Inventories.OverheatToolInfo)}")]
    public class OverheatToolInfo : ScriptableObject
    {
        [Tooltip($"The maximum amount of heat that can be generated before the {nameof(UnityEngine.Inventories.Tool)} becomes unusable (overheats).")]
        public float MaxHeat = 100f;

        [Tooltip("The amount of heat generated per use.")]
        public float HeatGeneratedPerUse = 50f;

        [Tooltip(
            $"If true, then the {nameof(UnityEngine.Inventories.Tool)} cools by an absolute amount per second. " +
            $"If false, then the {nameof(UnityEngine.Inventories.Tool)} cools by a fraction of {nameof(OverheatToolInfo.MaxHeat)} per second."
        )]
        public bool AbsoluteHeat = true;

        [Tooltip(
            $"The {nameof(UnityEngine.Inventories.Tool)} will cool by this many units " +
            $"(or this fraction of {nameof(OverheatToolInfo.MaxHeat)}) per second, unless overheated."
        )]
        public float CoolRate = 75f;

        [Tooltip($"Once overheated, this many seconds must pass before the {nameof(UnityEngine.Inventories.Tool)} will start cooling again.")]
        public float OverheatDuration = 2f;

        [Tooltip($"The amount of heat that this {nameof(UnityEngine.Inventories.Tool)} has when instantiated. Must be <= {nameof(OverheatToolInfo.MaxHeat)}.")]
        public int StartingHeat;

    }

}
