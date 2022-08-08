using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class SmoothCharacterPhoton : MonoBehaviourPun, IPunObservable, ISavable, IOnPhotonViewOwnerChange {
    [System.Serializable]
    private class VectorPid {
        [SerializeField] private float pFactor;
        [SerializeField] private float iFactor;
        [SerializeField] private float dFactor;
        private Vector3 integral;
        private Vector3 lastError;

        public VectorPid(float p, float i, float d) {
            pFactor = p;
            iFactor = i;
            dFactor = d;
        }

        public Vector3 Update(Vector3 currentError, float timeFrame) {
            integral += currentError * timeFrame;
            var derivative = (currentError - lastError) / timeFrame;
            lastError = currentError;
            return currentError * pFactor + integral * iFactor + derivative * dFactor;
        }
    }

    [SerializeField] private VectorPid pid = new VectorPid(25f, 0f, 15f);
    [SerializeField] private float teleportDistance = 2f;
    
    private Vector3 networkedPosition;

    private void Awake() {
        body = GetComponent<Rigidbody>();
    }

    private void Start() {
        networkedPosition = body.position;
    }

    private void FixedUpdate() {
        if (photonView.IsMine) {
            return;
        }

        Vector3 difference = networkedPosition - body.position;
        if (difference.magnitude > teleportDistance) {
            body.position = networkedPosition;
            return;
        }
        Vector3 adjustment = pid.Update(difference, Time.deltaTime);
        body.AddForce(adjustment, ForceMode.Acceleration);
    }
    
    private Rigidbody body;
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(body.position);
        } else {
            networkedPosition = (Vector3)stream.ReceiveNext();
        }
    }

    public void Save(BinaryWriter writer, string version) {
        writer.Write(body.position.x);
        writer.Write(body.position.y);
        writer.Write(body.position.z);
    }

    public void Load(BinaryReader reader, string version) {
        float x = reader.ReadSingle();
        float y = reader.ReadSingle();
        float z = reader.ReadSingle();
        body.position = new Vector3(x, y, z);
    }

    public void OnOwnerChange(Player newOwner, Player previousOwner) {
        body.useGravity = newOwner == PhotonNetwork.LocalPlayer;
    }
}
