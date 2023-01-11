using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using ExitGames.Client.Photon.StructWrapping;
using Naelstrof.Inflatable;
using Photon.Pun;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;

public class Fruit : MonoBehaviourPun, IDamagable, IAdvancedInteractable, IPunObservable, ISavable, IGrabbable,
    ISpoilable, IPunInstantiateMagicCallback {
    [SerializeField] private VisualEffect itemParticles;
    private Rigidbody body;
    [SerializeField] private VisualEffect gibSplash;
    private float health = 100f;
    public GenericReagentContainer.InspectorReagent startingReagent;
    private GenericReagentContainer container;
    private Renderer[] renderers;
    [SerializeField] private Inflatable fruitInflater;
    [SerializeField] private bool startFrozen = true;
    [SerializeField] private AudioPack gibSound;
    [SerializeField] private Transform centerTransform;

    private void SetFrozen(bool frozen) {
        itemParticles.enabled = frozen;
        body.constraints = frozen ? RigidbodyConstraints.FreezeAll : RigidbodyConstraints.None;
    }

    private bool GetFrozen() {
        return itemParticles.enabled;
    }

    private void Awake() {
        body = GetComponent<Rigidbody>();
        renderers = GetComponentsInChildren<Renderer>();
        container = gameObject.AddComponent<GenericReagentContainer>();
        container.type = GenericReagentContainer.ContainerType.Mouth;

        InflatableTransform inflatableTransform = new InflatableTransform();
        inflatableTransform.SetTransform(transform);
        fruitInflater.AddListener(inflatableTransform);

        fruitInflater.OnEnable();

        container.OnChange.AddListener(OnReagentContentsChanged);
        container.GetContents().AddMix(startingReagent.reagent.GetReagent(startingReagent.volume), container);
        OnReagentContentsChanged(container.GetContents(), GenericReagentContainer.InjectType.Inject);
        photonView.ObservedComponents.Add(container);
        if (centerTransform == null) {
            centerTransform = transform;
        }
    }

    void OnReagentContentsChanged(ReagentContents contents, GenericReagentContainer.InjectType type) {
        fruitInflater.SetSize(Mathf.Log(1f + contents.volume / 20f, 2f), this);
    }

    void Start() {
        SpoilableHandler.AddSpoilable(this);
        SetFrozen(startFrozen);
        PlayAreaEnforcer.AddTrackedObject(photonView);
    }

    private void OnDestroy() {
        SpoilableHandler.RemoveSpoilable(this);
        PlayAreaEnforcer.RemoveTrackedObject(photonView);
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.rigidbody != null && !collision.rigidbody.isKinematic && collision.impulse.magnitude > 0.1f) {
            SetFrozen(false);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(GetFrozen());
            stream.SendNext(health);
        } else {
            SetFrozen((bool)stream.ReceiveNext());
            health = (float)stream.ReceiveNext();
            PhotonProfiler.LogReceive(sizeof(bool) + sizeof(float));
        }
    }

    public void Save(JSONNode node) {
        node["frozen"] = GetFrozen();
        node["health"] = health;
    }

    public void Load(JSONNode node) {
        SetFrozen(node["frozen"]);
        health = node["health"];
    }

    public void InteractTo(Vector3 worldPosition, Quaternion worldRotation) {
    }

    public void OnInteract(Kobold k) {
        SetFrozen(false);
    }

    public void OnEndInteract() {
    }

    public bool PhysicsGrabbable() {
        return true;
    }

    public float GetHealth() {
        return health;
    }

    void Die() {
        GameObject obj = GameObject.Instantiate(gibSplash.gameObject);
        obj.transform.position = transform.position;
        VisualEffect effect = obj.GetComponentInChildren<VisualEffect>();
        effect.SetVector4("Color", container.GetColor());
        GameManager.instance.SpawnAudioClipInWorld(gibSound, transform.position);
        Destroy(obj, 5f);
        if (photonView.IsMine) {
            PhotonNetwork.Destroy(photonView.gameObject);
        }
    }

    [PunRPC]
    public void Damage(float amount) {
        health -= amount;
        if (health <= 0f) {
            Die();
            health = 0f;
        }

        PhotonProfiler.LogReceive(sizeof(float));
    }

    public void Heal(float amount) {
        health += amount;
    }

    [PunRPC]
    public void OnGrabRPC(int koboldID) {
        SetFrozen(false);
        PhotonProfiler.LogReceive(sizeof(int));
    }

    public bool CanGrab(Kobold kobold) {
        return true;
    }

    [PunRPC]
    public void OnReleaseRPC(int koboldId, Vector3 velocity) {
        PhotonProfiler.LogReceive(sizeof(int)+sizeof(float)*3);
    }

    public Transform GrabTransform() {
        return centerTransform;
    }

    public void OnSpoil() {
        Die();
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info) {
        FarmSpawnEventHandler.TriggerProduceSpawn(gameObject);
    }
}
