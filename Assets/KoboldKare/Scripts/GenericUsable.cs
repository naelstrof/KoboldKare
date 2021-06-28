using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VisualLogic;
using XNode;

public class GenericUsable : SceneGraph<VisualLogicGraph> {
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
        (graph as VisualLogicGraph).TriggerEvent(gameObject, VisualLogic.Event.EventType.OnUse, new object[]{kobold, position});
    }

    public bool IsUsable(Kobold kobold) {
        return playerUsable;
    }
    public void Use() {
        Use(null);
    }
    public void Use(Kobold k) {
        if (k == null) {
            if (NetworkManager.instance.localPlayerInstance != null) {
                k = NetworkManager.instance.localPlayerInstance.GetComponent<Kobold>();
            }
        }
        if (k != null) {
            Vector3 pos = k.transform.position;
            SaveManager.RPC(GetComponentInParent<PhotonView>(), "RPCUse", RpcTarget.AllBuffered, new object[] { k.GetComponentInParent<PhotonView>().ViewID, pos });
        } else {
            SaveManager.RPC(GetComponentInParent<PhotonView>(), "RPCUse", RpcTarget.AllBuffered, new object[] { null, Vector3.zero });
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
        PhotonView view = GetComponentInParent<PhotonView>();
        if (view != null && view.IsMine) {
            PhotonNetwork.CleanRpcBufferIfMine(view);
        }
    }
    public void DestroyThing(UnityEngine.Object g) {
        if (g is GameObject) {
            PhotonView other = ((GameObject)g).GetComponentInParent<PhotonView>();
            if (other != null && other.IsMine) {
                SaveManager.Destroy(other.gameObject);
                return;
            }
        } else {
            Destroy(g);
        }
    }
}
