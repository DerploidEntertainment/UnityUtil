using UnityEngine;

using System.Collections.Generic;

namespace Danware.Unity3D {

    [RequireComponent(typeof(Collider))]
    public class CollisionSounds : MonoBehaviour {
        // HIDDEN FIELDS
        private int _clip = -1;

        // INSPECTOR FIELDS
        public AudioSource AudioSource;
        public bool RandomizeClips;
        public List<AudioClip> AudioClips;

        // EVENT HANDLERS
        private void Awake() { }
        private void OnCollisionEnter(Collision collision) {
            if (AudioClips.Count == 0)
                return;

            // Play the next clip
            int clip = nextClip();
            AudioSource.clip=AudioClips[clip];
            AudioSource.Play();
        }

        // HIDDEN FUNCTIONS
        private int nextClip() {
            // Get the next AudioClip to be played (random or in order)
            if (RandomizeClips)
                _clip = Random.Range(0, AudioClips.Count);
            else
                _clip = (_clip + 1) % AudioClips.Count;

            return _clip;
        }

    }

}
