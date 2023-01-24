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
    [SerializeField]
    private GameObject mainCanvas;
    public static void SetUIVisible(bool visible) => instance.UIVisible(visible);
    public UnityEngine.Audio.AudioMixerGroup soundEffectGroup;
    public UnityEngine.Audio.AudioMixerGroup soundEffectLoudGroup;
    public LayerMask precisionGrabMask;
    public LayerMask walkableGroundMask;
    public LayerMask waterSprayHitMask;
    public LayerMask plantHitMask;
    public LayerMask decalHitMask;
    public LayerMask usableHitMask;
    [SerializeField]
    private NetworkManager networkManager;
    public AnimationCurve volumeCurve;
    public GameObject selectOnPause;
    public AudioClip buttonHoveredMenu, buttonHoveredSubmenu, buttonClickedMenu, buttonClickedSubmenu;
    public LoadingListener loadListener;
    [SerializeField]
    private GameObject MultiplayerTab;
    [SerializeField]
    private GameObject OptionsTab;
    [SerializeField]
    private GameObject MainViewTab;
    [SerializeField]
    private GameObject CreditsTab;
    [SerializeField]
    private GameObject SaveTab;

    [SerializeField] private PrefabDatabase penisDatabase;

    public static PrefabDatabase GetPenisDatabase() => instance.penisDatabase;

    public static Coroutine StartCoroutineStatic(IEnumerator routine) {
        if (instance == null) {
            return null;
        }
        return instance.StartCoroutine(routine);
    }

    [HideInInspector]
    public bool isPaused = false;

    public void Pause(bool pause) {
        PopupHandler.instance.ClearAllPopups();
        if (!pause) {
            InputOptions.SaveControls();
            UnityScriptableSettings.SettingsManager.Save();
            MultiplayerTab.gameObject.SetActive(false);
            OptionsTab.gameObject.SetActive(false);
            CreditsTab.gameObject.SetActive(false);
            SaveTab.gameObject.SetActive(false);
            MainViewTab.gameObject.SetActive(true);
        }
        if (pause) {
            //OnPause.Invoke();
        }
        isPaused = pause;
        if (!isPaused && SceneManager.GetActiveScene().name != "MainMenu") {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        } else {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        mainCanvas.SetActive(isPaused || SceneManager.GetActiveScene().name == "MainMenu");

        if (!PhotonNetwork.OfflineMode || SceneManager.GetActiveScene().name == "MainMenu") {
            Time.timeScale = 1.0f;
            return;
        }
        Time.timeScale = isPaused ? 0.0f : 1.0f;
        if(selectOnPause != null)
            selectOnPause.GetComponent<Selectable>().Select();
        else
            Debug.LogError("[GameManager] selectOnPause is not bound to the resume button! Button was not selected for controller support.");
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

        ModManager.AddFinishedLoadingListener(ReloadMapIfInEditor);
        // FIXME: Photon isn't initialized early enough for scriptable objects to add themselves as a callback...
        // So I do it here-- I guess!
        PhotonNetwork.AddCallbackTarget(NetworkManager.instance);
        DontDestroyOnLoad(gameObject);
    }

    private void ReloadMapIfInEditor() {
        if (Application.isEditor && SceneManager.GetActiveScene().name != "MainMenu") {
            StartCoroutine(ReloadMapRoutine());
        }
    }

    private IEnumerator ReloadMapRoutine() {
        Debug.LogWarning("Reloading scene due to mods not being ready yet...");
        yield return LevelLoader.instance.LoadLevel("MainMap");
        NetworkManager.instance.StartSinglePlayer();
        Pause(false);
    }

    void Start() {
        SaveManager.Init();
    }
    private void UIVisible(bool visible) {
        foreach(Canvas c in GetComponentsInChildren<Canvas>()) {
            c.enabled = visible;
        }
        
        if(Camera.main != null && Camera.main.gameObject.GetComponentInChildren<Canvas>(true) != null){ //Camera isn't guaranteed to be available           
            Camera.main.gameObject.GetComponentInChildren<Canvas>(true).enabled = visible;
        }
    }

    public AudioClip SpawnAudioClipInWorld(AudioPack pack, Vector3 position) {
        GameObject g = new GameObject("One shot Audio");
        g.transform.position = position;
        AudioSource source = g.AddComponent<AudioSource>();
        source.rolloffMode = AudioRolloffMode.Custom;
        source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, volumeCurve);
        source.minDistance = 0f;
        source.maxDistance = 25f;
        source.outputAudioMixerGroup = soundEffectGroup;
        source.spatialBlend = 1f;
        source.pitch = UnityEngine.Random.Range(0.85f,1.15f);
        pack.Play(source);
        Destroy(g, source.clip.length);
        return source.clip;
    }
    
    public void QuitToMenu(){
        StartCoroutine(QuitToMenuRoutine());
    }

    private IEnumerator QuitToMenuRoutine() {
        PhotonNetwork.Disconnect();
        yield return new WaitUntil(()=>!PhotonNetwork.IsConnected);
        ObjectiveManager.GetCurrentObjective()?.Unregister();
        yield return LevelLoader.instance.LoadLevel("MainMenu");
        PhotonNetwork.OfflineMode = false;
    }

    public void SpawnAudioClipInWorld(AudioClip clip, Vector3 position, float volume = 1f, UnityEngine.Audio.AudioMixerGroup group = null) {
        if (group == null) {
            group = soundEffectGroup;
        }
        //var steamAudioSetting = UnityScriptableSettings.ScriptableSettingsManager.instance.GetSetting("SteamAudio");
        GameObject g = new GameObject("One shot Audio");
        g.transform.position = position;
        AudioSource source = g.AddComponent<AudioSource>();
        source.rolloffMode = AudioRolloffMode.Custom;
        source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, volumeCurve);
        source.minDistance = 0f;
        source.maxDistance = 25f;
        source.outputAudioMixerGroup = soundEffectGroup;
        //source.spatialize = steamAudioSetting.value > 0f;
        source.clip = clip;
        source.spatialBlend = 1f;
        source.volume = volume;
        source.pitch = UnityEngine.Random.Range(0.85f,1.15f);
        source.Play();
        Destroy(g, clip.length);
        //AudioSource.PlayClipAtPoint(clip, position, volume);
    }

    public void OnDestroy() {
        if (instance != this) {
            return;
        }
        ModManager.RemoveFinishedLoadingListener(ReloadMapIfInEditor);
        string targetString = NetworkManager.instance.settings.AppSettings.AppVersion;
        if (Application.isEditor && targetString.EndsWith("Editor")) {
            NetworkManager.instance.settings.AppSettings.AppVersion = targetString.Substring(0, targetString.Length - 6);
        }
        if (Application.isEditor && PhotonNetwork.GameVersion != null) {
            targetString = PhotonNetwork.GameVersion;
            if (targetString.EndsWith("Editor")) {
                PhotonNetwork.GameVersion = targetString.Substring(0,targetString.Length-6);
            }
        }
    }

    public void PlayUISFX(ButtonMouseOver btn, ButtonMouseOver.EventType evtType){
        if(btn.buttonType == ButtonMouseOver.ButtonTypes.Default){
            if(evtType == ButtonMouseOver.EventType.Hover)
                SpawnAudioClipInWorld(buttonHoveredMenu, Vector3.zero);
            else
                SpawnAudioClipInWorld(buttonClickedMenu, Vector3.zero); 
        }
        else if (btn.buttonType == ButtonMouseOver.ButtonTypes.Save){
            if(evtType == ButtonMouseOver.EventType.Hover)
                SpawnAudioClipInWorld(buttonHoveredSubmenu, Vector3.zero);
            else
                SpawnAudioClipInWorld(buttonClickedSubmenu, Vector3.zero);
        }
    }
}
