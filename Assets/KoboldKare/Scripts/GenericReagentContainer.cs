using System;
using Photon.Pun;
using System.IO;
using NetStack.Serialization;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;

public class NoTouchGenericReagentContainer : GeneHolder {
    [SerializeField]
    protected float startingMaxVolume = float.MaxValue;
    private ReagentContents contents;
    protected virtual void Awake() {
        contents ??= new ReagentContents(startingMaxVolume);
    }
    public ReagentContents GetContents() {
        return contents ??= new ReagentContents(startingMaxVolume);
    }
}

public class GenericReagentContainer : NoTouchGenericReagentContainer, IValuedGood, IPunObservable, ISavable, IPunInstantiateMagicCallback {
    public delegate void ContainerFilledAction(GenericReagentContainer container);
    public static event ContainerFilledAction containerFilled;
    public static event ContainerFilledAction containerInflated;
    [System.Serializable]
    public class InspectorReagent {
        public ScriptableReagent reagent;
        public float volume;
    }
    public enum ContainerType : byte {
        OpenTop,
        Sealed,
        Mouth,
    }
    public enum InjectType : byte {
        Inject,
        Spray,
        Flood,
        Metabolize,
        Vacuum,
    }
    private static bool[,] ReagentMixMatrix = new bool[,]{
        // OpenTop, Sealed, Mouth
        {  true,   true,    true }, // Inject
        {  true,   false,   true }, // Spray
        {  true,   false,   false }, // Flood
        {  true,   true,   true }, // Metabolize
        {  true,   true,   true }, // Vacuum
    };
    [System.Serializable]
    public class ReagentContainerChangedEvent : UnityEvent<ReagentContents, InjectType> {}
    public static bool IsMixable(ContainerType container, InjectType injectionType) {
        return ReagentMixMatrix[(int)injectionType,(int)container];
    }
    public float volume => GetContents().volume;

    public float maxVolume {
        get => GetContents().GetMaxVolume();
        set {
            GetContents().SetMaxVolume(value);
            OnChange?.Invoke(GetContents(), InjectType.Metabolize);
        }
    }

    public Color GetColor() => GetContents().GetColor();
    public ContainerType type;
    public ReagentContainerChangedEvent OnChange, OnFilled, OnEmpty;
    public bool isFull => Mathf.Approximately(GetContents().volume, GetContents().GetMaxVolume());
    public bool isEmpty => Mathf.Approximately(GetContents().volume,0f);
    public bool IsCleaningAgent() => GetContents().IsCleaningAgent();
    public float GetVolumeOf(ScriptableReagent reagent) => GetContents().GetVolumeOf(reagent);
    public float GetVolumeOf(byte id) => GetContents().GetVolumeOf(id);
    public InspectorReagent[] startingReagents;

    private bool filled = false;
    private bool emptied = false;
    protected override void Awake() {
        base.Awake();
        OnChange ??= new ReagentContainerChangedEvent();
        OnFilled ??= new ReagentContainerChangedEvent();
        OnEmpty ??= new ReagentContainerChangedEvent();
        if (startingReagents != null) {
            foreach (var reagent in startingReagents) {
                AddMix(reagent.reagent, reagent.volume, InjectType.Inject);
            }
        }
    }
    public void Start() {
        
        filled = isFull;
        emptied = isEmpty;
    }
    [PunRPC]
    public ReagentContents Spill(float spillVolume) {
        ReagentContents spillContents = GetContents().Spill(spillVolume);
        OnReagentContentsChanged(InjectType.Vacuum);
        PhotonProfiler.LogReceive(sizeof(float));
        return spillContents;
    }

    private void TransferMix(GenericReagentContainer injector, float amount, InjectType injectType) {
        if (!IsMixable(this.type, injectType) || !photonView.IsMine) {
            return;
        }
        ReagentContents spill = injector.Spill(amount);
        AddMix(spill, injectType);
        SetGenes(injector.GetGenes());
    }
    private bool AddMix(ScriptableReagent incomingReagent, float volume, InjectType injectType) {
        if (!IsMixable(type, injectType) || !photonView.IsMine) {
            return false;
        }
        GetContents().AddMix(ReagentDatabase.GetID(incomingReagent), volume, this);
        OnReagentContentsChanged(injectType);
        return true;
    }
    public bool AddMix(ReagentContents incomingReagents, InjectType injectType) {
        if (!IsMixable(type, injectType) || !photonView.IsMine) {
            return false;
        }
        GetContents().AddMix(incomingReagents, this);
        OnReagentContentsChanged(injectType);
        return true;
    }

    [PunRPC]
    public void AddMixRPC(BitBuffer incomingReagentsData, int geneViewID, byte injectType) {
        ReagentContents incomingReagents = incomingReagentsData.ReadReagentContents();
        PhotonView view = PhotonNetwork.GetPhotonView(geneViewID);
        // FIXME: Not smart enough to decide which source of genes to use. We prioritize kobolds, but this would be incorrect in the case that a kobold is vomiting cum on another. (The genes should be sourced from the stomach instead).
        if (view != null && view.TryGetComponent(out Kobold kobold)) {
            SetGenes(kobold.GetGenes());
        } else if (view!=null && view.TryGetComponent(out GeneHolder geneHolder)) {
            if (geneHolder.GetGenes() != null) {
                SetGenes(geneHolder.GetGenes());
            }
        }
        GetContents().AddMix(incomingReagents, this);
        OnReagentContentsChanged((InjectType)injectType);
        PhotonProfiler.LogReceive(sizeof(int) + sizeof(byte) + incomingReagentsData.Length);
    }

    [PunRPC]
    public void ForceMixRPC(BitBuffer incomingReagentData, int geneViewID, byte injectType) {
        ReagentContents incomingReagents = incomingReagentData.ReadReagentContents();
        
        PhotonView view = PhotonNetwork.GetPhotonView(geneViewID);
        // FIXME: Not smart enough to decide which source of genes to use. We prioritize kobolds, but this would be incorrect in the case that a kobold is vomiting cum on another. (The genes should be sourced from the stomach instead).
        if (view != null && view.TryGetComponent(out Kobold kobold)) {
            SetGenes(kobold.GetGenes());
        } else if (view!=null && view.TryGetComponent(out GeneHolder geneHolder)) {
            if (geneHolder.GetGenes() != null) {
                SetGenes(geneHolder.GetGenes());
            }
        }
        maxVolume = Mathf.Max(GetContents().volume + incomingReagents.volume, maxVolume);
        
        if (TryGetComponent(out Kobold kob)) {
            kob.SetGenes(kob.GetGenes().With(bellySize: maxVolume));
        }

        GetContents().AddMix(incomingReagents, this);
        OnReagentContentsChanged((InjectType)injectType);
        containerInflated?.Invoke(this);
        PhotonProfiler.LogReceive(sizeof(int) + sizeof(byte) + incomingReagentData.Length);
    }

    public ReagentContents Peek() => new(GetContents());
    public ReagentContents Metabolize(float deltaTime) => GetContents().Metabolize(deltaTime);
    public void OverrideReagent(Reagent r) => GetContents().OverrideReagent(r.id, r.volume);
    public void OverrideReagent(ScriptableReagent r, float volume) => GetContents().OverrideReagent(ReagentDatabase.GetID(r), volume);
    public void OnReagentContentsChanged(InjectType injectType) {
        //Debug.Log("[Generic Reagent Container] :: <Reagent Contents were changed on object "+gameObject.name+"!>");
        if (!filled && isFull) {
            //Debug.Log("[Generic Reagent Container] :: STATE_FILLING_TO_FULL_EVENT");
            OnFilled.Invoke(GetContents(), injectType);
            containerFilled?.Invoke(this);
        }
        //Debug.Log("[Generic Reagent Container] :: STATE FILLED AND ISFULL: "+filled+","+isFull);
        filled = isFull;
        OnChange?.Invoke(GetContents(), injectType);
        if (!emptied && isEmpty) {
            SetGenes(null);
            //Debug.Log("[Generic Reagent Container] :: STATE_EMPTY_BUT_NOT_EMPTY");
            OnEmpty?.Invoke(GetContents(), injectType);
        }
        //Debug.Log("[Generic Reagent Container] :: STATE EMPTIED AND ISEMPTY: "+emptied+","+isEmpty);
        emptied = isEmpty;
    }

    public void RefillToFullWithDefaultContents(){
        if(startingReagents.Length != 0){
            foreach (var item in startingReagents){
                AddMix(item.reagent,item.volume,InjectType.Spray);
            }
        }
    }

    public float GetWorth() {
        return GetContents().GetValue();
    }

    public void OnValidate() {
        if (startingReagents == null) {
            return;
        }
        float volume = 0f;
        foreach(var reagent in startingReagents) {
            volume += reagent.volume;
        }
        startingMaxVolume = Mathf.Max(startingMaxVolume, volume);
    }
    public override string ToString()
    {
        string blah = "[";
        foreach(var reagent in ReagentDatabase.GetReagents()) {
            if (GetContents().GetVolumeOf(reagent) != 0f) {
                blah += reagent.name + ": " + GetContents().GetVolumeOf(reagent) + ", ";
            }
        }
        blah += "]";
        return base.ToString() + blah;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            BitBuffer bitBuffer = new BitBuffer(8);
            bitBuffer.AddReagentContents(GetContents());
            stream.SendNext(bitBuffer);
        } else {
            BitBuffer data = (BitBuffer)stream.ReceiveNext();
            ReagentContents newContents = data.ReadReagentContents();
            GetContents().Copy(newContents);
            OnReagentContentsChanged(InjectType.Metabolize);
            PhotonProfiler.LogReceive(data.Length);
        }
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info) {
        if (info.photonView.InstantiationData == null) {
            return;
        }
        if (info.photonView.InstantiationData.Length > 0 && info.photonView.InstantiationData[0] is BitBuffer) {
            BitBuffer buffer = (BitBuffer)info.photonView.InstantiationData[0];
            // This buffer might be shared.
            buffer.SetReadPosition(0);
            SetGenes(buffer.ReadKoboldGenes());
            PhotonProfiler.LogReceive(buffer.Length);
        }
        if (info.photonView.InstantiationData.Length > 0 && info.photonView.InstantiationData[0] is not BitBuffer) {
            throw new UnityException("Unexpected spawn data for container");
        }
    }

    public void Save(JSONNode node) {
        GetContents().Save(node, "contents");
    }

    public void Load(JSONNode node) {
        GetContents().Load(node, "contents");
        OnReagentContentsChanged(InjectType.Metabolize);
    }
}
