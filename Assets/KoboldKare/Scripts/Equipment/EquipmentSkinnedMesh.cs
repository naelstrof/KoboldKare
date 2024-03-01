using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JigglePhysics;
using Naelstrof.Inflatable;
using UnityEngine;

[CreateAssetMenu(fileName = "New SkinnedMeshEquipment", menuName = "Equipment/SkinnedMesh Equipment", order = 1)]
public class EquipmentSkinnedMesh : Equipment {
    [SerializeField] private GameObject prefabContainingSkinnedMeshRenderers;
    [SerializeField] private SkinnedMeshTweak[] blendShapesToTweak;
    private List<SkinnedMeshRenderer> targets;
    

    private class BlendShapeCopier : MonoBehaviour {
        public SkinnedMeshRenderer source;
        private List<SkinnedMeshRenderer> targets;

        void Awake() {
            targets = new List<SkinnedMeshRenderer>(GetComponentsInChildren<SkinnedMeshRenderer>());
        }

        void Update() {
            foreach (var target in targets) {
                CopyBlendShapes(target);
            }
        }

        void CopyBlendShapes(SkinnedMeshRenderer target) {
            int blendShapeCount = source.sharedMesh.blendShapeCount;
            for (int i = 0; i < blendShapeCount; i++) {
                string blendShapeName = source.sharedMesh.GetBlendShapeName(i);
                int sourceBlendShapeIndex = source.sharedMesh.GetBlendShapeIndex(blendShapeName);
                int targetBlendShapeIndex = target.sharedMesh.GetBlendShapeIndex(blendShapeName);
                if (sourceBlendShapeIndex != -1 && targetBlendShapeIndex != -1) {
                    float weight = source.GetBlendShapeWeight(sourceBlendShapeIndex);
                    target.SetBlendShapeWeight(targetBlendShapeIndex, weight);
                }
            }
        }
    }

    public override GameObject[] OnEquip(Kobold k, GameObject groundPrefab) {
        base.OnEquip(k, groundPrefab);
        SkinnedMeshRenderer targetSkinnedMesh = k.koboldBodyRenderers[0] as SkinnedMeshRenderer;
        GameObject instance = Instantiate(prefabContainingSkinnedMeshRenderers, k.transform);
        JiggleSkin jiggleSkin = k.GetComponent<JiggleSkin>();
        instance.name = name;
        // Tweak - change blend shapes on equip
        if (blendShapesToTweak != null)
        {
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
        }
        BlendShapeCopier copier = instance.AddComponent<BlendShapeCopier>();
        copier.source = targetSkinnedMesh;
        foreach (var skinnedMesh in instance.GetComponentsInChildren<SkinnedMeshRenderer>()) {
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

        return new[] {
            instance
        };
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
        if (blendShapesToTweak != null)
        {
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
        }    
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
    public float originalShapeValue = 0;  // Store values here on equip, just in case the value isn't 0 when equipped
}