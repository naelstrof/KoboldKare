using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using ExitGames.Client.Photon.StructWrapping;
using Naelstrof.Inflatable;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;

public class Fruit : MonoBehaviourPun, IDamagable, IAdvancedInteractable, IPunObservable, ISavable, IGrabbable, ISpoilable {
    [SerializeField] private VisualEffect itemParticles;
    private Rigidbody body;
    [SerializeField] private VisualEffect gibSplash;
    private float health = 100f;
    [SerializeField] private GenericReagentContainer.InspectorReagent startingReagent;
    private GenericReagentContainer container;
    private Rigidbody[] bodies;
    private Renderer[] renderers;
    [SerializeField]
    private Inflatable fruitInflater;
    [SerializeField]
    private bool startFrozen = true;
    [SerializeField]
    private AudioPack gibSound;
    
    private void SetFrozen(bool frozen) {
        itemParticles.enabled = frozen;
        body.constraints = frozen ? RigidbodyConstraints.FreezeAll : RigidbodyConstraints.None;
    }
    private bool GetFrozen() {
        return body.constraints == RigidbodyConstraints.FreezeAll;
    }

    private void Awake() {
        body = GetComponent<Rigidbody>();
        bodies = new Rigidbody[1];
        bodies[0] = body;
        renderers = GetComponentsInChildren<Renderer>();
        container = gameObject.AddComponent<GenericReagentContainer>();
        container.type = GenericReagentContainer.ContainerType.Mouth;
        container.maxVolume = startingReagent.volume;
        
        InflatableTransform inflatableTransform = new InflatableTransform();
        inflatableTransform.SetTransform(transform);
        fruitInflater.AddListener(inflatableTransform);
        
        fruitInflater.OnEnable();
        
        container.OnChange.AddListener(OnReagentContentsChanged);
        photonView.ObservedComponents.Add(container);
    }

    void OnReagentContentsChanged(GenericReagentContainer.InjectType type) {
        fruitInflater.SetSize(Mathf.Log(1f + container.volume/20f, 2f), this);
    }

    void Start() {
        SpoilableHandler.AddSpoilable(this);
        container.AddMix(startingReagent.reagent, startingReagent.volume, GenericReagentContainer.InjectType.Inject);
        SetFrozen(startFrozen);
    }

    private void OnDestroy() {
        SpoilableHandler.RemoveSpoilable(this);
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
        }
    }

    public void Save(BinaryWriter writer, string version) {
        writer.Write(GetFrozen());
        writer.Write(health);
    }

    public void Load(BinaryReader reader, string version) {
        SetFrozen(reader.ReadBoolean());
        health = reader.ReadSingle();
    }

    public void InteractTo(Vector3 worldPosition, Quaternion worldRotation) {
    }

    public void OnInteract(Kobold k) {
        SetFrozen(false);
    }

    public void OnEndInteract(Kobold k) {
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

    public void Damage(float amount) {
        health -= amount;
        if (health <= 0f) {
            Die();
            health = 0f;
        }
    }

    public void Heal(float amount) {
        health += amount;
    }

    public bool OnGrab(Kobold kobold) {
        SetFrozen(false);
        return true;
    }

    public void OnRelease(Kobold kobold) {
    }

    public void OnThrow(Kobold kobold) {
    }

    public Rigidbody[] GetRigidBodies() {
        return bodies;
    }

    public Renderer[] GetRenderers() {
        return renderers;
    }

    public Transform GrabTransform(Rigidbody body) {
        return transform;
    }

    public void OnSpoil() {
        Die();
    }
}
