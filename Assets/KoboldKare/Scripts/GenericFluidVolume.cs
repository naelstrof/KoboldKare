using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GenericFluidVolume : MonoBehaviour, IReagentContainerListener {
    private static List<Renderer> staticTempRenderers = new List<Renderer>();
    private static List<Renderer> staticRenderers = new List<Renderer>();
    public bool infiniteSource = false;
    public float fillRate = 1f;
    public Transform fluidScaler;
    public ReagentContents.ReagentInjectType injectType;
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
        while (volumeContainer.contents.volume > 0f) {
            volumeContainer.contents.Spill(fillRate * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }
        drainEnd.Invoke();
    }

    public void TriggerDrain() {
        drainStart.Invoke();
        StartCoroutine(DrainProcess());
    }

    public void Start() {
        volumeContainer.contents.AddListener(this);
        OnReagentContainerChanged(volumeContainer.contents, ReagentContents.ReagentInjectType.Inject);
    }
    public void OnDestroy() {
        volumeContainer.contents.RemoveListener(this);
    }
    public void Update() {
        paintableObjects.RemoveWhere(o=>o == null);
        dippableObjects.RemoveWhere(o=>o == null);
        foreach(LODGroup g in paintableObjects) {
            DipDecal(g);
            if (Mathf.Approximately(volumeContainer.contents.volume, 0f)) {
                continue;
            }
            foreach(var container in dippableObjects) {
                if (container.contents.IsMixable(injectType)) {
                    if (infiniteSource) {
                        container.contents.Mix(volumeContainer.contents * fillRate * Time.deltaTime, injectType);
                    } else {
                        container.contents.Mix(volumeContainer.contents.Spill(Mathf.Min(container.contents.maxVolume-container.contents.volume,fillRate * Time.deltaTime)), injectType);
                    }
                }
            }
        }
        paintableObjects.Clear();
        dippableObjects.Clear();
    }

    public void DipDecal(LODGroup g) {
        if (g.gameObject.layer == LayerMask.NameToLayer("World") || g.transform.root == this.transform.root) {
            return;
        }
        if (volumeContainer.contents.volume <= 0f) {
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

            Color c = volumeContainer.contents.GetColor(ReagentDatabase.instance);
            c.a = 1f;
            if (volumeContainer.contents.ContainsKey(ReagentData.ID.Water) && volumeContainer.contents[ReagentData.ID.Water].volume > volumeContainer.contents.volume*0.9f) {
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

    public void OnReagentContainerChanged(ReagentContents contents, ReagentContents.ReagentInjectType injectType) {
        foreach(Renderer r in fluidRenderers) {
            foreach(Material material in r.materials) {
                if ( contents.volume <= 0 ) {
                    material.SetColor("_BaseColor", new Color(0,0,0,0));
                    material.SetFloat("_Position", 0);
                    continue;
                }
                material.SetColor("_BaseColor", contents.GetColor(ReagentDatabase.instance));
                material.SetFloat("_Position", contents.volume / volumeContainer.maxVolume);
            }
        }
        //foreach(BoxCollider collider in fluidHitboxes) {
            //collider.transform.parent.localScale = collider.transform.parent.localScale.With(y:contents.volume/volumeContainer.maxVolume);
        //}
        if (fluidScaler != null) {
            fluidScaler.localScale = fluidScaler.localScale.With(y:contents.volume/volumeContainer.maxVolume);
        }
    }
    //private void OnTriggerExit(Collider other) {
    //LODGroup group = other.GetComponentInParent<LODGroup>();
    //if (group) {
    //paintableObjects.Remove(group.gameObject);
    //} else {
    //paintableObjects.Remove(other.gameObject);
    //}
    //}
}
