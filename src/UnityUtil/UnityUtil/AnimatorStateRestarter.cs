using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace UnityUtil;

[RequireComponent(typeof(Animator))]
public class AnimatorStateRestarter : MonoBehaviour
{
    private Animator? _animator;

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void Awake() => _animator = GetComponent<Animator>();

    private int GetCurrentAnimStateHash(int layerIndex) => _animator!.GetCurrentAnimatorStateInfo(layerIndex).shortNameHash;

    public void RestartCurrentState() => _animator!.Play(GetCurrentAnimStateHash(layerIndex: 0), layer: 0, normalizedTime: 0f);
    public void RestartCurrentState(int layer) => _animator!.Play(GetCurrentAnimStateHash(layer), layer, normalizedTime: 0f);
    public void ResetCurrentStateToTime(float normalizedTime) => _animator!.Play(GetCurrentAnimStateHash(layerIndex: 0), layer: 0, normalizedTime);
    public void ResetCurrentStateToTime(float normalizedTime, int layer) => _animator!.Play(GetCurrentAnimStateHash(layer), layer, normalizedTime);
}
