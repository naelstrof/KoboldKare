using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapPreviewSelectPanel : MonoBehaviour {
    [SerializeField]
    private List<Button> selectButtons;
    [SerializeField]
    private Image previewDisplay;

    private PlayableMap playableMap;

    public void SetMap(PlayableMap map) {
        playableMap = map;
        foreach (var button in selectButtons) {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
        previewDisplay.sprite = playableMap.preview;
    }

    void OnClick() {
        NetworkManager.instance.SetSelectedMap(playableMap);
        NetworkManager.instance.StartSinglePlayer();
    }
}
