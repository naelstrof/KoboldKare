using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public class SaveUIDisplay : MonoBehaviour {
    public Transform targetPanel;
    public GameObject savePrefab;
    public CanvasGroup saveCheckmark;
    public CanvasGroup failCross;
    private List<GameObject> saveList = new List<GameObject>();
    void OnEnable() {
        RefreshUI();
    }
    public void RefreshUI() {
        foreach(GameObject g in saveList) {
            Destroy(g);
        }
        List<SaveManager.SaveData> saveData = SaveManager.GetSaveDatas();
        foreach(var save in saveData) {
            GameObject newSaveItem = GameObject.Instantiate(savePrefab, targetPanel);
            newSaveItem.transform.Find("Image").GetComponent<RawImage>().texture = save.image;
            newSaveItem.transform.Find("Label").GetComponent<TMPro.TextMeshProUGUI>().text = Path.GetFileName(save.fileName);
            newSaveItem.transform.Find("LoadButton").GetComponent<Button>().onClick.AddListener(() => { SaveManager.Load(save.fileName); });
            //newSaveItem.transform.Find("DeleteButton").GetComponent<Button>().onClick.AddListener(() => { SaveManager.RemoveSave(filepath); RefreshUI(); });
            saveList.Add(newSaveItem);
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
