using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Naelstrof.Inflatable;
using JigglePhysics;
using PenetrationTech;

public class ThirdPersonMeshDisplay : MonoBehaviour {
    private List<GameObject> mirrorObjects;
    private Dictionary<SkinnedMeshRenderer, SkinnedMeshRenderer> smrCopies;
    private Kobold kobold;
    private ProceduralDeformation proceduralDeformation;
    private LODGroup group;
    private JiggleSkin physics;
    private List<SkinnedMeshRenderer> dissolveTargets;
    private static readonly int Head = Shader.PropertyToID("_Head");

    private void OnEnable() {
        mirrorObjects ??= new List<GameObject>();
        smrCopies ??= new Dictionary<SkinnedMeshRenderer, SkinnedMeshRenderer>();
        dissolveTargets ??= new List<SkinnedMeshRenderer>();
        
        kobold = GetComponentInParent<Kobold>();
        physics = kobold.GetComponent<JiggleSkin>();
        group = kobold.GetComponentInChildren<LODGroup>();
        proceduralDeformation = kobold.GetComponentInChildren<ProceduralDeformation>();
        if (isActiveAndEnabled) {
            RegenerateMirror();
        }
    }

    public void SetDissolveTargets(ICollection<SkinnedMeshRenderer> newDissolveTargets) {
        dissolveTargets = new List<SkinnedMeshRenderer>(newDissolveTargets);
        OnEnable();
    }

    private void OnDisable() {
        foreach (var r in dissolveTargets) {
            foreach(Material m in r.materials) {
                m.SetFloat(Head, 1f);
            }
        }
        DestroyMirror();
    }

    private void Update() {
        foreach(KeyValuePair<SkinnedMeshRenderer, SkinnedMeshRenderer> pair in smrCopies) {
            for(int i=0;i<pair.Key.sharedMesh.blendShapeCount;i++) {
                pair.Value.SetBlendShapeWeight(i,pair.Key.GetBlendShapeWeight(i));
            }
        }
    }

    private void DestroyMirror() {
        foreach(GameObject g in mirrorObjects) {
            if (g == null) {
                continue;
            }
            foreach (SkinnedMeshRenderer r in g.GetComponentsInChildren<SkinnedMeshRenderer>()) {
                kobold.RemoveKoboldBodyRenderer(r);

                if (physics != null) {
                    if (physics.targetSkins.Contains(r)) {
                        physics.targetSkins.Remove(r);
                    }
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

        mirrorObjects.Clear();
        foreach (SkinnedMeshRenderer s in dissolveTargets) {
            foreach (Material m in s.GetComponent<SkinnedMeshRenderer>().materials) {
                m.SetFloat(Head, 1f);
            }
            s.gameObject.layer = LayerMask.NameToLayer("Player");
        }
    }

    private void RegenerateMirror() {
        DestroyMirror();
        if (group != null) {
            LOD[] lods = group.GetLODs();
            List<Renderer> renderers = new List<Renderer>(lods[0].renderers);
            for (int i = 0; i < renderers.Count; i++) {
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
        }

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
            kobold.AddKoboldBodyRenderer(smrCopies[s]);
            if (group != null) {
                var lods = group.GetLODs();
                var renderers = new List<Renderer>(lods[0].renderers);
                for (int i = 0; i < renderers.Count; i++) {
                    if (renderers[i] == null || renderers[i].gameObject == null) {
                        renderers.RemoveAt(i);
                    }
                }

                renderers.Add(g.GetComponent<SkinnedMeshRenderer>());
                lods[0].renderers = renderers.ToArray();
                group.SetLODs(lods);
            }
            if (physics != null) {
                physics.targetSkins.Add(smrCopies[s]);
            }
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
            foreach(Material m in g.GetComponent<SkinnedMeshRenderer>().materials) {
                m.SetFloat(Head, 1f);
            }
            foreach(Material m in s.materials) {
                m.SetFloat(Head, 0f);
            }
            s.gameObject.layer = LayerMask.NameToLayer("LocalPlayer");
        }
    }
}
