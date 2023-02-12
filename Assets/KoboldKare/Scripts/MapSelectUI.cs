using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MapSelectUI : MonoBehaviour {
    [SerializeField] private MapPreviewSelectPanel previewSelectPanelPrefab;
    [SerializeField] private TMP_Text mapTitleText;
    [SerializeField] private TMP_Text mapDescriptionText;
    private List<GameObject> panels;
    private void OnEnable() {
        panels = new List<GameObject>();
        foreach (var map in PlayableMapDatabase.GetPlayableMaps()) {
            var obj = Instantiate(previewSelectPanelPrefab.gameObject, transform);
            panels.Add(obj);
            var mapPreview = obj.GetComponentInChildren<MapPreviewSelectPanel>();
            mapPreview.SetMap(map);
        }
    }
    private void OnDisable() {
        foreach (var obj in panels) {
            Destroy(obj);
        }
    }
}
