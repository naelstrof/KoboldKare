using System.Collections;
using System.Collections.Generic;
using System.IO;
using KoboldKare;
using Photon.Pun;
using UnityEngine;
using UnityEngine.VFX;

public class GenericPurchasable : GenericUsable {

    [SerializeField]
    private ScriptableFloat money;
    [SerializeField]
    private Sprite displaySprite;

    [SerializeField]
    private ScriptablePurchasable purchasable;
    [SerializeField]
    private AudioPack purchaseSoundPack;
    private bool inStock = true;
    [SerializeField]
    private GameEventGeneric restockEvent;
    private GameObject display;
    private AudioSource source;
    [SerializeField]
    private Transform textTransform;
    [SerializeField]
    private TMPro.TMP_Text text;

    public ScriptablePurchasable GetPurchasable() => purchasable;
    public delegate void PurchasableChangedAction(ScriptablePurchasable newPurchasable);
    public PurchasableChangedAction purchasableChanged;
    void Start() {
        source = new AudioSource();
        source = gameObject.AddComponent<AudioSource>();
        source.spatialBlend = 1f;
        source.rolloffMode = AudioRolloffMode.Custom;
        source.minDistance = 0f;
        source.maxDistance = 25f;
        source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, GameManager.instance.volumeCurve);
        source.outputAudioMixerGroup = GameManager.instance.soundEffectGroup;
        if (restockEvent != null) {
            restockEvent.AddListener(OnEventRaised);
        }
        SwapTo(purchasable, true);
    }
    public override Sprite GetSprite(Kobold k) {
        return displaySprite;
    }
    void OnEnable() {
        StartCoroutine(UpdateRoutine());
    }
    // Disables all components except for renderers. also returns the bounds of the renderers
    Bounds DisableAllButGraphics(GameObject target) {
        Bounds centerBounds = new Bounds(transform.position, Vector3.zero);
        foreach(Component c in target.GetComponentsInChildren<Component>()) {
            if (c is Renderer) {
                centerBounds.Encapsulate((c as Renderer).bounds);
                continue;
            }
            if (c is MeshFilter || c is LODGroup) {
                continue;
            }
            if (c is Behaviour) {
                (c as Behaviour).enabled = false;
            }
            if (c is Rigidbody) {
                (c as Rigidbody).isKinematic = true;
            }
        }
        return centerBounds;
    }
    void SwapTo(ScriptablePurchasable newPurchasable, bool forceRefresh = false) {
        if (purchasable == newPurchasable && !forceRefresh) {
            return;
        }
        if (display != null) {
            Destroy(display);
        }
        purchasable = newPurchasable;
        display = GameObject.Instantiate(purchasable.display, transform);
        Bounds centerBounds = DisableAllButGraphics(display);
        textTransform.position = centerBounds.center;
        textTransform.localScale = Vector3.one * centerBounds.size.magnitude;
        display.SetActive(inStock);
        text.text = purchasable.cost.ToString();
        purchasableChanged?.Invoke(purchasable);
    }
    IEnumerator UpdateRoutine() {
        while(isActiveAndEnabled) {
            // Every few frames we update
            for(int i=0;i<2;i++) {
                yield return null;
            }
            // Skip if camera is null
            if (Camera.main == null) {
                continue;
            }
            float distance = textTransform.DistanceTo(Camera.main.transform);
            textTransform.LookAt(Camera.main.transform, Vector3.up);
            text.alpha = Mathf.Clamp01(10f-distance);
        }
    }
    void OnDestroy() {
        if (restockEvent != null) {
            restockEvent.RemoveListener(OnEventRaised);
        }
    }
    void OnEventRaised(object nothing) {
        if (!display.activeInHierarchy) {
            display.SetActive(true);
        }
    }
    public override bool CanUse(Kobold k) {
        return display.activeInHierarchy && money.has(purchasable.cost);
    }
    public override void Use(Kobold k) {
        base.Use(k);
        source.PlayOneShot(purchaseSoundPack.GetRandomClip(), purchaseSoundPack.volume);
        if (photonView.IsMine) {
            money.charge(purchasable.cost);
            PhotonNetwork.Instantiate(purchasable.spawnPrefab.photonName, transform.position, Quaternion.identity);
        }
        display.SetActive(false);
    }
    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        base.OnPhotonSerializeView(stream, info);
        if (stream.IsWriting) {
            stream.SendNext(inStock);
            stream.SendNext(PurchasableDatabase.GetID(purchasable));
        } else {
            inStock = (bool)stream.ReceiveNext();
            display.SetActive(inStock);
            short currentPurchasable = (short)stream.ReceiveNext();
            SwapTo(PurchasableDatabase.GetPurchasable(currentPurchasable));
        }
    }
    public override void Save(BinaryWriter writer, string version) {
        base.Save(writer, version);
        writer.Write(inStock);
        writer.Write(PurchasableDatabase.GetID(purchasable));
    }

    public override void Load(BinaryReader reader, string version) {
        base.Load(reader, version);
        inStock = reader.ReadBoolean();
        display.SetActive(inStock);
        short currentPurchasable = (short)reader.ReadInt16();
        SwapTo(PurchasableDatabase.GetPurchasable(currentPurchasable));
    }
}
