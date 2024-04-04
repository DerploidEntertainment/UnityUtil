using UnityEditor;

namespace UnityUtil.Editor;

public static class ForceReserializeAssetsMenu
{
    /// <summary>
    /// This is mainly meant to be called after upgrading a project to a new Unity version,
    /// but see <a href="https://docs.unity3d.com/ScriptReference/AssetDatabase.ForceReserializeAssets.html">the docs</a>
    /// for other use cases and examples.
    /// </summary>
    [MenuItem("UnityUtil/Force-Reserialize All Assets (Expensive!!)")]
    public static void ForceReserializeAllAssets() => AssetDatabase.ForceReserializeAssets();
}
