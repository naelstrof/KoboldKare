using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
public interface IGrabbable {
    bool CanGrab(Kobold kobold);
    
    [PunRPC]
    void OnGrabRPC(int koboldID);
    [PunRPC]
    void OnReleaseRPC(int koboldID, Vector3 velocity);
    
    Transform GrabTransform();
    Transform transform { get; }
    GameObject gameObject { get; }
    PhotonView photonView { get; }
}
