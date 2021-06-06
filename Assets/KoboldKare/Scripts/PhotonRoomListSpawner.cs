using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PhotonRoomListSpawner : MonoBehaviourPunCallbacks, ILobbyCallbacks {
    public GameObject roomPrefab;
    public GameObject hideOnRoomsFound;
    private List<GameObject> roomPrefabs = new List<GameObject>();
    public override void OnRoomListUpdate(List<RoomInfo> roomList) {
        hideOnRoomsFound.SetActive(true);
        foreach(GameObject g in roomPrefabs) {
            Destroy(g);
        }
        roomPrefabs.Clear();
        foreach(RoomInfo info in roomList) {
            hideOnRoomsFound.SetActive(false);
            GameObject room = GameObject.Instantiate(roomPrefab, this.transform);
            roomPrefabs.Add(room);
            room.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = info.Name;
            room.transform.Find("Info").GetComponent<TextMeshProUGUI>().text = info.PlayerCount + "/" + info.MaxPlayers;
            if (info.IsOpen) {
                room.transform.Find("Image").gameObject.SetActive(false);
            }
            room.GetComponent<Button>().onClick.AddListener(() => {
                NetworkManager.instance.JoinMatch(info.Name);
            });
        }
    }
}
