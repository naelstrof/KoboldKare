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
    
    [SerializeField] private TMP_InputField nameField;
    [SerializeField] private Slider playerCountSlider;
    [SerializeField] private Toggle privateRoom;
    [SerializeField] private MapSelectUI mapSelectUI;
    [SerializeField] private GameObject[] multiplayerUI;
    private List<bool> otherViewMemory;
    private bool busy;
    private MapSelectHandle currentHandle;
    private bool currentMultiplayer;

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
        if (instance.currentHandle != null) {
            instance.currentHandle.Invoke(true, default);
            instance.currentHandle = null;
        }
        instance.currentHandle = new MapSelectHandle();
        instance.currentMultiplayer = multiplayer;
        foreach(var obj in instance.multiplayerUI) {
            obj.SetActive(instance.currentMultiplayer);
        }
        MainMenu.ShowMenuStatic(MainMenu.MainMenuMode.MapSelect);
        return instance.currentHandle;
    }

    public void Cancel() {
        if (instance.currentHandle != null) {
            instance.currentHandle.Invoke(true, default);
            instance.currentHandle = null;
        }
    }

    public void Confirm() {
        if (instance.currentHandle != null) {
            MainMenu.ShowMenuStatic(MainMenu.MainMenuMode.None);
            instance.currentHandle.Invoke(false, new MapSelectResults() {
                playableMap = mapSelectUI.GetSelectedMap(),
                multiplayer = instance.currentMultiplayer,
                playerCount = Mathf.RoundToInt(playerCountSlider.value),
                privateRoom = privateRoom.isOn,
                roomName = nameField.text
            });
            instance.currentHandle = null;
        }
    }
}
