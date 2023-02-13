using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MapSelectUI : MonoBehaviour {
    [SerializeField] private MapPreviewSelectPanel previewSelectPanelPrefab;
    [SerializeField] private TMP_Text mapTitleText;
    [SerializeField] private TMP_Text mapDescriptionText;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject mapSelectPanel;
    
    private List<GameObject> panels;
    private void OnEnable() {
        panels = new List<GameObject>();
        foreach (var map in PlayableMapDatabase.GetPlayableMaps()) {
            var obj = Instantiate(previewSelectPanelPrefab.gameObject, transform);
            panels.Add(obj);
            var mapPreview = obj.GetComponentInChildren<MapPreviewSelectPanel>();
            mapPreview.SetMap(map, this);
        }
    }
    private void OnDisable() {
        foreach (var obj in panels) {
            Destroy(obj);
        }
    }

    public void OnHoverMap(PlayableMap map) {
        mapTitleText.text = map.title;
        mapDescriptionText.text = map.description;
    }

    public void OnSelectMap() {
        mainMenuPanel.gameObject.SetActive(true);
        mapSelectPanel.gameObject.SetActive(false);
    }
}
