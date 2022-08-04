using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Vilar.AnimationStation;
using System.Collections.ObjectModel;
using System.IO;

public class GrinderManager : UsableMachine, IAnimationStationSet {
    public delegate void GrindedObjectAction(ReagentContents contents);

    public static event GrindedObjectAction grindedObject;
        
    [SerializeField]
    private Sprite onSprite;
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

    public override Sprite GetSprite(Kobold k) {
        return onSprite;
    }
    public override bool CanUse(Kobold k) {
        return k.GetEnergy() > 0 && !grinding && station.info.user == null && constructed;
    }

    public override void LocalUse(Kobold k) {
        k.photonView.RPC(nameof(CharacterControllerAnimator.BeginAnimationRPC), RpcTarget.All, photonView.ViewID, 0);
        base.LocalUse(k);
    }

    private void BeginGrind() {
        grinding = true;
        animator.SetBool("Grinding", true);
        grindSound.enabled = true;
        grindSound.Play(); 
    }

    private void StopGrind() {
        grinding = false;
        animator.SetBool("Grinding", false);
        grindSound.Stop();
        grindSound.enabled = false;
    }

    IEnumerator WaitThenConsumeEnergy() {
        StopGrind();
        yield return new WaitForSeconds(8f);
        if (station.info.user == null) {
            yield break;
        }
        
        if (station.info.user.TryConsumeEnergy(1)) {
            station.info.user.photonView.RPC(nameof(CharacterControllerAnimator.StopAnimationRPC), RpcTarget.All);
            BeginGrind();
            yield return new WaitForSeconds(12f);
            StopGrind();
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
    void Grind(ReagentContents incomingContents, KoboldGenes genes) {
        grindedObject?.Invoke(incomingContents);
        container.AddMix(incomingContents, GenericReagentContainer.InjectType.Inject);
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
                r.AddExplosionForce(400f, transform.position+Vector3.down*5f, 100f);
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
            photonView.RPC(nameof(Grind), RpcTarget.All, genericReagentContainer.GetContents(), genericReagentContainer.GetGenes());
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

    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        base.OnPhotonSerializeView(stream, info);
        if (stream.IsWriting) {
            stream.SendNext(grinding);
        } else {
            bool newGrinding = (bool)stream.ReceiveNext();
            if (!grinding && newGrinding) {
                BeginGrind();
            } else if (grinding && !newGrinding) {
                StopGrind();
            }
        }
    }

    public override void Load(BinaryReader reader, string version) {
        base.Load(reader, version);
        bool newGrinding = reader.ReadBoolean();
        if (!grinding && newGrinding) {
            BeginGrind();
        } else if (grinding && !newGrinding) {
            StopGrind();
        }
    }

    public override void Save(BinaryWriter writer, string version) {
        base.Save(writer, version);
        writer.Write(grinding);
    }
}
