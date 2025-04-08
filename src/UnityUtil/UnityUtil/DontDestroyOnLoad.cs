using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace UnityUtil;

public class DontDestroyOnLoad : MonoBehaviour
{
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void Awake() => DontDestroyOnLoad(gameObject);

}
