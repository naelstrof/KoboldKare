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

public class GameManager : MonoBehaviour, IGameEventListener, IGameEventGenericListener<Vector3> {
    public GameEventVector3 OnPlayerDie;
    public GameEvent UnpauseEvent;
    public GameEvent PauseEvent;
    public GameEvent OnPlayerRespawn;
    public GameEvent Sleep;
    public static GameManager instance = null;              //Static instance of GameManager which allows it to be accessed by any other script.
    public GameObject decalPrefab;
    public GameEventFloat MetabolizeEvent;
    public ScriptableFloat mouseSensitivity;
    private float metabolizationTimer = 0;
    public float metabolizeTimeStep = 1.6f;
    public ScriptableFloat money;
    public PaintDecal decalPainter;
    public GraphicsOptions options;
    public GameEvent sceneLoaded;
    public GameObject loadingPanel;
    //public UIView loadingPanel;
    //public Progressor loadingPanelProgress;
    public ReagentDatabase reagentDatabase;
    public int randomSeed;
    public Dictionary<int, Material> decalMaterialCache = new Dictionary<int, Material>();
    [HideInInspector]
    public bool loadingLevel = false;
    [HideInInspector]
    public GameObject deathGameObject;

    [System.Serializable]
    public class Resources {
        public GenericLODConsumer.ConsumerType type;
        public int highQualityCount;
        public int mediumQualityCount;
    }
    public List<Resources> consumerResources;
    private List<List<GenericLODConsumer>> registeredConsumers = new List<List<GenericLODConsumer>>();
    public NetworkManager networkManager;
    private Camera internalMainCamera;
    private Camera mainCamera {
        get {
            if (internalMainCamera == null) {
                internalMainCamera = Camera.current;
            }
            if (internalMainCamera == null) {
                internalMainCamera = Camera.main;
            }
            return internalMainCamera;
        }
    }
    public UnityEngine.Audio.AudioMixerGroup soundEffectGroup;
    public UnityEngine.Audio.AudioMixerGroup soundEffectLoudGroup;
    public LayerMask precisionGrabMask;
    public LayerMask walkableGroundMask;
    public LayerMask waterSprayHitMask;
    public LayerMask decalHitMask;

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
        PhotonNetwork.AddCallbackTarget(networkManager);
        //Check if instance already exists
        if (instance == null) {
            //if not, set instance to this
            instance = this;
        } else if (instance != this) { //If instance already exists and it's not this:
            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);
            return;
        }
        //if (Camera.main != null) {
        //frameSettings = Camera.main.GetComponent<HDAdditionalCameraData>().renderingPathCustomFrameSettings;
        //frameSettingsMask = Camera.main.GetComponent<HDAdditionalCameraData>().renderingPathCustomFrameSettingsOverrideMask;
        //}
        DontDestroyOnLoad(gameObject);
        randomSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
    }

    void Start() {
        if (Application.isEditor && SceneManager.GetActiveScene().name != "MainMenu") {
            //PhotonNetwork.JoinRandomRoom();
            //SpawnControllablePlayer();
            networkManager.StartSinglePlayer();
        }
        foreach(GraphicsOptions.Option o in options.options) {
            if (o.type == GraphicsOptions.OptionType.Language) {
                options.ChangeOption(o.type, o.value);
            }
            if (Application.isEditor) {
                continue;
            }
            if (o.type == GraphicsOptions.OptionType.ProceduralTextureSize) {
                StartCoroutine(options.WaitAndThenSetTextureSize(o.value));
                break;
            }
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
                    if (ren.gameObject.activeInHierarchy) {
                        decalPainter.RenderDecal(ren, decalMat.GetTexture("_BaseMap"), position, rot, new Color(color.r, color.g, color.b, color.a), size / 2f, depth, false, ignoreBackface, subtractive);
                    }
                }
            }
            return;
        }
        foreach (Renderer r in obj.GetComponentsInChildren<Renderer>()) {
            decalPainter.RenderDecal(r, decalMat.GetTexture("_BaseMap"), position, rot, new Color(color.r, color.g, color.b, color.a), size / 2f, depth, false, ignoreBackface, subtractive);
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
        source.clip = clip;
        source.spatialBlend = 1f;
        source.volume = volume;
        source.pitch = UnityEngine.Random.Range(0.85f,1.15f);
        source.Play();
        Destroy(g, clip.length);
        //AudioSource.PlayClipAtPoint(clip, position, volume);
    }

    void Update() {
        metabolizationTimer += Time.deltaTime;
        if (metabolizationTimer>metabolizeTimeStep) {
            metabolizationTimer -= metabolizeTimeStep;
            MetabolizeEvent.Raise(metabolizeTimeStep);
        }

        if (mainCamera == null && networkManager.localPlayerInstance == null && !loadingLevel && !SaveManager.isLoading) {
            if (deathGameObject == null) {
                deathGameObject = GameObject.Instantiate(DiePrefab);
                internalMainCamera = deathGameObject.GetComponentInChildren<Camera>();
            }
        }
        // If we don't have a camera, don't continue;
        if (mainCamera == null) {
            return;
        }
        Vector3 cameraPos = mainCamera.transform.position;
        // Lazily bubble sort
        // Make sure only `availableResources` are activated
        for (int o=0;o<registeredConsumers.Count;o++) {
            float a = 0;
            for (int i = 0; i < registeredConsumers[o].Count; i++) {
                if (registeredConsumers[o][i] == null) {
                    registeredConsumers[o].RemoveAt(i);
                    continue;
                }
                int veryFarSwapBarrier = consumerResources[o].highQualityCount + consumerResources[o].mediumQualityCount;
                registeredConsumers[o][i].SetClose(i <= consumerResources[o].highQualityCount);
                registeredConsumers[o][i].SetVeryFar(i > veryFarSwapBarrier);

                float b = Vector3.Distance(registeredConsumers[o][i].transform.position, cameraPos);
                if (b < a) {
                    var swap = registeredConsumers[o][i - 1];
                    registeredConsumers[o][i - 1] = registeredConsumers[o][i];
                    registeredConsumers[o][i] = swap;
                    if (i - 1 <= consumerResources[o].highQualityCount && i > consumerResources[o].highQualityCount) {
                        registeredConsumers[o][i - 1].SetClose(true);
                        registeredConsumers[o][i].SetClose(false);
                    }
                    if (i - 1 <= veryFarSwapBarrier && i > veryFarSwapBarrier) {
                        registeredConsumers[o][i - 1].SetVeryFar(false);
                        registeredConsumers[o][i].SetVeryFar(true);
                    }
                }
                a = b;
            }
        }
    }

    public void RegisterConsumer(GenericLODConsumer g, GenericLODConsumer.ConsumerType type) {
        while (registeredConsumers.Count <= (int)type) {
            registeredConsumers.Add(new List<GenericLODConsumer>());
        }
        registeredConsumers[(int)type].Add(g);
    }

    public void UnregisterConsumer(GenericLODConsumer g, GenericLODConsumer.ConsumerType type) {
        if (registeredConsumers[(int)type].Contains(g)) {
            registeredConsumers[(int)type].Remove(g);
        }
    }
    public ScriptableFloat Health;

    public GameObject DiePrefab;

    [NonSerialized]
    private bool dead = false;
    public void OnEnable() {
        OnPlayerDie.RegisterListener(this);
        OnPlayerRespawn.RegisterListener(this);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
    }

    public void OnDisable() {
        OnPlayerDie.UnregisterListener(this);
        OnPlayerRespawn.UnregisterListener(this);
        if (options.renderer.rendererFeatures.Contains(options.cloudsSettings)) {
            options.renderer.rendererFeatures.Remove(options.cloudsSettings);
        }
    }

    public void OnEventRaised(GameEvent e) {
        if (e == OnPlayerRespawn && !loadingLevel) {
            if (deathGameObject != null) {
                Destroy(deathGameObject);
            }
            Vector3 spawnLocation = Vector3.zero;
            // Get time to move forward before spawning the player so we don't kill them.
            dead = false;
            //GameObject[] spawns = GameObject.FindGameObjectsWithTag("PlayerSpawn");
            Health.fill();
            //if ( spawns.Length > 0 ) {
                //Transform spawnLoc = spawns[UnityEngine.Random.Range(0, spawns.Length - 1)].transform;
                //spawnLocation = spawnLoc.position;
            //}
            //GameObject localPlayerInstance = GameObject.Instantiate(playerPrefab, spawnLocation, Quaternion.identity);
            //GameObject localPlayerInstance = PhotonNetwork.Instantiate("GrabbableKobold4", spawnLocation, Quaternion.identity, 0, Kobold.GetRandomSerializableSave().ToPhoton());
            //localPlayerInstance.GetComponentInChildren<PlayerPossession>(true).gameObject.SetActive(true);
            networkManager.SpawnControllablePlayer();
            return;
        }
    }

    public void OnEventRaised(GameEventGeneric<Vector3> e, Vector3 t) {
        if (e == OnPlayerDie && !dead && !loadingLevel) {
            if (deathGameObject == null) {
                deathGameObject = GameObject.Instantiate(DiePrefab);
                internalMainCamera = deathGameObject.GetComponentInChildren<Camera>();
            }
            dead = true;
        }
    }
    public Coroutine LoadLevel(string name) {
        return StartCoroutine(LoadLevelRoutine(name));
    }
    public IEnumerator LoadLevelRoutine(string name) {
        if (!SaveManager.isLoading) {
            SaveManager.ClearData();
        }
        UnpauseEvent.Raise();
        loadingLevel = true;
        loadingPanel.SetActive(true);
        //loadingPanel.Show();
        //loadingPanelProgress.SetProgress(0f);
        yield return new WaitForSeconds(1f);
        PhotonNetwork.LoadLevel(name);
        while (!SceneManager.GetSceneByName(name).isLoaded) {
            loadingPanel.GetComponentInChildren<TMP_Text>().text = "Loading ... " + PhotonNetwork.LevelLoadingProgress.ToString("0") + " %";
            //loadingPanelProgress.SetProgress(PhotonNetwork.LevelLoadingProgress);
            yield return new WaitForEndOfFrame();
        }
        //loadingPanelProgress.SetProgress(1f);
        //loadingPanel.Hide();
        loadingPanel.SetActive(false);
        loadingLevel = false;
        sceneLoaded.Raise();
        PopupHandler.instance.ClearAllPopups();
    }
}