using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ModInfoDisplay : MonoBehaviour {
    private ModManager.ModStub info;
    [SerializeField]
    private Toggle toggle;
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

    public void SetModInfo(ModInfoDisplaySpawner spawner, ModManager.ModStub newInfo) {
        info = newInfo;
        modName.text = newInfo.title;
        rawImage.texture = newInfo.preview;
        modDescription.text = newInfo.description;
        modInfoDisplaySpawner = spawner;
        toggle.onValueChanged.RemoveAllListeners();
        toggle.onValueChanged.AddListener(OnToggle);
        toggle.SetIsOnWithoutNotify(newInfo.enabled);
        steamImage.gameObject.SetActive(newInfo.source == ModManager.ModSource.SteamWorkshop);
    }

    private void OnToggle(bool newState) {
        ModManager.SetModActive(info, newState);
    }
}
