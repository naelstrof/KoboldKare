using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using SimpleJSON;

[RequireComponent(typeof(AudioSource), typeof(Rigidbody))]
public class BreakOnGrab : MonoBehaviourPun, IPunObservable, ISavable, IGrabbable {
    private bool grabbed = false;
    private AudioSource source;
    [SerializeField]
    private GameObject disableOnGrab;
    private Rigidbody body;
    void Start() {
        source = GetComponent<AudioSource>();
        body = GetComponent<Rigidbody>();
        PlayAreaEnforcer.AddTrackedObject(photonView);
    }

    private void OnDestroy() {
        PlayAreaEnforcer.RemoveTrackedObject(photonView);
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

    public bool CanGrab(Kobold kobold) {
        return true;
    }

    [PunRPC]
    public void OnGrabRPC(int koboldID) {
        if (photonView.IsMine) {
            SetState(true);
        }
        PhotonProfiler.LogReceive(sizeof(int));
    }

    [PunRPC]
    public void OnReleaseRPC(int koboldID, Vector3 velocity) {
    }

    public Transform GrabTransform() {
        return transform;
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(grabbed);
        } else {
            SetState((bool)stream.ReceiveNext());
            PhotonProfiler.LogReceive(sizeof(bool));
        }
    }

    public void Save(JSONNode node) {
        node["grabbed"] = grabbed;
    }

    public void Load(JSONNode node) {
        SetState(node["grabbed"]);
    }
}
