using System.Diagnostics.CodeAnalysis;

namespace UnityEngine {

    [RequireComponent(typeof(Animator))]
    public class AnimatorStateRestarter : MonoBehaviour {

        private Animator _animator;

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
        private void Awake() => _animator = GetComponent<Animator>();

        private int CurrentAnimStateHash => _animator.GetCurrentAnimatorStateInfo(layerIndex: 0).shortNameHash; 

        public void RestartCurrentState() => _animator.Play(CurrentAnimStateHash, layer: -1, normalizedTime: 0f);
        public void ResetCurrentStateToTime(float normalizedTime) => _animator.Play(CurrentAnimStateHash, layer: -1, normalizedTime);

    }
}
