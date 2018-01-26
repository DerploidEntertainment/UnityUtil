using UnityEngine;
using UnityEngine.Events;

namespace UnityUtil.Input {

    [RequireComponent(typeof(Collider))]
    public class Interactable : MonoBehaviour {
        
        public UnityEvent Interacting = new UnityEvent();

        public void Interact() => Interacting.Invoke();

    }

}
