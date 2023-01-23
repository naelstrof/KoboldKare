using System.Collections;
using System.Collections.Generic;
using Naelstrof.Mozzarella;
using NetStack.Serialization;
using Photon.Pun;
using SkinnedMeshDecals;
using UnityEngine;

[System.Serializable]
public class MilkLactator {
    [SerializeField]
    private List<Transform> nipples;
    [SerializeField]
    private Material milkSplatMaterial;

    private WaitForSeconds waitSpurt;

    private bool milking;

    public void Awake() {
        waitSpurt = new WaitForSeconds(1f);
    }

    public void StartMilking(Kobold targetKobold) {
        targetKobold.StartCoroutine(MilkRoutine(targetKobold));
    }

    private IEnumerator MilkRoutine(Kobold kobold) {
        PhotonProfiler.LogReceive(1);
        while (milking) {
            yield return null;
        }
        milking = true;
        int pulses = 12;
        // Now do some milk stuff.
        for (int i = 0; i < pulses; i++) {
            foreach (Transform t in nipples) {
                if (MozzarellaPool.instance.TryInstantiate(out Mozzarella mozzarella)) {
                    mozzarella.SetFollowTransform(t);
                    ReagentContents alloc = new ReagentContents();
                    alloc.AddMix(ReagentDatabase.GetReagent("Milk").GetReagent(kobold.GetGenes().breastSize/(pulses*nipples.Count)));
                    mozzarella.SetVolumeMultiplier(alloc.volume);
                    mozzarella.SetLocalForward(Vector3.up);
                    Color color = alloc.GetColor();
                    mozzarella.hitCallback += (hit, startPos, dir, length, volume) => {
                        if (kobold.photonView.IsMine) {
                            GenericReagentContainer container =
                                hit.collider.GetComponentInParent<GenericReagentContainer>();
                            if (container != null && kobold != null) {
                                BitBuffer buffer = new BitBuffer(4);
                                buffer.AddReagentContents(alloc.Spill(alloc.volume * 0.1f));
                                container.photonView.RPC(nameof(GenericReagentContainer.AddMixRPC), RpcTarget.All,
                                    buffer, kobold.photonView.ViewID, (byte)GenericReagentContainer.InjectType.Spray);
                            }
                        }
                        milkSplatMaterial.color = color;
                        PaintDecal.RenderDecalForCollider(hit.collider, milkSplatMaterial,
                            hit.point - hit.normal * 0.1f, Quaternion.LookRotation(hit.normal, Vector3.up)*Quaternion.AngleAxis(UnityEngine.Random.Range(-180f,180f), Vector3.forward),
                            Vector2.one * (volume * 4f), length);
                    };
                }
            }
            yield return waitSpurt;
        }
        milking = false;
    }
}
