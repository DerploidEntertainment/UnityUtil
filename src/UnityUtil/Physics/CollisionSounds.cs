using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace UnityEngine {

    [RequireComponent(typeof(Collider))]
    public class CollisionSounds : MonoBehaviour
    {
        private int _clip = -1;

        [Required]
        public AudioSource? AudioSource;
        public bool RandomizeClips;
        public List<AudioClip> AudioClips = new();

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Unity message")]
        private void OnCollisionEnter(Collision collision) {
            if (AudioClips.Count == 0)
                return;

            // Play the next clip
            int clip = nextClip();
            AudioSource.clip=AudioClips[clip];
            AudioSource.Play();
        }

        /// <summary>
        /// Get the next AudioClip to be played (random or in order)
        /// </summary>
        /// <returns></returns>
        private int nextClip() => _clip = RandomizeClips ? Random.Range(0, AudioClips.Count) : (_clip + 1) % AudioClips.Count;

    }

}
