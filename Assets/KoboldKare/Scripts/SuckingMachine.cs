using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class SuckingMachine : UsableMachine {
    [SerializeField]
    private SphereCollider suckZone;
    public List<int> suckingIDs=new List<int>();
    private HashSet<Rigidbody> trackedRigidbodies;
    private bool sucking;
    private WaitForFixedUpdate waitForFixedUpdate;
    protected virtual void Awake() {
        trackedRigidbodies = new HashSet<Rigidbody>();
        waitForFixedUpdate = new WaitForFixedUpdate();
    }

    private Vector3 GetSuckLocation() {
        return suckZone.transform.TransformPoint(suckZone.center);
    }
    private float GetSuckRadius() {
        return suckZone.transform.lossyScale.x*suckZone.radius;
    }

    [PunRPC]
    protected virtual IEnumerator OnSwallowed(int viewID) {
        PhotonProfiler.LogReceive(sizeof(int));
        PhotonView view = PhotonNetwork.GetPhotonView(viewID);
        yield return new WaitForSeconds(0.1f);
        // Possible that it has already been removed.
        if (view == null) {
            yield break;
        }
        PhotonNetwork.Destroy(view.gameObject);
        suckingIDs.Remove(viewID);
    }

    protected virtual bool ShouldStopTracking(Rigidbody body) {
        if (body == null) {
            return true;
        }

        float distance = Vector3.Distance(body.ClosestPointOnBounds(GetSuckLocation()), GetSuckLocation());
        if (distance > GetSuckRadius()+1f) {
            return true;
        }
        if (distance < 0.1f) {
            PhotonView view = body.gameObject.GetComponentInParent<PhotonView>();
            if (view != null && view.IsMine) {
                photonView.RPC(nameof(OnSwallowed), RpcTarget.All, view.ViewID);
            }
            return true;
        }
        return false;
    }

    IEnumerator SuckAndSwallow() {
        sucking = true;
        while (isActiveAndEnabled && trackedRigidbodies.Count > 0) {
            trackedRigidbodies.RemoveWhere(ShouldStopTracking);
            foreach (var body in trackedRigidbodies) {
                body.velocity = Vector3.MoveTowards(body.velocity, Vector3.zero, body.velocity.magnitude*Time.deltaTime * 10f);
                body.AddForce((GetSuckLocation()-body.transform.TransformPoint(body.centerOfMass))*30f, ForceMode.Acceleration);
            }
            yield return waitForFixedUpdate;
        }
        sucking = false;
    }

    protected virtual void OnTriggerEnter(Collider other) {
        Kobold targetKobold = other.GetComponentInParent<Kobold>();
        if (targetKobold != null) {
            foreach (var player in PhotonNetwork.PlayerList) {
                if ((Kobold)player.TagObject == targetKobold) {
                    return;
                }
            }

            if (targetKobold.grabbed || !targetKobold.GetComponent<Ragdoller>().ragdolled) {
                return;
            }

            LocalUse(targetKobold);
            return;
        }
        Rigidbody body = other.GetComponentInParent<Rigidbody>();
        if (body != null && body.gameObject.GetComponent<MoneyPile>() == null) {
            trackedRigidbodies.Add(body);
            if (!sucking) {
                StartCoroutine(SuckAndSwallow());
            }
        }
    }
}
