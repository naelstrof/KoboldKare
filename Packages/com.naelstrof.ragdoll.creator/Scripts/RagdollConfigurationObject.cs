using System;
using UnityEngine;

[Serializable, CreateAssetMenu(fileName = "Ragdoll Configuration", menuName = "Data/Ragdoll Configuration")]
public class RagdollConfigurationObject : ScriptableObject {
    public RagdollConfiguration configuration;
}
