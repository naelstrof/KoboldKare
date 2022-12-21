using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New WearableEquipment", menuName = "Equipment/WearableEquipment", order = 1)]
public class WearableEquipment : Equipment {
    [System.Serializable]
    public class PrefabAttachPointPair {
        public GameObject wearablePrefab;
        public Equipment.AttachPoint attachPoint;
        public bool stacks = false;
        public Vector3 stackOffset = Vector3.zero;
    }
    public List<PrefabAttachPointPair> wearables = new List<PrefabAttachPointPair>();
    public override GameObject[] OnEquip(Kobold k, GameObject groundPrefab) {
        base.OnEquip(k, groundPrefab);
        KoboldInventory inventory = k.GetComponent<KoboldInventory>();
        GameObject[] stuff = new GameObject[wearables.Count];
        int i = 0;
        foreach (PrefabAttachPointPair p in wearables) {
            int ecount = inventory.GetEquipmentInstanceCount(this);

            if (p.stacks) {
                Transform targetAttach = k.GetAttachPointTransform(p.attachPoint);
                Vector3 targetForward = Vector3.up;
                if (targetAttach.childCount != 0) {
                    targetForward = targetAttach.GetChild(0).localPosition.normalized;
                }
                Vector3 offset = ecount * p.stackOffset;

                offset += p.wearablePrefab.transform.GetChild(0).localPosition;
                if (Vector3.Dot(offset, targetForward) > 0f) {
                    targetAttach = FindBoneAttachDown(targetAttach, ref offset);
                    offset -= p.wearablePrefab.transform.GetChild(0).localPosition;
                } else {
                    targetAttach = FindBoneAttachUp(targetAttach, ref offset);
                }

                GameObject wearable = GameObject.Instantiate(p.wearablePrefab, targetAttach );
                wearable.transform.localPosition += offset;
                stuff[i++] = wearable;
            } else {
                GameObject wearable = GameObject.Instantiate(p.wearablePrefab, k.GetAttachPointTransform(p.attachPoint));
                stuff[i++] = wearable;
            }
        }
        return stuff;
    }

    private Transform FindBoneAttachUp(Transform currentBone, ref Vector3 remainingOffset) {
        if (currentBone.parent == null) {
            return currentBone;
        }
        float boneLength = currentBone.localPosition.magnitude; 
        if (remainingOffset.magnitude < boneLength) {
            return currentBone.parent;
        }
        remainingOffset *= (remainingOffset.magnitude - boneLength)/remainingOffset.magnitude;
        return FindBoneAttachUp(currentBone.parent, ref remainingOffset);
    }
    private Transform FindBoneAttachDown(Transform currentBone, ref Vector3 remainingOffset) {
        if (currentBone.childCount <=0) {
            return currentBone;
        }
        float boneLength = currentBone.GetChild(0).localPosition.magnitude; 
        if (remainingOffset.magnitude < boneLength) {
            return currentBone;
        }
        remainingOffset *= (remainingOffset.magnitude - boneLength)/remainingOffset.magnitude;
        return FindBoneAttachDown(currentBone.GetChild(0), ref remainingOffset);
    }

    public override GameObject OnUnequip(Kobold k, bool dropOnGround = true) {
        return base.OnUnequip(k, dropOnGround);
    }
}
