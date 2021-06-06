using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubstanceSubscribeToMaterial : MonoBehaviour, IGameEventOptionListener {
    public GraphicsOptions options;
    public Material substanceGraph;
    public Material targetMaterial;
    /*private Texture2D GetTextureByName( Substance.Game.SubstanceGraph graph, string name) {
        foreach(Texture2D t in graph.GetGeneratedTextures()) {
            if (t.name.Contains(name)) {
                return t;
            }
        }
        return null;
    }*/
    IEnumerator WaitAndSetTextures() {
        yield return new WaitForEndOfFrame();
        //yield return new WaitForEndOfFrame();
        //while (Substance.Game.Substance.IsProcessing()) {
            //yield return new WaitForEndOfFrame();
        //}
        targetMaterial.SetTexture("_BaseColorMap", substanceGraph.GetTexture("_BaseColorMap"));
        targetMaterial.SetTexture("_MaskMap", substanceGraph.GetTexture("_MaskMap"));
        targetMaterial.SetTexture("_NormalMap", substanceGraph.GetTexture("_NormalMap"));
        //targetMaterial.SetTexture("_HeightMap", substanceGraph.GetTexture("_HeightMap"));
    }
#if UNITY_EDITOR
    private Dictionary<string, Texture> savedTextures = new Dictionary<string, Texture>();
    private void SaveTextures() {
        savedTextures["_BaseColorMap"] = targetMaterial.GetTexture("_BaseColorMap");
        savedTextures["_MaskMap"] = targetMaterial.GetTexture("_MaskMap");
        savedTextures["_NormalMap"] = targetMaterial.GetTexture("_NormalMap");
        //savedTextures["_HeightMap"] = targetMaterial.GetTexture("_HeightMap");
    }
    private void LoadTextures() {
        targetMaterial.SetTexture("_BaseColorMap", savedTextures["_BaseColorMap"]);
        targetMaterial.SetTexture("_MaskMap", savedTextures["_MaskMap"]);
        targetMaterial.SetTexture("_NormalMap", savedTextures["_NormalMap"]);
        //targetMaterial.SetTexture("_HeightMap", savedTextures["_HeightMap"]);
    }
#endif

    void OnDestroy() {
#if UNITY_EDITOR
        LoadTextures();
#endif
        options.UnregisterListener(this);
    }

    public void Start() {
#if UNITY_EDITOR
        SaveTextures();
#endif
        options.RegisterListener(this);
        foreach(GraphicsOptions.Option o in options.options) {
            OnEventRaised(o.type, o.value);
        }
    }

    public void OnEventRaised(GraphicsOptions.OptionType e, float value) {
        switch(e) {
            case GraphicsOptions.OptionType.ProceduralTextureSize:
                StopAllCoroutines();
                StartCoroutine(WaitAndSetTextures());
            break;
        }
    }
}
