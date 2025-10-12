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
    
    private static AnimationCurve audioFalloff = new() {keys=new Keyframe[] { new (0f, 1f, 0, -3.1f), new (1f, 0f, 0f, 0f) } };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Init() {
        audioFalloff = new() {keys=new Keyframe[] { new (0f, 1f, 0, -3.1f), new (1f, 0f, 0f, 0f) } };
    }
    public AudioClip GetClip() {
        return clips[Random.Range(0, clips.Length)];
    }

    public float GetVolume() {
        return volume;
    }

    public AudioClip Play(AudioSource source, float vol = 1f) {
        source.outputAudioMixerGroup = group;
        var clip = clips[Random.Range(0, clips.Length)];
        source.clip = clip;
        source.volume = volume*vol;
        source.pitch = Random.Range(1f-pitchRange,1f+pitchRange);
        source.Play();
        return clip;
    }
    public void PlayOneShot(AudioSource source) {
        source.outputAudioMixerGroup = group;
        source.pitch = Random.Range(1f-pitchRange,1f+pitchRange);
        source.PlayOneShot(clips[Random.Range(0, clips.Length)], volume);
    }
    public static AudioSource PlayClipAtPoint(AudioPack pack, Vector3 position, float volume = 1f) {
        GameObject obj = new GameObject("OneOffSoundEffect", typeof(AudioSource)) {
            transform = {
                position = position
            }
        };
        if (!obj.TryGetComponent(out AudioSource source)) {
            throw new UnityException("No AudioSource component found on oneoff");
        }
        
        source.spatialBlend = 1f;
        source.minDistance = 1f;
        source.maxDistance = 25f;

        source.rolloffMode = AudioRolloffMode.Custom;
        if (audioFalloff != null) {
            source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, audioFalloff);
        }
        var clip = pack.Play(source, volume);
        if (clip) {
            Destroy(obj, clip.length + 0.1f);
        } else {
            Destroy(obj, 1f);
        }
        return source;
    }
}
