using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NetStack.Quantization;
using NetStack.Serialization;
using Photon.Pun;
using SimpleJSON;
using UnityEngine;
using UnityEngine.VFX;

public class Projectile : GeneHolder, IPunObservable, ISavable, IPunInstantiateMagicCallback {
    private Vector3 velocity;
    [SerializeField]
    private GameObject splash;
    [SerializeField]
    private GameObject projectile;
    [SerializeField]
    private AudioPack splashSound;
    
    [SerializeField] private VisualEffect splashEffect;
    [SerializeField] private Renderer projectileBlob;
    [SerializeField] private Material decalProjector;
    [SerializeField] private Material decalProjectorSubtractive;
    private ReagentContents contents;
    private HashSet<Collider> ignoreColliders;
    private static Collider[] colliders = new Collider[32];
    private static RaycastHit[] raycastHits = new RaycastHit[32];
    private static HashSet<GenericReagentContainer> hitContainers = new HashSet<GenericReagentContainer>();
    private bool splashed = false;
    private static readonly int FluidColor = Shader.PropertyToID("_FluidColor");
    private AudioSource splashSoundSource;
    private static readonly int ColorID = Shader.PropertyToID("_Color");

    private void Awake() {
        ignoreColliders ??= new HashSet<Collider>();
    }

    private void OnDestroy() {
        PlayAreaEnforcer.RemoveTrackedObject(photonView);
    }

    void Update() {
        if (splashed) {
            return;
        }

        velocity += Physics.gravity * Time.deltaTime;
        int hits = Physics.SphereCastNonAlloc(transform.position, 0.25f, velocity.normalized, raycastHits, velocity.magnitude * Time.deltaTime,
            GameManager.instance.waterSprayHitMask, QueryTriggerInteraction.Ignore);
        int closestHit = -1;
        float closestDistance = float.MaxValue;
        for (int i = 0; i < hits; i++) {
            if (ignoreColliders.Contains(raycastHits[i].collider) || raycastHits[i].point == Vector3.zero) {
                continue;
            }

            if (raycastHits[i].distance < closestDistance) {
                closestHit = i;
                closestDistance = raycastHits[i].distance;
            }
        }

        if (closestHit != -1) {
            transform.position = raycastHits[closestHit].point + raycastHits[closestHit].normal*0.1f;
            OnSplash();
        } else {
            transform.position += velocity * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(velocity, Vector3.up);
        }
    }

    public void LaunchFrom(Rigidbody body) {
        ignoreColliders = new HashSet<Collider>();
        foreach (Collider collider in body.GetComponentsInChildren<Collider>()) {
            ignoreColliders.Add(collider);
        }
    }

    private void Start() {
        splashSoundSource = gameObject.AddComponent<AudioSource>();
        splashSoundSource.maxDistance = 20f;
        splashSoundSource.minDistance = 0.25f;
        splashSoundSource.spatialBlend = 1f;
        splashSoundSource.playOnAwake = false;
        splashSoundSource.loop = false;
        splashSoundSource.rolloffMode = AudioRolloffMode.Linear;
        PlayAreaEnforcer.AddTrackedObject(photonView);
    }

    private void OnSplash() {
        Color color = contents.GetColor();
        decalProjector.SetColor(ColorID, color);
        decalProjectorSubtractive.SetColor(ColorID, color);
        SkinnedMeshDecals.PaintDecal.RenderDecalInSphere(transform.position, 1f, contents.IsCleaningAgent() ? decalProjectorSubtractive : decalProjector,
            Quaternion.identity,
            GameManager.instance.decalHitMask);

        splash.SetActive(true);
        projectile.SetActive(false);
        splashed = true;
        if (photonView.IsMine) {
            hitContainers.Clear();
            int hits = Physics.OverlapSphereNonAlloc(transform.position, 1f, colliders,
                GameManager.instance.waterSprayHitMask);
            for (int i = 0; i < hits; i++) {
                GenericReagentContainer container = colliders[i].GetComponentInParent<GenericReagentContainer>();
                if (container != null) {
                    hitContainers.Add(container);
                }
            }
            float perVolume = contents.volume / hitContainers.Count;
            foreach (GenericReagentContainer container in hitContainers) {
                BitBuffer buffer = new BitBuffer(4);
                buffer.AddReagentContents(contents.Spill(perVolume));
                container.photonView.RPC(nameof(GenericReagentContainer.AddMixRPC), RpcTarget.All,
                    buffer, photonView.ViewID, (byte)GenericReagentContainer.InjectType.Spray);
            }
        }

        if (photonView.IsMine) {
            StartCoroutine(DestroyAfterTime());
        }
        transform.rotation = Quaternion.identity;

        if (splashSoundSource != null) {
            splashSound.Play(splashSoundSource);
        }
    }

    private IEnumerator DestroyAfterTime() {
        yield return new WaitForSeconds(5f);
        PhotonNetwork.Destroy(photonView);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(velocity);
            stream.SendNext(splashed);
        } else {
            velocity = (Vector3)stream.ReceiveNext();
            bool newSplash = (bool)stream.ReceiveNext();
            if (!splashed && newSplash) {
                OnSplash();
            }
            splashed = newSplash;
            PhotonProfiler.LogReceive(sizeof(float) * 3 + sizeof(bool));
        }
    }

    public void Save(JSONNode node) {
        node["velocity.x"] = velocity.x;
        node["velocity.y"] = velocity.y;
        node["velocity.z"] = velocity.z;
        contents.Save(node, "fluidContents");
        bool hasGenes = GetGenes() != null;
        node["hasGenes"] = hasGenes;
        if (hasGenes) {
            GetGenes().Save(node,"genes");
        }
        node["splashed"] = splashed;
    }

    public void Load(JSONNode node) {
        float vx = node["velocity.x"];
        float vy = node["velocity.y"];
        float vz = node["velocity.z"];
        velocity = new Vector3(vx, vy, vz);
        contents = new ReagentContents();
        contents.Load(node, "fluidContents");
        bool hasGenes = node["hasGenes"];
        if (hasGenes) {
            KoboldGenes loadedGenes = new KoboldGenes();
            loadedGenes.Load(node,"genes");
            SetGenes(loadedGenes);
        }
        bool newSplashed = node["splashed"];
        if (newSplashed && !splashed) {
            OnSplash();
        }
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info) {
        if (info.photonView == null || info.photonView.InstantiationData == null || info.photonView.InstantiationData[0] is not BitBuffer) {
            throw new UnityException("Projectile created without proper instantiation data!");
        }

        BitBuffer buffer = (BitBuffer)info.photonView.InstantiationData[0];
        
        contents = buffer.ReadReagentContents();
        Color color = contents.GetColor();
        splashEffect.SetVector4("Color", new Vector4(color.r,color.g,color.b, color.a));
        projectileBlob.material.SetColor(FluidColor, color);

        float vx = HalfPrecision.Dequantize(buffer.ReadUShort());
        float vy = HalfPrecision.Dequantize(buffer.ReadUShort());
        float vz = HalfPrecision.Dequantize(buffer.ReadUShort());
        velocity = new Vector3(vx, vy, vz);
        
        splashed = false;
        SetGenes(buffer.ReadKoboldGenes());
        PhotonProfiler.LogReceive(buffer.Length);
    }
}
