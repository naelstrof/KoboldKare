using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Localization;
using UnityEngine.Events;
using ExitGames.Client.Photon;

public interface IReagentContainerListener {
    void OnReagentContainerChanged(ReagentContents contents, ReagentContents.ReagentInjectType type);
}
[System.Serializable]
public class ReagentContents : Dictionary<ReagentData.ID, Reagent> {
    public enum ReagentContainerType {
        OpenTop = 0,
        Sealed,
        Mouth,
    }
    public enum ReagentInjectType {
        Inject = 0,
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
    private static bool IsMixable(ReagentContainerType container, ReagentInjectType injectionType ) {
        return ReagentMixMatrix[(int)injectionType,(int)container];
    }
    public bool IsMixable(ReagentInjectType injectionType ) {
        return IsMixable(containerType, injectionType);
    }
    public enum DirtyFlag {
        Volume = 1,
        Color = 2,
        Value = 4,
        Heat = 8,
    }
    private bool runningListenerUpdate = false;
    public List<IReagentContainerListener> listeners = new List<IReagentContainerListener>();
    public void AddListener(IReagentContainerListener listener) {
        listeners.Add(listener);
    }
    public void RemoveListener(IReagentContainerListener listener) {
        listeners.Remove(listener);
    }
    public void InvokeListenerUpdate(ReagentContents.ReagentInjectType injectType) {
        SetDirty(true);
        if (runningListenerUpdate) {
            return;
        }
        runningListenerUpdate = true;
        for (int i=listeners.Count-1;i>=0;i--) {
            try {
                listeners[i].OnReagentContainerChanged(this, injectType);
            } catch (Exception e){
                Debug.LogException(e);
            }
        }
        runningListenerUpdate = false;
    }
    private int dirtyFlags = 0;
    public float maxVolume = float.MaxValue;
    public ReagentContainerType containerType = ReagentContainerType.OpenTop;
    public GameObject gameObject;
    public MonoBehaviour behaviour;
    public List<Tuple<Coroutine,string>> runningRoutines = new List<Tuple<Coroutine,string>>();

    private bool IsDirty() {
        return dirtyFlags != 0;
    }
    private bool IsDirty(DirtyFlag dirt) {
        return (dirtyFlags & (1<<(int)dirt)) != 0;
    }
    private void SetDirty(bool dirty) {
        if (dirty) {
            dirtyFlags = ~(0);
        } else {
            dirtyFlags = 0;
        }
    }
    private void SetDirty(DirtyFlag dirt, bool dirty) {
        if (!dirty) {
            dirtyFlags &= ~(1<<(int)dirt);
        } else {
            dirtyFlags |= 1<<(int)dirt;
        }
    }

    public void Empty() {
        Clear();
        SetDirty(true);
        InvokeListenerUpdate(ReagentInjectType.Vacuum);
    }

    private float cachedVolume;
    private Color cachedColor;
    private float cachedValue;
    private float cachedHeat;
    private void RegenerateCacheIfNeeded(ReagentDatabase database = null) {
        if (dirtyFlags == 0) {
            return;
        }
        if (IsDirty(DirtyFlag.Volume)) {
            cachedVolume = 0f;
            foreach(KeyValuePair<ReagentData.ID, Reagent> pair in this ) {
                cachedVolume += pair.Value.volume;
            }
            // Unset the dirty flag
            SetDirty(DirtyFlag.Volume, false);
        }
        if (database == null) {
            return;
        }
        if (IsDirty(DirtyFlag.Color)) {
            cachedColor = Color.black;
            cachedColor.a = 0f;
            if (cachedVolume > 0.01f) {
                foreach (KeyValuePair<ReagentData.ID, Reagent> pair in this) {
                    cachedColor += database.reagents[pair.Key].color * (pair.Value.volume / cachedVolume);
                }
            }
            SetDirty(DirtyFlag.Color, false);
        }
        if (IsDirty(DirtyFlag.Heat)) {
            cachedHeat = 0;
            if (cachedVolume > 0.01f) {
                foreach (KeyValuePair<ReagentData.ID, Reagent> pair in this) {
                    cachedHeat += pair.Value.heat * (pair.Value.volume / Mathf.Max(cachedVolume, Mathf.Epsilon));
                }
            }
            SetDirty(DirtyFlag.Heat, false);
        }
        if (IsDirty(DirtyFlag.Value)) {
            cachedValue = 0;
            foreach(KeyValuePair<ReagentData.ID, Reagent> pair in this ) {
                cachedValue += database.reagents[pair.Key].value*pair.Value.volume;
            }
            SetDirty(DirtyFlag.Value, false);
        }
    }
    public void Mix(ReagentContents contents, ReagentInjectType injectType = ReagentInjectType.Inject) {
        foreach(KeyValuePair<ReagentData.ID, Reagent> pair in contents ) {
            Mix(pair.Key, pair.Value, injectType);
        }
    }
    public void Mix(ReagentData.ID id, Reagent r, ReagentInjectType injectType = ReagentInjectType.Inject) {
        Mix(id,r.volume, r.potentcy, r.heat, injectType);
    }
    public void DoReaction(ReagentReaction r) {
        float minReaction = Mathf.Min(this[r.a].volume / r.aAmount, this[r.b].volume / r.bAmount);
        this[r.a].volume -= minReaction * r.aAmount;
        this[r.b].volume -= minReaction * r.bAmount;
        float averagePotentcy = (this[r.a].potentcy + this[r.b].potentcy) / 2f;
        float averageHeat = (this[r.a].heat + this[r.b].heat) / 2f;
        if (this[r.a].volume <= 0.01f) {
            this.Remove(r.a);
        }
        if (this[r.b].volume <= 0.01f) {
            this.Remove(r.b);
        }
        SetDirty(true);
        if (minReaction * r.cAmount > Mathf.Epsilon) {
            Mix(r.c, minReaction * r.cAmount, averagePotentcy, averageHeat);
        }
    }
    public void RedoOnExistCallbacks() {
        foreach (var pair in this) {
            if (pair.Value.volume > 0.01f) {
                GameManager.instance.reagentDatabase.reagents[pair.Key].onExistCallback.Invoke(this);
            }
        }
    }
    public void Mix(ReagentData.ID id, float volume, float potentcy = 1f, float heat = 293f, ReagentInjectType injectType = ReagentInjectType.Inject) {
        if (!IsMixable(injectType)) { 
            return;
        }
        if (ContainsKey(id)) {
            float oldVolume = this[id].volume;
            this[id].volume += volume;
            float ratio = oldVolume/Mathf.Max(this[id].volume,Mathf.Epsilon);
            this[id].heat = this[id].heat*(ratio)+heat*(1f/ratio);
            this[id].potentcy = this[id].potentcy*(ratio)+potentcy*(1f/ratio);
        } else {
            Add(id, new Reagent{volume = volume, potentcy = potentcy, heat = heat});
        }
        if (gameObject != null && GameManager.instance != null) {
            List<ReagentData.ID> keys = new List<ReagentData.ID>(this.Keys);
            foreach (ReagentData.ID key in keys) {
                int reactionID = ReagentData.GetIDPair(key, id);
                if (GameManager.instance.reagentDatabase.reactions.ContainsKey(reactionID)) {
                    ReagentReaction react = GameManager.instance.reagentDatabase.reactions[reactionID];
                    if (this.ContainsKey(react.a) && this.ContainsKey(react.b) && this[react.a].volume > Mathf.Epsilon && this[react.b].volume > Mathf.Epsilon) {
                        DoReaction(react);
                    }
                }
            }
            GameManager.instance.reagentDatabase.reagents[id].onExistCallback.Invoke(this);
        }
        // Things changed! set ourselves dirty
        SetDirty(true);
        if (this.volume > maxVolume) {
            Spill(this.volume - maxVolume, injectType);
        } else {
            InvokeListenerUpdate(injectType);
        }
    }
    public void TriggerChange(ReagentInjectType injectType = ReagentInjectType.Inject) {
        SetDirty(true);
        InvokeListenerUpdate(injectType);
    }
    public ReagentContents FilterFluids(float spilledVolume, ReagentDatabase database, ReagentInjectType injectType = ReagentInjectType.Vacuum) {
        ReagentContents spilledContents = new ReagentContents();
        if (spilledVolume <= 0f) {
            return spilledContents;
        }
        float fluidVolume = 0f;
        foreach (KeyValuePair<ReagentData.ID, Reagent> pair in this) {
            if (!database.reagents[pair.Key].isFluid) {
                continue;
            }
            fluidVolume += pair.Value.volume;
        }

        float percentageSpill = Mathf.Min(spilledVolume/Mathf.Max(fluidVolume,Mathf.Epsilon), 1f);
        foreach(KeyValuePair<ReagentData.ID, Reagent> pair in this ) {
            if (!database.reagents[pair.Key].isFluid) {
                continue;
            }
            float lossAmount = pair.Value.volume * percentageSpill;
            pair.Value.volume -= lossAmount;
            spilledContents.Mix(pair.Key, lossAmount, pair.Value.potentcy, pair.Value.heat);
        }
        SetDirty(true);
        InvokeListenerUpdate(injectType);
        return spilledContents;
    }
    public ReagentContents Spill(float spilledVolume, ReagentInjectType injectType = ReagentInjectType.Vacuum) {
        ReagentContents spilledContents = new ReagentContents();
        if (spilledVolume <= 0f) {
            return spilledContents;
        }
        float percentageSpill = Mathf.Min(spilledVolume/Mathf.Max(volume,Mathf.Epsilon), 1f);
        foreach(KeyValuePair<ReagentData.ID, Reagent> pair in this ) {
            float lossAmount = pair.Value.volume * percentageSpill;
            pair.Value.volume -= lossAmount;
            spilledContents.Mix(pair.Key, lossAmount, pair.Value.potentcy, pair.Value.heat);
        }
        SetDirty(true);
        InvokeListenerUpdate(injectType);
        return spilledContents;
    }
    public ReagentContents Metabolize(ReagentDatabase database, float deltaTime) {
        ReagentContents metabolizedReagents = new ReagentContents();
        foreach(KeyValuePair<ReagentData.ID, Reagent> pair in this ) {
            float v = pair.Value.volume-database.reagents[pair.Key].metabolizationMin;
            if (Mathf.Approximately(v,0)) {
                continue;
            }
            // Prevent really tiny, useless updates.
            if (v <= 0.1f) {
                metabolizedReagents.Mix(pair.Key, v, pair.Value.potentcy, pair.Value.heat);
                pair.Value.volume = 0f;
                continue;
            }
            float metaHalfLife = database.reagents[pair.Key].metabolizationHalfLife == 0 ? v : v * Mathf.Pow(0.5f, deltaTime / database.reagents[pair.Key].metabolizationHalfLife);
            float loss = Mathf.Max(v - metaHalfLife, 0f);
            pair.Value.volume = Mathf.Max(metaHalfLife, 0f);
            metabolizedReagents.Mix(pair.Key, loss, pair.Value.potentcy, pair.Value.heat);
        }
        if (!Mathf.Approximately(metabolizedReagents.volume, 0f)) {
            SetDirty(true);
            InvokeListenerUpdate(ReagentInjectType.Metabolize);
        }
        return metabolizedReagents;
    }
    public float volume {
        get {
            RegenerateCacheIfNeeded();
            return cachedVolume;
        }
    }
    public float heat {
        get {
            RegenerateCacheIfNeeded();
            return cachedHeat;
        }
    }
    public float GetValue(ReagentDatabase database) {
        RegenerateCacheIfNeeded(database);
        return cachedValue;
    }
    public Color GetColor(ReagentDatabase database) {
        RegenerateCacheIfNeeded(database);
        if (float.IsNaN(cachedColor.r) || float.IsNaN(cachedColor.g) || float.IsNaN(cachedColor.b) || float.IsNaN(cachedColor.a)) {
            return Color.black;
        }
        return cachedColor;
    }
    public static ReagentContents operator /(ReagentContents a, float b) {
        ReagentContents dividedContents = new ReagentContents();
        foreach(KeyValuePair<ReagentData.ID, Reagent> pair in a ) {
            dividedContents.Mix(pair.Key, pair.Value.volume/b, pair.Value.potentcy, pair.Value.heat);
        }
        return dividedContents;
    }
    public static ReagentContents operator *(ReagentContents a, float b) {
        ReagentContents multipliedContents = new ReagentContents();
        foreach(KeyValuePair<ReagentData.ID, Reagent> pair in a ) {
            multipliedContents.Mix(pair.Key, pair.Value.volume*b, pair.Value.potentcy, pair.Value.heat);
        }
        return multipliedContents;
    }
    public static readonly byte[] memReagent = new byte[sizeof(float)*3];
    public static short SerializeReagent(StreamBuffer outStream, object customObject) {
        Reagent reagent = (Reagent)customObject;
        lock (memReagent) {
            byte[] bytes = memReagent;
            int index = 0;
            Protocol.Serialize(reagent.heat, bytes, ref index);
            Protocol.Serialize(reagent.potentcy, bytes, ref index);
            Protocol.Serialize(reagent.volume, bytes, ref index);
            outStream.Write(bytes, 0, sizeof(float)*3);
        }

        return sizeof(float)*3;
    }
    public static object DeserializeReagent(StreamBuffer inStream, short length) {
        Reagent reagent = new Reagent();
        lock (memReagent) {
            inStream.Read(memReagent, 0, sizeof(float)*3);
            int index = 0;
            Protocol.Deserialize(out reagent.heat, memReagent, ref index);
            Protocol.Deserialize(out reagent.potentcy, memReagent, ref index);
            Protocol.Deserialize(out reagent.volume, memReagent, ref index);
        }

        return reagent;
    }
    public static readonly byte[] memReagentID = new byte[sizeof(short)];
    public static short SerializeReagentDataID(StreamBuffer outStream, object customObject) {
        ReagentData.ID id = (ReagentData.ID)customObject;
        lock (memReagentID) {
            byte[] bytes = memReagentID;
            int index = 0;
            Protocol.Serialize((short)id, bytes, ref index);
            outStream.Write(bytes, 0, sizeof(short));
        }
        return sizeof(short);
    }
    public static object DeserializeReagentDataID(StreamBuffer inStream, short length) {
        short id = (short)ReagentData.ID.Water;
        
        lock (memReagentID) {
            inStream.Read(memReagentID, 0, sizeof(short));
            int index = 0;
            Protocol.Deserialize(out id, memReagentID, ref index);
        }
        return (ReagentData.ID)id;
    }
    public static short SerializeReagentContents(StreamBuffer outStream, object customObject) {
        ReagentContents reagentContents = (ReagentContents)customObject;
        short size = (short)((sizeof(short) + sizeof(float)*3) * reagentContents.Count);
        byte[] bytes = new byte[size];
        int index = 0;
        foreach (KeyValuePair<ReagentData.ID, Reagent> pair in reagentContents) {
            Protocol.Serialize((short)pair.Key, bytes, ref index);
            Protocol.Serialize(pair.Value.heat, bytes, ref index);
            Protocol.Serialize(pair.Value.potentcy, bytes, ref index);
            Protocol.Serialize(pair.Value.volume, bytes, ref index);
        }
        outStream.Write(bytes, 0, size);
        return size;
    }
    public static object DeserializeReagentContents(StreamBuffer inStream, short length) {
        ReagentContents reagentContents = new ReagentContents();
        byte[] bytes = new byte[length];
        inStream.Read(bytes, 0, length);
        int index = 0;
        while (index < length) {
            Reagent r = new Reagent();
            short id = 0;

            Protocol.Deserialize(out id, bytes, ref index);
            Protocol.Deserialize(out r.heat, bytes, ref index);
            Protocol.Deserialize(out r.potentcy, bytes, ref index);
            Protocol.Deserialize(out r.volume, bytes, ref index);
            reagentContents[(ReagentData.ID)id] = r;
        }
        reagentContents.SetDirty(true);
        return reagentContents;
    }
}

//[System.Serializable]
//public class ReagentReaction {
    //public Reagent.ID a,b;
    //public float aAmount, bAmount = 1f;
    //public Reagent.ID result;
    //public float resultAmount = 2f;
//}
