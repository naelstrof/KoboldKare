using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
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
    public GenericReagentContainer volumeContainer;
    public HashSet<LODGroup> paintableObjects = new HashSet<LODGroup>();
    public HashSet<GenericReagentContainer> dippableObjects = new HashSet<GenericReagentContainer>();
    public UnityEvent drainStart;
    public UnityEvent drainEnd;

    public IEnumerator DrainProcess() {
        while (volumeContainer.volume > 0f) {
            volumeContainer.Spill(fillRate * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }
        drainEnd.Invoke();
    }

    public void TriggerDrain() {
        drainStart.Invoke();
        StartCoroutine(DrainProcess());
    }

    public void Start() {
        volumeContainer.OnChange.AddListener(OnReagentContainerChanged);
        OnReagentContainerChanged(GenericReagentContainer.InjectType.Metabolize);
    }
    public void OnDestroy() {
        volumeContainer.OnChange.RemoveListener(OnReagentContainerChanged);
    }
    public void Update() {
        paintableObjects.RemoveWhere(o=>o == null);
        dippableObjects.RemoveWhere(o=>o == null);
        foreach(LODGroup g in paintableObjects) {
            DipDecal(g);
            if (Mathf.Approximately(volumeContainer.volume, 0f)) {
                continue;
            }
            foreach(var container in dippableObjects) {
                container.TransferMix(volumeContainer, Mathf.Min(container.maxVolume-container.volume,fillRate * Time.deltaTime), GenericReagentContainer.InjectType.Flood);
            }
        }
        paintableObjects.Clear();
        dippableObjects.Clear();
    }

    public void DipDecal(LODGroup g) {
        if (g.gameObject.layer == LayerMask.NameToLayer("World") || g.transform.root == this.transform.root) {
            return;
        }
        if (volumeContainer.volume <= 0f) {
            return;
        }
        foreach(BoxCollider b in fluidHitboxes) {
            Vector3 boxCorner = b.size/2f;
            Vector3 boxFrontWorld = b.transform.TransformPoint(b.center + new Vector3(0, 0, boxCorner.z));
            Vector3 boxRightWorld = b.transform.TransformPoint(b.center + new Vector3(boxCorner.x, 0, 0));
            Vector3 boxUpWorld = b.transform.TransformPoint(b.center + new Vector3(0, boxCorner.y, 0));
            Vector3 boxCenterWorld = b.transform.TransformPoint(b.center);

            Vector2 rectangle = new Vector2((boxRightWorld - boxCenterWorld).magnitude, (boxUpWorld - boxCenterWorld).magnitude)*4f;
            float depth = (boxFrontWorld - boxCenterWorld).magnitude*2f;

            Vector3 pos = boxFrontWorld;
            Vector3 norm = (boxCenterWorld-boxFrontWorld).normalized;

            Color c = volumeContainer.GetColor();
            c.a = 1f;
            if (volumeContainer.IsCleaningAgent()) {
                staticRenderers.Clear();
                g.transform.GetComponentsInChildrenNoAlloc<Renderer>(staticTempRenderers, staticRenderers);
                foreach(Renderer r in staticRenderers) {
                    SkinnedMeshDecals.PaintDecal.RenderDecal(r, decalClearMaterial, pos, Quaternion.FromToRotation(Vector3.forward, norm), rectangle, depth);
                }
            } else {
                decalDipMaterial.color = c;
                staticRenderers.Clear();
                g.transform.GetComponentsInChildrenNoAlloc<Renderer>(staticTempRenderers, staticRenderers);
                foreach(Renderer r in staticRenderers) {
                    SkinnedMeshDecals.PaintDecal.RenderDecal(r, decalDipMaterial, pos, Quaternion.FromToRotation(Vector3.forward, norm), rectangle, depth);
                }
            }
        }
    }

    public void OnTriggerEnter(Collider other) {
        LODGroup group = other.GetComponentInParent<LODGroup>();
        if (group) {
            paintableObjects.Add(group);
        }
        GenericReagentContainer container = other.GetComponentInParent<GenericReagentContainer>();
        if (container != null) {
            dippableObjects.Add(container);
        }
    }
    public void OnTriggerStay(Collider other) {
        LODGroup group = other.GetComponentInParent<LODGroup>();
        if (group) {
            paintableObjects.Add(group);
        }
        GenericReagentContainer container = other.GetComponentInParent<GenericReagentContainer>();
        if (container != null) {
            dippableObjects.Add(container);
        }
    }
    private void OnDrawGizmos() {
        Gizmos.DrawIcon(transform.position, "ico_fluidvolume.png", true);
    }

    public void OnReagentContainerChanged(GenericReagentContainer.InjectType injectType) {
        foreach(Renderer r in fluidRenderers) {
            foreach(Material material in r.materials) {
                material.color = volumeContainer.GetColor();
                material.SetFloat("_Position", volumeContainer.volume / volumeContainer.maxVolume);
            }
        }
        if (fluidScaler != null) {
            fluidScaler.localScale = fluidScaler.localScale.With(y:volumeContainer.volume/volumeContainer.maxVolume);
        }
    }

    public bool HasAnyFillAmount(){
        return !volumeContainer.isEmpty; //If not empty, return true; if empty, return false
    }
}
