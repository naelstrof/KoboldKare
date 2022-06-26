using System.Collections;
using System.Collections.Generic;
using Naelstrof.Easing;
using UnityEngine;


[CreateAssetMenu(fileName = "Physics Audio Group", menuName = "Data/Physics Audio", order = 1)]
public class PhysicsAudioGroup : ScriptableObject {
    public enum SurfaceImpactType {
        Hard,
        Soft,
        Footstep
    }
    public float minScrapeSpeed = 0.06f;
    public SurfaceImpactType surfaceType = SurfaceImpactType.Hard;
    [System.Serializable]
    public class ImpactGroup {
        public SurfaceImpactType impactType = SurfaceImpactType.Hard;
        public bool isSortedByImpactVelocity = false;
        public float minImpactMagnitude = 1f;
        public float maxImpactMagnitude = 10f;
        public List<AudioClip> impactSounds;
        public AudioClip scrapeSound;
    }
    public List<ImpactGroup> impactGroups = new List<ImpactGroup>();
    public List<PhysicMaterial> associatedMaterials;

    // Since these aren't referenced anywhere directly, this is the only way to get them to load properly.
    [RuntimeInitializeOnLoadMethod]
    public void OnInitialize() {
        PhysicsMaterialDatabase.AddToLookup(this);
    }
    public void OnEnable() {
        OnInitialize();
    }
    public AudioClip GetScrapeClip(Collider otherCollider) {
        var otherGroup = PhysicsMaterialDatabase.GetPhysicsAudioGroup(otherCollider.sharedMaterial);
        if (otherGroup != null) {
            return GetScrapeClip(otherGroup.surfaceType);
        }
        return GetScrapeClip(SurfaceImpactType.Hard);
    }
    public AudioClip GetScrapeClip(SurfaceImpactType otherSurfaceImpactType) {
        ImpactGroup targetGroup = impactGroups[0];
        foreach(var impactGroup in impactGroups) {
            if (impactGroup.impactType == otherSurfaceImpactType) {
                targetGroup = impactGroup;
                break;
            }
        }
        return targetGroup.scrapeSound;
    }
    public float GetImpactVolume(Collider otherCollider, float impactMagnitude) {
        var otherGroup = PhysicsMaterialDatabase.GetPhysicsAudioGroup(otherCollider.sharedMaterial);
        if (otherGroup != null) {
            return GetImpactVolume(otherGroup.surfaceType, impactMagnitude);
        }
        return GetImpactVolume(SurfaceImpactType.Hard, impactMagnitude);
    }
    public float GetImpactVolume(SurfaceImpactType otherSurfaceImpactType, float impactMagnitude) {
        ImpactGroup targetGroup = impactGroups[0];
        foreach(var impactGroup in impactGroups) {
            if (impactGroup.impactType == otherSurfaceImpactType) {
                targetGroup = impactGroup;
                break;
            }
        }
        return Mathf.Clamp01(Easing.Cubic.Out(0.02f+((impactMagnitude - targetGroup.minImpactMagnitude) / targetGroup.maxImpactMagnitude))*2f);
    }
    public AudioClip GetImpactClip(Collider otherCollider, float impactMagnitude) {
        var otherGroup = PhysicsMaterialDatabase.GetPhysicsAudioGroup(otherCollider.sharedMaterial);
        if (otherGroup != null) {
            return GetImpactClip(otherGroup.surfaceType, impactMagnitude);
        }
        return GetImpactClip(SurfaceImpactType.Hard, impactMagnitude);
    }
    public AudioClip GetImpactClip(SurfaceImpactType otherSurfaceImpactType, float impactMagnitude) {
        ImpactGroup targetGroup = impactGroups[0];
        foreach(var impactGroup in impactGroups) {
            if (impactGroup.impactType == otherSurfaceImpactType) {
                targetGroup = impactGroup;
                break;
            }
        }
        if (!targetGroup.isSortedByImpactVelocity) {
            return targetGroup.impactSounds[Random.Range(0, targetGroup.impactSounds.Count)];
        }
        float select = Mathf.Clamp01((impactMagnitude - targetGroup.minImpactMagnitude) / targetGroup.maxImpactMagnitude);
        return targetGroup.impactSounds[Mathf.FloorToInt(select * (targetGroup.impactSounds.Count-1))];
    }
}
