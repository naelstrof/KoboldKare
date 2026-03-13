using System.Threading.Tasks;
using NetStack.Serialization;
using SimpleJSON;
using UnityEngine;

public class GenericReagentContainer : MonoBehaviour {
    [SerializeField]
    protected float startingMaxVolume = float.MaxValue;
    
    public float GetStartingMaxVolume() => startingMaxVolume;
    
    [SerializeField]
    public InspectorReagent[] startingReagents;
    
    [System.Serializable]
    public class InspectorReagent {
        public ScriptableReagent reagent;
        public float volume;
    }
}
