using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Photon;
using ExitGames.Client.Photon;

public class Plant : MonoBehaviour {
    public GenericReagentContainer container;
    public string prefabToSpawn;

    public AudioSource gurgleSource;
    public bool filled = false;

    private int currentPhase = 0;
    public List<GameObject> phases = new List<GameObject>();
    public GameObject heartPoof;
    private Vector4 HueBrightnessContrastSaturation;

    public List<MeshRenderer> bouncyMaterials = new List<MeshRenderer>();
    public List<SkinnedMeshRenderer> happyKobolds = new List<SkinnedMeshRenderer>();
    public int GetID(SkinnedMeshRenderer r, string blendshapeName) {
        for(int i=0;i<r.sharedMesh.blendShapeCount;i++) {
            if (r.sharedMesh.GetBlendShapeName(i) == blendshapeName ) {
                return i;
            }
        }
        return -1;
    }

    public void Start() {
        HueBrightnessContrastSaturation = new Vector4(UnityEngine.Random.Range(0f,1f), UnityEngine.Random.Range(0.3f,.7f),UnityEngine.Random.Range(0.3f,0.7f),UnityEngine.Random.Range(0.3f,0.7f));
        foreach(GameObject phase in phases) {
            foreach(Kobold k in phase.GetComponentsInChildren<Kobold>(true)) {
                k.HueBrightnessContrastSaturation = HueBrightnessContrastSaturation;
            }
            foreach(Renderer r in phase.GetComponentsInChildren<SkinnedMeshRenderer>(true)) {
                foreach(Material m in r.materials) {
                    m.SetVector("_HueBrightnessContrastSaturation", HueBrightnessContrastSaturation);
                }
            }
        }
        container.OnChange.AddListener(OnReagentContainerChanged);
    }
    public void OnDestroy() {
        container.OnChange.RemoveListener(OnReagentContainerChanged);
    }

    private void AdvanceStage() {
        foreach (GameObject phase in phases) {
            phase.SetActive(false);
        }
        currentPhase++;
        if (currentPhase < phases.Count) {
            foreach (MeshRenderer m in bouncyMaterials) {
                m.material.SetFloat("_BounceAmount", 0f);
            }
            foreach (SkinnedMeshRenderer k in happyKobolds) {
                //m.material.SetFloat("_BounceAmount", 0.5f);
                k.SetBlendShapeWeight(GetID(k, "HappyLeft"), 0f);
                k.SetBlendShapeWeight(GetID(k, "HappyRight"), 0f);
            }
            phases[currentPhase].SetActive(true);
            filled = false;
            transform.Rotate(new Vector3(0, 1, 0), UnityEngine.Random.Range(0, 360));
            return;
        }
        // Final spawn!
        if (GetComponentInParent<PhotonView>().IsMine) {
            SaveManager.Instantiate(prefabToSpawn, transform.position, Quaternion.identity, 0, null);
            SaveManager.Destroy(this.gameObject);
        }
    }

    IEnumerator WaitAndThenStopGargling(float time) {
        yield return new WaitForSeconds(time);
        gurgleSource.Pause();
    }

    public void Metabolize(float time) {
        time *= DayNightCycle.instance.dayLength;
        if (container.volume >= container.maxVolume) {
            container.Spill(container.volume);
            AdvanceStage();
        }
    }

    public void OnReagentContainerChanged() {
        if (filled) {
            return;
        }
        if (container.volume >= container.maxVolume) {
            foreach (MeshRenderer m in bouncyMaterials) {
                m.material.SetFloat("_BounceAmount", 0.5f);
            }
            foreach (SkinnedMeshRenderer k in happyKobolds) {
                //m.material.SetFloat("_BounceAmount", 0.5f);
                k.SetBlendShapeWeight(GetID(k, "HappyLeft"), 100f);
                k.SetBlendShapeWeight(GetID(k, "HappyRight"), 100f);
            }
            GameObject.Instantiate(heartPoof, transform.position + Vector3.up * 0.5f, transform.rotation * Quaternion.AngleAxis(-90, Vector3.right));
            filled = true;
        } else {
            foreach (Animator a in phases[currentPhase].GetComponentsInChildren<Animator>()) {
                if (a.runtimeAnimatorController == null ) {
                    continue;
                }
                if (gurgleSource.isActiveAndEnabled && !gurgleSource.isPlaying) {
                    gurgleSource.Play();
                    StopCoroutine("WaitAndThenStopGargling");
                    StartCoroutine(WaitAndThenStopGargling(0.25f));
                }
                foreach( var p in a.parameters) {
                    if (p.name == "Quaff") {
                        a.SetTrigger("Quaff");
                        break;
                    }
                }
            }
        }
    }
}
