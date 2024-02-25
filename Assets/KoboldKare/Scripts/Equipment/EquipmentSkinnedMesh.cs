using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JigglePhysics;
using Naelstrof.Inflatable;
using PenetrationTech;
using UnityEngine;

[CreateAssetMenu(fileName = "New SkinnedMeshEquipment", menuName = "Equipment/SkinnedMesh Equipment", order = 1)]
public class EquipmentSkinnedMesh : Equipment {
    [SerializeField]
    private GameObject prefabContainingSkinnedMeshRenderers;
    [SerializeField]
    private SkinnedMeshTweak[] blendShapesToTweak;
    public override GameObject[] OnEquip(Kobold k, GameObject groundPrefab) {
        base.OnEquip(k, groundPrefab);
        SkinnedMeshRenderer targetSkinnedMesh = k.koboldBodyRenderers[0] as SkinnedMeshRenderer;
        GameObject instance = Instantiate(prefabContainingSkinnedMeshRenderers, k.transform);
        JiggleSkin jiggleSkin = k.GetComponent<JiggleSkin>();
        instance.name = name;
        // Tweak - change blend shapes on equip
        foreach (SkinnedMeshTweak blend in blendShapesToTweak)
        {
            foreach (SkinnedMeshRenderer mesh in k.koboldBodyRenderers)
            {
                Mesh m = mesh.sharedMesh;
                int index = m.GetBlendShapeIndex(blend.blendShapeName);
                if(index == -1) continue;
                blend.originalShapeValue = mesh.GetBlendShapeWeight(index);
                mesh.SetBlendShapeWeight(index, blend.shapeValue);
            }
        }
        // - - - - - - - - - - - - 
        foreach(var skinnedMesh in instance.GetComponentsInChildren<SkinnedMeshRenderer>()) {
            Transform[] newBoneList = new Transform[targetSkinnedMesh.bones.Length];
            for (int i = 0; i < targetSkinnedMesh.bones.Length; i++) {
                newBoneList[i] = targetSkinnedMesh.bones[i];
            }
            skinnedMesh.bones = newBoneList;
            skinnedMesh.rootBone = targetSkinnedMesh.rootBone;
            k.koboldBodyRenderers.Add(skinnedMesh);
            foreach (var inflater in k.GetAllInflatableListeners()) {
                if (inflater is InflatableBreast inflatableBreast) {
                    inflatableBreast.AddTargetRenderer(skinnedMesh);
                }
                if (inflater is InflatableBelly belly) {
                    belly.AddTargetRenderer(skinnedMesh);
                }
                if (inflater is InflatableBlendShape inflatableBlendShape) {
                   inflatableBlendShape.AddTargetRenderer(skinnedMesh);
                }
            }

            if (jiggleSkin != null) {
                jiggleSkin.targetSkins.Add(skinnedMesh);
            }
        }
        return new[] { instance };
    }

    public override GameObject OnUnequip(Kobold k, bool dropOnGround = true) {
        JiggleSkin jiggleSkin = k.GetComponent<JiggleSkin>();
        Transform search = k.transform.Find(name);
        if (search == null) return base.OnUnequip(k, dropOnGround);
        foreach (var skinnedMesh in search.GetComponentsInChildren<SkinnedMeshRenderer>()) {
            if (jiggleSkin != null) {
                jiggleSkin.targetSkins.Remove(skinnedMesh);
            }
            foreach (var inflater in k.GetAllInflatableListeners()) {
                if (inflater is InflatableBreast inflatableBreast) {
                    inflatableBreast.RemoveTargetRenderer(skinnedMesh);
                }
                if (inflater is InflatableBelly belly) {
                    belly.RemoveTargetRenderer(skinnedMesh);
                }
                if (inflater is InflatableBlendShape inflatableBlendShape) {
                   inflatableBlendShape.RemoveTargetRenderer(skinnedMesh);
                }
            }
            k.koboldBodyRenderers.Remove(skinnedMesh);
        }
        // Tweak - reset blend shapes on unequip
        foreach (SkinnedMeshTweak blend in blendShapesToTweak)
        {
            foreach (SkinnedMeshRenderer mesh in k.koboldBodyRenderers)
            {
                Mesh m = mesh.sharedMesh;
                int index = m.GetBlendShapeIndex(blend.blendShapeName);
                if(index == -1) continue;
                mesh.SetBlendShapeWeight(index, blend.originalShapeValue);
            }
        }
        // - - - - - - - - - - - - 
        Destroy(search.gameObject);
        return base.OnUnequip(k, dropOnGround);
    }
}

[System.Serializable]
public class SkinnedMeshTweak
{
    [SerializeField]
    public string blendShapeName;
    [SerializeField] [Range(0.0f, 100.0f)]
    public float shapeValue;
    [NonSerialized] 
    public float originalShapeValue = 0;
}
