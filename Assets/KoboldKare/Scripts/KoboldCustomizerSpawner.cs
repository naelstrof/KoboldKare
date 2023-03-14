using System;
using System.Collections;
using System.Collections.ObjectModel;
using UnityEngine;

public class KoboldCustomizerSpawner : MonoBehaviour {
    [SerializeField] private PrefabSelectSingleSetting playerSetting; 
    [SerializeField] private PrefabDatabase playerPrefabDatabase;
    private OrbitCameraConfigurationBlend cameraConfiguration;

    private GameObject player;
    private OrbitCameraLockedLerpTrackPivot shoulderPivot;
    private OrbitCameraLockedLerpTrackPivot buttPivot;

    void Start() {
        ModManager.AddFinishedLoadingListener(FinishedLoading);
        if (ModManager.GetReady()) {
            FinishedLoading();
        }
        shoulderPivot = new GameObject("ShoulderCamPivot", typeof(OrbitCameraLockedLerpTrackPivot)).GetComponent<OrbitCameraLockedLerpTrackPivot>();
        buttPivot = new GameObject("ButtPivot", typeof(OrbitCameraLockedLerpTrackPivot)).GetComponent<OrbitCameraLockedLerpTrackPivot>();
        shoulderPivot.gameObject.SetActive(false);
        buttPivot.gameObject.SetActive(false);
        cameraConfiguration = new OrbitCameraConfigurationBlend();
        cameraConfiguration.SetPivots(shoulderPivot, buttPivot, 0.5f);
    }

    void FinishedLoading() {
        OnChangedPlayer();
        playerSetting.changed += OnChangedPlayer;
        GameManager.GetPenisDatabase().AddPrefabReferencesChangedListener(OnChangedPrefabDatabase);
        playerPrefabDatabase.AddPrefabReferencesChangedListener(OnChangedPrefabDatabase);
        ModManager.RemoveFinishedLoadingListener(FinishedLoading);
    }

    private void OnDestroy() {
        playerSetting.changed -= OnChangedPlayer;
        GameManager.GetPenisDatabase().RemovePrefabReferencesChangedListener(OnChangedPrefabDatabase);
        playerPrefabDatabase.RemovePrefabReferencesChangedListener(OnChangedPrefabDatabase);
    }

    void OnChangedPrefabDatabase(ReadOnlyCollection<PrefabDatabase.PrefabReferenceInfo> infos) {
        StopAllCoroutines();
        StartCoroutine(EnsureModsAreLoadedThenChangePlayer());
    }
    void OnChangedPlayer(int newValue = -1) {
        StopAllCoroutines();
        StartCoroutine(EnsureModsAreLoadedThenChangePlayer());
    }

    IEnumerator EnsureModsAreLoadedThenChangePlayer() {
        if (player != null) {
            shoulderPivot.transform.SetParent(null);
            buttPivot.transform.SetParent(null);
            shoulderPivot.gameObject.SetActive(false);
            buttPivot.gameObject.SetActive(false);
            Destroy(player);
            OrbitCamera.RemoveConfiguration(cameraConfiguration);
        }
        
        yield return new WaitUntil(ModManager.GetReady);
        OnChangePlayerRoutine();
    }


    void HandlePlayerSpawn(GameObject player) {
        var characterDescriptor = player.GetComponent<CharacterDescriptor>();
        characterDescriptor.finishedLoading += (view) => {
            player.AddComponent<PlayerKoboldLoader>();
            shoulderPivot.SetInfo(new Vector2(0.666f, 0.666f), 2f);
            shoulderPivot.Initialize(characterDescriptor.GetDisplayAnimator(), HumanBodyBones.Head, 1f);

            buttPivot.SetInfo(new Vector2(0.666f, 0.333f), 2f);
            buttPivot.Initialize(characterDescriptor.GetDisplayAnimator(), HumanBodyBones.Hips, 1f);
            shoulderPivot.gameObject.SetActive(true);
            buttPivot.gameObject.SetActive(true);

            characterDescriptor.GetDisplayAnimator().gameObject.AddComponent<LookAtCursor>();
            OrbitCamera.AddConfiguration(cameraConfiguration);
        };
    }

    private void OnDisable() {
        OrbitCamera.RemoveConfiguration(cameraConfiguration);
    }

    void OnChangePlayerRoutine(int newValue = -1) {
        foreach (var info in playerPrefabDatabase.GetPrefabReferenceInfos()) {
            if (!info.IsValid() || info.GetKey() != playerSetting.GetPrefab()) continue;
            player = Instantiate(info.GetPrefab(), transform.position, transform.rotation);
            HandlePlayerSpawn(player);
            return;
        }

        foreach (var info in playerPrefabDatabase.GetPrefabReferenceInfos()) {
            if (!info.IsValid()) continue;
            player = Instantiate(info.GetPrefab(), transform.position, transform.rotation);
            HandlePlayerSpawn(player);
        }
    }
}
