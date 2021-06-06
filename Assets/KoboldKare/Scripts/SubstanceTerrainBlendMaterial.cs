using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubstanceTerrainBlendMaterial : MonoBehaviour, IGameEventOptionListener
{
    public GraphicsOptions options;
    //public Substance.Game.SubstanceGraph grapha;
    //public Substance.Game.SubstanceGraph graphb;
    public Material grapha;
    public Material graphb;
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
        targetMaterial.SetTexture("_BaseColorMap", grapha.GetTexture("_BaseColorMap"));
        targetMaterial.SetTexture("_BaseColorMapB", graphb.GetTexture("_BaseColorMap"));
        targetMaterial.SetTexture("_MaskMap", grapha.GetTexture("_MaskMap"));
        targetMaterial.SetTexture("_MaskMapB", graphb.GetTexture("_MaskMap"));
        targetMaterial.SetTexture("_NormalMap", grapha.GetTexture("_NormalMap"));
        targetMaterial.SetTexture("_NormalMapB", graphb.GetTexture("_NormalMap"));
        targetMaterial.SetTexture("_HeightMap", grapha.GetTexture("_HeightMap"));
        targetMaterial.SetTexture("_HeightMapB", graphb.GetTexture("_HeightMap"));
    }
#if UNITY_EDITOR
    private Dictionary<string, Texture> savedTextures = new Dictionary<string, Texture>();
    private void SaveTextures() {
        savedTextures["_BaseColorMap"] = targetMaterial.GetTexture("_BaseColorMap");
        savedTextures["_BaseColorMapB"] = targetMaterial.GetTexture("_BaseColorMapB");
        savedTextures["_MaskMap"] = targetMaterial.GetTexture("_MaskMap");
        savedTextures["_MaskMapB"] = targetMaterial.GetTexture("_MaskMapB");
        savedTextures["_NormalMap"] = targetMaterial.GetTexture("_NormalMap");
        savedTextures["_NormalMapB"] = targetMaterial.GetTexture("_NormalMapB");
        savedTextures["_HeightMap"] = targetMaterial.GetTexture("_HeightMap");
        savedTextures["_HeightMapB"] = targetMaterial.GetTexture("_HeightMapB");
    }
    private void LoadTextures() {
        targetMaterial.SetTexture("_BaseColorMap", savedTextures["_BaseColorMap"]);
        targetMaterial.SetTexture("_BaseColorMapB", savedTextures["_BaseColorMapB"]);
        targetMaterial.SetTexture("_MaskMap", savedTextures["_MaskMap"]);
        targetMaterial.SetTexture("_MaskMapB", savedTextures["_MaskMapB"]);
        targetMaterial.SetTexture("_NormalMap", savedTextures["_NormalMap"]);
        targetMaterial.SetTexture("_NormalMapB", savedTextures["_NormalMapB"]);
        targetMaterial.SetTexture("_HeightMap", savedTextures["_HeightMap"]);
        targetMaterial.SetTexture("_HeightMapB", savedTextures["_HeightMapB"]);
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
