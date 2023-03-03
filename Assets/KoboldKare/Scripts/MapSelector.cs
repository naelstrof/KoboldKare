using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapSelector : MonoBehaviour {
    private static MapSelector instance;
    [SerializeField] private List<GameObject> otherViews;
    [SerializeField] private GameObject mapSelectView;
    [SerializeField] private Button backButton;
    [SerializeField] private Button confirmButton;
    
    [SerializeField] private TMP_InputField nameField;
    [SerializeField] private Slider playerCountSlider;
    [SerializeField] private Toggle privateRoom;
    [SerializeField] private MapSelectUI mapSelectUI;
    [SerializeField] private GameObject[] multiplayerUI;
    private List<bool> otherViewMemory;
    private bool busy;

    public struct MapSelectResults {
        public bool multiplayer;
        public int playerCount;
        public bool privateRoom;
        public string roomName;
        public PlayableMap playableMap;
    }

    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }
        instance = this;
        otherViewMemory = new List<bool>();
    }
    public class MapSelectHandle : IEnumerator {
        public delegate void FinishedMapSelectAction(bool cancelled, MapSelectResults mapSelectResults);
        public event FinishedMapSelectAction finished;
        public bool IsDone;
        public bool Cancelled;
        public MapSelectResults Result;
        public void Invoke(bool cancelled, MapSelectResults mapSelectResults) {
            Cancelled = cancelled;
            Result = mapSelectResults;
            IsDone = true;
            finished?.Invoke(cancelled, mapSelectResults);
        }
        bool IEnumerator.MoveNext() {
            return !IsDone;
        }
        void IEnumerator.Reset() {}
        public object Current => IsDone;
    }

    public static MapSelectHandle PromptForMapSelect(bool multiplayer) {
        MapSelectHandle newHandle = new MapSelectHandle();
        GameManager.StartCoroutineStatic(instance.SelectMapRoutine(newHandle, multiplayer));
        return newHandle;
    }

    private void SetVisibility(bool visible, bool multiplayer = false) {
        if (visible) {
            otherViewMemory.Clear();
            foreach (var view in otherViews) {
                otherViewMemory.Add(view.activeSelf);
                view.SetActive(false);
            }
        } else {
            for (int i=0;i<otherViews.Count;i++) {
                otherViews[i].SetActive(otherViewMemory[i]);
            }
        }
        mapSelectView.SetActive(visible);
        foreach(var obj in multiplayerUI) {
            obj.SetActive(visible && multiplayer);
        }
    }

    private IEnumerator SelectMapRoutine(MapSelectHandle handle, bool multiplayer) {
        yield return new WaitUntil(() => !busy);
        busy = true;
        try {
            SetVisibility(true, multiplayer);
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() => {
                SetVisibility(false);
                handle.Invoke(true, default);
            });
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() => {
                SetVisibility(false);
                handle.Invoke(false, new MapSelectResults() {
                    playableMap = mapSelectUI.GetSelectedMap(),
                    multiplayer = multiplayer,
                    playerCount = Mathf.RoundToInt(playerCountSlider.value),
                    privateRoom = privateRoom.isOn,
                    roomName = nameField.text
                });
            });
            yield return new WaitUntil(() => handle.IsDone);
        } finally {
            busy = false;
        }
    }
}
