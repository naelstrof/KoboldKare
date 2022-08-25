using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Naelstrof.Inflatable;
using JigglePhysics;
using PenetrationTech;

public class ThirdPersonMeshDisplay : MonoBehaviour {
    private List<GameObject> mirrorObjects = new List<GameObject>();
    private Dictionary<SkinnedMeshRenderer, SkinnedMeshRenderer> smrCopies = new Dictionary<SkinnedMeshRenderer, SkinnedMeshRenderer>();
    public Kobold kobold;
    private ProceduralDeformation proceduralDeformation;
    public LODGroup group;
    public JigglePhysics.JiggleSkin physics;
    public List<SkinnedMeshRenderer> dissolveTargets = new List<SkinnedMeshRenderer>();
    public void OnEnable() {
        foreach (SkinnedMeshRenderer s in dissolveTargets) {
            foreach (Material m in s.GetComponent<SkinnedMeshRenderer>().materials) {
                m.SetFloat("_Head", 0f);
            }
        }

        proceduralDeformation = kobold.GetComponentInChildren<ProceduralDeformation>();
        RegenerateMirror();
    }

    public void OnDisable() {
        foreach (SkinnedMeshRenderer s in dissolveTargets) {
            foreach (Material m in s.GetComponent<SkinnedMeshRenderer>().materials) {
                m.SetFloat("_Head", 1f);
            }
        }
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

                proceduralDeformation.RemoveTargetRenderer(r);
                foreach (var inflatable in kobold.GetAllInflatableListeners()) {
                    if (inflatable is InflatableBreast breast) {
                        breast.RemoveTargetRenderer(r);
                    }
                    if (inflatable is InflatableBlendShape blendshape) {
                        blendshape.RemoveTargetRenderer(r);
                    }
                    if (inflatable is InflatableBelly belly) {
                        belly.RemoveTargetRenderer(r);
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
                physics.targetSkins.Add(smrCopies[s]);
                foreach (var inflatable in kobold.GetAllInflatableListeners()) {
                    if (inflatable is InflatableBreast breast) {
                        breast.AddTargetRenderer(g.GetComponent<SkinnedMeshRenderer>());
                    }
                    if (inflatable is InflatableBlendShape blendshape) {
                        blendshape.AddTargetRenderer(g.GetComponent<SkinnedMeshRenderer>());
                    }
                    if (inflatable is InflatableBelly belly) {
                        belly.AddTargetRenderer(g.GetComponent<SkinnedMeshRenderer>());
                    }
                }
                proceduralDeformation.AddTargetRenderer(g.GetComponent<SkinnedMeshRenderer>());
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
