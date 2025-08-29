using UnityEngine;

namespace UnityUtil;

[RequireComponent(typeof(Animator))]
public class AnimatorStateRestarter : MonoBehaviour
{
    private Animator? _animator;

    private void Awake() => _animator = GetComponent<Animator>();

    public void RestartCurrentState() => _animator!.Play(getCurrentAnimStateHash(layerIndex: 0), layer: 0, normalizedTime: 0f);
    public void RestartCurrentState(int layer) => _animator!.Play(getCurrentAnimStateHash(layer), layer, normalizedTime: 0f);
    public void ResetCurrentStateToTime(float normalizedTime) => _animator!.Play(getCurrentAnimStateHash(layerIndex: 0), layer: 0, normalizedTime);
    public void ResetCurrentStateToTime(float normalizedTime, int layer) => _animator!.Play(getCurrentAnimStateHash(layer), layer, normalizedTime);

    private int getCurrentAnimStateHash(int layerIndex) => _animator!.GetCurrentAnimatorStateInfo(layerIndex).shortNameHash;
}
