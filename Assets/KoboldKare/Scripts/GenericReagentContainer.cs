using ExitGames.Client.Photon;
using KoboldKare;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GenericReagentContainer : MonoBehaviourPun, IReagentContainerListener, IValuedGood, IPunObservable, IInRoomCallbacks {
    public float maxVolume = float.MaxValue;
    public ReagentContents.ReagentContainerType containerType = ReagentContents.ReagentContainerType.OpenTop;
    public UnityEvent OnChange, OnFilled, OnEmpty;
    private bool networkChanged = false;
    private bool emptied = false;
    public bool isFull {
        get {
            return Mathf.Approximately(contents.volume, contents.maxVolume);
        }
    }

    private ReagentContents internalContents = new ReagentContents();
    public ReagentContents contents {
        set {
            var savedListeners = internalContents.listeners;
            internalContents = value;
            internalContents.containerType = containerType;
            internalContents.gameObject = gameObject;
            internalContents.behaviour = this;
            internalContents.maxVolume = maxVolume;
            internalContents.listeners = savedListeners;
            internalContents.TriggerChange();
        }
        get => internalContents;
    }
    public List<InspectorReagent> startReagents = new List<InspectorReagent>();
    private bool filled = false;
    //private bool changed = false;
    //public IEnumerator UpdateOverNetwork() {
        //yield return new WaitForSeconds(1f);
        //if (this != null) {
            //if (GetComponentInParent<PhotonView>().IsMine) {
                //GameManager.instance.networkManager.RPCUpdateReagentContainer(this, contents);
            //}
        //}
        //changed = false;
    //}

    public void Awake() {
        contents.containerType = containerType;
        contents.gameObject = gameObject;
        contents.behaviour = this;
        contents.maxVolume = maxVolume;
        foreach( InspectorReagent r in startReagents) {
            contents.Mix(r.id, r.volume, r.potentcy, r.heat);
        }
        contents.AddListener(this);
        filled = false;
    }
    public void Start() {
        PhotonNetwork.AddCallbackTarget(this);
    }
    public void OnDestroy() {
        PhotonNetwork.RemoveCallbackTarget(this);
        contents.RemoveListener(this);
    }
    public void OnReagentContainerChanged(ReagentContents contents, ReagentContents.ReagentInjectType injectType) {
        if (!filled && contents.volume >= contents.maxVolume) {
            filled = true;
            OnFilled.Invoke();
        }
        if (contents.volume < contents.maxVolume) {
            filled = false;
        }
        if (!filled) {
            OnChange.Invoke();
        }
        // We ignore metabolization events, other players can do that themselves.
        if (injectType != ReagentContents.ReagentInjectType.Metabolize) {
            networkChanged = true;
        }
        //if (!changed) {
            //new Task(UpdateOverNetwork());
            //changed = true;
        //}
        if (contents.volume > 0f) {
            emptied = false;
        }
        if (!emptied && Mathf.Approximately(contents.volume,0f)) {
            emptied = true;
            OnEmpty.Invoke();
        }
    }

    public float GetWorth() {
        if (GetComponentInParent<Kobold>() != null) {
            return contents.GetValue(GameManager.instance.reagentDatabase) * 0.5f;
        } else {
            return contents.GetValue(GameManager.instance.reagentDatabase);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            if (networkChanged || SaveManager.isSaving) {
                stream.SendNext(contents);
                networkChanged = false;
            } else {
                stream.SendNext(false);
            }
        } else {
            try {
                if (stream.PeekNext() is bool) {
                    stream.ReceiveNext(); // skip
                    return;
                }
            } catch {
                Debug.LogError(gameObject.name + " reagent container has mismatched observers!", this.gameObject);
                return;
            }
            if (stream.PeekNext() is ReagentContents) {
                contents = (ReagentContents)stream.ReceiveNext();
                contents.InvokeListenerUpdate(ReagentContents.ReagentInjectType.Metabolize);
            } else {
                Debug.LogError(gameObject.name + " reagent container has mismatched observers!", this.gameObject);
            }
        }
    }
    public void OnPlayerEnteredRoom(Player newPlayer) {
        // Send a new update asap
        networkChanged = true;
    }

    public void OnPlayerLeftRoom(Player otherPlayer) {
    }

    public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged) {
    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps) {
    }

    public void OnMasterClientSwitched(Player newMasterClient) {
        networkChanged = true;
    }
}
