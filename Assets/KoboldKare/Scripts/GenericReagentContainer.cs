using Photon.Pun;
using System.IO;
using NetStack.Serialization;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;

public class GenericReagentContainer : GeneHolder, IValuedGood, IPunObservable, ISavable, IPunInstantiateMagicCallback {
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
    public float startingMaxVolume = float.MaxValue;
    public float volume => contents.volume;

    public float maxVolume {
        get => contents.GetMaxVolume();
        set {
            contents.SetMaxVolume(value);
            OnChange.Invoke(contents, InjectType.Metabolize);
        }
    }

    public Color GetColor() => contents.GetColor();
    public ContainerType type;
    public ReagentContainerChangedEvent OnChange, OnFilled, OnEmpty;
    public bool isFull => Mathf.Approximately(contents.volume, contents.GetMaxVolume());
    public bool isEmpty => Mathf.Approximately(contents.volume,0f);
    public bool IsCleaningAgent() => contents.IsCleaningAgent();
    public float GetVolumeOf(ScriptableReagent reagent) => contents.GetVolumeOf(reagent);
    public float GetVolumeOf(byte id) => contents.GetVolumeOf(id);
    public InspectorReagent[] startingReagents;
    [SerializeField]
    private ReagentContents contents;

    public ReagentContents GetContents() {
        return contents;
    }

    private bool filled = false;
    private bool emptied = false;
    protected void Awake() {
        OnChange ??= new ReagentContainerChangedEvent();
        OnFilled ??= new ReagentContainerChangedEvent();
        OnEmpty ??= new ReagentContainerChangedEvent();
        contents ??= new ReagentContents(startingMaxVolume);
    }
    public void Start() {
        if (startingReagents != null) {
            foreach (var reagent in startingReagents) {
                AddMix(reagent.reagent, reagent.volume, InjectType.Inject);
            }
        }
        filled = isFull;
        emptied = isEmpty;
    }
    [PunRPC]
    public ReagentContents Spill(float spillVolume) {
        ReagentContents spillContents = contents.Spill(spillVolume);
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
        contents.AddMix(ReagentDatabase.GetID(incomingReagent), volume, this);
        OnReagentContentsChanged(injectType);
        return true;
    }
    public bool AddMix(ReagentContents incomingReagents, InjectType injectType) {
        if (!IsMixable(type, injectType) || !photonView.IsMine) {
            return false;
        }
        contents.AddMix(incomingReagents, this);
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
        contents.AddMix(incomingReagents, this);
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
        maxVolume = Mathf.Max(contents.volume + incomingReagents.volume, maxVolume);
        
        if (TryGetComponent(out Kobold kob)) {
            kob.SetGenes(kob.GetGenes().With(bellySize: maxVolume));
        }

        contents.AddMix(incomingReagents, this);
        OnReagentContentsChanged((InjectType)injectType);
        containerInflated?.Invoke(this);
        PhotonProfiler.LogReceive(sizeof(int) + sizeof(byte) + incomingReagentData.Length);
    }

    public ReagentContents Peek() => new ReagentContents(contents);
    public ReagentContents Metabolize(float deltaTime) => contents.Metabolize(deltaTime);
    public void OverrideReagent(Reagent r) => contents.OverrideReagent(r.id, r.volume);
    public void OverrideReagent(ScriptableReagent r, float volume) => contents.OverrideReagent(ReagentDatabase.GetID(r), volume);
    public void OnReagentContentsChanged(InjectType injectType) {
        //Debug.Log("[Generic Reagent Container] :: <Reagent Contents were changed on object "+gameObject.name+"!>");
        if (!filled && isFull) {
            //Debug.Log("[Generic Reagent Container] :: STATE_FILLING_TO_FULL_EVENT");
            OnFilled.Invoke(contents, injectType);
            containerFilled?.Invoke(this);
        }
        //Debug.Log("[Generic Reagent Container] :: STATE FILLED AND ISFULL: "+filled+","+isFull);
        filled = isFull;
        OnChange.Invoke(contents, injectType);
        if (!emptied && isEmpty) {
            SetGenes(null);
            //Debug.Log("[Generic Reagent Container] :: STATE_EMPTY_BUT_NOT_EMPTY");
            OnEmpty.Invoke(contents, injectType);
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
        return contents.GetValue();
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
            if (contents.GetVolumeOf(reagent) != 0f) {
                blah += reagent.name + ": " + contents.GetVolumeOf(reagent) + ", ";
            }
        }
        blah += "]";
        return base.ToString() + blah;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            BitBuffer bitBuffer = new BitBuffer(8);
            bitBuffer.AddReagentContents(contents);
            stream.SendNext(bitBuffer);
        } else {
            BitBuffer data = (BitBuffer)stream.ReceiveNext();
            ReagentContents newContents = data.ReadReagentContents();
            contents.Copy(newContents);
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
        contents ??= new ReagentContents(startingMaxVolume);
        contents.Save(node, "contents");
    }

    public void Load(JSONNode node) {
        contents ??= new ReagentContents(startingMaxVolume);
        contents.Load(node, "contents");
        OnReagentContentsChanged(InjectType.Metabolize);
    }
}
