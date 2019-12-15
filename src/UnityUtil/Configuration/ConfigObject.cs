using Sirenix.OdinInspector;
using System;

namespace UnityEngine {

    public enum ConfigValueType {
        Integer,
        Boolean,
        Float,
        String,
        Color,
        ObjectReference,
        LayerMask,
        Vector2,
        Vector3,
        Vector4,
        Rect,
        AnimationCurve,
        Bounds,
        Gradient,
        Quaternion,
        Vector2Int,
        Vector3Int,
        RectInt,
        BoundsInt
    }

    [Serializable]
    public class Config {
        public string Key;
        public ConfigValueType Type;

        [ShowIf(nameof(Config.Type), ConfigValueType.Integer)]
        public int IntValue;

        [ShowIf(nameof(Config.Type), ConfigValueType.Boolean)]
        public bool BoolValue;

        [ShowIf(nameof(Config.Type), ConfigValueType.Float)]
        public float FloatValue;

        [ShowIf(nameof(Config.Type), ConfigValueType.String)]
        public string StringValue;

        [ShowIf(nameof(Config.Type), ConfigValueType.Color)]
        public Color ColorValue;

        [ShowIf(nameof(Config.Type), ConfigValueType.ObjectReference)]
        public Object ObjectValue;

        [ShowIf(nameof(Config.Type), ConfigValueType.LayerMask)]
        public LayerMask LayerMaskValue;

        [ShowIf(nameof(Config.Type), ConfigValueType.Vector2)]
        public Vector2 Vector2Value;

        [ShowIf(nameof(Config.Type), ConfigValueType.Vector3)]
        public Vector3 Vector3Value;

        [ShowIf(nameof(Config.Type), ConfigValueType.Vector4)]
        public Vector4 Vector4Value;

        [ShowIf(nameof(Config.Type), ConfigValueType.Rect)]
        public Rect RectValue;

        [ShowIf(nameof(Config.Type), ConfigValueType.AnimationCurve)]
        public AnimationCurve AnimationCurveValue;

        [ShowIf(nameof(Config.Type), ConfigValueType.Bounds)]
        public Bounds BoundsValue;

        [ShowIf(nameof(Config.Type), ConfigValueType.Gradient)]
        public Gradient GradientValue;

        [ShowIf(nameof(Config.Type), ConfigValueType.Quaternion)]
        public Quaternion QuaternionValue;

        [ShowIf(nameof(Config.Type), ConfigValueType.Vector2Int)]
        public Vector2Int Vector2IntValue;

        [ShowIf(nameof(Config.Type), ConfigValueType.Vector3Int)]
        public Vector3Int Vector3IntValue;

        [ShowIf(nameof(Config.Type), ConfigValueType.RectInt)]
        public RectInt RectIntValue;

        [ShowIf(nameof(Config.Type), ConfigValueType.BoundsInt)]
        public BoundsInt BoundsIntValue;

        public object GetValue() {
            switch (Type) {
                case ConfigValueType.Integer: return IntValue;
                case ConfigValueType.Boolean: return BoolValue; 
                case ConfigValueType.Float: return FloatValue;
                case ConfigValueType.String: return StringValue;
                case ConfigValueType.Color: return ColorValue;
                case ConfigValueType.ObjectReference: return ObjectValue;
                case ConfigValueType.LayerMask: return LayerMaskValue;
                case ConfigValueType.Vector2: return Vector2Value;
                case ConfigValueType.Vector3: return Vector3Value;
                case ConfigValueType.Vector4: return Vector4Value;
                case ConfigValueType.Rect: return RectValue;
                case ConfigValueType.AnimationCurve: return AnimationCurveValue;
                case ConfigValueType.Bounds: return BoundsValue;
                case ConfigValueType.Gradient: return GradientValue;
                case ConfigValueType.Quaternion: return QuaternionValue;
                case ConfigValueType.Vector2Int: return Vector2IntValue;
                case ConfigValueType.Vector3Int: return Vector3IntValue;
                case ConfigValueType.RectInt: return RectIntValue;
                case ConfigValueType.BoundsInt: return BoundsIntValue;
                default: throw new NotImplementedException(BetterLogger.GetSwitchDefault(Type));
            }
        }
    }

    [CreateAssetMenu(menuName = nameof(UnityUtil) + "/" + nameof(ConfigObject), fileName = "appsettings.asset")]
    public class ConfigObject : ScriptableObject {
        public Config[] Configs;
    }

}
