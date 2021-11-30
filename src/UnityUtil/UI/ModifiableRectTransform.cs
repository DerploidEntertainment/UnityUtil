namespace UnityEngine.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class ModifiableRectTransform : MonoBehaviour {

        private RectTransform? _rectTransform;
        private RectTransform RectTransform => _rectTransform ??= GetComponent<RectTransform>();

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

        private bool hasRect()
        {
            if (RectTransform is null) {
                Debug.LogWarning($"Could not get the {nameof(RectTransform)} of this {nameof(ModifiableRectTransform)}. No values will be changed. Try enterring Play Mode once to correct this.");
                return false;
            }
            return true;
        }

    }
}
