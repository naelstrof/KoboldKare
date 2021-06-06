using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;


[CreateAssetMenu(fileName = "NewReagentDatabase", menuName = "Data/Reagent Database", order = 1)]
public class ReagentDatabase : ScriptableObject {
    [SerializeField]
    private List<ReagentData> serializedReagents = new List<ReagentData>();

    [SerializeField]
    private List<ReagentReaction> serializedReactions = new List<ReagentReaction>();

    [NonSerialized]
    public Dictionary<ReagentData.ID, ReagentData> reagents = new Dictionary<ReagentData.ID, ReagentData>();

    [NonSerialized]
    public Dictionary<int, ReagentReaction> reactions = new Dictionary<int, ReagentReaction>();
    //public List<ReagentReaction> reagentReactions = new List<ReagentReaction>();
    public void OnEnable() {
        reagents.Clear();
        // faster fetch
        foreach(ReagentData r in serializedReagents) {
            if (reagents.ContainsKey(r.id)) {
                Debug.LogException(new UnityException("ReagentData index " + serializedReagents.IndexOf(r) + " contains duplicate id " + r.id + "! Might not appear in-game."));
                continue;
            }
            reagents.Add(r.id, r);
        }
        reactions.Clear();
        foreach(ReagentReaction r in serializedReactions) {
            reactions.Add(ReagentData.GetIDPair(r.a,r.b), r);
        }
    }
}

[System.Serializable]
public class ReagentReaction {
    public ReagentData.ID a, b, c;
    public float aAmount = 1f;
    public float bAmount = 1f;
    public float cAmount = 2f;
}

[System.Serializable]
public class ReagentEvent : UnityEvent<ReagentContents> { }
[System.Serializable]
public class ReagentData {
    public ID id;
    public LocalizedString localizedName;
    [ColorUsage(true, true)]
    public Color color;
    public float value;
    public float metabolizationHalfLife;
    public float metabolizationMin = 0f;
    public bool isFluid;
    public ReagentEvent onExistCallback;

    public enum ID : short {
        Water,
        Blood,
        Ice,
        Milk,
        MilkShake,
        GrowthSerum,
        MelonJuice,
        EggplantJuice,
        UnstableReagent,
        Cum,
        Egg,
        Fat,
        IncompleteGrowthSerum,
        Love,
        ScrambledEgg,
        Explosium,
        Potassium,
        IncompleteGrowthSerum2,
        Wood,
        PineappleJuice,
    }
    public static int GetIDPair(ID a, ID b) {
        short small = a < b ? (short)a : (short)b; 
        short big = a < b ? (short)b : (short)a; 
        int id = (((int)small)<<sizeof(short)*4) | ((int)big);
        return id;
    }
}