using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace UnityUtil.UI;

public class CursorManager
{
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "UnityEvents can't call static methods")]
    public void SetCursorConfined() => Cursor.lockState = CursorLockMode.Confined;

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "UnityEvents can't call static methods")]
    public void SetCursorLocked() => Cursor.lockState = CursorLockMode.Locked;

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "UnityEvents can't call static methods")]
    public void SetCursorUnlocked() => Cursor.lockState = CursorLockMode.None;
}
