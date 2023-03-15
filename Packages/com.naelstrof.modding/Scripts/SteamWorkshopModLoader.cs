using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SteamWorkshopModLoader : MonoBehaviour {
    private static SteamWorkshopModLoader instance;
    
    [SerializeField] private Animator progressBarAnimator;
    [SerializeField] private ProgressBar progressBar;
    [SerializeField] private TMP_Text targetText;
    [SerializeField] private LocalizedString downloadingText;
    [SerializeField] private LocalizedString installingText;
    [SerializeField] private LocalizedString failedToConnectToSteam;
    private bool busy = false;
    private Callback<DownloadItemResult_t> m_DownloadItemResult;
    private Callback<ItemInstalled_t> m_ItemInstalled;
    private CallResult<SteamUGCQueryCompleted_t> m_QueryCompleted;
    private Callback<RemoteStoragePublishedFileSubscribed_t> m_RemoteStoragePublishedFileSubscribed;
    private Callback<RemoteStoragePublishedFileUnsubscribed_t> m_RemoteStoragePublishedFileUnsubscribed;
    private int waitingForResultCount;
    private bool waitingForQuery;
    private List<SteamUGCDetails_t> queryDetails;

    public class FinishedDownloadingHandle : IEnumerator {
        public delegate void FinishedDownloadingAction();
        public event FinishedDownloadingAction finished;
        private bool IsDone = false;
        public void Invoke() {
            IsDone = true;
            finished?.Invoke();
        }
        bool IEnumerator.MoveNext() {
            return !IsDone;
        }
        void IEnumerator.Reset() {}
        public object Current => IsDone;
    }
    private void Awake() {
        if (instance != null) {
            Destroy(this);
            return;
        }

        instance = this;
    }

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
        m_QueryCompleted = CallResult<SteamUGCQueryCompleted_t>.Create(OnQueryCompleted);
        m_RemoteStoragePublishedFileSubscribed = Callback<RemoteStoragePublishedFileSubscribed_t>.Create(OnItemSubscribed);
        m_RemoteStoragePublishedFileUnsubscribed = Callback<RemoteStoragePublishedFileUnsubscribed_t>.Create(OnItemUnsubscribed);
        queryDetails = new List<SteamUGCDetails_t>();
        StartCoroutine(EnsureAllAreDownloaded(fileIds, populatedCount, null));
    }

    public static bool IsBusy => instance.busy;

    public static FinishedDownloadingHandle TryDownloadAllMods(PublishedFileId_t[] fileIds) {
        FinishedDownloadingHandle handle = new FinishedDownloadingHandle();
        instance.StartCoroutine(instance.EnsureAllAreDownloaded(fileIds, (uint)fileIds.Length, handle));
        return handle;
    }

    private IEnumerator EnsureAllAreDownloaded(PublishedFileId_t[] fileIds, uint count, FinishedDownloadingHandle finishedHandle) {
        if (count == 0) {
            yield return null;
            finishedHandle?.Invoke();
            yield break;
        }

        yield return LocalizationSettings.InitializationOperation;
        yield return new WaitUntil(() => !busy);
        var downloadTextHandle = downloadingText.GetLocalizedStringAsync();
        yield return downloadTextHandle;
        string downloadText = downloadTextHandle.Result;
        
        var installingTextHandle = installingText.GetLocalizedStringAsync();
        yield return installingTextHandle;
        string installText = installingTextHandle.Result;
        
        var failedToConnectToSteamHandle = failedToConnectToSteam.GetLocalizedStringAsync();
        yield return failedToConnectToSteamHandle;
        string failedToConnectToSteamText = failedToConnectToSteamHandle.Result;
        if (SteamManager.FailedToInitialize) {
            targetText.text = failedToConnectToSteamText;
            progressBar.gameObject.SetActive(false);
            progressBarAnimator.SetTrigger("Failed");
            yield return null;
            finishedHandle?.Invoke();
            yield break;
        }

        progressBarAnimator.SetBool("Active", true);
        progressBar.gameObject.SetActive(false);
        try {
            busy = true;
            waitingForQuery = true;
            var queryHandleT = SteamUGC.CreateQueryUGCDetailsRequest(fileIds, count);
            m_QueryCompleted.Set(SteamUGC.SendQueryUGCRequest(queryHandleT));
            yield return new WaitUntil(() => !waitingForQuery);
            for (int i = 0; i < count; i++) {
                uint status = SteamUGC.GetItemState(fileIds[i]);
                if ((status & (int)EItemState.k_EItemStateInstalled) != 0 &&
                    (status & (int)EItemState.k_EItemStateNeedsUpdate) == 0) {
                    if (queryDetails.Count == count) {
                        targetText.text = $"{installText} {queryDetails[i].m_rgchTitle}";
                    } else {
                        targetText.text = $"{installText} {fileIds[i]}";
                    }
                    OnInstalledItem(fileIds[i]);
                    continue;
                }
                if ((status & (int)EItemState.k_EItemStateInstalled) == 0 || (status & (int)EItemState.k_EItemStateNeedsUpdate) != 0) {
                    if (queryDetails.Count == count) {
                        Debug.Log($"Downloading {fileIds[i]}, `{queryDetails[i].m_rgchTitle}`...");
                        targetText.text = $"{downloadText} {queryDetails[i].m_rgchTitle}";
                    } else {
                        Debug.Log($"Downloading {fileIds[i]}...");
                        targetText.text = $"{downloadText} {fileIds[i]}";
                    }
                    waitingForResultCount++;
                    SteamUGC.DownloadItem(fileIds[i], false);
                }
            }

            for (int i = 0; i < count; i++) {
                uint status = SteamUGC.GetItemState(fileIds[i]);
                while ((status & (int)EItemState.k_EItemStateDownloading) != 0) {
                    if (!progressBar.gameObject.activeInHierarchy) {
                        progressBar.gameObject.SetActive(true);
                    }
                    SteamUGC.GetItemDownloadInfo(fileIds[i], out ulong punBytesDownloaded, out ulong punBytesTotal);
                    progressBar.SetProgress((float)punBytesDownloaded / (float)punBytesTotal);
                    if (queryDetails.Count == count) {
                        targetText.text = $"{downloadText} {queryDetails[i].m_rgchTitle}";
                    } else {
                        targetText.text = $"{downloadText} {fileIds[i]}";
                    }
                    status = SteamUGC.GetItemState(fileIds[i]);
                    yield return null;
                }
            }

            // Wait until everything is downloaded and installed.
            while (waitingForResultCount > 0) {
                yield return null;
            }
            
            // wait one extra frame for OnInstalledItem
            yield return null;

            progressBarAnimator.SetBool("Active", false);
        } finally {
            busy = false;
        }
        finishedHandle?.Invoke();
    }
    private void OnDownloadItemResult(DownloadItemResult_t downloadItemResultT) {
        Debug.Log($"Downloaded {downloadItemResultT.m_nPublishedFileId} with result {downloadItemResultT.m_eResult}");
        waitingForResultCount--;
    }
    private void OnInstalledItem(PublishedFileId_t publishedFile) {
        Debug.Log($"Installed item {publishedFile}.");
        bool hasData = SteamUGC.GetItemInstallInfo(publishedFile, out ulong punSizeOnDisk, out string pchFolder, 1024, out uint punTimeStamp);
        if (!hasData) {
            return;
        }
        ModManager.AddMod(pchFolder);
    }

    private void OnQueryCompleted(SteamUGCQueryCompleted_t query, bool status) {
        queryDetails.Clear();
        if (query.m_eResult != EResult.k_EResultOK) {
            Debug.LogError($"Query failed with code{query.m_eResult}, skipping");
            waitingForQuery = false;
            return;
        }
        uint count = query.m_unNumResultsReturned;
        for (uint i = 0; i < count; i++) {
            SteamUGC.GetQueryUGCResult(query.m_handle, i, out SteamUGCDetails_t details);
            queryDetails.Add(details);
        }
        waitingForQuery = false;
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
        StartCoroutine(EnsureAllAreDownloaded(new []{subscribedItem.m_nPublishedFileId}, 1, null));
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
