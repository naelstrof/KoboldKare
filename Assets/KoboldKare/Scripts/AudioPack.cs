using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "NewAudioPack", menuName = "Data/AudioPack")]
public class AudioPack : ScriptableObject {
    [SerializeField]
    private AudioClip[] clips;
    [SerializeField]
    private float volume = 1f;
    [SerializeField]
    private AudioMixerGroup group;
    [SerializeField]
    private float pitchRange = 0.2f;
    //public AudioClip GetRandomClip() {
        //return clips[Random.Range(0, clips.Length)];
    //}
    public AudioClip GetClip() {
        return clips[Random.Range(0, clips.Length)];
    }

    public float GetVolume() {
        return volume;
    }

    public void Play(AudioSource source) {
        source.outputAudioMixerGroup = group;
        source.clip = clips[Random.Range(0, clips.Length)];
        source.volume = volume;
        source.pitch = Random.Range(1f-pitchRange,1f+pitchRange);
        source.Play();
    }
    public void PlayOneShot(AudioSource source) {
        source.outputAudioMixerGroup = group;
        source.pitch = Random.Range(1f-pitchRange,1f+pitchRange);
        source.PlayOneShot(clips[Random.Range(0, clips.Length)], volume);
    }
}
