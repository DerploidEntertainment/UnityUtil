using UnityEngine;

namespace UnityUtil;

/// <summary>
/// Do not destroy the attached GameObject when loading a new scene.
/// </summary>
public class DontDestroyOnLoad : MonoBehaviour
{
    private void Awake() => DontDestroyOnLoad(gameObject);

}
