using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MultiSelectPanel : MonoBehaviour {
    [SerializeField]
    private ScrollRect scrollRect;
    [SerializeField]
    private MultiSelectToggle multiSelectTogglePrefab;

    private List<MultiSelectOption> options;
    private List<MultiSelectToggle> instantiatedOptions;

    public class MultiSelectOption {
        public string label;
        public bool enabled;
        public delegate void MultiSelectToggleAction(bool newState);
        public MultiSelectToggleAction onValueChanged;
    }

    private void Awake() {
        options = new List<MultiSelectOption>();
    }

    public void AddOption(MultiSelectOption option) {
        options.Add(option);
        Refresh();
    }
    
    public void RemoveOption(MultiSelectOption option) {
        options.Remove(option);
        Refresh();
    }

    public void SetOptions(ICollection<MultiSelectOption> newOptions) {
        options = new List<MultiSelectOption>(newOptions);
        Refresh();
    }

    private void Refresh() {
        instantiatedOptions ??= new List<MultiSelectToggle>();
        foreach (var option in instantiatedOptions) {
            Destroy(option.gameObject);
        }
        instantiatedOptions.Clear();
        foreach (var option in options) {
            var obj = Instantiate(multiSelectTogglePrefab, scrollRect.viewport.Find("Content"));
            if (!obj.TryGetComponent(out MultiSelectToggle toggle)) {
                throw new UnityException("Failed to get MultiSelectToggle from prefab, this shouldn't be possible!");
            }
            instantiatedOptions.Add(toggle);
            toggle.SetOption(option);
        }
    }
    
    protected virtual void Start() {
        Refresh();
    }
}
