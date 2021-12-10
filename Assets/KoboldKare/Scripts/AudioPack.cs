using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "NewAudioPack", menuName = "Data/AudioPack")]
public class AudioPack : ScriptableObject {
    public AudioClip[] clips;
    public float volume = 1f;
    public AudioClip GetRandomClip() {
        return clips[Random.Range(0, clips.Length)];
    }

}
