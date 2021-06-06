using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;

[RequireComponent(typeof(PhotonView))]
public class GenericAttachable : MonoBehaviourPun, IAdvancedInteractable {
    public LayerMask attachableSearchMask;
    public Transform attachNozzle;
    private Collider[] hits = new Collider[3];
    private Transform target;
    public Rigidbody body;
    public float pumpSpeed = -2f;
    public bool onlyFluids = true;
    public GenericReagentContainer container;
    public List<Collider> selfColldiers = new List<Collider>();
    [Tooltip("The name of the layer this object should switch to on attach in order to prevent crazy collisions.")]
    public string attachedLayerName;
    [Tooltip("The name of the layer this object should switch to on release in order to prevent falling through the floor.")]
    public string unattachedLayerName;
    [HideInInspector]
    public bool attached = false;
    private bool disabledPhysics = true;
    public void AttachNearby() {
        if (!this.photonView.IsMine) {
            return;
        }
        int hitCount = Physics.OverlapSphereNonAlloc(attachNozzle.position, 0.5f, hits, attachableSearchMask, QueryTriggerInteraction.Collide);
        Collider closestHit = null;
        float closestDist = float.MaxValue;
        for(int i=0;i<hitCount;i++) {
            if (hits[i].transform == attachNozzle) {
                continue;
            }
            // Don't attach to something that's already attached.
            GenericAttachable ga = hits[i].GetComponentInParent<GenericAttachable>();
            if (ga!=null && ga.attached) {
                continue;
            }
            float dist = Vector3.Distance(attachNozzle.position, hits[i].transform.position);
            if (closestHit == null || closestDist > dist) {
                closestHit = hits[i];
                closestDist = dist;
            }
        }
        if ( closestHit != null ) {
            PhotonView view = closestHit.GetComponentInParent<PhotonView>();
            if (view != null) {
                Collider[] colliders = view.GetComponentsInChildren<Collider>();
                for (int i = 0; i < colliders.Length; i++) {
                    if (closestHit == colliders[i]) {
                        SaveManager.RPC(photonView, "RPCAttach", RpcTarget.AllBuffered, new object[] { view.ViewID, i });
                        break;
                    }
                }
            }
        }
    }
    [PunRPC]
    public void RPCAttach(int photonID, int colliderIndex) {
        PhotonView view = PhotonView.Find(photonID);
        if (view == null) {
            return;
        }
        Collider[] colliders = view.GetComponentsInChildren<Collider>();
        if (colliderIndex >= 0 && colliderIndex < colliders.Length ) {
            AttachTo(colliders[colliderIndex].transform);
        }
    }
    public void AttachTo(Transform t, bool disablePhysics = true) {
        if (!isActiveAndEnabled) {
            return;
        }
        disabledPhysics = disablePhysics;
        target = t;
        StartAttach(disablePhysics);
        StopCoroutine("AttachOverDuration");
        StartCoroutine("AttachOverDuration");
    }


    private void StartAttach(bool disablePhysics) {
        if (disablePhysics) {
            body.isKinematic = true;
            foreach (Collider c in selfColldiers) {
                c.gameObject.layer = LayerMask.NameToLayer(attachedLayerName);
            }
        }
        GenericAttachable ga = target.GetComponentInParent<GenericAttachable>();
        if (ga != null && ga.target != attachNozzle && ((1<<attachNozzle.gameObject.layer) & attachableSearchMask) != 0) {
            ga.AttachTo(attachNozzle, false);
        }
    }
    [PunRPC]
    public void RPCEndAttach() {
        body.isKinematic = false;
        foreach(Collider c in selfColldiers) {
            c.gameObject.layer = LayerMask.NameToLayer(unattachedLayerName);
        }
        attached = false;
        StopCoroutine("AttachOverDuration");
        if (target == null) {
            return;
        }
        GenericAttachable ga = target.GetComponentInParent<GenericAttachable>();
        target = null;
        if (ga != null && ga.target == attachNozzle) {
            ga.EndAttach();
        }
    }
    public void EndAttach() {
        if (!this.photonView.IsMine) {
            return;
        }
        SaveManager.RPC(photonView, "RPCEndAttach", RpcTarget.AllBuffered, null);
    }

    public IEnumerator AttachOverDuration() {
        float startTime = Time.timeSinceLevelLoad;
        float duration = 1f;
        while (Time.timeSinceLevelLoad<startTime+duration && target != null) {
            yield return new WaitForEndOfFrame();
            float lerpAmount = (Time.timeSinceLevelLoad - startTime)/duration;
            Quaternion neededRotation = Quaternion.FromToRotation(attachNozzle.up, -target.up) * transform.rotation;
            transform.rotation = Quaternion.Lerp(transform.rotation, neededRotation, lerpAmount);
            Vector3 offset = attachNozzle.position - transform.position;
            transform.position = Vector3.Lerp(transform.position, target.position - offset, lerpAmount);
        }
        attached = true;
    }
    public void FixedUpdate() {
        if (!(attached && target != null)) {
            return;
        }
        GenericReagentContainer targetContainer = target.GetComponentInParent<GenericReagentContainer>();
        if (targetContainer != null && pumpSpeed != 0f) {
            if (!onlyFluids) {
                if (pumpSpeed < 0f) {
                    container.contents.Mix(targetContainer.contents.Spill((-pumpSpeed) * Time.deltaTime));
                } else {
                    targetContainer.contents.Mix(container.contents.Spill(pumpSpeed * Time.deltaTime));
                }
            } else {
                if (pumpSpeed < 0f) {
                    float transferAmount = Mathf.Min((-pumpSpeed) * Time.deltaTime, targetContainer.contents.volume, (container.contents.maxVolume - container.contents.volume));
                    ReagentContents spilled = targetContainer.contents.Spill(transferAmount);
                    ReagentContents cantTransfer = new ReagentContents();
                    foreach(var p in spilled) {
                        if (!GameManager.instance.reagentDatabase.reagents[p.Key].isFluid) {
                            cantTransfer.Mix(p.Key, p.Value);
                            spilled[p.Key].volume = 0f;
                        }
                    }
                    container.contents.Mix(spilled);
                    targetContainer.contents.Mix(cantTransfer);
                } else {
                    float transferAmount = Mathf.Min(pumpSpeed * Time.deltaTime, container.contents.volume, (targetContainer.contents.maxVolume - targetContainer.contents.volume));
                    ReagentContents spilled = container.contents.Spill(transferAmount);
                    ReagentContents cantTransfer = new ReagentContents();
                    foreach(var p in spilled) {
                        if (!GameManager.instance.reagentDatabase.reagents[p.Key].isFluid) {
                            cantTransfer.Mix(p.Key, p.Value);
                            spilled[p.Key].volume = 0f;
                        }
                    }
                    container.contents.Mix(cantTransfer);
                    targetContainer.contents.Mix(spilled);
                }
            }
        }
    }
    public void LateUpdate() {
        if (attached && target && disabledPhysics) {
            transform.rotation = Quaternion.FromToRotation(attachNozzle.up, -target.up) * transform.rotation;
            transform.position = target.position - (attachNozzle.position - transform.position);
            if (target == null) {
                EndAttach();
            }
        }
    }

    public void InteractTo(Vector3 worldPosition, Quaternion worldRotation) {
    }

    public void OnInteract(Kobold k) {
        EndAttach();
    }

    public void OnEndInteract(Kobold k) {
        AttachNearby();
    }

    public bool PhysicsGrabbable() {
        return true;
    }
    public void OnDestroy() {
        if (photonView.IsMine) {
            PhotonNetwork.CleanRpcBufferIfMine(photonView);
        }
    }
}
