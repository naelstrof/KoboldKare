using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Naelstrof.Inflatable;
using JigglePhysics;

public class ThirdPersonMeshDisplay : MonoBehaviour {
    private List<GameObject> mirrorObjects = new List<GameObject>();
    private Dictionary<SkinnedMeshRenderer, SkinnedMeshRenderer> smrCopies = new Dictionary<SkinnedMeshRenderer, SkinnedMeshRenderer>();
    public Kobold kobold;
    public LODGroup group;
    public JigglePhysics.JiggleSkin physics;
    public List<SkinnedMeshRenderer> dissolveTargets = new List<SkinnedMeshRenderer>();
    public void Start() {
        foreach (SkinnedMeshRenderer s in dissolveTargets) {
            foreach (Material m in s.GetComponent<SkinnedMeshRenderer>().materials) {
                m.SetFloat("_Head", 0f);
            }
        }
        RegenerateMirror();
    }
    public void Update() {
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
                if (physics.targetSkins.Contains(r)) {
                    physics.targetSkins.Remove(r);
                }
                foreach (InflatableBreast boob in kobold.boobListeners) {
                    boob.RemoveTargetRenderer(r);
                }
                foreach (InflatableBlendShape belly in kobold.bellyListeners) {
                    belly.RemoveTargetRenderer(r);
                }
                foreach (var fatness in kobold.fatnessListeners) {
                    fatness.RemoveTargetRenderer(r);
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
                physics.targetSkins.Add(smrCopies[s]);
                foreach(var boob in kobold.boobListeners) {
                    boob.AddTargetRenderer(g.GetComponent<SkinnedMeshRenderer>());
                }
                foreach (var belly in kobold.bellyListeners) {
                    belly.AddTargetRenderer(g.GetComponent<SkinnedMeshRenderer>());
                }
                foreach (var fatness in kobold.fatnessListeners) {
                    fatness.AddTargetRenderer(g.GetComponent<SkinnedMeshRenderer>());
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
