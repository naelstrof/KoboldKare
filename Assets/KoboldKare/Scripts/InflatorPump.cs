using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Naelstrof.Mozzarella;
using NetStack.Serialization;
using PenetrationTech;
using Photon.Pun;
using SkinnedMeshDecals;
using UnityEngine;
using Vilar.AnimationStation;

public class InflatorPump : UsableMachine, IAnimationStationSet {
    [SerializeField]
    private Sprite useSprite;
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
    [SerializeField]
    private Material cumCleanProjectorMaterial;
    [SerializeField]
    private Vector3 axis;
    [SerializeField]
    private AudioPack inflaterSloshPack;
    [SerializeField] private List<AnimationStation> stations;

    private ReadOnlyCollection<AnimationStation> readOnlyStations;

    private AudioSource sloshSource;

    [SerializeField] private Animator pumpAnimator;

    private bool spraying = false;
    private float accumulation;
    private int blendshapeID;
    private Vector3 startPos;
    private float inflateAmount;
    private float lastAccumulateTime = 0f;
    private static readonly int Pumping = Animator.StringToHash("Pumping");
    
    public override Sprite GetSprite(Kobold k) {
        return useSprite;
    }

    public override bool CanUse(Kobold k) {
        if (!constructed) {
            return false;
        }

        foreach (AnimationStation station in stations) {
            if (station.info.user == null) {
                return true;
            }
        }
        return false;
    }

    public override void LocalUse(Kobold k) {
        photonView.RequestOwnership();
        for (int i = 0; i < stations.Count; i++) {
            if (stations[i].info.user == null) {
                k.photonView.RPC(nameof(CharacterControllerAnimator.BeginAnimationRPC), RpcTarget.All,
                    photonView.ViewID, i);
                break;
            }
        }

    }

    protected override void Start() {
        readOnlyStations = stations.AsReadOnly();
        blendshapeID = pumpRenderer.sharedMesh.GetBlendShapeIndex("Expand");
        startPos = body.position;
        inflateAmount = 1f;
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = startPos;
        if (sloshSource == null) {
            sloshSource = body.gameObject.AddComponent<AudioSource>();
            sloshSource.playOnAwake = false;
            sloshSource.maxDistance = 10f;
            sloshSource.minDistance = 0.2f;
            sloshSource.rolloffMode = AudioRolloffMode.Linear;
            sloshSource.spatialBlend = 1f;
            sloshSource.loop = false;
        }
        pumpAnimator.enabled = constructed;
        pumper.enabled = constructed;
        sloshSource.enabled = false;
        base.Start();
    }

    public override void SetConstructed(bool isConstructed) {
        base.SetConstructed(isConstructed);
        pumpAnimator.enabled = isConstructed;
        pumper.enabled = isConstructed;
    }

    void FixedUpdate() {
        if (!constructed) {
            return;
        }
        //Vector3 axis = -Vector3.right;
        float distance = Vector3.Dot(body.transform.position-startPos, axis);
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
        joint.connectedAnchor = startPos + axis * ((1f - inflateAmount) * maxDistance);
    }

    private void Accumulate(float amount) {
        if (!constructed) {
            return;
        }
        accumulation += amount;
        if (amount > 0.1) {
            lastAccumulateTime = Time.time;
        }

        if (amount > 0.1 || accumulation > 5f) {
            if (!spraying && container.volume > 0f) {
                pumpAnimator.SetBool(Pumping, true);
                spraying = true;
                StartCoroutine(SprayRoutine());
            }
        }
    }

    private IEnumerator SprayRoutine() {
        if (!sloshSource.enabled) {
            sloshSource.enabled = true;
        }
        while (Time.time - lastAccumulateTime < 0.05f) {
            if (!sloshSource.isPlaying && sloshSource.gameObject.activeInHierarchy) {
                inflaterSloshPack.Play(sloshSource);
            }
            yield return null;
        }
        if (photonView.IsMine) {
            ReagentContents contents = container.Spill(accumulation);
            container.photonView.RPC(nameof(GenericReagentContainer.Spill), RpcTarget.Others, accumulation);
            if (pumper.TryGetPenetrable(out Penetrable penetrable)) {
                GenericReagentContainer holeContainer = penetrable.GetComponentInParent<GenericReagentContainer>();
                if (holeContainer != null) {
                    BitBuffer buffer = new BitBuffer(4);
                    buffer.AddReagentContents(contents);
                    holeContainer.photonView.RPC(nameof(GenericReagentContainer.ForceMixRPC), RpcTarget.All, buffer,
                        container.photonView.ViewID, (byte)GenericReagentContainer.InjectType.Inject);
                }
            } else {
                if (MozzarellaPool.instance.TryInstantiate(out Mozzarella mozzarella)) {
                    ReagentContents alloc = new ReagentContents();
                    alloc.AddMix(contents);
                    mozzarella.SetVolumeMultiplier(alloc.volume);
                    mozzarella.hitCallback += (hit, startPos, dir, length, volume) => {
                        if (photonView.IsMine) {
                            GenericReagentContainer cont = hit.collider.GetComponentInParent<GenericReagentContainer>();
                            if (cont != null && cont != container) {
                                BitBuffer buffer = new BitBuffer(4);
                                buffer.AddReagentContents(alloc.Spill(alloc.volume * 0.1f));
                                cont.photonView.RPC(nameof(GenericReagentContainer.AddMixRPC), RpcTarget.All, buffer, photonView.ViewID, (byte)GenericReagentContainer.InjectType.Inject);
                            }
                        }
                        
                        if (alloc.volume > 0f) {
                            cumSplatProjectorMaterial.color = alloc.GetColor();
                        }

                        PaintDecal.RenderDecalForCollider(hit.collider, alloc.IsCleaningAgent() ? cumCleanProjectorMaterial : cumSplatProjectorMaterial,
                            hit.point - hit.normal * 0.1f,
                            Quaternion.LookRotation(hit.normal, Vector3.up) *
                            Quaternion.AngleAxis(UnityEngine.Random.Range(-180f, 180f), Vector3.forward),
                            Vector2.one * (volume), length);
                    };
                    mozzarella.SetFollowPenetrator(pumper);
                }
            }
        }

        accumulation = 0f;
        pumpAnimator.SetBool(Pumping, false);
        spraying = false;
        sloshSource.enabled = false;
    }

    public ReadOnlyCollection<AnimationStation> GetAnimationStations() {
        return readOnlyStations;
    }
}
