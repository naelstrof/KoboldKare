using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Photon.Pun;
using UnityEditor;
using UnityEngine;
using UnityScriptableSettings;

public class KoboldCustomizerSpawner : MonoBehaviour {
    [SerializeField] private PrefabSelectSingleSetting playerSetting; 
    [SerializeField] private PrefabDatabase playerPrefabDatabase;

    private GameObject player;
    private bool waiting = false;
    

    void Start() {
        ModManager.AddFinishedLoadingListener(FinishedLoading);
        if (ModManager.GetReady()) {
            FinishedLoading();
        }
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
            Destroy(player);
        }
        yield return new WaitUntil(ModManager.GetReady);
        OnChangePlayerRoutine();
    }


    void OnChangePlayerRoutine(int newValue = -1) {
        foreach (var info in playerPrefabDatabase.GetPrefabReferenceInfos()) {
            if (!info.IsValid() || info.GetKey() != playerSetting.GetPrefab()) continue;
            player = Instantiate(info.GetPrefab(), transform.position, transform.rotation);
            player.AddComponent<PlayerKoboldLoader>();
            //player.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX |
                                                           //RigidbodyConstraints.FreezePositionZ |
                                                           //RigidbodyConstraints.FreezeRotation;
            player.GetComponent<CharacterDescriptor>().GetDisplayAnimator().gameObject.AddComponent<LookAtCursor>();
            break;
        }
    }
}
