using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class CreateCustomGameButton : MonoBehaviour {
    private void Start() {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick() {
        GameManager.StartCoroutineStatic(LoadMultiplayer());
    }

    IEnumerator LoadMultiplayer() {
        GetComponent<Button>().interactable = false;
        var handle = MapSelector.PromptForMapSelect(true);
        yield return handle;
        if (handle.Cancelled) {
            GetComponent<Button>().interactable = true;
            yield break;
        }

        if (PhotonRoomListSpawner.GetBlackListed(handle.Result.roomName)) {
            GetComponent<Button>().interactable = true;
            PopupHandler.instance.SpawnPopup("InappropriateName");
            yield break;
        }
        NetworkManager.instance.SetSelectedMap(handle.Result.playableMap);
        yield return GameManager.instance.StartCoroutine(NetworkManager.instance.EnsureOnlineAndReadyToLoad());
        PhotonNetwork.CreateRoom(handle.Result.roomName, new RoomOptions { MaxPlayers = (byte)handle.Result.playerCount, IsVisible = !handle.Result.privateRoom, CleanupCacheOnLeave = false});
        GetComponent<Button>().interactable = true;
    }
}
