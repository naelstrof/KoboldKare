using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[Serializable]
public class UsableCondition : SerializableCallback<Kobold,bool> { }
[System.Serializable]
public class KoboldUseEvent : UnityEvent<Kobold, Vector3> {}
public class GenericUsable : MonoBehaviourPun {
    [Tooltip("If the player can hit E to use this, otherwise it can only be activated from a UnityEvent. Call Use().")]
    public bool playerUsable = true;
    public bool keepCacheForever = false;
    public void SetPlayerUsable(bool usable) {
        playerUsable = usable;
    }
    [Serializable]
    public class ConditionEventPair {
        [SerializeField]
        public List<UsableCondition> conditions;
        public KoboldUseEvent onUse;
        public Sprite sprite;
    }
    public void Awake() {
        if (photonView.gameObject != gameObject) {
            Debug.LogError("GenericUsable is not directly on a PhotonView, this is required for them to work properly. (Just add one, or make sure it's at the root of the object).", gameObject);
        }
    }

    [SerializeField]
    public List<ConditionEventPair> usableEvents;
    public Sprite GetSprite(Kobold kobold) {
        foreach(ConditionEventPair pair in usableEvents) {
            bool conditionsMet = true;
            foreach(UsableCondition condition in pair.conditions) {
                conditionsMet &= condition.Invoke(kobold);
            }
            if (conditionsMet) {
                return pair.sprite;
            }
        }
        return null;
    }
    public void OnUse(Kobold kobold, Vector3 position)
    {
        foreach(ConditionEventPair pair in usableEvents) {
            bool conditionsMet = true;
            foreach(UsableCondition condition in pair.conditions) {
                conditionsMet &= condition.Invoke(kobold);
            }
            if (conditionsMet) {
                pair.onUse.Invoke(kobold, position);
                break;
            }
        }
    }

    public bool IsUsable(Kobold kobold)
    {
        if ( !playerUsable ) {
            return false;
        }
        foreach(ConditionEventPair pair in usableEvents) {
            bool conditionsMet = true;
            foreach(UsableCondition condition in pair.conditions) {
                conditionsMet &= condition.Invoke(kobold);
            }
            if( conditionsMet) {
                return true;
            }
        }
        return false;
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
            SaveManager.RPC(photonView, "RPCUse", RpcTarget.AllBuffered, new object[] { k.photonView.ViewID, pos });
        } else {
            SaveManager.RPC(photonView, "RPCUse", RpcTarget.AllBuffered, new object[] { null, Vector3.zero });
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
