using System;
using UnityEngine;
using UnityEngine.Localization;

public class ModLoadingErrorPopup : MonoBehaviour {
    [SerializeField]
    private LocalizedString failedToLoadModsDescription;
    private void Start() {
        ModManager.AddFinishedLoadingListener(CheckIfModLoadSucceeded);
    }
    private void OnDisable() {
        ModManager.RemoveFinishedLoadingListener(CheckIfModLoadSucceeded);
    }
    private void CheckIfModLoadSucceeded() {
        if (ModManager.TryGetLastException(out Exception e)) {
            PopupHandler.instance.SpawnPopup("FailedModLoads");
        }
    }
}
