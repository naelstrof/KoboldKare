using System.Collections;
using System.Collections.Generic;
using Naelstrof.Mozzarella;
using PenetrationTech;
using Photon.Pun;
using SkinnedMeshDecals;
using UnityEngine;

public class InflatorPump : MonoBehaviourPun {
    [SerializeField]
    private Rigidbody body;
    [SerializeField]
    private SpringJoint joint;
    [SerializeField]
    private SkinnedMeshRenderer pumpRenderer;
    [SerializeField]
    private float maxDistance = 0.7f;
    [SerializeField]
    private Penetrator pumper;
    [SerializeField]
    private GenericReagentContainer container;
    [SerializeField]
    private Material cumSplatProjectorMaterial;

    private bool spraying = false;
    private float accumulation;
    private int blendshapeID;
    private Vector3 startPos;
    private float inflateAmount;
    private float lastAccumulateTime = 0f;
    void Start() {
        blendshapeID = pumpRenderer.sharedMesh.GetBlendShapeIndex("Expand");
        startPos = body.position;
        inflateAmount = 1f;
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = startPos;
    }

    void FixedUpdate() {
        float distance = Vector3.Dot(body.transform.position-startPos, Vector3.down);
        float clamp = Mathf.Clamp01(distance/maxDistance);
        pumpRenderer.SetBlendShapeWeight(blendshapeID, (1f-clamp) * 100f);
        float newInflateAmount = Mathf.Min(Mathf.Min((1f - clamp)+0.1f,1f), inflateAmount);
        float diff = Mathf.Max(inflateAmount - newInflateAmount, 0f);
        float spillAmount = diff * 10f;
        if (photonView.IsMine) {
            Accumulate(spillAmount);
        }
        inflateAmount = newInflateAmount;
        inflateAmount = Mathf.MoveTowards(inflateAmount, 1f, Time.deltaTime*1f);
        joint.connectedAnchor = startPos + Vector3.down * ((1f - inflateAmount) * maxDistance);
    }

    private void Accumulate(float amount) {
        accumulation += amount;
        if (amount > 0.1) {
            lastAccumulateTime = Time.time;
        }

        if (amount > 0.1 || accumulation > 5f) {
            if (!spraying) {
                spraying = true;
                StartCoroutine(SprayRoutine());
            }
        }
    }

    private IEnumerator SprayRoutine() {
        while (Time.time - lastAccumulateTime < 0.05f) {
            yield return null;
        }
        if (photonView.IsMine) {
            ReagentContents contents = container.Spill(accumulation);
            container.photonView.RPC(nameof(GenericReagentContainer.Spill), RpcTarget.Others, accumulation);
            if (pumper.TryGetPenetrable(out Penetrable penetrable)) {
                GenericReagentContainer holeContainer = penetrable.GetComponentInParent<GenericReagentContainer>();
                if (holeContainer != null) {
                    holeContainer.photonView.RPC(nameof(GenericReagentContainer.ForceMixRPC), RpcTarget.All, contents,
                        container.photonView.ViewID);
                }
            } else {
                if (MozzarellaPool.instance.TryInstantiate(out Mozzarella mozzarella)) {
                    ReagentContents alloc = new ReagentContents();
                    alloc.AddMix(contents);
                    mozzarella.SetVolumeMultiplier(alloc.volume * 2f);
                    mozzarella.hitCallback += (hit, startPos, dir, length, volume) => {
                        if (photonView.IsMine) {
                            GenericReagentContainer cont = hit.collider.GetComponentInParent<GenericReagentContainer>();
                            if (cont != null && cont != container) {
                                cont.photonView.RPC(nameof(GenericReagentContainer.AddMixRPC), RpcTarget.All, alloc.Spill(alloc.volume * 0.1f), photonView.ViewID);
                            }
                        }
                        
                        if (alloc.volume > 0f) {
                            cumSplatProjectorMaterial.color = alloc.GetColor();
                        }

                        PaintDecal.RenderDecalForCollider(hit.collider, cumSplatProjectorMaterial,
                            hit.point - hit.normal * 0.1f,
                            Quaternion.LookRotation(hit.normal, Vector3.up) *
                            Quaternion.AngleAxis(UnityEngine.Random.Range(-180f, 180f), Vector3.forward),
                            Vector2.one * (volume * 4f), length);
                    };
                    mozzarella.SetFollowPenetrator(pumper);
                }
            }
        }

        accumulation = 0f;
        spraying = false;
    }
}
