using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

[RequireComponent(typeof(GenericGrabbable), typeof(AudioSource), typeof(Rigidbody))]
public class BreakOnGrab : MonoBehaviourPun, IPunObservable, ISavable {
    private GenericGrabbable grabbable;
    private bool grabbed = false;
    private AudioSource source;
    [SerializeField]
    private GameObject disableOnGrab;
    private Rigidbody body;
    void Start() {
        grabbable = GetComponent<GenericGrabbable>();
        source = GetComponent<AudioSource>();
        body = GetComponent<Rigidbody>();
        grabbable.onGrab.AddListener(OnGrabbed);
    }
    void OnDestroy() {
        if (grabbable != null) {
            grabbable.onGrab.RemoveListener(OnGrabbed);
        }
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
    void OnGrabbed(Kobold k) {
        if (photonView.IsMine) {
            SetState(true);
        }
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
