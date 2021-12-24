using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadCustomParts : MonoBehaviour
{
    public static Vector3 caveLaunchpadPos = new Vector3(74.6600037f, -62.1015511f, -18.0100002f);
    private static bool modifyingkobolds = false;
    public Grabber playerGrabber = null;
    public PrecisionGrabber playerPrecisionGrabber = null;
    public Kobold playerKobold;
    public KoboldCharacterController playerController;
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
        UnityScriptableSettings.ScriptableSettingsManager sm = GetComponent<UnityScriptableSettings.ScriptableSettingsManager>();
        GameObject ob = gameObject.transform.Find("ReiikzManager").gameObject;
        if(ob == null) Debug.LogError("ReiikzManager Missing from GameManager");
        UnityScriptableSettings.ScriptableSetting[] settings = ob.GetComponent<MySettings>().settings;
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
        GameObject versionNumber = GameObject.Find("VersionNumber");
        if(versionNumber != null){
            TMPro.TextMeshProUGUI txt = versionNumber.GetComponent<TMPro.TextMeshProUGUI>();
            txt.text += "\n(Cheat by Reiikz)\n(PENIS!)";
            // Canvas c = versionNumber.transform.parent.transform.parent.transform.parent.GetComponent<Canvas>();
            RectTransform rt = versionNumber.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, -64);
            // Canvas.ForceUpdateCanvases();
        }else{
            Debug.LogWarning("Could not find Version number game object");
        }
        StartAsyncTasks();
    }

    void Awake(){
        runMapCustoms();
        StartAsyncTasks();
    }
    void StartAsyncTasks(){
        if(instance == null) instance = this;
        StartCoroutine(modifyKobolds());
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
            grabberTransform = pk.root.Find("Camera");
            grabberTransform = grabberTransform.Find("Grabber");
            yield return null;
        }while(grabberTransform == null);
        GameObject playerGrabber_ = grabberTransform.gameObject;
        playerGrabber = playerGrabber_.GetComponent<Grabber>();
        playerPrecisionGrabber = playerGrabber_.GetComponent<PrecisionGrabber>();
        playerController = pk.root.gameObject.GetComponent<KoboldCharacterController>();
        playerKobold = pk;
        yield break;
    }

    void Update() {
        if(playerKobold == null){
            StartCoroutine(modifyKobolds());
        }else{
            if(playerGrabber.grabbing || playerPrecisionGrabber.grabbing){
                if(playerController.speed > maxSpeedOrJump){
                    playerController.speed = maxSpeedOrJump;
                }
                if(playerController.jumpStrength > maxSpeedOrJump){
                    playerController.jumpStrength = maxSpeedOrJump;
                }
                prevJump = playerController.jumpStrength;
                prevSpeed = playerController.speed;
                grabbed = true;
            }else{
                if(grabbed){
                    grabbed = false;
                    playerController.jumpStrength = prevJump;
                    playerController.speed = prevSpeed;
                }
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
                    if((currentBrightness >= 1.2) || (currentBrightness < -0.2)) brightnessDirection *= -1f;
                }
            }
        }
    }

    void runMapCustoms(){
        if(IsLoaded("MainMap")){
            customizeMainMap();
            if(!IsLoaded("ReiikzMainMapAditions")) {
                SceneManager.LoadScene("ReiikzMainMapAditions", LoadSceneMode.Additive);
            }
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
}
