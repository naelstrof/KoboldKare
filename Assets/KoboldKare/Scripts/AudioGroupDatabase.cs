using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

public class AudioGroupDatabase : MonoBehaviour {
    private static AudioGroupDatabase instance;
    
    [SerializeField]
    private AudioMixerGroup defaultMixer;
    
    [SerializeField]
    private List<AudioMixerGroup> mixerGroups;

    private void Start() {
        if (instance != null) {
            Destroy(this);
            return;
        }
        instance = this;
    }

    public static AudioMixerGroup GetMixerGroup(string name) {
        foreach(var group in instance.mixerGroups) {
            if (group.name == name) {
                return group;
            }
        }
        return instance.defaultMixer;
    }
}
