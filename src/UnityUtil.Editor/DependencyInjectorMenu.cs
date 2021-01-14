using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.DependencyInjection;

namespace UnityUtil.Editor
{
    public static class DependencyInjectorMenu
    {
        public const string ItemName = nameof(UnityUtil) + "/Record dependency resolutions";

        [MenuItem(ItemName)]
        private static void toggleRecording()
        {
            DependencyResolutionCounts counts = null;
            DependencyInjector.Instance.GetServiceResolutionCounts(ref counts);
            DependencyInjector.Instance.RecordingResolutions = !DependencyInjector.Instance.RecordingResolutions;
            if (DependencyInjector.Instance.RecordingResolutions)
                return;

            Debug.Log($@"
Uncached dependency resolution counts:
(If any of these counts are greater than 1, consider caching resolutions for that Type on the {nameof(DependencyInjector)} to improve performance)
{getCountLines(counts.Uncached)}

Cached dependency resolution counts:
(If any of these counts equal 1, consider NOT caching resolutions for that Type on the {nameof(DependencyInjector)}, to speed up its resolutions and save memory)
{getCountLines(counts.Cached)}
            ");

            static string getCountLines(IEnumerable<KeyValuePair<Type, int>> counts) => string.Join(
                Environment.NewLine,
                counts.OrderByDescending(x => x.Value).Select(x => $"    {x.Key.FullName}: {x.Value}")
            );
        }

        [MenuItem(ItemName, isValidateFunction: true)]
        private static bool canToggleRecording()
        {
            Menu.SetChecked(ItemName, DependencyInjector.Instance.RecordingResolutions);
            return true;
        }

    }
}
