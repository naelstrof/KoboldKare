using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Reiikz.UnityUtils;
using System.Threading;
using Photon.Pun;

public class LoadCustomParts : MonoBehaviour
{
    public static string cheatVersion = "1";

    public static Vector3 caveLaunchpadPos = new Vector3(74.6600037f, -62.1015511f, -18.0100002f);
    public static Vector3 insideDumpster =  new Vector3(-141.160583f,17.8840008f,275.673981f);
    private static bool modifyingkobolds = false;
    public Grabber playerGrabber = null;
    public PrecisionGrabber playerPrecisionGrabber = null;
    public Kobold playerKobold;
    public KoboldCharacterController playerController;
    public PlayerKoboldLoader playerLoader;
    public float maxSpeedOrJump = 50f;
    public float prevJump = 8f;
    public float prevSpeed = 10f;
    public bool grabbed = false;
    public float rainbowStep = 0.002f;
    public float rainbowUpdateRate = 0.0375f;
    public static LoadCustomParts instance = null;
    private float nextRainbowUpdate = 0f;
    public float currentHue = 0f;
    public float currentBrightness = 0f;
    public float brightnessDirection = 1f;
    public GameObject versionNumber = null;
    public TMPro.TextMeshProUGUI versionNumberText = null;
    static readonly SemaphoreSlim AsyncTasksLocker = new SemaphoreSlim (1, 1);
    public static bool AsyncTasksDone;
    public float slowUpdateRate = 2f;
    public float nextSlowUpdate = 0f;
    public enum STATE {
        MAIN_MENU = 0,
        PLAYING,
        UNKNOWN
    }
    public STATE gameState = STATE.UNKNOWN;
    // public bool postLoadMainMenuKoboldUpdateDone = false;

    public Dictionary<int, string> SpawnTable = new Dictionary<int, string> {
        {0, ""},
        {1, "Egg"},
        {2, "Bomb"},
        {3, "Eggplant"},
        {4, "EquineDickEquippable"},
        {5, "KandiDickEquippable"},
        {6, "KnottedDickEquippable"},
        {7, "TaperedDickEquippable"},
        {8, "Melon"},
        {9, "NipplePump"},
        {10, "Heart"},
        {11, "Banana"},
        {12, "Pineapple"},
        {13, "IceChunk"},
        {14, "SpikeCollarEquippable"},
        {15, "TailbagEquippable"},
        {16, "HardhatEquippable"},
        {17, "NippleBarbellPiercingsEquippable"}
    };

    public static void CleanUpServer (){
        PhotonView[] objects = FindObjectsOfType<PhotonView>();
        foreach(PhotonView pv in objects){
            if(pv.gameObject.name.Contains("Egg(Clone)") || pv.gameObject.name.Contains("TailbagEquippable(Clone)")){
                //PhotonNetwork.Destroy(
                pv.TransferOwnership(PhotonNetwork.LocalPlayer);
                if(pv.IsMine){
                    pv.gameObject.transform.position = insideDumpster + new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                }
            }
        }
    }

    private static bool IsLoaded(string name)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).name == name)
            {
                return true;
            }
        }
        return false;
    }

    void Start()
    {
        if(instance == null){
            instance = this;
        }
        UnityScriptableSettings.ScriptableSettingsManager sm = GetComponent<UnityScriptableSettings.ScriptableSettingsManager>();
        GameObject ob = gameObject.transform.Find("ReiikzManager").gameObject;
        if(ob == null) Debug.LogError("ReiikzManager Missing from GameManager");
        UnityScriptableSettings.ScriptableSetting[] settings = ob.GetComponent<MyCustomParts>().settings;
        UnityScriptableSettings.ScriptableSetting[] newSettings = new UnityScriptableSettings.ScriptableSetting[sm.settings.Length + settings.Length];
        for(int x = 0; x < newSettings.Length; x++){
            if(x < sm.settings.Length){
                newSettings[x] = sm.settings[x];
            }else{
                newSettings[x] = settings[x - sm.settings.Length];
            }
        }
        sm.settings = newSettings;
        runMapCustoms();
        StartAsyncTasks();
    }

    void Awake(){
        runMapCustoms();
        StartAsyncTasks();
    }

    void StartAsyncTasks(){
        AsyncTasksLocker.Wait();
        if(!AsyncTasksDone){
            if(instance == null) instance = this;
            StartCoroutine(modifyKobolds());
            StartCoroutine(modifyVersionNumber());
            StartCoroutine(SlowUpdateRunner());
        }
        AsyncTasksDone = true;
        AsyncTasksLocker.Release();
    }

    private IEnumerator modifyVersionNumber() {
        versionNumber = BruteForce.AggressiveTreeFind(gameObject.transform, "VersionNumber");
        do{
            if(versionNumber != null){
                if(versionNumberText == null) versionNumberText = versionNumber.GetComponent<TMPro.TextMeshProUGUI>();
                if(!versionNumberText.text.Contains("Reiikz")){
                    versionNumberText.text += "\n(Cheat by Reiikz)\n(PENIS!)";
                    // RectTransform rt = versionNumber.GetComponent<RectTransform>();
                    // rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, -64);
                    Debug.Log("Version number updated!");
                }
            }else{
                versionNumber = BruteForce.AggressiveTreeFind(gameObject.transform, "VersionNumber");
            }
            yield return null;
        }while(versionNumber == null);
        yield break;
    }

    private IEnumerator modifyKobolds() {
        if(modifyingkobolds) yield break;
        Kobold pk = null;
        Kobold[] kobolds = (Kobold[]) GameObject.FindObjectsOfType(typeof(Kobold));
        foreach(Kobold k in kobolds){
            if(k.isPlayer) {
                pk = k;
            }
        }
        if(pk == null) yield break;
        Transform grabberTransform = null;
        do{
            if(pk == null) yield break;
            grabberTransform = pk.root.Find("Camera");
            grabberTransform = grabberTransform.Find("Grabber");
            yield return null;
        }while(grabberTransform == null);
        GameObject playerGrabber_ = grabberTransform.gameObject;
        playerGrabber = playerGrabber_.GetComponent<Grabber>();
        playerPrecisionGrabber = playerGrabber_.GetComponent<PrecisionGrabber>();
        playerController = pk.root.gameObject.GetComponent<KoboldCharacterController>();
        playerLoader = pk.root.gameObject.GetComponent<PlayerKoboldLoader>();
        playerKobold = pk;
        yield break;
    }

    void Update() {
        if(playerKobold == null){
            StartCoroutine(modifyKobolds());
        }else{
            if((playerGrabber.grabbing || playerPrecisionGrabber.grabbing) && (grabbed == false)){
                prevJump = playerController.jumpStrength;
                prevSpeed = playerController.speed;
                if(playerController.speed > maxSpeedOrJump){
                    playerController.speed = maxSpeedOrJump;
                }
                if(playerController.jumpStrength > maxSpeedOrJump){
                    playerController.jumpStrength = maxSpeedOrJump;
                }
                if(playerKobold.body.velocity.magnitude > maxSpeedOrJump) {
                    float x = playerKobold.body.velocity.x, y = playerKobold.body.velocity.y, z = playerKobold.body.velocity.z;
                    x = Mathf.Clamp(x, 0, maxSpeedOrJump);
                    y = Mathf.Clamp(y, 0, maxSpeedOrJump);
                    z = Mathf.Clamp(z, 0, maxSpeedOrJump);
                    playerKobold.body.velocity = new Vector3(x, y, z);
                }
                grabbed = true;
            }
            if(((!playerGrabber.grabbing) && (!playerPrecisionGrabber.grabbing)) && (grabbed == true)){
                grabbed = false;
                playerController.jumpStrength = prevJump;
                playerController.speed = prevSpeed;
            }
            if(playerKobold.gay){
                if(Time.timeSinceLevelLoad >= nextRainbowUpdate){
                    currentHue += rainbowStep;
                    Mathf.Clamp01(currentHue);
                    currentBrightness += (UnityEngine.Random.Range(rainbowStep/5, (rainbowStep/5)*1.2f) * brightnessDirection);
                    Mathf.Clamp01(currentBrightness);
                    playerKobold.HueBrightnessContrastSaturation = playerKobold.HueBrightnessContrastSaturation.With(g:currentBrightness);
                    playerKobold.HueBrightnessContrastSaturation = playerKobold.HueBrightnessContrastSaturation.With(r:currentHue);
                    nextRainbowUpdate = Time.timeSinceLevelLoad + rainbowUpdateRate;
                    if(currentHue >= 1) currentHue = 0;
                    if((currentBrightness >= 1.2f) || (currentBrightness <= -0.2f)) brightnessDirection *= -1f;
                }
            }
        }
    }

    private IEnumerator SlowUpdateRunner() {
        while(true) {
            if(Time.timeSinceLevelLoad >= nextSlowUpdate){
                SlowUpdate();
                nextSlowUpdate = Time.timeSinceLevelLoad + slowUpdateRate;
            }
            yield return null;
        }
    }

    private void SlowUpdate(){
        if(versionNumber == null){
            StartCoroutine(modifyVersionNumber());
        }else{
            if(versionNumber.activeSelf){
                StartCoroutine(modifyVersionNumber());
            }
        }
        // if(gameState == STATE.MAIN_MENU) if(playerKobold != null) if(!postLoadMainMenuKoboldUpdateDone){
        //     if(Time.timeSinceLevelLoad > 10f){
        //         foreach(string settingName in PlayerKoboldLoader.settingNames){
        //             var option = UnityScriptableSettings.ScriptableSettingsManager.instance.GetSetting(settingName);
        //             if(option == null){
        //                 Debug.LogWarning("tried to retrieve missing setting: " + settingName);
        //                 continue;
        //             }
        //             PlayerKoboldLoader.ProcessOption(playerKobold, option);
        //         }
        //         postLoadMainMenuKoboldUpdateDone = true;
        //     }
        // }
    }

    void runMapCustoms(){
        if(IsLoaded("MainMap")){
            customizeMainMap();
            if(!IsLoaded("ReiikzMainMapAditions")) {
                SceneManager.LoadScene("ReiikzMainMapAditions", LoadSceneMode.Additive);
            }
            gameState = STATE.PLAYING;
        }else{
            gameState = STATE.MAIN_MENU;
        }
    }

    void customizeMainMap(){
        GameObject foundation = GameObject.Find("PlayerHouseConcreteFoundation");
        Destroy(foundation.transform.Find("default").GetComponent<MeshCollider>());
        Launchpad[] ls = (Launchpad[]) GameObject.FindObjectsOfType(typeof(Launchpad));
        if(ls.Length > 0){
            foreach(Launchpad l in ls){
                GameObject go = l.gameObject;
                if(Vector3.Distance(go.transform.position, caveLaunchpadPos) > 60){
                    GameObject.Destroy(go);
                }
            }
        }else{
            Debug.Log("No launchpads found");
        }
    }

    public void spawnShit(int thing, int howMuch){
        if (thing == 0) return;
        if(!SpawnTable.ContainsKey(thing)) return;
        for(int i = 1; i <= howMuch; i++){
            try {
                PhotonNetwork.Instantiate(SpawnTable[thing], playerKobold.transform.position, playerKobold.transform.rotation, 0);
            }catch(System.Exception e) {}
        }
    }
}
