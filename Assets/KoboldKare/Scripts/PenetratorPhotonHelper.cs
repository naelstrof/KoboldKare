using System;
using System.Collections;
using System.Collections.Generic;
using PenetrationTech;
using Photon.Pun;
using UnityEngine;

public class PenetratorPhotonHelper : MonoBehaviourPun {
    private Penetrator penetrator;
    private void Start() {
        penetrator = GetComponent<Penetrator>();
        penetrator.penetrationStart += OnPenetrationStart;
    }

    private void OnDestroy() {
        if (penetrator != null) {
            penetrator.penetrationStart -= OnPenetrationStart;
        }
    }

    void OnPenetrationStart(Penetrable p) {
        if (!photonView.IsMine) {
            return;
        }
        PhotonView other = p.GetComponentInParent<PhotonView>();
        Penetrable[] penetrables = other.GetComponentsInChildren<Penetrable>();
        for (int i = 0; i < penetrables.Length; i++) {
            if (penetrables[i] == p) {
                photonView.RPC(nameof(PenetrateRPC), RpcTarget.Others, other.ViewID, i);
            }
        }
    }
    [PunRPC]
    private void PenetrateRPC(int viewID, int penetrableID) {
        PhotonView other = PhotonNetwork.GetPhotonView(viewID);
        Penetrable[] penetrables = other.GetComponentsInChildren<Penetrable>();
        // Only penetrate if we already aren't
        if (!penetrator.TryGetPenetrable(out Penetrable checkPen) || checkPen != penetrables[penetrableID]) {
            penetrator.Penetrate(penetrables[penetrableID]);
        }
        PhotonProfiler.LogReceive(sizeof(int) * 2);
    }
}
