using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SubstanceMaterialForceLoader : MonoBehaviour {
    public UnityEvent OnFinishGenerateTextures;
    public GraphicsOptions options;
    public GameObject plane;
    // Start is called before the first frame update
    void Awake() {
        //foreach(var info in options.proceduralTextures) {
            //GameObject.Instantiate(plane).GetComponentInChildren<MeshRenderer>().sharedMaterial = info.graph.material;
        //}
    }
    public void FinishTextureLoad() {
        StopAllCoroutines();
        StartCoroutine(WaitAndThenFinishLoad());
    }
    public IEnumerator WaitAndThenFinishLoad() {
        //while (options.textureLoadingProgress < 0.55f && !Application.isEditor) {
            yield return new WaitForEndOfFrame();
        //}
        OnFinishGenerateTextures.Invoke();
    }
}
