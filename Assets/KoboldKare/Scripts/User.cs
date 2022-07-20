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
    private HashSet<Tuple<GenericUsable,GameObject>> removeLater = new HashSet<Tuple<GenericUsable,GameObject>>();
    private GenericUsable closestUsable = null;

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
    //private void OnTriggerStay(Collider other) {
        //GenericUsable g = other.GetComponentInParent<GenericUsable>();
        //if (g!=null) {
            //possibleUsables.Add(new Tuple<GenericUsable, GameObject>(g, other.gameObject));
        //}
    //}
    private void OnTriggerExit(Collider other) {
        GenericUsable g = other.GetComponentInParent<GenericUsable>();
        if (g!=null) {
            removeLater.Add(new Tuple<GenericUsable, GameObject>(g, other.gameObject));
            //possibleUsables.RemoveWhere(o=>o.Item1 == g);
        }
    }

    void FixedUpdate() {
        possibleUsables.ExceptWith(removeLater);
        removeLater.Clear();
        SortGrabbables();
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
        if (closestUsable != null ) {
            if (closestUsable.GetSprite(kobold) == null) {
                OnEnterUsable.Invoke(unknownUsableSprite);
            } else {
                OnEnterUsable.Invoke(closestUsable.GetSprite(kobold));
            }
        } else {
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
