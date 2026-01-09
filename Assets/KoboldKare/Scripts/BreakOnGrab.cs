using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using System.Threading.Tasks;
using SimpleJSON;

[RequireComponent(typeof(AudioSource))]
public class BreakOnGrab : MonoBehaviour, ISavable {
    private bool grabbed = false;
    private AudioSource source;
    [SerializeField]
    private GameObject disableOnGrab;
    private Rigidbody body;
    private NetworkedEntity networkedEntity;
    void Start() {
        source = GetComponent<AudioSource>();
        body = GetComponentInParent<Rigidbody>();
        networkedEntity = GetComponentInParent<NetworkedEntity>();
        if (networkedEntity == null) {
            return;
        }

        networkedEntity.grabRequested += OnGrabRequested;
        networkedEntity.grabbed += OnGrab;
        networkedEntity.released += OnRelease;
        networkedEntity.SetGrabTransform(transform);
        networkedEntity.frozen.OnChange += OnFrozenChanged;
        OnFrozenChanged(networkedEntity.frozen.Value, networkedEntity.frozen.Value, false);
        // FIXME FISHNET
        //PlayAreaEnforcer.AddTrackedObject(photonView);
    }

    private void OnFrozenChanged(bool prev, bool next, bool asServer) {
        SetState(next);
    }

    private void OnDestroy() {
        // FIXME FISHNET
        // PlayAreaEnforcer.RemoveTrackedObject(photonView);
    }

    void SetState(bool newGrabbed) {
        if (grabbed == newGrabbed) {
            return;
        }
        grabbed = newGrabbed;
        disableOnGrab.SetActive(!grabbed);
        body.isKinematic = !grabbed;
        if (grabbed) {
            source.Play();
        }
    }

    private bool OnGrabRequested(NetworkedKobold kobold) {
        return true;
    }

    private void OnGrab(NetworkedKobold by) {
        if (networkedEntity.IsOwner) {
            networkedEntity.SetFrozen(true);
        }
    }

    private void OnRelease(NetworkedKobold by, Vector3 velocity) {
    }

    // FIXME FISHNET
    /*
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(grabbed);
        } else {
            SetState((bool)stream.ReceiveNext());
            PhotonProfiler.LogReceive(sizeof(bool));
        }
    } */

    public void Save(JSONNode node) {
        node["grabbed"] = grabbed;
    }

    public Task Load(JSONNode node) {
        SetState(node["grabbed"]);
        return Task.CompletedTask;
    }
}
