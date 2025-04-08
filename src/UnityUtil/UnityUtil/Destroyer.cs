using UnityEngine;

namespace UnityUtil;

[CreateAssetMenu(menuName = $"{nameof(UnityUtil)}/{nameof(Destroyer)}", fileName = "destroyer")]
public class Destroyer : ScriptableObject
{
    [Tooltip($"Only relevant when calling {nameof(Destroy)}")]
    public float TimeBeforeDeletion;

    [Tooltip($"Only relevant when calling {nameof(DestroyImmediate)}")]
    public bool AllowDestroyingAssets;

    public new void Destroy(Object obj) => Destroy(obj, TimeBeforeDeletion);

    public new void DestroyImmediate(Object obj) => DestroyImmediate(obj, AllowDestroyingAssets);

}
