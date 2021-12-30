using Sirenix.OdinInspector;
using System;
using System.Diagnostics.CodeAnalysis;

namespace UnityEngine {

    [SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "The point of these members is to be type names...")]
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

        public string Key = "";
        public ConfigValueType Type;

        [ShowIf(nameof(Type), ConfigValueType.Integer)]
        public int IntValue;

        [ShowIf(nameof(Type), ConfigValueType.Boolean)]
        public bool BoolValue;

        [ShowIf(nameof(Type), ConfigValueType.Float)]
        public float FloatValue;

        [ShowIf(nameof(Type), ConfigValueType.String)]
        public string StringValue = "";

        [ShowIf(nameof(Type), ConfigValueType.Color)]
        public Color ColorValue;

        [ShowIf(nameof(Type), ConfigValueType.ObjectReference)]
        public Object? ObjectValue;

        [ShowIf(nameof(Type), ConfigValueType.LayerMask)]
        public LayerMask LayerMaskValue;

        [ShowIf(nameof(Type), ConfigValueType.Vector2)]
        public Vector2 Vector2Value;

        [ShowIf(nameof(Type), ConfigValueType.Vector3)]
        public Vector3 Vector3Value;

        [ShowIf(nameof(Type), ConfigValueType.Vector4)]
        public Vector4 Vector4Value;

        [ShowIf(nameof(Type), ConfigValueType.Rect)]
        public Rect RectValue;

        [ShowIf(nameof(Type), ConfigValueType.AnimationCurve)]
        public AnimationCurve? AnimationCurveValue;

        [ShowIf(nameof(Type), ConfigValueType.Bounds)]
        public Bounds BoundsValue;

        [ShowIf(nameof(Type), ConfigValueType.Gradient)]
        public Gradient? GradientValue;

        [ShowIf(nameof(Type), ConfigValueType.Quaternion)]
        public Quaternion QuaternionValue;

        [ShowIf(nameof(Type), ConfigValueType.Vector2Int)]
        public Vector2Int Vector2IntValue;

        [ShowIf(nameof(Type), ConfigValueType.Vector3Int)]
        public Vector3Int Vector3IntValue;

        [ShowIf(nameof(Type), ConfigValueType.RectInt)]
        public RectInt RectIntValue;

        [ShowIf(nameof(Type), ConfigValueType.BoundsInt)]
        public BoundsInt BoundsIntValue;

        #pragma warning restore CA2235 // Mark all non-serializable fields

        public object GetValue()
        {
            object? val = Type switch {
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

            return val ?? throw new InvalidOperationException($"{nameof(Type)} was set to '{Type}' but no value was provided");
        }
    }

    [CreateAssetMenu(menuName = $"{nameof(UnityUtil)}/Configuration/{nameof(ConfigObject)}", fileName = "appsettings.asset")]
    public class ConfigObject : ScriptableObject {
        public Config[] Configs = Array.Empty<Config>();
    }

}
