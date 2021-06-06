using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioReverbData: IComparable<AudioReverbData> {
    public AudioReverbData() {
    }
    public AudioReverbData(AudioReverbData a) {
        hfReference = a.hfReference;
        density = a.density;
        diffusion = a.diffusion;
        reverbDelay = a.reverbDelay;
        reverb = a.reverb;
        reflectDelay = a.reflectDelay;
        reflections = a.reflections;
        decayHFRatio = a.decayHFRatio;
        decayTime = a.decayTime;
        roomHF = a.roomHF;
        room = a.room;
        roomLF = a.roomLF;
        lfReference = a.lfReference;
    }
    public AudioReverbData(AudioReverbZone a) {
        hfReference = a.HFReference;
        density = a.density;
        diffusion = a.diffusion;
        reverbDelay = a.reverbDelay;
        reverb = a.reverb;
        reflectDelay = a.reflectionsDelay;
        reflections = a.reflections;
        decayHFRatio = a.decayHFRatio;
        decayTime = a.decayTime;
        roomHF = a.roomHF;
        room = a.room;
        roomLF = a.roomLF;
        lfReference = a.LFReference;
    }
    public int priority;
    public Collider shape;
    public float fadeDistance;
    public float hfReference;
    public float density;
    public float diffusion;
    public float reverbDelay;
    public float reverb;
    public float reflectDelay;
    public float reflections;
    public float decayHFRatio;
    public float decayTime;
    public float roomHF;
    public float room;
    public float roomLF;
    public float lfReference;
    private AudioReverbData alloc;
    public static AudioReverbData Lerp(AudioReverbData a, AudioReverbData b, float t) {
        a.hfReference = Mathf.Lerp(a.hfReference, b.hfReference, t);
        a.density = Mathf.Lerp(a.density, b.density, t);
        a.diffusion = Mathf.Lerp(a.diffusion, b.diffusion, t);
        a.reverbDelay = Mathf.Lerp(a.reverbDelay, b.reverbDelay,t);
        a.reverb = Mathf.Lerp(a.reverb, b.reverb,t);
        a.reflectDelay = Mathf.Lerp(a.reflectDelay, b.reflectDelay,t);
        a.reflections = Mathf.Lerp(a.reflections, b.reflections,t);
        a.decayHFRatio = Mathf.Lerp(a.decayHFRatio, b.decayHFRatio,t);
        a.decayTime = Mathf.Lerp(a.decayTime, b.decayTime,t);
        a.roomHF = Mathf.Lerp(a.roomHF, b.roomHF,t);
        a.room = Mathf.Lerp(a.room, b.room,t);
        a.roomLF = Mathf.Lerp(a.roomLF, b.roomLF,t);
        a.lfReference = Mathf.Lerp(a.lfReference, b.lfReference,t);
        return a;
    }

    public int CompareTo(AudioReverbData other) {
        return priority.CompareTo(other.priority);
    }
}
public class AudioReverbConsumer : MonoBehaviour {
    public LayerMask reverbLayer;
    public AudioMixer target;
    public AudioReverbData defaultSettings;
    public AudioReverbData data;
    public AudioReverbData[] nearbyAudioReverb = new AudioReverbData[4];
    private Camera attachedCamera;
    private AudioListener listener;
    private void Start() {
        attachedCamera = GetComponent<Camera>();
        defaultSettings = new AudioReverbData();
        target.GetFloat("HF Reference", out defaultSettings.hfReference);
        target.GetFloat("Density", out defaultSettings.density);
        target.GetFloat("Diffusion", out defaultSettings.diffusion);
        target.GetFloat("Reverb Delay", out defaultSettings.reverbDelay);
        target.GetFloat("Reflections", out defaultSettings.reflections);
        target.GetFloat("Decay HF Ratio", out defaultSettings.decayHFRatio);
        target.GetFloat("Decay Time", out defaultSettings.decayTime);
        target.GetFloat("Room HF", out defaultSettings.roomHF);
        target.GetFloat("Room", out defaultSettings.room);
        target.GetFloat("Room LF", out defaultSettings.roomLF);
        target.GetFloat("LF Reference", out defaultSettings.lfReference);
        data = new AudioReverbData(defaultSettings);
        listener = GetComponent<AudioListener>();
    }
    Collider[] colliders = new Collider[4];
    void Update() {
        if (!attachedCamera.isActiveAndEnabled || !isActiveAndEnabled || listener.isActiveAndEnabled) {
            return;
        }
        data.hfReference = defaultSettings.hfReference;
        data.density = defaultSettings.density;
        data.diffusion = defaultSettings.diffusion;
        data.reverbDelay = defaultSettings.reverbDelay;
        data.reflections = defaultSettings.reflections;
        data.decayHFRatio = defaultSettings.decayHFRatio;
        data.decayTime = defaultSettings.decayTime;
        data.roomLF = defaultSettings.roomLF;
        data.room = defaultSettings.room;
        data.roomHF = defaultSettings.roomHF;
        data.lfReference = defaultSettings.lfReference;

        //List<AudioReverbData> l = new List<AudioReverbData>();
        int hits = Physics.OverlapSphereNonAlloc(transform.position, 10f, colliders, reverbLayer, QueryTriggerInteraction.Collide);
        for (int i=0;i<hits;i++) {
            Collider c = colliders[i];
            if (c == null) {
                break;
            }
            AudioReverbArea d = c.GetComponentInParent<AudioReverbArea>();
            if (d != null) {
                nearbyAudioReverb[i] = d.data;
            }
        }
        Array.Sort(nearbyAudioReverb, 0, hits);
        for (int i=0;i<hits;i++) {
            AudioReverbData d = nearbyAudioReverb[i];
            Vector3 closestPoint = d.shape.ClosestPoint(transform.position);
            float dist = Vector3.Distance(closestPoint, transform.position);
            AudioReverbData.Lerp(data, d, Mathf.Clamp01((d.fadeDistance-dist)/d.fadeDistance));
        }
        target.SetFloat("HF Reference", data.hfReference);
        target.SetFloat("Density", data.density);
        target.SetFloat("Diffusion", data.diffusion);
        target.SetFloat("Reverb Delay", data.reverbDelay);
        target.SetFloat("Reverb", data.reverb);
        target.SetFloat("Reflect Delay", data.reflectDelay);
        target.SetFloat("Reflections", data.reflections);
        target.SetFloat("Decay HF Ratio", data.decayHFRatio);
        target.SetFloat("Decay Time", data.decayTime);
        target.SetFloat("Room HF", data.roomHF);
        target.SetFloat("Room", data.room);
        //target.SetFloat("Dry Level", data);
        target.SetFloat("Room LF", data.roomLF);
        target.SetFloat("LF Reference", data.lfReference);
    }
}
