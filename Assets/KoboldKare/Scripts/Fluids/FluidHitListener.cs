using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MozzarellaHitEventListener))]
public class FluidHitListener : MonoBehaviour {
    public ReagentContents transferContents;
    [SerializeField]
    [Range(0.001f,1f)]
    public float decalSize = 0.1f;
    //[SerializeField]
    //private LayerMask hitMask;
    public Material projector;
    [SerializeField]
    private Material eraser;
    [HideInInspector]
    public bool erasing = false;
    private static HashSet<GenericReagentContainer> staticTargets = new HashSet<GenericReagentContainer>();
    private static Collider[] staticColliders = new Collider[32];
    void Awake() {
        transferContents = new ReagentContents();
    }
    void Start() {
        GetComponent<MozzarellaHitEventListener>().OnDepthBufferHit += OnDepthBufferHit;
    }
    void SplashTransfer(Vector3 position, float radius, float amount) {
        if (erasing) {
            radius *= 1.2f;
        }
        ReagentContents spill = transferContents.Spill(amount);
        if (spill.volume <= 0f) {
            return;
        }

        staticTargets.Clear();
        int hits = Physics.OverlapSphereNonAlloc(position, radius, staticColliders, GameManager.instance.waterSprayHitMask, QueryTriggerInteraction.Ignore);
        for(int i=0;i<hits;i++) {
            Collider c = staticColliders[i];
            GenericReagentContainer target = c.GetComponentInParent<GenericReagentContainer>();
            if (target != null && GenericReagentContainer.IsMixable(target.type, GenericReagentContainer.InjectType.Spray)) {
                staticTargets.Add(target);
            }
            SkinnedMeshDecals.PaintDecal.RenderDecalForCollider(c, erasing ? eraser : projector, position-Vector3.forward*radius, Quaternion.identity, Vector2.one*radius, radius*2f);
        }
        float totalTargets = staticTargets.Count;
        foreach(var target in staticTargets) {
            target.AddMix(spill, GenericReagentContainer.InjectType.Spray);
        }
    }
    void OnDepthBufferHit(List<MozzarellaHitEventListener.HitEvent> hitEvents) {
        foreach(var hitEvent in hitEvents) {
            DrawDecal(hitEvent);
        }
    }
    void DrawDecal(MozzarellaHitEventListener.HitEvent hitEvent) {
        float size = hitEvent.volume*decalSize;
        SplashTransfer(hitEvent.position, size*2f, Time.deltaTime*15f);
    }
}

