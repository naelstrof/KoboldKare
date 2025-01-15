using System;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Networking;
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

        if (PhotonRoomListSpawner.GetBlackListed(handle.Result.roomName, out var filtered)) {
            GetComponent<Button>().interactable = true;
            if (!Analytics.playerOptedOut) {
                UriBuilder builder = new UriBuilder("http://koboldkare.com/analytics.php");
                builder.Query += $"query={Uri.EscapeDataString(handle.Result.roomName)}";
                builder.Query += $"&filtered={Uri.EscapeDataString(filtered)}";
                var req = UnityWebRequest.Get(builder.ToString());
                var asyncreq = req.SendWebRequest();
                asyncreq.completed += (a) => {
                    Debug.Log(req.result);
                };
            }
            PopupHandler.instance.SpawnPopup("InappropriateName");
            yield break;
        } else {
            if (!Analytics.playerOptedOut) {
                UriBuilder builder = new UriBuilder("http://koboldkare.com/analytics.php");
                builder.Query += $"query={Uri.EscapeDataString(handle.Result.roomName)}";
                builder.Query += $"&filtered={Uri.EscapeDataString(filtered)}";
                Debug.Log(builder.ToString());
                var req = UnityWebRequest.Get(builder.ToString());
                var asyncreq = req.SendWebRequest();
                asyncreq.completed += (a) => {
                    Debug.Log(req.result);
                };
            }
        }
        NetworkManager.instance.SetSelectedMap(handle.Result.playableMap);
        yield return GameManager.instance.StartCoroutine(NetworkManager.instance.EnsureOnlineAndReadyToLoad());
        PhotonNetwork.CreateRoom(handle.Result.roomName, new RoomOptions { MaxPlayers = (byte)handle.Result.playerCount, IsVisible = !handle.Result.privateRoom, CleanupCacheOnLeave = false});
        GetComponent<Button>().interactable = true;
    }
}
