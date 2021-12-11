using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Scriptable Purchasable", menuName = "Data/Purchasable", order = 1)]
public class ScriptablePurchasable : ScriptableObject {
    // TODO: Maybe have items be a generic prop that uses this scriptable object to determine how it looks and acts.
    /*[System.Serializable]
    public class InspectorReagent {
        public ScriptableReagent reagent;
        public float volume;
    }

    public List<InspectorReagent> startingReagents;*/

    public GameObject display;
    public PhotonGameObjectReference spawnPrefab;
    public float cost;
    void OnValidate() {
        spawnPrefab.OnValidate();
    }
    public static Bounds DisableAllButGraphics(GameObject target) {
        Bounds centerBounds = new Bounds(target.transform.position, Vector3.zero);
        foreach(Component c in target.GetComponentsInChildren<Component>()) {
            if (c is Renderer) {
                centerBounds.Encapsulate((c as Renderer).bounds);
                continue;
            }
            if (c is MeshFilter || c is LODGroup) {
                continue;
            }
            if (c is Behaviour) {
                (c as Behaviour).enabled = false;
            }
            if (c is Rigidbody) {
                (c as Rigidbody).isKinematic = true;
            }
        }
        return centerBounds;
    }
}
