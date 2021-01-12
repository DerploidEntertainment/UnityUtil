using Sirenix.OdinInspector;
using System;
using UnityEngine.Logging;

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

        #pragma warning disable CA2235 // Mark all non-serializable fields

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

        #pragma warning restore CA2235 // Mark all non-serializable fields

        public object GetValue() =>
            Type switch {
                ConfigValueType.Integer => IntValue,
                ConfigValueType.Boolean => BoolValue,
                ConfigValueType.Float => FloatValue,
                ConfigValueType.String => StringValue,
                ConfigValueType.Color => ColorValue,
                ConfigValueType.ObjectReference => ObjectValue,
                ConfigValueType.LayerMask => LayerMaskValue,
                ConfigValueType.Vector2 => Vector2Value,
                ConfigValueType.Vector3 => Vector3Value,
                ConfigValueType.Vector4 => Vector4Value,
                ConfigValueType.Rect => RectValue,
                ConfigValueType.AnimationCurve => AnimationCurveValue,
                ConfigValueType.Bounds => BoundsValue,
                ConfigValueType.Gradient => GradientValue,
                ConfigValueType.Quaternion => QuaternionValue,
                ConfigValueType.Vector2Int => Vector2IntValue,
                ConfigValueType.Vector3Int => Vector3IntValue,
                ConfigValueType.RectInt => RectIntValue,
                ConfigValueType.BoundsInt => BoundsIntValue,
                _ => throw UnityObjectExtensions.SwitchDefaultException(Type),
            };
    }

    [CreateAssetMenu(menuName = nameof(UnityUtil) + "/" + "Configuration" + "/" + nameof(ConfigObject), fileName = "appsettings.asset")]
    public class ConfigObject : ScriptableObject {
        public Config[] Configs;
    }

}
