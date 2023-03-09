using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SaveSlot : MonoBehaviour {
    [SerializeField]
    private LocalizedString secondsUnit;
    [SerializeField]
    private LocalizedString minutesUnit;
    [SerializeField]
    private LocalizedString hoursUnit;
    [SerializeField]
    private LocalizedString daysUnit;
    [SerializeField]
    private LocalizedString yearsUnit;
    [SerializeField]
    private LocalizedString loadConfirmDescription;
    [SerializeField]
    private LocalizedString deleteConfirmDescription;
    [SerializeField]
    private LocalizedString areYouSureTitle;

    [SerializeField] private LocalizeStringEvent timeAgoText;
    [SerializeField] private TMPro.TMP_Text saveName;
    [SerializeField] private RawImage saveImage;
    
    private SaveManager.SaveData data;
    const int Second = 1;
    const int Minute = 60 * Second;
    const int Hour = 60 * Minute;
    const int Day = 24 * Hour;
    const int Year = 365 * Day;
    public void Initialize(SaveUIDisplay parent, SaveManager.SaveData data) {
        this.data = data;
        saveName.text = Path.GetFileName(data.fileName);
        saveImage.texture = data.image;
        GetComponent<Button>().onClick.RemoveAllListeners();
        GetComponent<Button>().onClick.AddListener(() => {
            var popup = PopupHandler.instance.SpawnPopup("SaveSlotInteract");
            popup.title.text = Path.GetFileName(data.fileName);
            popup.transform.Find("Container/OptionsDisplay/RawImage").GetComponent<RawImage>().texture = data.image;
            popup.transform.Find("Container/OptionsDisplay/Button - Load").GetComponent<Button>().onClick.AddListener(() => {
                if (SceneManager.GetActiveScene().name != "MainMenu") {
                    var confirm = PopupHandler.instance.SpawnPopup("ConfirmPopup", false, areYouSureTitle.GetLocalizedString(), loadConfirmDescription.GetLocalizedString());
                    confirm.okay.onClick.AddListener(() => {
                        SaveManager.Load(data.fileName);
                        confirm.Clear();
                        popup.Clear();
                    });
                } else {
                    SaveManager.Load(data.fileName);
                    popup.Clear();
                }
            });
            popup.transform.Find("Container/OptionsDisplay/Button - Delete").GetComponent<Button>().onClick.AddListener(() => {
                var confirm = PopupHandler.instance.SpawnPopup("ConfirmPopup", false, areYouSureTitle.GetLocalizedString(), deleteConfirmDescription.GetLocalizedString());
                confirm.okay.onClick.AddListener(() => {
                    SaveManager.RemoveSave(data.fileName);
                    parent.RefreshUI();
                    popup.Clear();
                });
            });
        });
        var ts = new TimeSpan(DateTime.Now.Ticks - data.time.Ticks);
        double delta = Math.Abs(ts.TotalSeconds);
        switch (delta) {
            case < Minute:
                timeAgoText.StringReference = new LocalizedString(secondsUnit.TableReference, secondsUnit.TableEntryReference) { Arguments = new List<object> { (int)(delta / Second) } };
                break;
            case < Hour:
                timeAgoText.StringReference = new LocalizedString(minutesUnit.TableReference, minutesUnit.TableEntryReference) { Arguments = new List<object> { (int)(delta / Minute) } };
                break;
            case < Day:
                timeAgoText.StringReference = new LocalizedString(hoursUnit.TableReference, hoursUnit.TableEntryReference) { Arguments = new List<object> { (int)(delta / Hour) } };
                break;
            case < Year:
                timeAgoText.StringReference = new LocalizedString(daysUnit.TableReference, daysUnit.TableEntryReference) { Arguments = new List<object> { (int)(delta / Day) } };
                break;
            default:
                timeAgoText.StringReference = new LocalizedString(yearsUnit.TableReference, yearsUnit.TableEntryReference) { Arguments = new List<object> { (int)(delta / Year) } };
                break;
        }
        timeAgoText.RefreshString();
    }
}
