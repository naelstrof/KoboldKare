using UnityEngine;
using System.Collections;
using System.Collections.Generic;       //Allows us to use Lists. 
using TMPro;
using KoboldKare;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using UnityEngine.Localization.Components;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.Events;

public class GameManager : MonoBehaviour {
    public static GameManager instance = null;              //Static instance of GameManager which allows it to be accessed by any other script.
    public PaintDecal decalPainter;
    public UnityEngine.Audio.AudioMixerGroup soundEffectGroup;
    public UnityEngine.Audio.AudioMixerGroup soundEffectLoudGroup;
    public LayerMask precisionGrabMask;
    public LayerMask walkableGroundMask;
    public LayerMask waterSprayHitMask;
    public LayerMask decalHitMask;
    public UnityEvent OnPause;
    public UnityEvent OnUnpause;
    [HideInInspector]
    public bool isPaused = false;

    public void Pause(bool pause) {
        if (!pause) {
            OnUnpause.Invoke();
        }
        if (pause) {
            OnPause.Invoke();
        }
        isPaused = pause;

        if (!PhotonNetwork.OfflineMode) {
            return;
        }
        Time.timeScale = isPaused ? 0.0f : 1.0f;
    }

    public void Quit() {
#if UNITY_EDITOR
        // Application.Quit() does not work in the editor so
        // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }

    //Awake is always called before any Start functions
    void Awake() {
        //Check if instance already exists
        if (instance == null) {
            //if not, set instance to this
            instance = this;
        } else if (instance != this) { //If instance already exists and it's not this:
            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);
            return;
        }
        // FIXME: Photon isn't initialized early enough for scriptable objects to add themselves as a callback...
        // So I do it here-- I guess!
        PhotonNetwork.AddCallbackTarget(NetworkManager.instance);
        DontDestroyOnLoad(gameObject);
    }

    void Start() {
        if (Application.isEditor && SceneManager.GetActiveScene().name != "MainMenu") {
            NetworkManager.instance.StartSinglePlayer();
            GameManager.instance.Pause(false);
        }
    }
    //public void OnDestroy() {
        //PhotonNetwork.RemoveCallbackTarget(networkManager);
    //}

    public void SpawnDecalInWorld(Material decalMat, Vector3 position, Vector3 normal, Vector2 size, Color color, GameObject obj, float depth = 0.5f, bool ignoreBackface = true, bool randomRotation = true, bool subtractive = false) {
        Quaternion rot = Quaternion.FromToRotation(Vector3.forward,normal);
        if (randomRotation) {
            rot = rot * Quaternion.AngleAxis(UnityEngine.Random.Range(0f,360f), Vector3.forward);
        } 
        LODGroup g = obj.GetComponentInParent<LODGroup>();
        if (g != null) {
            var lods = g.GetLODs();
            foreach (var lod in lods) {
                foreach (Renderer ren in lod.renderers) {
                    if (ren != null && ren.gameObject.activeInHierarchy) {
                        foreach (var mat in ren.sharedMaterials) {
                            if (decalPainter.IsDecalable(mat)) {
                                decalPainter.RenderDecal(ren, decalMat.GetTexture("_BaseMap"), position, rot, new Color(color.r, color.g, color.b, color.a), size / 2f, depth, false, ignoreBackface, subtractive);
                                break;
                            }
                        }
                    }
                }
            }
            return;
        }
        foreach (Renderer r in obj.GetComponentsInChildren<Renderer>()) {
            foreach (var mat in r.sharedMaterials) {
                if (decalPainter.IsDecalable(mat)) {
                    decalPainter.RenderDecal(r, decalMat.GetTexture("_BaseMap"), position, rot, new Color(color.r, color.g, color.b, color.a), size / 2f, depth, false, ignoreBackface, subtractive);
                    break;
                }
            }
        }
    }
    public void SpawnAudioClipInWorld(AudioClip clip, Vector3 position, float volume = 1f, UnityEngine.Audio.AudioMixerGroup group = null) {
        if (group == null) {
            group = soundEffectGroup;
        }
        GameObject g = new GameObject("One shot Audio");
        g.transform.position = position;
        AudioSource source = g.AddComponent<AudioSource>();
        source.outputAudioMixerGroup = soundEffectGroup;
        source.spatialize = true;
        source.clip = clip;
        source.spatialBlend = 1f;
        source.volume = volume;
        source.pitch = UnityEngine.Random.Range(0.85f,1.15f);
        source.Play();
        Destroy(g, clip.length);
        //AudioSource.PlayClipAtPoint(clip, position, volume);
    }
}