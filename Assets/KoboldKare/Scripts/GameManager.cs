using System;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.AddressableAssets;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
    public AudioPack buttonHovered, buttonClicked;

    [SerializeField] private PrefabDatabase penisDatabase;
    [SerializeField] private PrefabDatabase playerDatabase;
    private bool reloadedSceneAlready = false;

    public static PrefabDatabase GetPenisDatabase() => instance.penisDatabase;
    public static PrefabDatabase GetPlayerDatabase() => instance.playerDatabase;

    public static Coroutine StartCoroutineStatic(IEnumerator routine) {
        if (instance == null) {
            return null;
        }
        return instance.StartCoroutine(routine);
    }
    public static void StopCoroutineStatic(Coroutine routine) {
        if (instance == null) {
            return;
        }
        instance.StopCoroutine(routine);
    }
    
    #if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod]
        private static void OnInitialize() {
            // No gamemanager found! Spawn one.
            if (FindObjectOfType<GameManager>() != null) return;
            var path = AssetDatabase.GUIDToAssetPath("364d21a5e4c0c464784d42f01767a083");
            GameObject freshGameManager = Instantiate( AssetDatabase.LoadAssetAtPath<GameObject>(path));
            instance = freshGameManager.GetComponent<GameManager>();
            DontDestroyOnLoad(freshGameManager);
            Debug.LogError("Spawned a GameManager on the fly due to misconfigured scene. This is not intentional, and breaks hard references to required libraries. You should place a GameManager prefab into the scene.");
        }
    #endif

    public void Quit() {
        ModManager.SaveConfig();
#if UNITY_EDITOR
        // Application.Quit() does not work in the editor so
        // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
        EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }

    void Start() {
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Destroy(gameObject);
            return;
        }

        ModManager.AddFinishedLoadingListener(ReloadMapIfInEditor);
        // FIXME: Photon isn't initialized early enough for scriptable objects to add themselves as a callback...
        // So I do it here-- I guess!
        PhotonNetwork.AddCallbackTarget(NetworkManager.instance);
        DontDestroyOnLoad(gameObject);
        SaveManager.Init();
    }

    private void ReloadMapIfInEditor() {
        if (Application.isEditor && SceneManager.GetActiveScene().name != "MainMenu"  && SceneManager.GetActiveScene().name != "ErrorScene" && !reloadedSceneAlready) {
            StartCoroutine(ReloadMapRoutine());
        }
        reloadedSceneAlready = true;
    }

    private IEnumerator ReloadMapRoutine() {
        Debug.LogWarning("Reloading scene due to mods not being ready yet...");
        bool found = false;
        PlayableMap selectedMap = null;
        foreach(var playableMap in PlayableMapDatabase.GetPlayableMaps()) {
            if (SceneManager.GetActiveScene().name != playableMap.unityScene.GetName()) continue;
            NetworkManager.instance.SetSelectedMap(playableMap);
            selectedMap = playableMap;
            found = true;
            break;
        }

        if (!found) {
            throw new UnityException($"Failed to find a PlayableMap instance for the map {SceneManager.GetActiveScene().name}! Please make one!");
        }

        yield return LevelLoader.instance.LoadLevel((string)selectedMap.unityScene.RuntimeKey);
        NetworkManager.instance.StartSinglePlayer();
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

    public void PlayUISFX(ButtonMouseOver btn, ButtonMouseOver.EventType evtType) {
        if (!ModManager.GetReady()) {
            return;
        }
        var camera = Camera.main;
        if (!camera) {
            return;
        }
        switch (evtType) {
            case ButtonMouseOver.EventType.Hover:
                var sourceb = AudioPack.PlayClipAtPoint(buttonHovered, camera.transform.position);
                sourceb.spatialBlend = 0f;
                break;
            case ButtonMouseOver.EventType.Click:
                var sourcea = AudioPack.PlayClipAtPoint(buttonClicked, camera.transform.position);
                sourcea.spatialBlend = 0f;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
