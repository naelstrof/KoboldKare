using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using PenetrationTech;
using Photon.Pun;
using UnityEngine;

public class CreaturePath : CatmullDisplay {
    [SerializeField] private Transform[] followTransforms;
    [SerializeField] private PhotonGameObjectReference creatureToSpawn;
    [SerializeField] private float spawnDelay = 240f;
    private WaitForSeconds waitForSeconds;
    private PhotonView trackedCreature;
    private CatmullSpline path;

    public PhotonView photonView { get; private set; }

    private void OnValidate() {
        if (followTransforms is { Length: > 2 }) {
            path ??= new CatmullSpline();
            List<Vector3> points = new List<Vector3>();
            foreach (var t in followTransforms) {
                points.Add(t.position);
            }

            path.SetWeightsFromPoints(points);
        }

        creatureToSpawn.OnValidate();
    }

    private void Start() {
        waitForSeconds = new WaitForSeconds(spawnDelay);
        photonView = GetComponentInParent<PhotonView>();
        StartCoroutine(Think());
    }

    [PunRPC]
    private void SetCreature(int viewID) {
        trackedCreature = PhotonNetwork.GetPhotonView(viewID);
        PhotonProfiler.LogReceive(sizeof(int));
    }

    private IEnumerator Think() {
        while (isActiveAndEnabled) {
            yield return waitForSeconds;
            if (trackedCreature == null && photonView.IsMine) {
                GameObject obj = PhotonNetwork.Instantiate(creatureToSpawn.photonName, GetPath().GetPositionFromT(0f), Quaternion.identity, 0, new object[]{photonView.ViewID});
                photonView.RPC(nameof(SetCreature), RpcTarget.All, obj.GetPhotonView().ViewID);
            }
        }
    }

    public override CatmullSpline GetPath() {
        if (followTransforms is not { Length: > 2 }) return null;
        path ??= new CatmullSpline();
        List<Vector3> points = new List<Vector3>();
        foreach (var t in followTransforms) {
            points.Add(t.position);
        }
        path.SetWeightsFromPoints(points);
        return path;
    }
}
