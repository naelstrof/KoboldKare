using KoboldKare;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour {
    public GameObject loadingPanel;
    public static LevelLoader instance;
    public static bool loadingLevel;
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
        if (!SaveManager.isLoading) {
            SaveManager.ClearData();
        }
        loadingLevel = true;
        loadingPanel.SetActive(true);
        //loadingPanel.Show();
        //loadingPanelProgress.SetProgress(0f);
        yield return new WaitForSeconds(1f);
        PhotonNetwork.LoadLevel(name);
        while (!SceneManager.GetSceneByName(name).isLoaded) {
            loadingPanel.GetComponentInChildren<TMP_Text>().text = "Loading ... " + PhotonNetwork.LevelLoadingProgress.ToString("0") + " %";
            //loadingPanelProgress.SetProgress(PhotonNetwork.LevelLoadingProgress);
            yield return new WaitForEndOfFrame();
        }
        //loadingPanelProgress.SetProgress(1f);
        //loadingPanel.Hide();
        loadingPanel.SetActive(false);
        loadingLevel = false;
        PopupHandler.instance.ClearAllPopups();
        GameManager.instance.Pause(false);
    }
}
