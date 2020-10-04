using UnityEditor;
using UnityEngine.DependencyInjection;

namespace UnityUtil.Editor
{
    public static class DependencyInjectorMenu
    {
        public const string ItemName = nameof(UnityUtil) + "/Record dependency resolutions";

        [MenuItem(ItemName)]
        public static void ToggleRecording()
        {
            DependencyInjector.Instance.ToggleDependencyResolutionRecording();
        }

        [MenuItem(ItemName, isValidateFunction: true)]
        public static bool ToggleRecording_Validate()
        {
            Menu.SetChecked(ItemName, DependencyInjector.Instance.RecordingResolutions);
            return true;
        }

    }
}
