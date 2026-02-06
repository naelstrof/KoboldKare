using System.Threading.Tasks;
using NetStack.Serialization;
using SimpleJSON;
using UnityEngine;

public class GenericReagentContainer : MonoBehaviour {
    [SerializeField,Header("This component has been deprecated, use a NetworkedEntity instead")]
    protected float startingMaxVolume = float.MaxValue;
    [SerializeField]
    public InspectorReagent[] startingReagents;
    
    [System.Serializable]
    public class InspectorReagent {
        public ScriptableReagent reagent;
        public float volume;
    }
}
