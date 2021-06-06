using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonMeshDisplay : MonoBehaviour {
    private List<GameObject> mirrorObjects = new List<GameObject>();
    private Dictionary<SkinnedMeshRenderer, SkinnedMeshRenderer> smrCopies = new Dictionary<SkinnedMeshRenderer, SkinnedMeshRenderer>();
    public Kobold kobold;
    public LODGroup group;
    public SoftbodyPhysics physics;
    public List<SkinnedMeshRenderer> dissolveTargets = new List<SkinnedMeshRenderer>();
    public BodyProportion proportion;
    public void OnFinishProportionEdit() {
        RegenerateMirror();
    }
    public void Start() {
        proportion.OnComplete += OnFinishProportionEdit;
        foreach (SkinnedMeshRenderer s in dissolveTargets) {
            foreach (Material m in s.GetComponent<SkinnedMeshRenderer>().materials) {
                m.SetFloat("_Head", 0f);
            }
        }
    }
    public void OnDestroy() {
        proportion.OnComplete -= OnFinishProportionEdit;
    }
    public void FixedUpdate() {
        foreach(KeyValuePair<SkinnedMeshRenderer, SkinnedMeshRenderer> pair in smrCopies) {
            for(int i=0;i<pair.Key.sharedMesh.blendShapeCount;i++) {
                pair.Value.SetBlendShapeWeight(i,pair.Key.GetBlendShapeWeight(i));
            }
        }
    }
    public void RegenerateMirror() {
        foreach(GameObject g in mirrorObjects) {
            foreach (SkinnedMeshRenderer r in g.GetComponentsInChildren<SkinnedMeshRenderer>()) {
                if (kobold.koboldBodyRenderers.Contains(r)) {
                    kobold.koboldBodyRenderers.Remove(r);
                }
                if (physics.targetRenderers.Contains(r)) {
                    physics.targetRenderers.Remove(r);
                }
                foreach (var boob in kobold.boobs) {
                    if (boob.targetRenderers.Contains(r)) {
                        boob.targetRenderers.Remove(r);
                    }
                }
                foreach (var belly in kobold.bellies) {
                    if (belly.targetRenderers.Contains(r)) {
                        belly.targetRenderers.Remove(r);
                    }
                }
                foreach (var ss in kobold.subcutaneousStorage) {
                    if (ss.targetRenderers.Contains(r)) {
                        ss.targetRenderers.Remove(r);
                    }
                }
            }
            Destroy(g);
        }

        LOD[] lods = group.GetLODs();
        List<Renderer> renderers = new List<Renderer>(lods[0].renderers);
        for(int i=0;i<renderers.Count;i++) {
            if (renderers[i] == null || renderers[i].gameObject == null) {
                renderers.RemoveAt(i);
            }
        }
        foreach (GameObject g in mirrorObjects) {
            foreach (SkinnedMeshRenderer r in g.GetComponentsInChildren<SkinnedMeshRenderer>()) {
                if (renderers.Contains(r)) {
                    renderers.Remove(r);
                }
            }
        }
        lods[0].renderers = renderers.ToArray();
        group.SetLODs(lods);

        mirrorObjects.Clear();
        smrCopies.Clear();
        foreach(SkinnedMeshRenderer s in dissolveTargets) {
            if (!s.gameObject.activeInHierarchy) {
                continue;
            }
            GameObject g = GameObject.Instantiate(s.gameObject,s.transform.position, s.transform.rotation);
            mirrorObjects.Add(g);
            g.layer = LayerMask.NameToLayer("MirrorReflection");
            g.transform.parent = s.transform.parent;
            smrCopies[s] = g.GetComponent<SkinnedMeshRenderer>();
            kobold.koboldBodyRenderers.Add(smrCopies[s]);
            lods = group.GetLODs();
            renderers = new List<Renderer>(lods[0].renderers);
            for(int i=0;i<renderers.Count;i++) {
                if (renderers[i] == null || renderers[i].gameObject == null) {
                    renderers.RemoveAt(i);
                }
            }
            renderers.Add(g.GetComponent<SkinnedMeshRenderer>());
            lods[0].renderers = renderers.ToArray();
            group.SetLODs(lods);
            if (s.gameObject.name == "Body") {
                physics.targetRenderers.Add(smrCopies[s]);
                foreach(var boob in kobold.boobs) {
                    boob.targetRenderers.Add(g.GetComponent<SkinnedMeshRenderer>());
                }
                foreach (var belly in kobold.bellies) {
                    belly.targetRenderers.Add(g.GetComponent<SkinnedMeshRenderer>());
                }
                foreach (var ss in kobold.subcutaneousStorage) {
                    ss.targetRenderers.Add(g.GetComponent<SkinnedMeshRenderer>());
                }
            }
            foreach(Material m in g.GetComponent<SkinnedMeshRenderer>().materials) {
                m.SetFloat("_Head", 1f);
            }
            foreach(Material m in s.materials) {
                m.SetFloat("_Head", 0f);
            }
            s.gameObject.layer = LayerMask.NameToLayer("LocalPlayer");
        }
    }
}
