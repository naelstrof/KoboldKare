using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Vilar.AnimationStation;
using System.Collections.ObjectModel;
using System.IO;
using SimpleJSON;

public class GrinderManager : UsableMachine, IAnimationStationSet {
    public delegate void GrindedObjectAction(int viewID, ReagentContents contents);

    public static event GrindedObjectAction grindedObject;
        
    [SerializeField]
    private Sprite onSprite;

    [SerializeField] private List<Collider> cylinderColliders;
    public AudioSource grindSound;
    public Animator animator;
    public AudioSource deny;
    //public GenericReagentContainer container;
    [SerializeField] private AnimationStation station;
    private ReadOnlyCollection<AnimationStation> stations;
    [SerializeField]
    private FluidStream fluidStream;
    private HashSet<PhotonView> grindedThingsCache;
    private bool grinding;
    [SerializeField]
    private Collider grindingCollider;
    [SerializeField]
    private GenericReagentContainer container;

    // added by Godeken
    [SerializeField] private Animator anim;
    [SerializeField] private float animMinSpeed = 0.9f;
    [SerializeField] private float animMaxSpeed = 1.2f;

    public override Sprite GetSprite(Kobold k) {
        return onSprite;
    }
    public override bool CanUse(Kobold k) {
        return k.GetEnergy() >= 1f && !grinding && station.info.user == null && constructed;
    }

    public override void LocalUse(Kobold k) {
        k.photonView.RPC(nameof(CharacterControllerAnimator.BeginAnimationRPC), RpcTarget.All, photonView.ViewID, 0);
        base.LocalUse(k);
    }

    [PunRPC]
    private void BeginGrind() {
        grinding = true;
        animator.SetBool("Grinding", true);
        grindSound.enabled = true;
        grindSound.Play();
        foreach (Collider cylinderCollider in cylinderColliders) {
            cylinderCollider.enabled = false;
        }
        PhotonProfiler.LogReceive(1);
    }

    [PunRPC]
    private void StopGrind() {
        grinding = false;
        animator.SetBool("Grinding", false);
        grindSound.Stop();
        grindSound.enabled = false;
        foreach (Collider cylinderCollider in cylinderColliders) {
            cylinderCollider.enabled = true;
        }
        PhotonProfiler.LogReceive(1);
    }

    IEnumerator WaitThenConsumeEnergy() {

        //added by Godeken
        yield return new WaitForSeconds(1f);
        anim.SetTrigger("Cranking");
        anim.speed = animMinSpeed;
        yield return new WaitForSeconds(3f);
        anim.speed = animMaxSpeed;
        yield return new WaitForSeconds(4f);
        

        if (!photonView.IsMine) {
            yield break;
        }
        if (station.info.user == null) {
            yield break;
        }
        
        if (station.info.user.TryConsumeEnergy(1)) {
            station.info.user.photonView.RPC(nameof(CharacterControllerAnimator.StopAnimationRPC), RpcTarget.All);
            photonView.RPC(nameof(BeginGrind), RpcTarget.All);
            yield return new WaitForSeconds(18f);
            photonView.RPC(nameof(StopGrind), RpcTarget.All);
        }
    }

    protected override void Start() {
        base.Start();
        grindedThingsCache = new HashSet<PhotonView>();
        List<AnimationStation> tempList = new List<AnimationStation>();
        tempList.Add(station);
        stations = tempList.AsReadOnly();
        container.OnChange.AddListener(OnReagentsChanged);
    }

    private void OnReagentsChanged(ReagentContents contents, GenericReagentContainer.InjectType inject) {
        if (fluidStream.isActiveAndEnabled) {
            fluidStream.OnFire(container);
        }
    }

    [PunRPC]
    public override void Use() {

        StopCoroutine(nameof(WaitThenConsumeEnergy));
        StartCoroutine(nameof(WaitThenConsumeEnergy));
    }
    IEnumerator WaitAndThenClear() {
        yield return new WaitForSeconds(2f);
        grindedThingsCache.Clear();
    }
    [PunRPC]
    void Grind(int viewID, ReagentContents incomingContents, KoboldGenes genes) {
        grindedObject?.Invoke(viewID, incomingContents);
        container.AddMixRPC(incomingContents, photonView.ViewID, (byte)GenericReagentContainer.InjectType.Inject);
        container.SetGenes(genes);
        fluidStream.OnFire(container);
    }
    private void HandleCollision(Collider other) {
        if (!grinding) {
            return;
        }
        if (other.isTrigger) {
            return;
        }
        
        // Filter by the grinding collider
        if (!Physics.ComputePenetration(grindingCollider, grindingCollider.transform.position,
                grindingCollider.transform.rotation,
                other, other.transform.position, other.transform.rotation, out Vector3 dir, out float distance) || distance == 0f) {
            return;
        }
        
        PhotonView view = other.GetComponentInParent<PhotonView>();
        if (view == null) {
            return;
        }
        if (grindedThingsCache.Contains(view)) {
            return;
        }
        
        grindedThingsCache.Add(view);
        
        StopCoroutine(nameof(WaitAndThenClear));
        StartCoroutine(nameof(WaitAndThenClear));
        
        Kobold kobold = other.GetComponentInParent<Kobold>();
        if (kobold != null) {
            kobold.StartCoroutine(RagdollForTime(kobold));
            foreach (Rigidbody r in other.GetAllComponents<Rigidbody>()) {
                r.AddExplosionForce(4000f, transform.position+Vector3.down*5f, 100f);
            }
            if (!deny.isPlaying) {
                deny.Play();
            }
            return;
        }
        
        if (!view.IsMine) {
            return;
        }
        GenericReagentContainer genericReagentContainer = view.GetComponentInChildren<GenericReagentContainer>();
        // Finally we grind it
        if (genericReagentContainer != null) {
            photonView.RPC(nameof(Grind), RpcTarget.All, view.ViewID, genericReagentContainer.GetContents(), genericReagentContainer.GetGenes());
        }
        
        IDamagable d = view.GetComponentInParent<IDamagable>();
        if (d != null) {
            d.Damage(d.GetHealth() + 1f);
        } else {
            PhotonNetwork.Destroy(view.gameObject);
        }

    }

    private IEnumerator RagdollForTime(Kobold kobold) {
        kobold.photonView.RPC(nameof(Ragdoller.PushRagdoll), RpcTarget.All);
        yield return new WaitForSeconds(3f);
        kobold.photonView.RPC(nameof(Ragdoller.PopRagdoll), RpcTarget.All);
    }

    private void OnTriggerEnter(Collider other) {
        HandleCollision(other);
    }
    private void OnTriggerStay(Collider other) {
        HandleCollision(other);
    }

    public ReadOnlyCollection<AnimationStation> GetAnimationStations() {
        return stations;
    }

    public override void Load(JSONNode node) {
        base.Load(node);
        bool newGrinding = node["grinding"];
        if (!grinding && newGrinding) {
            BeginGrind();
        } else if (grinding && !newGrinding) {
            StopGrind();
        }
    }

    public override void Save(JSONNode node) {
        base.Save(node);
        node["grinding"] = grinding;
    }
}
