using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Reagent {
    public float potentcy;
    public float volume;
    public float heat;
}

[System.Serializable]
public class InspectorReagent {
    public ReagentData.ID id;
    public float potentcy = 1f;
    public float volume = 0f;
    public float heat = 270f;
}
