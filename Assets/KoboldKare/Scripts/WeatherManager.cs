using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using KoboldKare;
using SimpleJSON;
using UnityEngine;
using UnityEngine.VFX;

public class WeatherManager : MonoBehaviourPun, IPunObservable, ISavable {
    [Range(0f,1f)]
    public float rainAmount = 0f;
    public float fogRangeWhenRaining;
    public Color fogColorWhenRaining;
    private float origFogRange;
    private Color origFogColor;
    public Material skyboxMaterial;
    public VisualEffect dust, rain;
    public ParticleSystem rainBackup;
    private VisualEffect spawnedEffect;
    public AudioSource rainSounds;
    public AudioSource thunderSounds;
    public GameEventGeneric midnightEvent;
    private Camera cachedCamera;
    [SerializeField]
    private ReagentContents rainContents = new ReagentContents();
    public Material splashMaterial;
    private HashSet<PhotonView> views = new HashSet<PhotonView>();
    //private bool clearingViews = false;
    //private bool isSnowing = false;
    //public Vector3 cloudBounds;
    //public float cloudHeight;
    public Coroutine rainVFXUpdate;
    

    public IEnumerator WaitAndThenClear() {
        yield return new WaitForSeconds(5f);
        views.Clear();
        //clearingViews = false;
    }
    private Camera cam {
        get {
            if (cachedCamera == null || !cachedCamera.isActiveAndEnabled) {
                cachedCamera = Camera.current;
            }
            if (cachedCamera == null || !cachedCamera.isActiveAndEnabled) {
                cachedCamera = Camera.main;
            }
            return cachedCamera;
        }
    }
    private void Awake() {
        //StopSnow();
        origFogRange = RenderSettings.fogDensity;
        origFogColor = RenderSettings.fogColor;
        midnightEvent.AddListener(OnMidnight);
    }

    private void OnMidnight(object ignore) {
        StopRain();
        RandomlyRain(0.15f);
    }

    private void OnDestroy() {
        //cloudMaterial.SetFloat("densityOffset", -4);
        //cloudMaterial.SetFloat("shadowMin", 0.66f);
        //cloudMaterial.SetFloat("densityMultiplier", 1f);
        skyboxMaterial.SetFloat("_CloudDensityOffset", -0.3f);
        skyboxMaterial.SetFloat("_Brightness", 0.5f);
        midnightEvent.RemoveListener(OnMidnight);
    }

    public IEnumerator Rain() {
        //snow.Stop();
        rainSounds.enabled = true;
        thunderSounds.enabled = true;
        while (rainAmount < 1f) {
            rainAmount = Mathf.MoveTowards(rainAmount, 1f, Time.deltaTime*0.1f);
            RenderSettings.fogDensity = Mathf.MoveTowards(RenderSettings.fogDensity,fogRangeWhenRaining,Time.deltaTime*0.1f);
            RenderSettings.fogColor = Color.Lerp(origFogColor,fogColorWhenRaining,Time.deltaTime*0.1f);
            rainSounds.volume = rainAmount*0.7f;
            yield return null;
        }

        while (rainAmount >= 1f) {
            yield return new WaitForSeconds(100f);
            FillRaincatchers("Water");
            thunderSounds.Play();
        }
    }
    public IEnumerator StopRainRoutine() {
        while (rainAmount > 0f) {
            rainAmount = Mathf.MoveTowards(rainAmount, 0f, Time.fixedDeltaTime*0.1f);
            RenderSettings.fogDensity = Mathf.MoveTowards(RenderSettings.fogDensity,origFogRange,Time.fixedDeltaTime*0.1f);
            RenderSettings.fogColor = Color.Lerp(fogColorWhenRaining,origFogColor,Time.fixedDeltaTime*0.1f);
            rainSounds.volume = rainAmount*0.7f;
            //rainBackup.Stop();
            rain.Stop();
            yield return new WaitForFixedUpdate();
        }
        rainSounds.enabled = false;
        thunderSounds.enabled = false;
    }

    public void StopRain() {
        if (!photonView.IsMine) {
            return;
        }
        StopCoroutine("Rain");
        StartCoroutine("StopRainRoutine");
    }

    public void RandomlyRain(float chance) {
        if (!photonView.IsMine) {
            return;
        }
        if (UnityEngine.Random.Range(0f,1f) < chance) {
            StopCoroutine(nameof(StopRainRoutine));
            StopCoroutine(nameof(Rain));
            StartCoroutine(nameof(Rain));
            //rainBackup.Play();
            rain.SendEvent("Fire");
        } else {
            StopRain();
        }
    }

    public void FillRaincatchers(string Reagant){
        //var items = GameObject.FindGameObjectsWithTag("CatchesRain"); //TODO: Refactor this into a static list we add/remove from dynamically
        //foreach(GameObject GO in items){
            ////Fill each container with the amount of water we want to add
            //GO.GetComponent<GenericReagentContainer>().AddMixRPC(ReagentDatabase.GetReagent(Reagant),110,GenericReagentContainer.InjectType.Spray);
        //}        
    }

    //public void StartSnow() {
        //if (rainAmount <= 0f) {
            //isSnowing = true;
            //snow.Play();
            //dust.Stop();
        //}
    //}
    //public void StopSnow() {
        //isSnowing = false;
        //snow.Stop();
        //dust.Play();
    //}

    private void Update() {
        if (Mathf.Approximately(rainAmount, 0f) && !dust.gameObject.activeInHierarchy) {
            dust.gameObject.SetActive(true);
        }
        if (!Mathf.Approximately(rainAmount, 0f) && dust.gameObject.activeInHierarchy) {
            dust.gameObject.SetActive(false);
        }
        //cloudMaterial.SetFloat("densityOffset", Mathf.Lerp(-4, 0f, rainAmount));
        //cloudMaterial.SetFloat("shadowMin", Mathf.Lerp(0.66f, 0.4f, rainAmount));
        //cloudMaterial.SetFloat("densityMultiplier", Mathf.Lerp(1f, 4f, rainAmount));
//
        //cloudMaterial.SetVector("boundsMin", Vector3.Lerp(-cloudBounds + Vector3.up * cloudHeight, -cloudBounds + Vector3.up * cloudHeight*0.8f, rainAmount));
        //cloudMaterial.SetVector("boundsMax", Vector3.Lerp(cloudBounds + Vector3.up * cloudHeight, cloudBounds + Vector3.up * cloudHeight*0.4f, rainAmount));

        //skyboxMaterial.SetFloat("_CloudHeight", Mathf.Lerp(50f, 30f, rainAmount));
        skyboxMaterial.SetFloat("_CloudDensityOffset", Mathf.Lerp(-0.75f, 0.1f, rainAmount));
        skyboxMaterial.SetFloat("_Brightness", Mathf.Lerp(0.5f, 0.25f, rainAmount));

        //rain.SetFloat("WaterDensity", rainAmount);
        //float radius = rain.GetFloat("Radius");
        /*for (int i = 0; i < 3*rainAmount; i++) {
            Vector3 waterTop = rain.transform.position + Vector3.up * 100f;
            Vector3 randomSample = Vector3.ProjectOnPlane(UnityEngine.Random.insideUnitSphere, Vector3.up) * radius;
            Vector3 rayStart = waterTop + randomSample;
            RaycastHit hit;
            if (Physics.Raycast(rayStart, Vector3.down, out hit, 240f, GameManager.instance.waterSprayHitMask, QueryTriggerInteraction.Ignore)) {
                GenericReagentContainer container = hit.collider.GetComponentInParent<GenericReagentContainer>();
                if (container != null) {
                    if (container.contents.ContainsKey(ReagentData.ID.Water) && container.contents[ReagentData.ID.Water].volume > 20f) {
                        continue;
                    }
                    container.contents.Mix(rainContents, ReagentContents.ReagentInjectType.Spray);
                    PhotonView view = hit.collider.GetComponentInParent<PhotonView>();
                    if (view != null && !view.IsMine && !views.Contains(view)) {
                        view.RequestOwnership();
                        views.Add(view);
                        if (!clearingViews) {
                            StartCoroutine(WaitAndThenClear());
                            clearingViews = true;
                        }
                    }
                }
                if (rainContents.volume > 0f) {
                    bool shouldClean = rainContents.ContainsKey(ReagentData.ID.Water) && rainContents[ReagentData.ID.Water].volume > rainContents.volume * 0.9f;
                    GameManager.instance.SpawnDecalInWorld(splashMaterial, hit.point + hit.normal * 0.25f, -hit.normal, Vector2.one * 3f, rainContents.GetColor(ReagentDatabase.instance), hit.collider.gameObject, 0.5f, true, true, shouldClean);
                }
            }
        }*/
        /*if (Mathf.Approximately(rainAmount, 0f) && rainCamera.enabled) {
            rainCamera.enabled = false;
        } else if (!Mathf.Approximately(rainAmount, 0f) && !rainCamera.enabled) {
            rainCamera.enabled = true;
        }*/
    }
    public void LateUpdate() {
        if (cam != null) {
            transform.position = cam.transform.position + Vector3.up*10f;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(rainAmount);
        } else {
            rainAmount = (float)stream.ReceiveNext();
            PhotonProfiler.LogReceive(sizeof(float));
        }
    }

    public void Save(JSONNode node) {
        node["rainAmount"] = rainAmount;
    }

    public void Load(JSONNode node) {
        rainAmount = node["rainAmount"];
    }
}
