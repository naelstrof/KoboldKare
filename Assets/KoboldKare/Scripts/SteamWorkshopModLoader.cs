using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class SteamWorkshopModLoader : MonoBehaviour {
    [SerializeField] private Animator progressBarAnimator;
    [SerializeField] private ProgressBar progressBar;
    [SerializeField] private TMP_Text targetText;
    [SerializeField] private LocalizedString downloadingText;
    private bool busy = false;
    private Callback<DownloadItemResult_t> m_DownloadItemResult;
    private Callback<ItemInstalled_t> m_ItemInstalled;
    private Callback<RemoteStoragePublishedFileSubscribed_t> m_RemoteStoragePublishedFileSubscribed;
    private Callback<RemoteStoragePublishedFileUnsubscribed_t> m_RemoteStoragePublishedFileUnsubscribed;
    private IEnumerator Start() {
        yield return new WaitUntil(() => SteamManager.Initialized);
        if (!SteamUser.BLoggedOn()) {
            Debug.LogError("User isn't logged into Steam, cannot use workshop!");
            yield break;
        }
        
        uint subscribedItemCount = SteamUGC.GetNumSubscribedItems();
        PublishedFileId_t[] fileIds = new PublishedFileId_t[subscribedItemCount];
        uint populatedCount = SteamUGC.GetSubscribedItems(fileIds, subscribedItemCount);
        m_DownloadItemResult = Callback<DownloadItemResult_t>.Create(OnDownloadItemResult);
        m_ItemInstalled = Callback<ItemInstalled_t>.Create(OnInstalledItem);
        m_RemoteStoragePublishedFileSubscribed = Callback<RemoteStoragePublishedFileSubscribed_t>.Create(OnItemSubscribed);
        m_RemoteStoragePublishedFileUnsubscribed = Callback<RemoteStoragePublishedFileUnsubscribed_t>.Create(OnItemUnsubscribed);
        StartCoroutine(EnsureAllAreDownloaded(fileIds, populatedCount));
    }

    private IEnumerator EnsureAllAreDownloaded(PublishedFileId_t[] fileIds, uint count) {
        yield return LocalizationSettings.InitializationOperation;
        yield return new WaitUntil(() => !busy);
        progressBarAnimator.SetBool("Active", true);
        progressBar.SetProgress(0f);
        var handle = downloadingText.GetLocalizedStringAsync();
        yield return handle;
        targetText.text = handle.Result;
        
        for(int i=0;i<count;i++) {
            uint status = SteamUGC.GetItemState(fileIds[i]);
            if ((status & (int)EItemState.k_EItemStateInstalled) != 0 && (status & (int)EItemState.k_EItemStateNeedsUpdate) == 0) {
                OnInstalledItem(fileIds[i]);
                continue;
            }
            if ((status & (int)EItemState.k_EItemStateNeedsUpdate) != 0) {
                Debug.Log($"Downloading {fileIds[i]}...");
                    SteamUGC.DownloadItem(fileIds[i], false);
            }
            while((status & (int)EItemState.k_EItemStateNeedsUpdate) != 0 || (status & (int)EItemState.k_EItemStateDownloading) != 0) {
                SteamUGC.GetItemDownloadInfo(fileIds[i], out ulong punBytesDownloaded, out ulong punBytesTotal);
                progressBar.SetProgress((float)punBytesDownloaded/(float)punBytesTotal);
                targetText.text = handle.Result;
                status = SteamUGC.GetItemState(fileIds[i]);
                yield return null;
            }
        }
        progressBar.SetProgress(1f);
        progressBarAnimator.SetBool("Active", false);
    }
    private void OnDownloadItemResult(DownloadItemResult_t downloadItemResultT) {
        Debug.Log($"Downloaded {downloadItemResultT.m_nPublishedFileId} with result {downloadItemResultT.m_eResult}");
    }
    private void OnInstalledItem(PublishedFileId_t publishedFile) {
        Debug.Log($"Installed item {publishedFile}.");
        bool hasData = SteamUGC.GetItemInstallInfo(publishedFile, out ulong punSizeOnDisk, out string pchFolder, 1024, out uint punTimeStamp);
        if (!hasData) {
            return;
        }
        ModManager.AddMod(pchFolder);
    }

    private void OnInstalledItem(ItemInstalled_t installedItem) {
        if (SteamUtils.GetAppID() != installedItem.m_unAppID) {
            return;
        }
        OnInstalledItem(installedItem.m_nPublishedFileId);
    }
    private void OnItemSubscribed(RemoteStoragePublishedFileSubscribed_t subscribedItem) {
        if (SteamUtils.GetAppID() != subscribedItem.m_nAppID) {
            return;
        }
        StartCoroutine(EnsureAllAreDownloaded(new []{subscribedItem.m_nPublishedFileId}, 1));
    }
    private void OnItemUnsubscribed(RemoteStoragePublishedFileUnsubscribed_t unsubscribedItem) {
        if (SteamUtils.GetAppID() != unsubscribedItem.m_nAppID) {
            return;
        }
        bool hasData = SteamUGC.GetItemInstallInfo(unsubscribedItem.m_nPublishedFileId, out ulong punSizeOnDisk, out string pchFolder, 1024, out uint punTimeStamp);
        if (hasData) {
            ModManager.RemoveMod(pchFolder);
        }
    }
}
