using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SaveUIDisplay : MonoBehaviour {
    [SerializeField]
    private Transform targetPanel;
    [SerializeField]
    private GameObject savePrefab, noSavesText;
    [SerializeField]
    private CanvasGroup saveCheckmark;
    [SerializeField]
    private CanvasGroup failCross;
    
    [SerializeField]
    private Button createNewSaveButton;
    private List<GameObject> saveList = new List<GameObject>();

    private void Awake() {
        createNewSaveButton.onClick.AddListener(() => { AddNewSave(); });
    }

    void OnEnable() {
        RefreshUI();
        createNewSaveButton.interactable = SceneManager.GetActiveScene().name != "MainMenu";
    }

    public void RefreshUI() {
        //Debug.Log("[SaveUIDisplay] Save List Count: "+saveList.Count);
        foreach(GameObject g in saveList) {
            Destroy(g);
        }
        SaveManager.Init(); // Clear on refresh to ensure we're working with the latest data TODO: sync on completion of commands instead?
        List<SaveManager.SaveData> saveData = SaveManager.GetSaveDatas();
        if(saveData.Count != 0){
            noSavesText.SetActive(false);
            foreach(var save in saveData) {
                GameObject newSaveItem = Instantiate(savePrefab, targetPanel);
                SaveSlot slot = newSaveItem.GetComponentInChildren<SaveSlot>();
                slot.Initialize(this, save);
                saveList.Add(newSaveItem);
            }
            if (saveList[0] != null) { saveList[0].GetComponentInChildren<Button>().Select(); }
        } else { // If we don't have any saves, show the text/image that tells user we ain't got none.
            noSavesText.SetActive(true);
        }
    }

    private void AddNewSave(string saveName = "") {
        if (string.IsNullOrEmpty(saveName)) {
            DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
            int cur_time = (int)(System.DateTime.UtcNow - epochStart).TotalSeconds;
            saveName = cur_time.ToString();
        }

        SaveManager.Save(saveName, ()=>{
            StartCoroutine(FadeGroup(saveCheckmark));
            RefreshUI();
        });
    }
    IEnumerator FadeGroup(CanvasGroup group) {
        group.alpha = 1f;
        yield return new WaitForSecondsRealtime(1f);
        while (group.alpha != 0f) {
            group.alpha = Mathf.MoveTowards(group.alpha, 0f, Time.unscaledDeltaTime);
            yield return new WaitForEndOfFrame();
        }
    }
}
