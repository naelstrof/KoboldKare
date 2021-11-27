using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MozzarellaHitEventListener))]
public class FluidHitListener : MonoBehaviour {
    [SerializeField]
    [Range(0.001f,1f)]
    public float decalSize = 0.1f;
    //[SerializeField]
    //private LayerMask hitMask;
    public Material projector;
    [SerializeField]
    private Material eraser;
    private bool erasing = false;
    private Collider[] colliders;
    void Awake() {
        colliders = new Collider[32];
    }
    void Start() {
        GetComponent<MozzarellaHitEventListener>().OnDepthBufferHit += OnDepthBufferHit;
    }
    void OnDepthBufferHit(List<MozzarellaHitEventListener.HitEvent> hitEvents) {
        foreach(var hitEvent in hitEvents) {
            DrawDecal(hitEvent);
        }
    }
    void DrawDecal(MozzarellaHitEventListener.HitEvent hitEvent) {
        float size = hitEvent.volume*decalSize;
        if (erasing) {
            SkinnedMeshDecals.PaintDecal.RenderDecalInSphere(hitEvent.position, size, eraser, Quaternion.identity, GameManager.instance.waterSprayHitMask);
        } else {
            SkinnedMeshDecals.PaintDecal.RenderDecalInSphere(hitEvent.position, size, projector, Quaternion.identity, GameManager.instance.waterSprayHitMask);
        }
    }
}

