using KoboldKare;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
[CanEditMultipleObjects]
[CustomEditor(typeof(GenericUsable))]
public class GenericUsableEditor : Editor {
}
#endif
public class GenericUsable : MonoBehaviourPun {
    [System.Serializable]
    public class Condition : SerializableCallback<Kobold,bool> {}

    [System.Serializable]
    public class KoboldUseEvent : UnityEvent<Kobold, Vector3>{};
    public List<Condition> conditions = new List<Condition>();
    public KoboldUseEvent OnUseEvent;

    [Tooltip("If the player can hit E to use this, otherwise it can only be activated from a UnityEvent. Call Use().")]
    public bool playerUsable = true;
    public void SetPlayerUsable(bool usable) {
        playerUsable = usable;
    }
    public void Awake() {
        if (GetComponentInParent<PhotonView>().gameObject != gameObject) {
            Debug.LogError("GenericUsable is not directly on a PhotonView, this is required for them to work properly. (Just add one, or make sure it's at the root of the object).", gameObject);
        }
    }
    public Sprite displaySprite;
    public Sprite GetSprite(Kobold kobold) {
        return displaySprite;
    }
    public void OnUse(Kobold kobold, Vector3 position) {
        //VisualLogicGraph instance = (VisualLogicGraph)graph.Copy();
        //instance.TriggerEvent(gameObject, VisualLogic.Event.EventType.OnUse, new object[]{kobold, position}).Finished += (manuallyStopped)=>{ScriptableObject.Destroy(instance);};
        OnUseEvent.Invoke(kobold, position);
    }

    public bool IsUsable(Kobold kobold) {
        bool usable = playerUsable;
        foreach( Condition c in conditions) {
            usable = usable && c.Invoke(kobold);
        }
        return usable;
    }
    public void Use() {
        bool usable = true;
        foreach( Condition c in conditions) {
            usable = usable && c.Invoke(null);
        }
        if (usable) {
            Use(null);
        }
    }
    public void Use(Kobold k) {
        if (k == null) {
            foreach(var player in PhotonNetwork.PlayerList) {
                if (player.TagObject == null || (player.TagObject as Kobold) == null) {
                    continue;
                }
                k = (player.TagObject as Kobold);
            }
        }
        if (k != null) {
            Vector3 pos = k.transform.position;
            GetComponentInParent<PhotonView>().RPC("RPCUse", RpcTarget.AllBuffered, new object[] { k.GetComponentInParent<PhotonView>().ViewID, pos });
        } else {
            GetComponentInParent<PhotonView>().RPC("RPCUse", RpcTarget.AllBuffered, new object[] { null, Vector3.zero });
        }
    }

    [PunRPC]
    public void RPCUse(int koboldViewID, Vector3 pos) {
        var view = PhotonView.Find(koboldViewID);
        if (view == null) {
            OnUse(null, pos);
            return;
        }
        var kobold = view.GetComponentInChildren<Kobold>();
        if (kobold == null) {
            OnUse(null, pos);
            return;
        }
        if (!view.IsMine) {
            kobold.transform.position = pos;
        }
        OnUse(kobold, pos);
    }
    public void OnDestroy() {
        if (photonView.IsMine) {
            PhotonNetwork.CleanRpcBufferIfMine(photonView);
            PhotonNetwork.OpCleanRpcBuffer(photonView);
        }
    }
    public void DestroyThing(UnityEngine.Object g) {
        if (g is GameObject) {
            PhotonView other = ((GameObject)g).GetComponentInParent<PhotonView>();
            if (other != null && other.IsMine) {
                PhotonNetwork.Destroy(other.gameObject);
                return;
            }
        } else {
            Destroy(g);
        }
    }
}
