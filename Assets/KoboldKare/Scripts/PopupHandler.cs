using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "PopupHandler", menuName = "Data/PopupHandler", order = 1)]
public class PopupHandler : SingletonScriptableObject<PopupHandler> {
    [System.Serializable]
    public class PopupInfo {
        public string name;
        public GameObject popupPrefab;
    }
    public List<PopupInfo> popupDatabase = new List<PopupInfo>();
    [NonSerialized]
    private List<GameObject> popups = new List<GameObject>();
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
            //EventSystem e = internalCanvas.AddComponent<EventSystem>();
            //e.enabled = false;
            //internalCanvas.AddComponent<InputSystemUIInputModule>().actionsAsset = GameManager.instance.GetComponentInChildren<InputSystemUIInputModule>().actionsAsset;
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 2;
            DontDestroyOnLoad(internalCanvas.gameObject);
            internalCanvas.hideFlags = HideFlags.HideAndDontSave;
            return internalCanvas;
        }
    }
    // Start is called before the first frame update
    private void OnEnable() {
        foreach(GameObject p in popups) {
            Destroy(p);
        }
        popups.Clear();
        if (internalCanvas) {
            if (Application.isPlaying) {
                Destroy(internalCanvas);
            } else {
                DestroyImmediate(internalCanvas);
            }
        }
    }
    public void OnDisable() {
        OnDestroy();
    }
    public void OnDestroy() {
        if (!Application.isPlaying) {
            foreach (GameObject p in popups) {
                DestroyImmediate(p);
            }
            popups.Clear();
            if (internalCanvas != null) {
                DestroyImmediate(internalCanvas);
            }
            return;
        }
        foreach (GameObject p in popups) {
            Destroy(p);
        }
        popups.Clear();
        if (internalCanvas != null) {
            Destroy(internalCanvas);
        }
    }
    public void ClearAllPopups() {
        foreach (GameObject p in popups) {
            Destroy(p);
        }
        popups.Clear();
    }

    public void SpawnPopupBasic(string name) {
        SpawnPopup(name);
    }

    public void ClearPopup(Popup p) {
        popups.Remove(p.gameObject);
        Destroy(p.gameObject); 
        //if (popups.Count <=0) {
            //GameManager.instance.GetComponentInChildren<EventSystem>().enabled = true;
            //canvas.GetComponent<EventSystem>().enabled = false;
        //}
    }
    public Popup SpawnPopup(string name, bool solo = true, string description = default(string), Sprite icon = null) {
        Popup popup = null;
        foreach(PopupInfo p in popupDatabase) {
            if (p.name == name && canvas != null) {
                GameObject g = GameObject.Instantiate(p.popupPrefab, canvas.transform);
                popup = g.GetComponentInChildren<Popup>();
                if (popup == null) {
                    Debug.LogError("Popup " + name + " doesn't have a popup component, that's required in order to set things like the text or image of the popup!");
                }
                break;
            }
        }
        if (popup != null) {
            if (description != default(string)) {
                popup.description.text = description;
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
                popup.cancel.onClick.AddListener(() => { ClearPopup(popup); });
            }
            if (popup.okay != null) {
                popup.okay.onClick.AddListener(() => { ClearPopup(popup); });
            }
            popups.Add(popup.gameObject);
        }
        return popup;
    }
}
