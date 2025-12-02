using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
public interface IGrabbable {
    bool CanGrab(Kobold kobold);
    
    // FIXME FISHNET
    //[PunRPC]
    void OnGrabRPC(int koboldID);
    // FIXME FISHNET
    //[PunRPC]
    void OnReleaseRPC(int koboldID, Vector3 velocity);
    
    Transform GrabTransform();
    Transform transform { get; }
    GameObject gameObject { get; }
}
