using System.Collections;
using UnityEngine;
using UnityEngine.VFX;
using Photon.Pun;
using KoboldKare;
using System.IO;
using NetStack.Serialization;
using SimpleJSON;

[RequireComponent(typeof(GenericReagentContainer))]
public class Plant : GeneHolder, IPunInstantiateMagicCallback, IPunObservable, ISavable {
    public ScriptablePlant plant;
    [SerializeField]
    private GenericReagentContainer container;

    [SerializeField]
    public Color darkenedColor;

    [SerializeField]
    private VisualEffect effect, wateredEffect;
    [SerializeField]
    private GameObject display;

    [SerializeField]
    public AudioSource audioSource;
    public delegate void SwitchAction();
    public event SwitchAction switched;
    
    private static readonly int BrightnessContrastSaturation = Shader.PropertyToID("_HueBrightnessContrastSaturation");
    private bool growing;
    public delegate void PlantSpawnEventAction(GameObject obj, ScriptablePlant plant);
    public static event PlantSpawnEventAction planted;

    void Start() {
        container.OnFilled.AddListener(OnFilled);
    }

    void OnDestroy() {
        container.OnFilled.RemoveListener(OnFilled);
    }

    IEnumerator GrowRoutine() {
        growing = true;
        yield return new WaitForSeconds(30f);
        if (!photonView.IsMine) {
            yield break;
        }
        if (plant.possibleNextGenerations == null || plant.possibleNextGenerations.Length == 0f) {
            PhotonNetwork.Destroy(gameObject);
            yield break;
        }
        photonView.RPC(nameof(GenericReagentContainer.Spill), RpcTarget.All, container.volume);
        photonView.RPC(nameof(SwitchToRPC), RpcTarget.AllBufferedViaServer,
            PlantDatabase.GetID(plant.possibleNextGenerations[Random.Range(0, plant.possibleNextGenerations.Length)]));
        growing = false;
    }

    void OnFilled(ReagentContents contents, GenericReagentContainer.InjectType injectType) {
        if (plant.possibleNextGenerations == null || plant.possibleNextGenerations.Length == 0) {
            return;
        }
        foreach(Renderer renderer in display.GetComponentsInChildren<Renderer>()) {
            renderer.material.SetFloat("_BounceAmount", 1f);
            StartCoroutine(DarkenMaterial(renderer.material));
        }
        wateredEffect.SendEvent("Play");
        audioSource.Play();
        effect.gameObject.SetActive(false);
        effect.gameObject.SetActive(true);
        StopCoroutine(nameof(GrowRoutine));
        StartCoroutine(nameof(GrowRoutine));
    }

    [PunRPC]
    void SwitchToRPC(short newPlantID) {
        ScriptablePlant checkPlant = PlantDatabase.GetPlant(newPlantID);
        //Debug.Log("Switching from " + plant + " to " + checkPlant);
        if (checkPlant == plant) {
            return;
        }
        SwitchTo(checkPlant);
        PhotonProfiler.LogReceive(sizeof(short));
    }

    public override void SetGenes(KoboldGenes newGenes) {
        if (display != null) {
            Vector4 hbcs = new Vector4(newGenes.hue / 255f, newGenes.brightness / 255f, 0.5f, newGenes.saturation / 255f);
            foreach (var r in display.GetComponentsInChildren<Renderer>()) {
                foreach (var material in r.materials) {
                    material.SetColor(BrightnessContrastSaturation, hbcs);
                }
            }
        }
        base.SetGenes(newGenes);
    }

    void SwitchTo(ScriptablePlant newPlant) {
        if (plant == newPlant) {
            return;
        }
        plant = newPlant;
        UndarkenMaterials();
        wateredEffect.Stop();
         // Plant == newPlant should always return true for deserialization, skip that step and assert
        if(display != null){
            Destroy(display);
        }
        if(newPlant.display != null){
            display = GameObject.Instantiate(newPlant.display,transform);
            // TODO: This is a hack to make sure future iterations have received the genes.
            if (GetGenes() != null) {
                SetGenes(GetGenes());
            }
        }

        if (PhotonNetwork.IsMasterClient) {
            foreach (var produce in newPlant.produces) {
                int spawnCount = Random.Range(produce.minProduce, produce.maxProduce);
                for(int i=0;i<spawnCount;i++) {
                    BitBuffer buffer = new BitBuffer(4);
                    buffer.AddKoboldGenes(GetGenes());
                    buffer.AddBool(false);
                    PhotonNetwork.InstantiateRoomObject(produce.prefab.photonName,
                         transform.position + Vector3.up + Random.insideUnitSphere * 0.5f, Quaternion.identity, 0,
                         new object[] { buffer });
                }
            }
        }
        switched?.Invoke();
        if (plant.possibleNextGenerations == null || plant.possibleNextGenerations.Length == 0) {
            StartCoroutine(GrowRoutine());
        }
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info) {
        if (info.photonView.InstantiationData != null && info.photonView.InstantiationData[0] is BitBuffer) {
            BitBuffer buffer = (BitBuffer)info.photonView.InstantiationData[0];
            // Could be shared by other OnPhotonInstantiates.
            buffer.SetReadPosition(0);
            SetGenes(buffer.ReadKoboldGenes());
            SwitchTo(PlantDatabase.GetPlant(buffer.ReadShort()));
            PhotonProfiler.LogReceive(buffer.Length);
        } else {
            SetGenes(new KoboldGenes().Randomize());
            Debug.LogError("Plant created without proper instantiation data!");
        }
        
        planted?.Invoke(photonView.gameObject, plant);
    }

    void UndarkenMaterials(){
        if (display == null) {
            return;
        }
        foreach(Renderer renderer in display.GetComponentsInChildren<Renderer>()) {
            if (renderer.material.HasProperty("_Color")) {
                renderer.material.SetColor("_Color", Color.white);
            }

            if (renderer.material.HasProperty("_BaseColor")) {
                renderer.material.SetColor("_BaseColor", Color.white);
            }
        }
    }

    IEnumerator DarkenMaterial(Material tgtMat) {
        float startTime = Time.time;
        float duration = 1f;
        while(Time.time < startTime + duration) {
            float t = (Time.time - startTime) / duration;
            if (tgtMat.HasProperty("_Color")) {
                tgtMat.SetColor("_Color", Color.Lerp(tgtMat.GetColor("_Color"), darkenedColor, t));
            }
            if (tgtMat.HasProperty("_BaseColor")) {
                tgtMat.SetColor("_BaseColor", Color.Lerp(tgtMat.GetColor("_BaseColor"), darkenedColor, t));
            }
            yield return null;
        }
    }
    
    public void Save(JSONNode node) {
        node["plantID"] = PlantDatabase.GetID(plant);
        node["position.x"] = transform.position.x;
        node["position.y"] = transform.position.y;
        node["position.z"] = transform.position.z;
        GetGenes().Save(node, "genes");
        node["growing"] = growing;
    }

    public void Load(JSONNode node) {
        SwitchTo(PlantDatabase.GetPlant((short)node["plantID"].AsInt));
        float x = node.GetValueOrDefault("position.x", 0f);
        float y = node.GetValueOrDefault("position.y", 0f);
        float z = node.GetValueOrDefault("position.z", 0f);
        transform.position = new Vector3(x,y,z);
        KoboldGenes loadedGenes = new KoboldGenes();
        loadedGenes.Load(node, "genes");
        SetGenes(loadedGenes);
        
        if (!node.GetValueOrDefault("growing", false)) return;
        foreach(Renderer renderer in display.GetComponentsInChildren<Renderer>()) {
            renderer.material.SetFloat("_BounceAmount", 1f);
            StartCoroutine(DarkenMaterial(renderer.material));
        }
        wateredEffect.SendEvent("Play");
        audioSource.Play();
        effect.gameObject.SetActive(false);
        effect.gameObject.SetActive(true);
        StopCoroutine(nameof(GrowRoutine));
        StartCoroutine(nameof(GrowRoutine));
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
    }
}
