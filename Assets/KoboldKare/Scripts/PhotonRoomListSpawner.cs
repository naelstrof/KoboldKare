using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Linq;

public class PhotonRoomListSpawner : MonoBehaviourPunCallbacks, ILobbyCallbacks, IInRoomCallbacks {
    public GameObject roomPrefab;
    public GameObject hideOnRoomsFound;
    private List<GameObject> roomPrefabs = new List<GameObject>();
    public override void OnConnectedToMaster(){
        base.OnConnectedToMaster();
        Debug.Log("PhotonRoomListSpawner :: Connected to master");
    }
    public override void OnLeftLobby(){
        ClearRoomList();
        Debug.Log("[PhotonRoomListSpawner] :: Player left lobby");
    }

    private IEnumerator RefreshRoomRoutine() {
        while (isActiveAndEnabled) {
            if (!PhotonNetwork.InLobby && PhotonNetwork.IsConnectedAndReady) {
                PhotonNetwork.JoinLobby();
            }
            yield return new WaitForSeconds(1f);
        }
    }

    public override void OnEnable() {
        base.OnEnable();
        StartCoroutine(RefreshRoomRoutine());
    }

    private void SetupRoom(GameObject room, RoomInfo info) {
        room.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = info.Name;
        room.transform.Find("Info").GetComponent<TextMeshProUGUI>().text = info.PlayerCount + "/" + info.MaxPlayers;
        if (info.IsOpen) {
            room.transform.Find("Image").gameObject.SetActive(false);
        }

        room.GetComponent<Button>().onClick.RemoveAllListeners();
        room.GetComponent<Button>().onClick.AddListener(() => {
            NetworkManager.instance.JoinMatch(info.Name);
        });
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList) {
        base.OnRoomListUpdate(roomList); // Perform default expected behavior
        //Debug.Log("[PhotonRoomListSpawner] :: Got room list update from master server");
        //ClearRoomList();
        //Build UI from current room list
        foreach (RoomInfo info in roomList) {
            bool skip = false;
            for (int i = 0; i < roomPrefabs.Count; i++) {
                if (roomPrefabs[i].transform.Find("Name").GetComponent<TextMeshProUGUI>().text == info.Name) {
                    if (info.RemovedFromList) {
                        Destroy(roomPrefabs[i]);
                        roomPrefabs.RemoveAt(i--);
                    } else {
                        SetupRoom(roomPrefabs[i], info);
                    }
                    skip = true;
                }
            }
            
            if (info.RemovedFromList || skip) {
                continue;
            }

            GameObject room = Instantiate(roomPrefab, transform);
            roomPrefabs.Add(room);
            SetupRoom(room, info);
        }
        hideOnRoomsFound.SetActive(roomList.Count == 0);
    }

    private void ClearRoomList(){
        foreach(GameObject g in roomPrefabs) {
            Destroy(g);
        }
        roomPrefabs.Clear();
    }

    public override void OnDisconnected(DisconnectCause cause) {
        base.OnDisconnected(cause);
        ClearRoomList();
    }
}
