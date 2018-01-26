using UnityEngine;
using UnityEngine.Events;

namespace UnityUtil.Input {

    [RequireComponent(typeof(Collider2D))]
    public class Interactable2D : MonoBehaviour {
        
        public UnityEvent Interacting = new UnityEvent();

        public void Interact() => Interacting.Invoke();

    }

}
