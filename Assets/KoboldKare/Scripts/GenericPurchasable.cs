using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FishNet;
using KoboldKare;
using Photon.Pun;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;

public class GenericPurchasable : MonoBehaviour {

    [SerializeField]
    public PhotonGameObjectReference spawn;
    [SerializeField]
    private float price;

    [SerializeField]
    private Sprite displaySprite;

    [SerializeField]
    private Shader displayShader;

    private struct AssetGroupKeyPair {
        public string group;
        public string key;
    }
    private AssetGroupKeyPair purchaseable;
    
    [SerializeField]
    private AudioPack purchaseSoundPack;
    public bool inStock {
        get {
            if (display) {
                return display.activeInHierarchy;
            }
            return false;
        }
    }
    private GameObject display;
    private AudioSource source;
    [SerializeField]
    private MoneyFloater floater;

    private NetworkedEntity networkedEntity;

    public float GetPrice() => price;

    public virtual void Start() {
        source = gameObject.AddComponent<AudioSource>();
        source.spatialBlend = 1f;
        source.rolloffMode = AudioRolloffMode.Custom;
        source.minDistance = 0f;
        source.maxDistance = 25f;
        source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, GameManager.instance.volumeCurve);
        source.outputAudioMixerGroup = GameManager.instance.soundEffectGroup;
        if (spawn != null && spawn.TryGetAssetGroupAndKey(out var group, out var key)) {
            SwapTo(group, key);
        }

        networkedEntity = GetComponentInParent<NetworkedEntity>();
        if (networkedEntity) {
            networkedEntity.SetSprite(displaySprite);
            networkedEntity.useRequested += OnUseRequested;
            networkedEntity.used += OnUse;
        }
    }

    public static GameObject GenerateDisplay(GameObject targetPrefab, Shader displayShader, Transform parent) {
        GameObject newDisplay = new GameObject("Display");
        newDisplay.transform.SetParent(parent);
        newDisplay.transform.localPosition = Vector3.zero;
        newDisplay.transform.localRotation = Quaternion.identity;
        List<Renderer> displayRenderers = new List<Renderer>(targetPrefab.GetComponentsInChildren<Renderer>());
        LODGroup checkGroup = targetPrefab.GetComponentInChildren<LODGroup>();
        if (checkGroup != null) {
            var lods = checkGroup.GetLODs();
            for (int i = 1; i < lods.Length; i++) {
                var renderers = lods[i].renderers;
                foreach (var t in renderers) {
                    if (displayRenderers.Contains(t)) {
                        displayRenderers.Remove(t);
                    }
                }
            }
        }

        foreach (var r in displayRenderers) {
            if (!r.enabled) {
                continue;
            }

            if (r is SkinnedMeshRenderer targetSkinnedMeshRenderer) {
                var obj = new GameObject("DisplayPart", typeof(MeshRenderer), typeof(MeshFilter));
                obj.transform.SetParent(newDisplay.transform);
                obj.transform.localPosition = targetPrefab.transform.InverseTransformPoint(r.transform.position);
                obj.transform.localRotation = Quaternion.Inverse(targetPrefab.transform.rotation) * r.transform.rotation;
                if (targetSkinnedMeshRenderer.rootBone == null) {
                    obj.transform.localScale = targetSkinnedMeshRenderer.transform.lossyScale;
                } else {
                    obj.transform.localScale = targetSkinnedMeshRenderer.rootBone.lossyScale;
                }
                var meshRenderer = obj.GetComponent<MeshRenderer>();
                List<Material> newMaterials = new List<Material>();
                foreach (var mat in targetSkinnedMeshRenderer.sharedMaterials) {
                    newMaterials.Add(Material.Instantiate(mat));
                }

                foreach (var mat in newMaterials) {
                    mat.shader = displayShader;
                }

                meshRenderer.materials = newMaterials.ToArray();
                var meshFilter = obj.GetComponent<MeshFilter>();
                meshFilter.sharedMesh = targetSkinnedMeshRenderer.sharedMesh;
            }

            if (r is MeshRenderer targetMeshRenderer) {
                var obj = new GameObject("DisplayPart", typeof(MeshRenderer), typeof(MeshFilter));
                obj.transform.SetParent(newDisplay.transform);
                obj.transform.localPosition = targetPrefab.transform.InverseTransformPoint(r.transform.position);
                obj.transform.localRotation =
                    Quaternion.Inverse(targetPrefab.transform.rotation) * r.transform.rotation;
                obj.transform.localScale = r.transform.lossyScale;
                var meshRenderer = obj.GetComponent<MeshRenderer>();
                List<Material> newMaterials = new List<Material>();
                foreach (var mat in targetMeshRenderer.sharedMaterials) {
                    newMaterials.Add(Material.Instantiate(mat));
                }

                foreach (var mat in newMaterials) {
                    mat.shader = displayShader;
                }

                meshRenderer.sharedMaterials = newMaterials.ToArray();
                var meshFilter = obj.GetComponent<MeshFilter>();
                var targetMeshFilter = r.GetComponent<MeshFilter>();
                meshFilter.sharedMesh = targetMeshFilter.sharedMesh;
            }
        }

        return newDisplay;
    }

    private async void SwapTo(string group, string targetPurchasableKey) {
        if (purchaseable.key == targetPurchasableKey && purchaseable.group == group) {
            return;
        }
        
        if (display != null) {
            Destroy(display);
        }

        purchaseable = new AssetGroupKeyPair() {
            group = group,
            key = targetPurchasableKey
        };
        
        var assetHandle = await KoboldKareObjectPostProcessor.GetAssetAsync(group, targetPurchasableKey, GameManager.GetErrorGeneric());

        var targetPrefab = assetHandle.asset;
        display = GenerateDisplay(targetPrefab, displayShader, transform);
        
        Bounds encapsulate = new Bounds(transform.position, Vector3.zero);
        foreach(var r in display.GetComponentsInChildren<Renderer>()) {
            encapsulate.Encapsulate(r.bounds);
        }
        floater.SetBounds(encapsulate);
        display.SetActive(inStock);
        floater.SetText(price.ToString());
        
        assetHandle.Release();
    }
    public virtual void OnDestroy() {
    }
    public virtual void OnRestock(object nothing) {
        if (spawn.TryGetAssetGroupAndKey(out var group, out var key)) {
            SwapTo(group, key);
        }

        if (!display.activeInHierarchy) {
            display.SetActive(true);
            floater.gameObject.SetActive(true);
        }
    }
    private void OnUse(NetworkedKobold k) {
        if (k.TryGetKobold(out var kobold)) {
            k.ChargeMoney(networkedEntity);
            purchaseSoundPack.Play(source);
            floater.gameObject.SetActive(false);
            display.SetActive(false);
            
            var data = KoboldEntitySpawner.NetworkedEntityInstantiationData.Default();
            data.assetName = purchaseable.key;
            data.groupName = purchaseable.group;
            data.position = transform.position;
            
            InstanceFinder.NetworkManager.GetComponent<KoboldEntitySpawner>().SpawnAsServer(data, true, k.Owner);
            
            StartCoroutine(Restock());
        }
    }
    private bool OnUseRequested(NetworkedKobold k) {
        return (display != null && display.activeInHierarchy) && (k.TryGetKobold(out var kobold) && kobold.GetComponent<MoneyHolder>().HasMoney(price));
    }
    
    private IEnumerator Restock() {
        yield return new WaitForSeconds(30f);
        OnRestock(null);
    }
}
