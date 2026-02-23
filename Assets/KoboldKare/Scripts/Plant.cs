using System.Collections;
using UnityEngine;
using UnityEngine.VFX;
using Photon.Pun;
using KoboldKare;
using System.IO;
using System.Threading.Tasks;
using NetStack.Serialization;
using SimpleJSON;

[RequireComponent(typeof(GenericReagentContainer))]
public class Plant : MonoBehaviour, ISavable {
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

    private AssetGroup.AssetLocation.AssetHandle<ScriptablePlant> plantHandle;
    
    private static readonly int BrightnessContrastSaturation = Shader.PropertyToID("_HueBrightnessContrastSaturation");
    private bool growing;
    public delegate void PlantSpawnEventAction(GameObject obj, ScriptablePlant plant);
    public static event PlantSpawnEventAction planted;

    private NetworkedEntity networkedEntity;

    void Start() {
        networkedEntity = GetComponentInParent<NetworkedEntity>();
        if (networkedEntity) {
            networkedEntity.OnFilled += OnFilled;
            networkedEntity = GetComponentInParent<NetworkedEntity>();
            networkedEntity.hue.OnChange += OnColorChange;
            networkedEntity.brightness.OnChange += OnColorChange;
            networkedEntity.saturation.OnChange += OnColorChange;
            planted?.Invoke(networkedEntity.gameObject, plant);
        }
    }


    void OnDestroy() {
        if (networkedEntity) {
            networkedEntity.OnFilled -= OnFilled;
            networkedEntity.hue.OnChange -= OnColorChange;
            networkedEntity.brightness.OnChange -= OnColorChange;
            networkedEntity.saturation.OnChange -= OnColorChange;
        }
    }
    
    private void OnColorChange(byte prev, byte next, bool asServer) {
        if (!display) {
            return;
        }

        Vector4 hbcs = new Vector4(networkedEntity.hue.Value / 255f, networkedEntity.brightness.Value / 255f, 0.5f, networkedEntity.saturation.Value / 255f);
        foreach (var r in display.GetComponentsInChildren<Renderer>()) {
            foreach (var material in r.materials) {
                material.SetColor(BrightnessContrastSaturation, hbcs);
            }
        }
    }

    IEnumerator GrowRoutine() {
        growing = true;
        yield return new WaitForSeconds(30f);
        if (!networkedEntity.IsOwner) {
            yield break;
        }
        if (plant.possibleNextGenerations == null || plant.possibleNextGenerations.Length == 0f) {
            networkedEntity.Destroy();
            yield break;
        }

        networkedEntity.Spill(networkedEntity.volume);
        networkedEntity.SetAsset("Plant", plant.possibleNextGenerations[Random.Range(0, plant.possibleNextGenerations.Length)].name);
        growing = false;
    }

    void OnFilled(ReagentContents contents, GeneHolder.InjectType type) {
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

    
    // FIXME FISHNET
    //[PunRPC]
    private async Task SwitchToRPC(short newPlantID) {
        if (plantHandle != null) {
            plantHandle.Release();
        }
        plantHandle = await KoboldKareObjectPostProcessor.GetAssetAsync("Plant", newPlantID, GameManager.GetErrorPlant());
        SwitchTo(plantHandle.asset);
    }

    public void SwitchTo(ScriptablePlant newPlant) {
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
            display = Instantiate(newPlant.display,transform);
            OnColorChange(0,0, false);
        }

        
        // FIXME FISHNET
        /*
        if (PhotonNetwork.IsMasterClient) {
            foreach (var produce in newPlant.produces) {
                int spawnCount = Random.Range(produce.minProduce, produce.maxProduce);
                for(int i=0;i<spawnCount;i++) {
                    BitBuffer buffer = new BitBuffer(4);
                    buffer.AddKoboldGenes(GetGenes());
                    buffer.AddBool(false);
                    if (produce.prefab.GetOptionalDatabase() == GameManager.GetPlayerDatabase()) {
                        var speciesName = GameManager.GetPlayerDatabase().GetValidPrefabReferenceInfos()[GetGenes().species].GetKey();
                        PhotonNetwork.InstantiateRoomObject(speciesName, transform.position + Vector3.up + Random.insideUnitSphere * 0.5f, Quaternion.identity, 0, new object[] { buffer });
                    } else {
                        PhotonNetwork.InstantiateRoomObject(produce.prefab.photonName,
                             transform.position + Vector3.up + Random.insideUnitSphere * 0.5f, Quaternion.identity, 0,
                             new object[] { buffer });
                    }
                }
            }
        }*/
        switched?.Invoke();
        if (plant.possibleNextGenerations == null || plant.possibleNextGenerations.Length == 0) {
            StartCoroutine(GrowRoutine());
        }
    }

    
    // FIXME FISHNET
    /*
    public void OnPhotonInstantiate(PhotonMessageInfo info) {
        if (info.photonView.InstantiationData != null && info.photonView.InstantiationData[0] is BitBuffer) {
            BitBuffer buffer = (BitBuffer)info.photonView.InstantiationData[0];
            // Could be shared by other OnPhotonInstantiates.
            buffer.SetReadPosition(0);
            SetGenes(buffer.ReadKoboldGenes());
            if (PlantDatabase.TryGetAsset(buffer.ReadShort(), out var newPlant)) {
                SwitchTo(newPlant);
            } else {
                Debug.LogError("Tried to instantiate Plant with invalid plant ID!");
            }
            PhotonProfiler.LogReceive(buffer.Length);
        } else {
            SetGenes(new KoboldGenes().Randomize("Kobold"));
            Debug.LogWarning("Plant created without proper instantiation data!", gameObject);
        }
        
        planted?.Invoke(photonView.gameObject, plant);
    }*/

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
        node["plant"] = plant.name;
        node["position.x"] = transform.position.x;
        node["position.y"] = transform.position.y;
        node["position.z"] = transform.position.z;
        node["growing"] = growing;
    }

    public async Task Load(JSONNode node) {
        plantHandle = await KoboldKareObjectPostProcessor.GetAssetAsync("Plant", node["plant"].ToString(), GameManager.GetErrorPlant());
        SwitchTo(plantHandle.asset);
        float x = node.GetValueOrDefault("position.x", 0f);
        float y = node.GetValueOrDefault("position.y", 0f);
        float z = node.GetValueOrDefault("position.z", 0f);
        transform.position = new Vector3(x,y,z);
        
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

}
