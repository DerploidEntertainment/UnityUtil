using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityUtil;

[CreateAssetMenu(menuName = $"{nameof(UnityUtil)}/{nameof(AppEnvironment)}", fileName = "environment")]
public class AppEnvironment : ScriptableObject, IAppEnvironment
{
    [field: SerializeField, ShowInInspector, LabelText(nameof(Name))]
    [field: Tooltip("This can be any string to describe the environment, but will usually be a single word like 'production' or 'BETA'.")]
    public string Name { get; set; } = "production";
}
