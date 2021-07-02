using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class GraphicsContentViewBrain : MonoBehaviour, IGameEventOptionListener {
    public GameObject dropdown;
    public GameObject slider;
    public GraphicsOptions.OptionGroup group;
    public Dictionary<GraphicsOptions.OptionType,Slider> sliders = new Dictionary<GraphicsOptions.OptionType, Slider>();
    public Dictionary<GraphicsOptions.OptionType,TMP_Dropdown> dropdowns = new Dictionary<GraphicsOptions.OptionType, TMP_Dropdown>();
    public void Start() {
        GraphicsOptions.instance.RegisterListener(this);
        foreach(GraphicsOptions.Option o in GraphicsOptions.instance.options) {
            if (o == null || o.group != group) {
                continue;
            }
            if (o.dropDownDescriptions.Count == 0) {
                CreateSlider(o);
                continue;
            }
            CreateDropDown(o);
        }
        LocalizationSettings.SelectedLocaleChanged += StringChanged;
        StopAllCoroutines();
        StartCoroutine(ChangeStrings());
    }

    public void CreateSlider(GraphicsOptions.Option o) {
        GameObject s = GameObject.Instantiate(slider, Vector3.zero, Quaternion.identity);
        s.transform.SetParent(this.transform);
        s.transform.localScale = Vector3.one;
        foreach( TMP_Text t in s.GetComponentsInChildren<TMP_Text>()) {
            if (t.name == "Label") {
                t.text = o.type.ToString();
                t.text = o.name.GetLocalizedString();
            }
            if (t.name == "Min") {
                t.text = o.min.ToString();
            }
            if (t.name == "Max") {
                t.text = o.max.ToString();
            }
        }
        Slider slid = s.GetComponentInChildren<Slider>();
        slid.minValue = o.min;
        slid.maxValue = o.max;
        slid.SetValueWithoutNotify(o.value);
        if (Mathf.Abs(o.min-o.max) > 1f && Mathf.Approximately(Mathf.Round(o.min), o.min) && Mathf.Approximately(Mathf.Round(o.max),o.max)) {
            slid.wholeNumbers = true;
        } else {
            slid.wholeNumbers = false;
        }
        slid.onValueChanged.AddListener(delegate{ OnValueChanged(slid, o.type); });
        sliders.Add(o.type, slid);
    }

    public void CreateDropDown(GraphicsOptions.Option o) {
        GameObject d = GameObject.Instantiate(dropdown, Vector3.zero, Quaternion.identity);
        d.transform.SetParent(this.transform);
        d.transform.localScale = Vector3.one;
        foreach( TMP_Text t in d.GetComponentsInChildren<TMP_Text>()) {
            if (t.name == "Label") {
                //t.text = o.type.ToString();
                t.text = o.name.GetLocalizedString();
            }
        }
        List<TMP_Dropdown.OptionData> data = new List<TMP_Dropdown.OptionData>();
        if (o.dropDownDescriptions.Count > 0 && o.localizedDropDownDescriptions.Count <= 0) {
            foreach(string desc in o.dropDownDescriptions) {
                data.Add(new TMP_Dropdown.OptionData(desc));
            }
        } else {
            foreach(LocalizedString str in o.localizedDropDownDescriptions) {
                data.Add(new TMP_Dropdown.OptionData(str.GetLocalizedString()));
            }
        }
        TMP_Dropdown drop = d.GetComponentInChildren<TMP_Dropdown>();
        drop.options = data;
        if (o.dropDownDescriptions.Count <= 0) {
            for (int i=0;i<o.localizedDropDownDescriptions.Count;i++) {
                o.localizedDropDownDescriptions[i].StringChanged += (s=>drop.options[i].text = s);
            }
        }
        drop.value = Mathf.RoundToInt(o.value);
        drop.SetValueWithoutNotify(Mathf.RoundToInt(o.value));
        drop.onValueChanged.AddListener(delegate { OnValueChanged(drop, o.type); });
        dropdowns.Add(o.type, drop);
    }
    IEnumerator ChangeStrings() {
        var otherAsync = LocalizationSettings.SelectedLocaleAsync;
        yield return new WaitUntil(()=>otherAsync.IsDone);
        yield return new WaitForSeconds(1f);
        if (otherAsync.Result != null){
            yield return LocalizationSettings.InitializationOperation;
            foreach (GraphicsOptions.OptionType type in (GraphicsOptions.OptionType[]) Enum.GetValues(typeof(GraphicsOptions.OptionType))) {
                GraphicsOptions.Option o = GraphicsOptions.instance.options.Find(t => t.type == type);
                if (o == null || o.group != group) {
                    continue;
                }
                if (o.dropDownDescriptions.Count == 0 && o.localizedDropDownDescriptions.Count == 0) {
                    foreach( TMP_Text t in sliders[type].GetComponentsInChildren<TMP_Text>()) {
                        if (t.name == "Label") {
                            t.text = o.name.GetLocalizedString();
                        }
                        if (t.name == "Min") {
                            t.text = o.min.ToString();
                        }
                        if (t.name == "Max") {
                            t.text = o.max.ToString();
                        }
                    }
                } else {
                    for(int i=0;i<o.localizedDropDownDescriptions.Count;i++) {
                        while (dropdowns[type].options.Count <= i) {
                            dropdowns[type].options.Add(new TMP_Dropdown.OptionData(o.localizedDropDownDescriptions[i].GetLocalizedString()));
                        }
                        dropdowns[type].options[i].text = o.localizedDropDownDescriptions[i].GetLocalizedString();
                        //dropdowns[type].SetValueWithoutNotify(dropdowns[type].value+1);
                        //dropdowns[type].SetValueWithoutNotify(dropdowns[type].value-1);
                    }
                    foreach( TMP_Text t in dropdowns[type].transform.parent.GetComponentsInChildren<TMP_Text>()) {
                        if (t.name == "Label") {
                            t.text = o.name.GetLocalizedString();
                        }
                    }
                }
            }
        }
    }

    public void Refresh() {
        foreach(GraphicsOptions.Option o in GraphicsOptions.instance.options) {
            OnEventRaised(o.type, o.value);
        }
    }
    public void StringChanged(Locale locale) {
        StopAllCoroutines();
        StartCoroutine(ChangeStrings());
    }
    public void OnValueChanged(TMP_Dropdown dropdown, GraphicsOptions.OptionType option) {
        if (!this.isActiveAndEnabled) {
            return;
        }
        GraphicsOptions.instance.ChangeOption(option, dropdown.value);
    }
    public void OnValueChanged(Slider slider, GraphicsOptions.OptionType option) {
        if (!this.isActiveAndEnabled) {
            return;
        }
        GraphicsOptions.instance.ChangeOption(option, slider.value);
    }

    public void OnDestroy() {
        GraphicsOptions.instance.UnregisterListener(this);
        LocalizationSettings.SelectedLocaleChanged -= StringChanged;
    }

    public void OnEventRaised(GraphicsOptions.OptionType e, float value) {
        if (sliders.ContainsKey(e)) {
            sliders[e].SetValueWithoutNotify(value);
        }
        if (dropdowns.ContainsKey(e)) {
            dropdowns[e].SetValueWithoutNotify(Mathf.RoundToInt(value));
        }
    }
}
