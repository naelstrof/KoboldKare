using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;
using System.Collections;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PopupHandler : MonoBehaviour {
    public static PopupHandler instance;
    [SerializeField]
    private CanvasGroup mainCanvasGroup;
    [Serializable]
    public class PopupInfo {
        public string name;
        public AsyncOperationHandle<GameObject> popupPrefabHandle;
        public AssetReferenceGameObject popupPrefabReference;
    }
    public List<PopupInfo> popupDatabase;
    [NonSerialized]
    private List<GameObject> popups;
    [NonSerialized]
    private GameObject internalCanvas;
    private GameObject canvas {
        get {
            if (!Application.isPlaying) {
                return null;
            }
            if (internalCanvas != null) {
                return internalCanvas;
            }
            internalCanvas = new GameObject("PopupCanvas");
            Canvas c = internalCanvas.AddComponent<Canvas>();
            internalCanvas.AddComponent<GraphicRaycaster>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 2;
            DontDestroyOnLoad(internalCanvas.gameObject);
            internalCanvas.hideFlags = HideFlags.HideAndDontSave;
            return internalCanvas;
        }
    }
    // Start is called before the first frame update
    private void OnEnable() {
        if (instance == null) {
            popups = new List<GameObject>();
            instance = this;
        }
    }

    void Awake() {
        foreach (var popupInfo in popupDatabase) {
            popupInfo.popupPrefabHandle = Addressables.LoadAssetAsync<GameObject>(popupInfo.popupPrefabReference);
        }
    }

    private void OnDisable() {
        if (popups != null) {
            foreach (GameObject p in popups) {
                Destroy(p);
            }
        }

        if (internalCanvas != null) {
            Destroy(internalCanvas);
        }
    }
    public void ClearAllPopups() {
        foreach (GameObject p in popups) {
            Destroy(p);
        }
        popups.Clear();
        mainCanvasGroup.interactable = true;
    }

    public bool PopupIsActive() {
        return popups.Count > 0;
    }

    public void ClearPopup(Popup p) {
        if (p == null) {
            return;
        }

        popups.Remove(p.gameObject);
        Destroy(p.gameObject);
        if (popups.Count <= 0) {
            mainCanvasGroup.interactable = true;
        }
    }
    public Popup SpawnPopup(string name, bool solo = true, string title = default, string description = default, Sprite icon = null) {
        Popup popup = null;
        foreach(PopupInfo p in popupDatabase) {
            if (p.name == name && canvas != null) {
                if (!p.popupPrefabHandle.IsDone) {
                    p.popupPrefabHandle.WaitForCompletion();
                }
                GameObject g = Instantiate(p.popupPrefabHandle.Result, canvas.transform);
                popup = g.GetComponentInChildren<Popup>();
                if (popup == null) {
                    Debug.LogError("Popup " + name + " doesn't have a popup component, that's required in order to set things like the text or image of the popup!");
                }
                break;
            }
        }
        if (popup != null) {
            if (description != default) {
                popup.description.text = description;
            }
            if (title != default) {
                popup.title.text = title;
            }
            if (icon != null) {
                popup.icon.sprite = icon;
            }
            if (solo) {
                foreach(GameObject p in popups) {
                    Destroy(p);
                }
                popups.Clear();
                //GameManager.instance.GetComponentInChildren<EventSystem>().enabled = false;
                //canvas.GetComponent<EventSystem>().enabled = true;
            }
            if (popup.cancel != null) {
                popup.cancel.onClick.AddListener(() => { popup.Clear(); });
            }
            if (popup.okay != null) {
                popup.okay.onClick.AddListener(() => { popup.Clear(); });
                popup.okay.Select();
            }
            popups.Add(popup.gameObject);
            mainCanvasGroup.interactable = false;
        }
        return popup;
    }
}
