using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

public class ModdingStatusDisplay : MonoBehaviour {
    [SerializeField] private CanvasGroup group;
    [SerializeField] private LocalizeStringEvent lse;
    [SerializeField] private TMPro.TMP_Text text;
    
    [SerializeField] private LocalizedString init;
    [SerializeField] private LocalizedString waitingForDownloads;
    [SerializeField] private LocalizedString scanningForMods;
    [SerializeField] private LocalizedString unloadingMods;
    [SerializeField] private LocalizedString loadingMods;
    [SerializeField] private LocalizedString importingAssets;
    [SerializeField] private LocalizedString checkingForErrors;
    [SerializeField] private LocalizedString ready;

    private ModManager.ModStatus lastStatus;
    void Update() {
        if (ModManager.GetStatus() == lastStatus) {
            return;
        }
        lastStatus = ModManager.GetStatus();
        group.alpha = 1f;
        switch (lastStatus) {
            case ModManager.ModStatus.Initializing: lse.StringReference = init; break;
            case ModManager.ModStatus.WaitingForDownloads: lse.StringReference = waitingForDownloads; break;
            case ModManager.ModStatus.ScanningForMods: lse.StringReference = scanningForMods; break;
            case ModManager.ModStatus.UnloadingMods: lse.StringReference = unloadingMods; break;
            case ModManager.ModStatus.LoadingMods: lse.StringReference = loadingMods; break;
            case ModManager.ModStatus.LoadingAssets: lse.StringReference = importingAssets; break;
            case ModManager.ModStatus.InspectingForErrors: lse.StringReference = checkingForErrors; break;
            case ModManager.ModStatus.Ready:
                lse.StringReference = ready;
                group.alpha = 0f;
                break;
        }
        StartCoroutine(UpdateText());
    }

    IEnumerator UpdateText() {
        var handle = lse.StringReference.GetLocalizedStringAsync();
        yield return handle;
        text.text = handle.Result;
    }
}
