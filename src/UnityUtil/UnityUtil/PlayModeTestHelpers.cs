using UnityEngine;

namespace UnityUtil;

public static class PlayModeTestHelpers
{

    public static void ResetScene()
    {
        Transform[] transforms = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
        for (int t = 0; t < transforms.Length; ++t)
            Object.Destroy(transforms[t].gameObject);
    }

}
