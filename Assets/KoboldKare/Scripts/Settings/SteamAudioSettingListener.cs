using System.Collections;
using System.Collections.Generic;
using SteamAudio;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering.Universal;


namespace UnityScriptableSettings {

public class SteamAudioSettingListener : MonoBehaviour {
    public ScriptableSetting steamAudio;
    public ScriptableSetting steamAudioAccel;
    private SteamAudio.SteamAudioManager manager;
    public AudioMixer mixer;
    private SteamAudio.SteamAudioCustomSettings customSettings;
    void Start() {
        manager = GetComponent<SteamAudio.SteamAudioManager>();
        customSettings = GetComponent<SteamAudio.SteamAudioCustomSettings>();
        steamAudio.onValueChange += OnValueChanged;
        steamAudioAccel.onValueChange += OnValueChanged;
        OnValueChanged(steamAudio);
        OnValueChanged(steamAudioAccel);
    }
    public void OnValueChanged(ScriptableSetting option) {
        if (manager == null) {
            return;
        }
        if (option == steamAudio) {
            manager.enabled = option.value > 0;
            mixer.SetFloat("ReverbVolume", option.value > 0f ? 0f:float.MinValue);
            switch(Mathf.RoundToInt(option.value)) {
                case 1: manager.simulationPreset = SteamAudio.SimulationSettingsPreset.Low; break;
                case 2: manager.simulationPreset = SteamAudio.SimulationSettingsPreset.Medium; break;
                case 3: manager.simulationPreset = SteamAudio.SimulationSettingsPreset.High; break;
            }
            foreach(AudioSource a in Object.FindObjectsOfType<AudioSource>(true)) {
                a.spatialize = option.value > 0f;
            }
            foreach(SteamAudioSource s in Object.FindObjectsOfType<SteamAudioSource>(true)) {
                s.enabled = option.value > 0f;
            }
        }
        if (option == steamAudioAccel) {
            if (customSettings != null) {
                customSettings.rayTracerOption = option.value != 0 ? SteamAudio.SceneType.RadeonRays : SteamAudio.SceneType.Phonon;
                //customSettings.convolutionOption = option.value != 0 ? SteamAudio.ConvolutionOption.TrueAudioNext : SteamAudio.ConvolutionOption.Phonon;
            }
        }
        if (manager.enabled) {
            manager.Destroy();
            manager.Initialize(SteamAudio.GameEngineStateInitReason.Playing);
        }
    }
}
}