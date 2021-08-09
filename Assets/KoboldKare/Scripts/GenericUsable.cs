using KoboldKare;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VisualLogic;
using XNode;

#if UNITY_EDITOR
using UnityEditor;
[CanEditMultipleObjects]
[CustomEditor(typeof(GenericUsable))]
public class GenericUsableEditor : Editor {
}
#endif
public class GenericUsable : SceneGraph<VisualLogicGraph> {
    [System.Serializable]
    public class Condition : SerializableCallback<Kobold,bool> {}

    [System.Serializable]
    public class KoboldUseEvent : UnityEvent<Kobold, Vector3>{};
    public List<VisualLogicGraph.BlackboardValue> blackboardValues = new List<VisualLogicGraph.BlackboardValue>();
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
        if (graph != null) {
            foreach(var bvalue in blackboardValues) {
                (graph as VisualLogicGraph).blackboard[bvalue.name] = bvalue.value;
            }
            (graph as VisualLogicGraph).blackboard["useKobold"] = kobold;
            (graph as VisualLogicGraph).blackboard["usePosition"] = position;
            (graph as VisualLogicGraph).TriggerEvent(gameObject, VisualLogic.Event.EventType.OnUse);
        }
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
