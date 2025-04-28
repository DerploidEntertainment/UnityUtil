using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using UnityEngine;
using UnityUtil.DependencyInjection;
using static Microsoft.Extensions.Logging.LogLevel;
using MEL = Microsoft.Extensions.Logging;

namespace UnityUtil.UI;

[RequireComponent(typeof(RectTransform))]
public class ModifiableRectTransform : MonoBehaviour
{
    private ILogger<ModifiableRectTransform>? _logger;

    private RectTransform? _rectTransform;
    private RectTransform RectTransform => _rectTransform ??= GetComponent<RectTransform>();

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void Awake() => DependencyInjector.Instance.ResolveDependenciesOf(this);

    public void Inject(ILoggerFactory loggerFactory) => _logger = loggerFactory.CreateLogger(this);

    public void SetAnchoredPositionX(float value) { if (!hasRect()) return; Vector2 curr = RectTransform.anchoredPosition; curr.x = value; RectTransform.anchoredPosition = curr; }
    public void SetAnchoredPositionY(float value) { if (!hasRect()) return; Vector2 curr = RectTransform.anchoredPosition; curr.y = value; RectTransform.anchoredPosition = curr; }

    public void SetOffsetMaxX(float value) { if (!hasRect()) return; Vector2 curr = RectTransform.offsetMax; curr.x = value; RectTransform.offsetMax = curr; }
    public void SetOffsetMaxY(float value) { if (!hasRect()) return; Vector2 curr = RectTransform.offsetMax; curr.y = value; RectTransform.offsetMax = curr; }

    public void SetOffsetMinX(float value) { if (!hasRect()) return; Vector2 curr = RectTransform.offsetMin; curr.x = value; RectTransform.offsetMin = curr; }
    public void SetOffsetMinY(float value) { if (!hasRect()) return; Vector2 curr = RectTransform.offsetMin; curr.y = value; RectTransform.offsetMin = curr; }

    public void SetAnchoredPosition3dX(float value) { if (!hasRect()) return; Vector3 curr = RectTransform.anchoredPosition3D; curr.x = value; RectTransform.anchoredPosition3D = curr; }
    public void SetAnchoredPosition3dY(float value) { if (!hasRect()) return; Vector3 curr = RectTransform.anchoredPosition3D; curr.y = value; RectTransform.anchoredPosition3D = curr; }
    public void SetAnchoredPosition3dZ(float value) { if (!hasRect()) return; Vector3 curr = RectTransform.anchoredPosition3D; curr.z = value; RectTransform.anchoredPosition3D = curr; }

    public void SetAnchorMinX(float value) { if (!hasRect()) return; Vector2 curr = RectTransform.anchorMin; curr.x = value; RectTransform.anchorMin = curr; }
    public void SetAnchorMinY(float value) { if (!hasRect()) return; Vector2 curr = RectTransform.anchorMin; curr.y = value; RectTransform.anchorMin = curr; }

    public void SetAnchorMaxX(float value) { if (!hasRect()) return; Vector2 curr = RectTransform.anchorMax; curr.x = value; RectTransform.anchorMax = curr; }
    public void SetAnchorMaxY(float value) { if (!hasRect()) return; Vector2 curr = RectTransform.anchorMax; curr.y = value; RectTransform.anchorMax = curr; }

    public void SetPivotX(float value) { if (!hasRect()) return; Vector2 curr = RectTransform.pivot; curr.x = value; RectTransform.pivot = curr; }
    public void SetPivotY(float value) { if (!hasRect()) return; Vector2 curr = RectTransform.pivot; curr.y = value; RectTransform.pivot = curr; }

    public void SetSizeDeltaX(float value) { if (!hasRect()) return; Vector2 curr = RectTransform.sizeDelta; curr.x = value; RectTransform.sizeDelta = curr; }
    public void SetSizeDeltaY(float value) { if (!hasRect()) return; Vector2 curr = RectTransform.sizeDelta; curr.y = value; RectTransform.sizeDelta = curr; }

    public void SetLocalScaleX(float value) { if (!hasRect()) return; Vector2 curr = RectTransform.localScale; curr.x = value; RectTransform.localScale = curr; }
    public void SetLocalScaleY(float value) { if (!hasRect()) return; Vector2 curr = RectTransform.localScale; curr.y = value; RectTransform.localScale = curr; }

    private bool hasRect()
    {
        if (RectTransform == null) {
            log_NoRectTransform();
            return false;
        }
        return true;
    }

    #region LoggerMessages

    private static readonly Action<MEL.ILogger, Exception?> LOG_NO_RECTTRANSFORM_ACTION = LoggerMessage.Define(Warning,
        new EventId(id: 0, nameof(log_NoRectTransform)),
        $"Could not get the {nameof(RectTransform)} of this {nameof(ModifiableRectTransform)}. No values will be changed. Try enterring Play Mode once to correct this."
    );
    private void log_NoRectTransform() => LOG_NO_RECTTRANSFORM_ACTION(_logger!, null);

    #endregion
}
