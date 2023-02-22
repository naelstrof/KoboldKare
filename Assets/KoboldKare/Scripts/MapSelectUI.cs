using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapSelectUI : MonoBehaviour {
    [SerializeField] private MapPreviewSelectPanel previewSelectPanelPrefab;
    [SerializeField] private Image mapPreview;
    [SerializeField] private TMP_Text mapTitleText;
    [SerializeField] private TMP_Text mapDescriptionText;

    private PlayableMap selectedMap;
    
    private List<GameObject> panels;

    private void OnEnable() {
        if (ModManager.GetFinishedLoading()) {
            Regenerate();
        }
        ModManager.AddFinishedLoadingListener(Regenerate);
    }
    private void Regenerate() {
        panels ??= new List<GameObject>();
        foreach (var obj in panels) {
            Destroy(obj);
        }
        panels.Clear();
        
        foreach (var map in PlayableMapDatabase.GetPlayableMaps()) {
            if (selectedMap == null) {
                OnSelectMap(map);
            }
            var obj = Instantiate(previewSelectPanelPrefab.gameObject, transform);
            panels.Add(obj);
            var mapPreview = obj.GetComponentInChildren<MapPreviewSelectPanel>();
            mapPreview.SetMap(map, this);
        }
    }
    private void OnDisable() {
        ModManager.RemoveFinishedLoadingListener(Regenerate);
        foreach (var obj in panels) {
            Destroy(obj);
        }
    }

    //public void OnHoverMap(PlayableMap map) {
        //mapTitleText.text = map.title;
        //mapDescriptionText.text = map.description;
    //}
    public PlayableMap GetSelectedMap() => selectedMap;

    public void OnSelectMap(PlayableMap map) {
        mapTitleText.text = map.title;
        mapDescriptionText.text = map.description;
        mapPreview.sprite = map.preview;
        selectedMap = map;
    }
}
