using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class GraphicsOptionsLoadingDisplay : MonoBehaviour, IGameEventOptionListener {
    public GraphicsOptions options;
    public UnityEvent OnStartLoad;
    public UnityEvent OnEndLoad;
    public TMP_Text loadPercentage;
    public TMP_Text texturename;
    IEnumerator WaitAndThenDo() {
        OnStartLoad.Invoke();
        yield return new WaitForEndOfFrame();
        //while (!Application.isEditor && (Substance.Game.Substance.IsProcessing() || options.textureLoadingProgress != 1f)) {
            //loadPercentage.text = "Generating textures..." + Mathf.RoundToInt(options.textureLoadingProgress*100f).ToString() + "%";
            //texturename.text = options.textureLoadingName;
            //yield return new WaitForEndOfFrame();
        //}
        OnEndLoad.Invoke();
    }
    public void Start() {
        options.RegisterListener(this);
        StartCoroutine(WaitAndThenDo());
    }
    void OnDestroy() {
        options.UnregisterListener(this);
    }
    public void OnEventRaised(GraphicsOptions.OptionType e, float value) {
        if (e == GraphicsOptions.OptionType.ProceduralTextureSize) {
            StopAllCoroutines();
            StartCoroutine(WaitAndThenDo());
        }
    }
}
