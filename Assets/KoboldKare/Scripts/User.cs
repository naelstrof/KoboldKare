using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

[System.Serializable]
public class SpriteEvent : UnityEvent<Sprite> {}

public class User : MonoBehaviourPun {
    private Kobold internalKobold;
    public Kobold kobold {
        get {
            if (internalKobold == null) {
                internalKobold = GetComponentInParent<Kobold>();
            }
            return internalKobold;
        }
    }

    public SpriteEvent OnEnterUsable;
    public UnityEvent OnExitUsable;
    public UnityEvent OnUse;
    public Sprite unknownUsableSprite;
    private HashSet<Tuple<GenericUsable,GameObject>> possibleUsables = new HashSet<Tuple<GenericUsable,GameObject>>();
    private GenericUsable closestUsable = null;
    private CapsuleCollider capsuleCollider;

    private void Awake() {
        capsuleCollider = GetComponent<CapsuleCollider>();
    }

    public void LateUpdate() {
        var ownedKobold = kobold;
        if (!photonView.IsMine || (Kobold)PhotonNetwork.LocalPlayer.TagObject != ownedKobold) return;
        transform.rotation = OrbitCamera.GetPlayerIntendedRotation();
        
        var desiredPosition = OrbitCamera.GetCamera().transform.position + transform.forward * (capsuleCollider.height*0.5f);
        float distance = Vector3.Distance(ownedKobold.transform.position, desiredPosition);
        transform.position = Vector3.MoveTowards(desiredPosition, ownedKobold.transform.position, Mathf.Max(distance - ownedKobold.GetGenes().baseSize*0.2f, 0f));
    }

    public IEnumerator WaitAndThenTrigger(UnityEvent e) {
        yield return new WaitForSeconds(1f);
        yield return new WaitForEndOfFrame();
        e.Invoke();
    }
    private void OnTriggerEnter(Collider other) {
        GenericUsable g = other.GetComponentInParent<GenericUsable>();
        if (g!=null) {
            possibleUsables.Add(new Tuple<GenericUsable, GameObject>(g, other.gameObject));
        }
    }
    private void OnTriggerStay(Collider other) {
        GenericUsable g = other.GetComponentInParent<GenericUsable>();
        if (g!=null) {
            possibleUsables.Add(new Tuple<GenericUsable, GameObject>(g, other.gameObject));
        }
    }
    void FixedUpdate() {
        capsuleCollider.height = kobold.GetGenes().baseSize * 0.20f;
        SortGrabbables();
        possibleUsables.Clear();
    }

    void SortGrabbables() {
        possibleUsables.RemoveWhere(o=>o == null || ((Component)o.Item1) == null || o.Item2 == null || !o.Item2.activeInHierarchy);
        float distance = float.MaxValue;
        GenericUsable closest = null;
        foreach( Tuple<GenericUsable,GameObject> u in possibleUsables ) {
            if (!u.Item1.CanUse(kobold) || u.Item1.transform.root == transform.root) {
                continue;
            }
            float d = Vector3.Distance(u.Item1.transform.position, transform.position);
            if (closest == null || d < distance) {
                closest = u.Item1;
                distance = d;
            }
        }
        closestUsable = closest;
        if (closestUsable != null) {
            if (closestUsable.GetSprite(kobold) == null) {
                OnEnterUsable.Invoke(unknownUsableSprite);
            } else {
                OnEnterUsable.Invoke(closestUsable.GetSprite(kobold));
            }
        } else {
            closestUsable = null;
            OnExitUsable.Invoke();
        }
    }
    public void Use() {
        if (closestUsable != null) {
            //closestUsable.photonView.RPC("RPCUse", RpcTarget.All, new object[]{photonView.ViewID});
            closestUsable.LocalUse(kobold);
            OnUse.Invoke();
        }
    }
}
