using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityUtil.Physics;

[RequireComponent(typeof(Collider))]
public class CollisionSounds : MonoBehaviour
{
    private int _clip = -1;

    [RequiredIn(PrefabKind.PrefabInstanceAndNonPrefabInstance)]
    public AudioSource? AudioSource;
    public bool RandomizeClips;
    public AudioClip[] AudioClips = [];

    private void OnCollisionEnter(Collision collision)
    {
        if (AudioClips.Length == 0)
            return;

        // Play the next clip
        int clip = nextClip();
        AudioSource!.clip = AudioClips[clip];
        AudioSource.Play();
    }

    /// <summary>
    /// Get the next AudioClip to be played (random or in order)
    /// </summary>
    /// <returns></returns>
    private int nextClip() => _clip = RandomizeClips ? Random.Range(0, AudioClips.Length) : (_clip + 1) % AudioClips.Length;

}
