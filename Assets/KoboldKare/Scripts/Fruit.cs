using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using Naelstrof.Inflatable;
using Photon.Pun;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;

public class Fruit : MonoBehaviour, IAdvancedInteractable, ISavable, ISpoilable {
    [SerializeField] private VisualEffect itemParticles;
    private Rigidbody body;
    [SerializeField] private VisualEffect gibSplash;
    private float health = 100f;
    public GenericReagentContainer.InspectorReagent startingReagent;
    private Renderer[] renderers;
    [SerializeField] private Inflatable fruitInflater;
    [SerializeField] private bool startFrozen = true;
    [SerializeField] private AudioPack gibSound;
    [SerializeField] private Transform centerTransform;
    private NetworkedEntity networkedEntity;

    private void Awake() {
        body = GetComponent<Rigidbody>();
        renderers = GetComponentsInChildren<Renderer>();

        InflatableTransform inflatableTransform = new InflatableTransform();
        inflatableTransform.SetTransform(transform);
        fruitInflater.AddListener(inflatableTransform);

        fruitInflater.OnEnable();
        // FIXME FISHNET
        //photonView.ObservedComponents.Add(container);
        if (centerTransform == null) {
            centerTransform = transform;
        }
    }

    void OnReagentContentsChanged(ReagentContents prev, ReagentContents next, bool asServer) {
        fruitInflater.SetSize(Mathf.Max(Mathf.Log(1f + next.volume / 20f, 2f),0.15f), this);
    }

    void Start() {
        SpoilableHandler.AddSpoilable(this);
        networkedEntity = GetComponentInParent<NetworkedEntity>();
        networkedEntity.SetFrozen(startFrozen);
        networkedEntity.grabbed += OnGrab;
        networkedEntity.released += OnRelease;
        networkedEntity.grabRequested += OnGrabRequested;
        networkedEntity.SetGrabTransform(centerTransform);
        networkedEntity.frozen.OnChange += OnFrozenChanged;
        networkedEntity.health.OnChange += OnHealthChanged;
        networkedEntity.type = GeneHolder.ContainerType.Mouth;

        networkedEntity.reagentContents.OnChange += OnReagentContentsChanged;
        networkedEntity.AddMix(startingReagent.reagent.GetReagent(startingReagent.volume), GeneHolder.InjectType.Inject);
        OnReagentContentsChanged(networkedEntity.reagentContents.Value, networkedEntity.reagentContents.Value, true);
        // FIXME FISHNET
        //PlayAreaEnforcer.AddTrackedObject(photonView);
    }

    private void OnHealthChanged(float prev, float next, bool asServer) {
        if (prev > 0f && next <= 0f) {
            Die();
        }
    }

    private void OnFrozenChanged(bool prev, bool next, bool asServer) {
        itemParticles.enabled = next;
        body.constraints = next ? RigidbodyConstraints.FreezeAll : RigidbodyConstraints.None;
    }

    private void OnDestroy() {
        SpoilableHandler.RemoveSpoilable(this);
        // FIXME FISHNET
        //PlayAreaEnforcer.RemoveTrackedObject(photonView);
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.rigidbody != null && !collision.rigidbody.isKinematic && collision.impulse.magnitude > 0.1f) {
            networkedEntity.SetFrozen(false);
        }
    }

    // FIXME FISHNET
    /*
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(GetFrozen());
            stream.SendNext(health);
        } else {
            SetFrozen((bool)stream.ReceiveNext());
            health = (float)stream.ReceiveNext();
            PhotonProfiler.LogReceive(sizeof(bool) + sizeof(float));
        }
    }*/

    public void Save(JSONNode node) {
        node["frozen"] = networkedEntity.frozen.Value;
        node["health"] = health;
    }

    public Task Load(JSONNode node) {
        networkedEntity.SetFrozen(node["frozen"]);
        health = node["health"];
        return Task.CompletedTask;
    }

    public void InteractTo(Vector3 worldPosition, Quaternion worldRotation) {
    }

    // FIXME FISHNET
    public void OnInteract(Kobold k) {
        networkedEntity.SetFrozen(false);
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
        effect.SetVector4("Color", networkedEntity.GetColor());
        GameManager.instance.SpawnAudioClipInWorld(gibSound, transform.position);
        Destroy(obj, 5f);
        // FIXME FISHNET
        /*
        if (photonView.IsMine) {
            PhotonNetwork.Destroy(photonView.gameObject);
        }*/
    }

    private void OnGrab(NetworkedKobold by) {
        networkedEntity.SetFrozen(false);
    }

    private bool OnGrabRequested(NetworkedKobold by) {
        return true;
    }
    private void OnRelease(NetworkedKobold by, Vector3 velocity) {
    }

    public void OnSpoil() {
        Die();
    }

    // FIXME FISHNET
    /*public void OnPhotonInstantiate(PhotonMessageInfo info) {
        FarmSpawnEventHandler.TriggerProduceSpawn(gameObject);
    }*/
}
