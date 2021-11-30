using System.Collections.Generic;

namespace UnityEngine {

    [RequireComponent(typeof(Collider))]
    public class CollisionSounds : MonoBehaviour
    {
        private int _clip = -1;

        public AudioSource AudioSource;
        public bool RandomizeClips;
        public List<AudioClip> AudioClips = new();

        private void OnCollisionEnter(Collision collision) {
            if (AudioClips.Count == 0)
                return;

            // Play the next clip
            int clip = nextClip();
            AudioSource.clip=AudioClips[clip];
            AudioSource.Play();
        }

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
