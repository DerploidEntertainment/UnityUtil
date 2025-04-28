using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityUtil.DependencyInjection;

namespace UnityUtil.UI;

/// <summary>
/// Add this component to a <see cref="Canvas"/> in any scene to re-parent it to a <see cref="Canvas"/> from the DI system.
/// This allows creation of a "root" <see cref="Canvas"/> in a loading scene (with components adjusting its scaling and event handling),
/// from which <see cref="Canvas"/>es in other scenes will inherit those settings.
/// </summary>
public class RootCanvasSetter : MonoBehaviour
{
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message.")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void Awake() => DependencyInjector.Instance.ResolveDependenciesOf(this);

    public void Inject(Canvas rootCanvas) => transform.SetParent(rootCanvas.transform);
}
