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
    public List<UnityEngine.InputSystem.InputActionReference> controls = new List<InputActionReference>();
    public List<LocalizedString> controlNames = new List<LocalizedString>();
    public GameObject rebindPrefab;
    public GameObject rebindUI;
    public TMPro.TextMeshProUGUI rebindUIText;
    private Dictionary<InputActionReference, GameObject> controlUI = new Dictionary<InputActionReference, GameObject>();
    void Start() {
        controlUI.Clear();
        int controlNum = 0;
        foreach( InputActionReference r in controls) {
            GameObject i = GameObject.Instantiate(rebindPrefab);
            i.transform.SetParent(transform, false);
            i.transform.localScale = Vector3.one;
            var lstring = controlNames[controlNum++].GetLocalizedString();
            if (lstring.IsDone) {
                i.GetComponentInChildren<TextMeshProUGUI>().text = lstring.Result;
            }
            controlUI[r] = i;
            int id = 0;
            foreach(RebindActionUI rebinder in i.GetComponentsInChildren<RebindActionUI>()) {
                rebinder.actionReference = r;
                rebinder.bindingId = r.action.bindings[id++].id.ToString();
                //rebinder.bindingId = r.action.bindings[id++].id.ToString();
                rebinder.rebindOverlay = rebindUI;
                rebinder.rebindPrompt = rebindUIText;
                rebinder.UpdateBindingDisplay();
            }
        }
        LocalizationSettings.SelectedLocaleChanged += StringChanged;
        StringChanged(null);
    }
    public void RefreshDisplay() {
        foreach(KeyValuePair<InputActionReference, GameObject> p in controlUI) {
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
            int controlNum = 0;
            foreach( InputActionReference r in controls) {
                var lstring = controlNames[controlNum++].GetLocalizedString();
                yield return new WaitUntil(()=>lstring.IsDone);
                if (lstring.IsValid()) {
                    controlUI[r].GetComponentInChildren<TextMeshProUGUI>().text = lstring.Result;
                }
            }
        }
    }
    public void StringChanged(Locale locale) {
        StopAllCoroutines();
        StartCoroutine(ChangeStrings());
    }
}
