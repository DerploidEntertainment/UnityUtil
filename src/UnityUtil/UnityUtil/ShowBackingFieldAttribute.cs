using System;
using Sirenix.OdinInspector;

namespace UnityUtil;

/// <summary>
/// Force Unity to serialize the backing field of a C# auto-implemented property.
/// Consuming properties should be declared as follows
/// (note that both a getter and a setter are required for Unity to recognize the backing field):
/// <code>
/// [field: SerializeBackingField]
/// public SomeType SomeProperty { get; private set; }
/// </code>
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
[IncludeMyAttributes]
[ShowInInspector, LabelText("@$property.NiceName")]
public class ShowBackingFieldAttribute : Attribute { }
