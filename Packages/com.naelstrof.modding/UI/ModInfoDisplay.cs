using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ModInfoDisplay : MonoBehaviour {
    private ModInfo info;
    [SerializeField]
    private Toggle toggle;
    [SerializeField]
    private Button moveUp;
    [SerializeField]
    private Button moveDown;
    [SerializeField]
    private TMP_Text modName;
    [SerializeField]
    private TMP_Text modDescription;
    [SerializeField]
    private Image steamImage;
    [SerializeField]
    private RawImage rawImage;
    [SerializeField]
    private ModInfoDisplaySpawner modInfoDisplaySpawner;

    public void SetModInfo(ModInfoDisplaySpawner spawner, ModInfo newInfo) {
        info = newInfo;
        modName.text = newInfo.title;
        rawImage.texture = newInfo.preview;
        modDescription.text = newInfo.description;
        modInfoDisplaySpawner = spawner;
        toggle.onValueChanged.RemoveAllListeners();
        toggle.onValueChanged.AddListener(OnToggle);
        toggle.SetIsOnWithoutNotify(newInfo.enabled);
        steamImage.gameObject.SetActive(newInfo.modSource == ModInfo.ModSource.SteamWorkshop);
        
        moveUp.onClick.RemoveAllListeners();
        moveUp.onClick.AddListener(OnMoveUp);
        moveDown.onClick.RemoveAllListeners();
        moveDown.onClick.AddListener(OnMoveDown);
    }

    private void OnToggle(bool newState) {
        info.enabled = newState;
        StartCoroutine(ReloadWait());
    }

    private void OnMoveUp() {
        ModManager.IncrementPriority(info);
        modInfoDisplaySpawner.Refresh();
        StartCoroutine(ReloadWait());
    }
    
    private void OnMoveDown() {
        ModManager.DecrementPriority(info);
        modInfoDisplaySpawner.Refresh();
        StartCoroutine(ReloadWait());
    }

    private IEnumerator ReloadWait() {
        Task t = ModManager.Reload();
        yield return new WaitUntil(()=>t.IsCompleted);
    }
}
