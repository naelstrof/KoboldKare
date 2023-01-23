using System.Collections;
using System.Collections.Generic;
using JigglePhysics;
using Naelstrof.Inflatable;
using UnityEngine;

[CreateAssetMenu(fileName = "New SkinnedMeshEquipment", menuName = "Equipment/SkinnedMesh Equipment", order = 1)]
public class EquipmentSkinnedMesh : Equipment {
    [SerializeField]
    private GameObject prefabContainingSkinnedMeshRenderers;
    public override GameObject[] OnEquip(Kobold k, GameObject groundPrefab) {
        base.OnEquip(k, groundPrefab);
        SkinnedMeshRenderer targetSkinnedMesh = k.koboldBodyRenderers[0] as SkinnedMeshRenderer;
        GameObject instance = Instantiate(prefabContainingSkinnedMeshRenderers, k.transform);
        JiggleSkin jiggleSkin = k.GetComponent<JiggleSkin>();
        instance.name = name;
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
        Destroy(search.gameObject);
        return base.OnUnequip(k, dropOnGround);
    }
}
