using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using KoboldKare;
using Photon.Pun;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;

public class GenericPurchasable : GenericUsable, IPunObservable, ISavable {

    [SerializeField]
    public PhotonGameObjectReference spawn;
    [SerializeField]
    private float price;

    [SerializeField]
    private Sprite displaySprite;

    [SerializeField]
    private Shader displayShader;

    [SerializeField]
    private ScriptablePurchasable purchasable;
    
    private string purchasablePhotonName;
    
    [SerializeField]
    private AudioPack purchaseSoundPack;
    private bool inStock {
        get {
            return display.activeInHierarchy;
        }
    }
    private GameObject display;
    private AudioSource source;
    [SerializeField]
    private MoneyFloater floater;

    //public ScriptablePurchasable GetPurchasable() => purchasable;
    public delegate void PurchasableChangedAction(ScriptablePurchasable newPurchasable);
    public virtual void Start() {
        source = gameObject.AddComponent<AudioSource>();
        source.spatialBlend = 1f;
        source.rolloffMode = AudioRolloffMode.Custom;
        source.minDistance = 0f;
        source.maxDistance = 25f;
        source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, GameManager.instance.volumeCurve);
        source.outputAudioMixerGroup = GameManager.instance.soundEffectGroup;
        SwapTo(spawn.photonName);
    }
    public override Sprite GetSprite(Kobold k) {
        return displaySprite;
    }
    protected void SwapTo(string targetPurchasable) {
        if (purchasablePhotonName == targetPurchasable || !ModManager.GetReady()) {
            return;
        }
        if (display != null) {
            Destroy(display);
        }
        purchasablePhotonName = targetPurchasable;
        display = new GameObject("Display");
        display.transform.SetParent(transform);
        display.transform.localPosition = Vector3.zero;
        display.transform.localRotation = Quaternion.identity;

        var targetPrefab = ((DefaultPool)PhotonNetwork.PrefabPool).ResourceCache[targetPurchasable];
        Bounds encapsulate = new Bounds(transform.position, Vector3.zero);

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
                var obj = new GameObject("DisplayPart",typeof(MeshRenderer), typeof(MeshFilter));
                obj.transform.SetParent(display.transform);
                obj.transform.localPosition = targetPrefab.transform.InverseTransformPoint(r.transform.position);
                obj.transform.localRotation = Quaternion.Inverse(targetPrefab.transform.rotation)*r.transform.rotation;
                obj.transform.localScale = targetSkinnedMeshRenderer.rootBone.lossyScale;
                var meshRenderer = obj.GetComponent<MeshRenderer>();
                List<Material> newMaterials = new List<Material>();
                foreach(var mat in targetSkinnedMeshRenderer.sharedMaterials) {
                    newMaterials.Add(Material.Instantiate(mat));
                }
                foreach (var mat in newMaterials) {
                    mat.shader = displayShader;
                }
                meshRenderer.materials = newMaterials.ToArray();
                var meshFilter = obj.GetComponent<MeshFilter>();
                meshFilter.sharedMesh = targetSkinnedMeshRenderer.sharedMesh;
                encapsulate.Encapsulate(meshRenderer.bounds);
            }

            if (r is MeshRenderer targetMeshRenderer) {
                var obj = new GameObject("DisplayPart",typeof(MeshRenderer), typeof(MeshFilter));
                obj.transform.SetParent(display.transform);
                obj.transform.localPosition = targetPrefab.transform.InverseTransformPoint(r.transform.position);
                obj.transform.localRotation = Quaternion.Inverse(targetPrefab.transform.rotation)*r.transform.rotation;
                obj.transform.localScale = r.transform.lossyScale;
                var meshRenderer = obj.GetComponent<MeshRenderer>();
                List<Material> newMaterials = new List<Material>();
                foreach(var mat in targetMeshRenderer.sharedMaterials) {
                    newMaterials.Add(Material.Instantiate(mat));
                }
                foreach (var mat in newMaterials) {
                    mat.shader = displayShader;
                }
                meshRenderer.sharedMaterials = newMaterials.ToArray();
                var meshFilter = obj.GetComponent<MeshFilter>();
                var targetMeshFilter = r.GetComponent<MeshFilter>();
                meshFilter.sharedMesh = targetMeshFilter.sharedMesh;
                encapsulate.Encapsulate(targetMeshRenderer.bounds);
            }

        }

        //Bounds centerBounds = ScriptablePurchasable.DisableAllButGraphics(display);
        floater.SetBounds(encapsulate);
        display.SetActive(inStock);
        floater.SetText(price.ToString());
    }
    public virtual void OnDestroy() {
    }
    public virtual void OnRestock(object nothing) {
        if (!display.activeInHierarchy) {
            display.SetActive(true);
            floater.gameObject.SetActive(true);
        }
    }
    public override void LocalUse(Kobold k) {
        //base.LocalUse(k);
        photonView.RPC("RPCUse", RpcTarget.All);
        k.GetComponent<MoneyHolder>().ChargeMoney(price);
    }
    public override bool CanUse(Kobold k) {
        return display.activeInHierarchy && (k == null || k.GetComponent<MoneyHolder>().HasMoney(price));
    }
    [PunRPC]
    public override void Use() {
        purchaseSoundPack.Play(source);
        floater.gameObject.SetActive(false);
        display.SetActive(false);
        if (PhotonNetwork.IsMasterClient && !string.IsNullOrEmpty(purchasablePhotonName)) {
            PhotonNetwork.InstantiateRoomObject(purchasablePhotonName, transform.position, Quaternion.identity);
            StartCoroutine(Restock());
        }
        PhotonProfiler.LogReceive(1);
    }
    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(inStock);
            stream.SendNext(purchasablePhotonName);
        } else {
            display.SetActive((bool)stream.ReceiveNext());
            string currentPurchasable = (string)stream.ReceiveNext();
            SwapTo(currentPurchasable);
            PhotonProfiler.LogReceive(sizeof(bool)+currentPurchasable.Length);
        }
    }
    public override void Save(JSONNode node) {
        base.Save(node);
        node["inStock"] = inStock;
        node["purchasable"] = purchasablePhotonName;
    }

    public override void Load(JSONNode node) {
        base.Load(node);
        display.SetActive(node["inStock"]);
        if (node.HasKey("purchasable")) {
            SwapTo(node["purchasable"]);
        } else {
            SwapTo(spawn.photonName);
        }
    }

    private IEnumerator Restock() {
        yield return new WaitForSeconds(30f);
        OnRestock(null);
    }
}
