using ExitGames.Client.Photon;
using KoboldKare;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class GenericReagentContainer : GeneHolder, IValuedGood, IPunObservable, ISavable {
    public delegate void ContainerFilledAction(GenericReagentContainer container);
    public static event ContainerFilledAction containerFilled;
    [System.Serializable]
    public class InspectorReagent {
        public ScriptableReagent reagent;
        public float volume;
    }
    public enum ContainerType {
        OpenTop,
        Sealed,
        Mouth,
    }
    public enum InjectType {
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
    public float GetVolumeOf(short id) => contents.GetVolumeOf(id);
    public InspectorReagent[] startingReagents;
    [SerializeField]
    private ReagentContents contents;

    public ReagentContents GetContents() {
        return contents;
    }

    private bool filled = false;
    private bool emptied = false;
    private bool ready = false;
    protected void Awake() {
        if(ready){return;}

        OnChange ??= new ReagentContainerChangedEvent();
        OnFilled ??= new ReagentContainerChangedEvent();
        OnEmpty ??= new ReagentContainerChangedEvent();

        contents = new ReagentContents(startingMaxVolume);
        //Debug.Log("[Generic Reagent Container] :: Initializing Contents...");
    }
    public void Start() {
        if(ready){return;}

        if (startingReagents != null) {
            foreach (var reagent in startingReagents) {
                AddMix(reagent.reagent, reagent.volume, InjectType.Inject);
            }
        }

        filled = isFull;
        emptied = isEmpty;
        ready = true;

        //Debug.Log(string.Format("[Generic Reagent Container] :: States of isFull, isEmpty, filled, and emptied: {0},{1},{2},{3}",isFull,isEmpty,filled,emptied));
    }
    public ReagentContents Spill(float spillVolume) {
        ReagentContents spillContents = contents.Spill(spillVolume);
        OnReagentContentsChanged(InjectType.Vacuum);
        return spillContents;
    }

    public void TransferMix(GenericReagentContainer injector, float amount, InjectType injectType) {
        if (!IsMixable(this.type, injectType)) {
            return;
        }
        ReagentContents spill = injector.Spill(amount);
        AddMix(spill, injectType);
        SetGenes(injector.GetGenes());
    }
    public bool AddMix(ScriptableReagent incomingReagent, float volume, InjectType injectType) {
        if (!IsMixable(type, injectType)) {
            return false;
        }
        contents.AddMix(ReagentDatabase.GetID(incomingReagent), volume, this);
        OnReagentContentsChanged(injectType);
        return true;
    }
    public bool AddMix(ReagentContents incomingReagents, InjectType injectType) {
        if (!IsMixable(type, injectType)) {
            return false;
        }
        contents.AddMix(incomingReagents, this);
        OnReagentContentsChanged(injectType);
        return true;
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
            SetGenes(new KoboldGenes());
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
            stream.SendNext(contents);
        } else {
            contents = (ReagentContents)stream.ReceiveNext();
            contents.SetMaxVolume(startingMaxVolume);
            OnReagentContentsChanged(InjectType.Metabolize);
        }
    }

    public void Save(BinaryWriter writer, string version) {
        if (contents == null) {
            Awake(); Start();
        }
        contents.Serialize(writer);
    }

    public void Load(BinaryReader reader, string version) {
        if (contents == null) {
            Awake(); Start();
        }
        //Debug.Log("[Generic Reagent Container] :: <Deserialization Process> Starting for GRC of "+gameObject.name);
        contents.Deserialize(reader);
        //Debug.Log("[Generic Reagent Container] :: <Firing OnReagentContents Change if Valid.......>");
        OnReagentContentsChanged(InjectType.Metabolize);
        ready = true;
    }
}
