using KoboldKare;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour {
    public GameObject loadingPanel, masterCanvas;
    public static LevelLoader instance;
    public static bool loadingLevel;
    public TextMeshProUGUI tmpText;
    public delegate void SceneEventAction();
    public event SceneEventAction sceneLoadStart;
    public event SceneEventAction sceneLoadEnd;

    void Awake() {
        //Check if instance already exists
        if (instance == null) {
            //if not, set instance to this
            instance = this;
        } else if (instance != this) { //If instance already exists and it's not this:
            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);
            return;
        }
    }
    public Coroutine LoadLevel(string name) {
        return StartCoroutine(LoadLevelRoutine(name));
    }
    public IEnumerator LoadLevelRoutine(string name) {
        GameManager.instance.Pause(false);
        sceneLoadStart?.Invoke();
        loadingLevel = true;
        masterCanvas.SetActive(true);
        loadingPanel.GetComponent<CanvasGroup>().alpha = 1f;
        //loadingPanel.Show();
        //loadingPanelProgress.SetProgress(0f);
        yield return new WaitForSeconds(1f);
        PhotonNetwork.LoadLevel(name);
        while (!SceneManager.GetSceneByName(name).isLoaded) {
            //tmpText.text = "Loading ... " + (PhotonNetwork.LevelLoadingProgress/100).ToString("0") + " %";
            //loadingPanelProgress.SetProgress(PhotonNetwork.LevelLoadingProgress);
            yield return new WaitForEndOfFrame();
        }
        //loadingPanelProgress.SetProgress(1f);
        //loadingPanel.Hide();
        loadingPanel.GetComponent<CanvasGroup>().alpha = 0f;
        loadingLevel = false;
        masterCanvas.SetActive(false);
        PopupHandler.instance.ClearAllPopups();
        GameManager.instance.Pause(false);
        sceneLoadEnd?.Invoke();
    }
}
