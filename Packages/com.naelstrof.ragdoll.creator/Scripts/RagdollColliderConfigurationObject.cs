using System;
using UnityEngine;

[Serializable, CreateAssetMenu(fileName = "Ragdoll Collider Configuration", menuName = "Data/Ragdoll Collider Configuration")]
public class RagdollColliderConfigurationObject : ScriptableObject {
    public RagdollColliderConfiguration configuration;
}
