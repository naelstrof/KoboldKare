using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public class SaveUIDisplay : MonoBehaviour {
    public Transform targetPanel;
    public GameObject savePrefab, noSavesText;
    public CanvasGroup saveCheckmark;
    public CanvasGroup failCross;
    private List<GameObject> saveList = new List<GameObject>();

    void OnEnable() {
        RefreshUI();
    }
    public void RefreshUI(bool shouldRefresh = true) {
        //Debug.Log("[SaveUIDisplay] Save List Count: "+saveList.Count);
        foreach(GameObject g in saveList) {
            Destroy(g);
        }
        SaveManager.Init(); // Clear on refresh to ensure we're working with the latest data TODO: sync on completion of commands instead?
        List<SaveManager.SaveData> saveData = SaveManager.GetSaveDatas();
        if(saveData.Count != 0){
            noSavesText.SetActive(false);
            foreach(var save in saveData) {
                GameObject newSaveItem = GameObject.Instantiate(savePrefab, targetPanel);
                newSaveItem.transform.Find("SaveImageBorder").transform.GetChild(0).GetComponent<RawImage>().texture = save.image;
                newSaveItem.transform.Find("SaveNameImage").transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = Path.GetFileName(save.fileName);
                newSaveItem.transform.Find("LoadDeletePanel").transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => { SaveManager.Load(save.fileName); });
                newSaveItem.transform.Find("LoadDeletePanel").transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => { RefreshUI(SaveManager.RemoveSave(save.fileName)); });
                saveList.Add(newSaveItem);
            }
        }
        else{ // If we don't have any saves, show the text/image that tells user we ain't got none.
            noSavesText.SetActive(true);
        }
    }

    public void AddNewSave() {
        System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
        int cur_time = (int)(System.DateTime.UtcNow - epochStart).TotalSeconds;
        SaveManager.Save(cur_time.ToString(), ()=>{
            StartCoroutine(FadeGroup(saveCheckmark));
            RefreshUI();
        });
    }
    public IEnumerator FadeGroup(CanvasGroup group) {
        group.alpha = 1f;
        yield return new WaitForSecondsRealtime(1f);
        while (group.alpha != 0f) {
            group.alpha = Mathf.MoveTowards(group.alpha, 0f, Time.unscaledDeltaTime);
            yield return new WaitForEndOfFrame();
        }
    }
}
