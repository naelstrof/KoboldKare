using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

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
        }
    }

    public void Save(BinaryWriter writer, string version) {
        writer.Write(grabbed);
    }

    public void Load(BinaryReader reader, string version) {
        SetState(reader.ReadBoolean());
    }
}
