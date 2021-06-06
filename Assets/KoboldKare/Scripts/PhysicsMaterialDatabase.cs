using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PhysicsMaterialDatabase  {
    private static Dictionary<PhysicMaterial, List<PhysicsAudioGroup>> optimizedLookup = new Dictionary<PhysicMaterial, List<PhysicsAudioGroup>>();
    public static void AddToLookup(PhysicsAudioGroup group) {
        foreach(PhysicMaterial material in group.associatedMaterials) {
            if (optimizedLookup.ContainsKey(material)) {
                if (!optimizedLookup[material].Contains(group)) {
                    optimizedLookup[material].Add(group);
                }
                continue;
            }
            optimizedLookup.Add(material, new List<PhysicsAudioGroup>());
            optimizedLookup[material].Add(group);
        }
    }
    public static PhysicsAudioGroup GetPhysicsAudioGroup(PhysicMaterial material) {
        if (material == null) {
            return null;
        }
        if (!optimizedLookup.ContainsKey(material)) {
            return null;
        }
        return optimizedLookup[material][UnityEngine.Random.Range(0, optimizedLookup[material].Count)];
    }
}
