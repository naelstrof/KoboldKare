using System;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using NetStack.Serialization;
using UnityEngine;
using UnityEngine.Events;

public class GenericFluidVolume : MonoBehaviour {
    private static List<Renderer> staticTempRenderers = new List<Renderer>();
    private static List<Renderer> staticRenderers = new List<Renderer>();
    public float fillRate = 1f;
    public Transform fluidScaler;
    public List<Renderer> fluidRenderers = new List<Renderer>();
    public Material decalDipMaterial;
    public Material decalClearMaterial;
    public List<BoxCollider> fluidHitboxes = new List<BoxCollider>();
    
    private HashSet<NetworkedEntity> dippedObjects;
    private NetworkedEntity networkedEntity;
    
    
    [SerializeField, HideInInspector]
    private UnityEvent drainStart;
    [SerializeField, HideInInspector]
    private UnityEvent drainEnd;
    
    [SerializeField, SubclassSelector, SerializeReference]
    private List<GameEventResponse> drainStartResponses = new List<GameEventResponse>();
    [SerializeField, SubclassSelector, SerializeReference]
    private List<GameEventResponse> drainEndResponses = new List<GameEventResponse>();

    public IEnumerator DrainProcess() {
        while (networkedEntity.reagentContents.Value.volume > 0f) {
            networkedEntity.reagentContents.Value.Spill(fillRate * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }
        foreach(var response in drainEndResponses) {
            response?.Invoke(this);
        }
    }

    public void TriggerDrain() {
        foreach(var response in drainStartResponses) {
            response?.Invoke(this);
        }
        StartCoroutine(DrainProcess());
    }

    private void Awake() {
        dippedObjects = new ();
        GameEventSanitizer.SanitizeRuntime(drainStart, drainStartResponses, this);
        GameEventSanitizer.SanitizeRuntime(drainEnd, drainEndResponses, this);
    }

    private void OnValidate() {
        GameEventSanitizer.SanitizeEditor(nameof(drainStart), nameof(drainStartResponses), this);
        GameEventSanitizer.SanitizeEditor(nameof(drainEnd), nameof(drainEndResponses), this);
    }

    public void Start() {
        networkedEntity = GetComponentInParent<NetworkedEntity>();
        if (networkedEntity != null) {
            networkedEntity.reagentContents.OnChange += OnReagentContainerChanged;
            OnReagentContainerChanged(networkedEntity.GetContents(), networkedEntity.GetContents(), false);
        }
    }
    public void OnDestroy() {
        if (networkedEntity != null) {
            networkedEntity.reagentContents.OnChange -= OnReagentContainerChanged;
        }
    }
    
    public void Update() {
        dippedObjects.RemoveWhere(o=>o == null);
        if (dippedObjects.Count <= 0) {
            return;
        }
        foreach(var ent in dippedObjects) {
            if (ent == networkedEntity) {
                continue;
            }
            DipDecal(ent);
            if (!ent.IsOwner) {
                ent.TryTakeOwnership();
                continue;
            }

            if (GeneHolder.IsMixable(ent.type, GeneHolder.InjectType.Flood)) {
                float spillVolume = Mathf.Min(ent.maxVolume - ent.volume, fillRate * Time.deltaTime);
                ReagentContents spill = networkedEntity.Spill(spillVolume);
                ent.AddMix(spill, GeneHolder.InjectType.Flood);
            }
        }
        dippedObjects.Clear();
    }

    public void DipDecal(NetworkedEntity view) {
        if (view.gameObject.layer == LayerMask.NameToLayer("World")) {
            return;
        }
        if (networkedEntity.reagentContents.Value.volume <= 0f) {
            return;
        }
        foreach(BoxCollider b in fluidHitboxes) {
            Vector3 boxCorner = b.size/2f;
            Vector3 boxFrontWorld = b.transform.TransformPoint(b.center + new Vector3(0, 0, boxCorner.z));
            Vector3 boxRightWorld = b.transform.TransformPoint(b.center + new Vector3(boxCorner.x, 0, 0));
            Vector3 boxUpWorld = b.transform.TransformPoint(b.center + new Vector3(0, boxCorner.y, 0));
            Vector3 boxCenterWorld = b.transform.TransformPoint(b.center);

            Vector2 rectangle = new Vector2((boxRightWorld - boxCenterWorld).magnitude, (boxUpWorld - boxCenterWorld).magnitude)*2f;
            float depth = (boxFrontWorld - boxCenterWorld).magnitude*2f;

            Vector3 pos = boxFrontWorld;
            Vector3 norm = (boxCenterWorld-boxFrontWorld).normalized;

            Color c = networkedEntity.GetColor();
            c.a = 1f;
            if (networkedEntity.IsCleaningAgent()) {
                staticRenderers.Clear();
                view.transform.GetComponentsInChildrenNoAlloc(staticTempRenderers, staticRenderers);
                foreach(Renderer r in staticRenderers) {
                    SkinnedMeshDecals.PaintDecal.RenderDecal(r, decalClearMaterial, pos, Quaternion.FromToRotation(Vector3.forward, norm), rectangle, depth);
                }
            } else {
                decalDipMaterial.color = c;
                staticRenderers.Clear();
                view.transform.GetComponentsInChildrenNoAlloc(staticTempRenderers, staticRenderers);
                foreach(Renderer r in staticRenderers) {
                    SkinnedMeshDecals.PaintDecal.RenderDecal(r, decalDipMaterial, pos, Quaternion.FromToRotation(Vector3.forward, norm), rectangle, depth);
                }
            }
        }
    }

    public void OnTriggerEnter(Collider other) {
        var view = other.GetComponentInParent<NetworkedEntity>();
        if (view != null) {
            dippedObjects.Add(view);
        }
    }
    public void OnTriggerStay(Collider other) {
        var view = other.GetComponentInParent<NetworkedEntity>();
        if (view != null) {
            dippedObjects.Add(view);
        }
    }
    private void OnDrawGizmos() {
        Gizmos.DrawIcon(transform.position, "ico_fluidvolume.png", true);
    }

    public void OnReagentContainerChanged(ReagentContents contents, ReagentContents next, bool asServer) {
        foreach(Renderer r in fluidRenderers) {
            foreach(Material material in r.materials) {
                material.color = contents.GetColor();
                material.SetFloat("_Position", contents.volume / contents.GetMaxVolume());
            }
        }
        if (fluidScaler != null) {
            fluidScaler.localScale = fluidScaler.localScale.With(y:contents.volume/contents.GetMaxVolume());
        }
    }

    public bool HasAnyFillAmount(){
        return !networkedEntity.isEmpty; //If not empty, return true; if empty, return false
    }
}
