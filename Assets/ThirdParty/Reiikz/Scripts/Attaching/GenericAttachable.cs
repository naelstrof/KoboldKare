using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GenericAttachable : MonoBehaviour
{
    private bool attached_;
    public bool attached {
        get {
            return attached_;
        }
        private set {
            attached_ = value;
        }
    }
    public Transform parent {
        get {
            return rootTransform.parent;
        }
        set {
            rootTransform.parent = value;
        }
    }
    public Transform rootTransform;
    public bool disableColliderOnAttach;
    public Rigidbody body = null;
    public List<Collider> colliders;
    // public List<PhotonView> photonViews;
    public PhotonView mainPV;
    public PhotonRigidbodyView rigidbodyView;
    public List<GameObject> disableOnAttach;
    
    private int Attach(Transform newParent) {
        bool isMine = mainPV.IsMine;
        // foreach(PhotonView pv in photonViews){
        //     isMine = isMine && pv.IsMine;
        //     if(!isMine) break;
        // }
        foreach(GameObject ob in disableOnAttach){
            if(ob.activeSelf){
                ob.SetActive(false);
            }
        }
        foreach(Collider c in colliders){
            if(c.enabled) c.enabled = false;
        }
        body.isKinematic = true;
        parent = newParent;
        if(rigidbodyView == null) Debug.LogWarning("GenericAttachable was missing rigidbodyView reference, ignore object doesn't contain one");
        rigidbodyView.enabled = false;
        rootTransform.localPosition = new Vector3(0, 0, 0);
        rootTransform.localEulerAngles = new Vector3(0, 0, 0);
        rootTransform.localScale = new Vector3(1, 1, 1);
        return 0;
    }

    [PunRPC]
    void RPC_Attach(int ob, int snapTo) {
        PhotonView targetView = PhotonView.Find(snapTo);
        PhotonView snap = PhotonView.Find(ob);
        if(targetView == null) return;
        if(snap == null) return;
        Transform target = targetView.gameObject.GetComponent<IHolder>().GetAttachPoint();
        if(target == null) return;
        snap.GetComponent<GenericAttachable>().Attach(target);
    }
}
