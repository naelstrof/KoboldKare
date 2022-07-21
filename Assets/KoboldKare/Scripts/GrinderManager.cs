using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Vilar.AnimationStation;
using System.Collections.ObjectModel;
using System.IO;

public class GrinderManager : GenericUsable, IAnimationStationSet {
    [SerializeField]
    private Sprite onSprite;
    public AudioSource grindSound;
    public Animator animator;
    public AudioSource deny;
    public GenericReagentContainer container;
    [SerializeField] private AnimationStation station;
    private ReadOnlyCollection<AnimationStation> stations;
    [SerializeField]
    private FluidStream fluidStream;
    private HashSet<GameObject> grindedThingsCache = new HashSet<GameObject>();
    private bool grinding;
    public void Purchase(bool purchase) {
        foreach (Transform t in transform) {
            if (t != transform) {
                t.gameObject.SetActive(purchase);
            }
        }
    }

    public override Sprite GetSprite(Kobold k) {
        return onSprite;
    }
    public override bool CanUse(Kobold k) {
        return k.GetEnergy() > 0 && !grinding && station.info.user == null;
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

    void Start() {
        List<AnimationStation> tempList = new List<AnimationStation>();
        tempList.Add(station);
        stations = tempList.AsReadOnly();
    }

    [PunRPC]
    public override void Use() {
        StopCoroutine(nameof(WaitThenConsumeEnergy));
        StartCoroutine(nameof(WaitThenConsumeEnergy));
    }
    IEnumerator WaitAndThenClear() {
        yield return new WaitForSeconds(0.5f);
        grindedThingsCache.Clear();
    }
    void Grind(GameObject obj) {
        Debug.Log("Grinding " + obj.name, obj);
        IGrabbable root = obj.GetComponentInParent<IGrabbable>();
        if (root == null) {
            return;
        }
        if (grindedThingsCache.Contains(root.gameObject)) {
            return;
        }
        
        photonView.RequestOwnership();
        grindedThingsCache.Add(root.gameObject);
        foreach (GenericReagentContainer c in root.gameObject.GetComponentsInChildren<GenericReagentContainer>()) {
            container.TransferMix(c, c.volume, GenericReagentContainer.InjectType.Inject);
        }
        IDamagable d = obj.GetComponentInParent<IDamagable>();
        if (d != null) {
            d.Damage(d.GetHealth() + 1f);
        } else {
            PhotonView other = obj.GetComponentInParent<PhotonView>();
            if (other != null) {
                PhotonNetwork.Destroy(other.gameObject);
            } else {
                Destroy(root.gameObject);
            }
        }
        fluidStream.OnFire(container);
        StopCoroutine(nameof(WaitAndThenClear));
        StartCoroutine(nameof(WaitAndThenClear));
    }
    private void HandleCollision(Collider other) {
        if (!grinding) {
            return;
        }
        if (other.isTrigger) {
            return;
        }
        
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

        PhotonView view = other.GetComponentInParent<PhotonView>();
        if (view == null || !view.IsMine) {
            return;
        }
        Grind(other.gameObject);
    }

    private IEnumerator RagdollForTime(Kobold kobold) {
        kobold.ragdoller.PushRagdoll();
        yield return new WaitForSeconds(3f);
        kobold.ragdoller.PopRagdoll();
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
