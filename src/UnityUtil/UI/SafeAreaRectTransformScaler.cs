using Sirenix.OdinInspector;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.DependencyInjection;
using UnityEngine.Logging;

namespace UnityEngine.UI
{
    public class SafeAreaRectTransformScaler : MonoBehaviour
    {
        private ILogger _logger;

        [Required]
        public RectTransform? RectTransform;

        public void Inject(ILoggerProvider loggerProvider) => _logger = loggerProvider.GetLogger(this);

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
        private void Awake()
        {
            DependencyInjector.Instance.ResolveDependenciesOf(this);

            Rect safeArea = Device.Screen.safeArea;
            _logger.Log(
                $"Current anchors of {RectTransform.GetHierarchyNameWithType()} (min, max): ({RectTransform.anchorMin}, {RectTransform.anchorMax}). " +
                $"Updating for current screen (width x height) = ({Device.Screen.width} x {Device.Screen.height}) and safe area (width x height) = ({safeArea.width} x {safeArea.height})"
            , context: this);

            // Calculations inspired by this article: https://connect.unity.com/p/updating-your-gui-for-the-iphone-x-and-other-notched-devices
            var scaleVect = new Vector2(1f / Device.Screen.width, 1f / Device.Screen.height);
            RectTransform.anchorMin = safeArea.position * scaleVect;
            RectTransform.anchorMax = (safeArea.position + safeArea.size) * scaleVect;

            _logger.Log($"New anchors of {RectTransform.GetHierarchyNameWithType()} (min, max): ({RectTransform.anchorMin}, {RectTransform.anchorMax})");
        }
    }
}
