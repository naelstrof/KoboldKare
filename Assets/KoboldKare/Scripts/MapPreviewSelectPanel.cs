using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapPreviewSelectPanel : MonoBehaviour, IPointerEnterHandler, ISelectHandler, ISubmitHandler {
    [SerializeField]
    private List<Button> selectButtons;
    [SerializeField]
    private Image previewDisplay;

    private PlayableMap playableMap;
    private MapSelectUI parentUI;

    public void SetMap(PlayableMap map, MapSelectUI parentUI) {
        this.parentUI = parentUI;
        playableMap = map;
        foreach (var button in selectButtons) {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
        previewDisplay.sprite = playableMap.preview;
    }
    
    void OnClick() {
        parentUI.OnSelectMap(playableMap);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        //parentUI.OnHoverMap(playableMap);
    }

    public void OnSelect(BaseEventData eventData) {
        //parentUI.OnHoverMap(playableMap);
    }

    public void OnSubmit(BaseEventData eventData) {
        //parentUI.OnHoverMap(playableMap);
    }
}
