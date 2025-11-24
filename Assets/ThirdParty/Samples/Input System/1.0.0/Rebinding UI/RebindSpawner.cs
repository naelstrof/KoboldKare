using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.InputSystem.Samples.RebindUI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class RebindSpawner : MonoBehaviour
{
    [System.Serializable]
    public class RebindActionNamePair {
        public UnityEngine.InputSystem.InputActionReference control;
        public LocalizedString controlName;
    }
    public List<RebindActionNamePair> rebindActionNamePairs= new List<RebindActionNamePair>();
    public GameObject rebindPrefab;
    public GameObject rebindUI;
    public TMPro.TextMeshProUGUI rebindUIText;
    private Dictionary<RebindActionNamePair, GameObject> controlUI = new Dictionary<RebindActionNamePair, GameObject>();
    IEnumerator StartRoutine() {
        yield return LocalizationSettings.InitializationOperation;
        // Wait for settings to spawn
        yield return new WaitForSecondsRealtime(0.25f);
        controlUI.Clear();
        var controls = GameManager.GetPlayerControls();
        foreach( var r in rebindActionNamePairs) {
            GameObject i = GameObject.Instantiate(rebindPrefab);
            i.transform.SetParent(transform, false);
            i.transform.localScale = Vector3.one;
            i.GetComponentInChildren<TextMeshProUGUI>().text = r.controlName.GetLocalizedString();
            controlUI[r] = i;
            int id = 0;
            
            foreach(RebindActionUI rebinder in i.GetComponentsInChildren<RebindActionUI>()) {
                rebinder.actionReference = controls.FindAction(r.control.name);
                rebinder.bindingId = r.control.action.bindings[id++].id.ToString();
                //rebinder.bindingId = r.action.bindings[id++].id.ToString();
                rebinder.rebindOverlay = rebindUI;
                rebinder.rebindPrompt = rebindUIText;
                rebinder.UpdateBindingDisplay();
            }
        }
        LocalizationSettings.SelectedLocaleChanged += StringChanged;
        StringChanged(null);
    }
    void Start() {
        GameManager.instance.StartCoroutine(StartRoutine());
    }
    public void RefreshDisplay() {
        foreach(var p in controlUI) {
            foreach (RebindActionUI rebinder in p.Value.GetComponentsInChildren<RebindActionUI>()) {
                rebinder.UpdateBindingDisplay();
            }
        }
    }

    IEnumerator ChangeStrings() {
        var otherAsync = LocalizationSettings.SelectedLocaleAsync;
        yield return new WaitUntil(()=>otherAsync.IsDone);
        yield return new WaitForSecondsRealtime(1f);
        if (otherAsync.Result != null){
            foreach( var r in rebindActionNamePairs) {
                controlUI[r].GetComponentInChildren<TextMeshProUGUI>().text = r.controlName.GetLocalizedString();
            }
        }
    }
    public void StringChanged(Locale locale) {
        StopAllCoroutines();
        GameManager.instance.StartCoroutine(ChangeStrings());
    }
}
