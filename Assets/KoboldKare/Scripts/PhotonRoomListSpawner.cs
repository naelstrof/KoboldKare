using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class PhotonRoomListSpawner : MonoBehaviourPunCallbacks, ILobbyCallbacks {
    public GameObject roomPrefab;
    public GameObject hideOnRoomsFound;
    private List<GameObject> roomPrefabs = new List<GameObject>();
    public List<RoomInfo> curRoomList;

    public override void OnConnectedToMaster(){
        base.OnConnectedToMaster();
        Debug.Log("PhotonRoomListSpawner :: Connected to master");
    }
    public override void OnLeftLobby(){
        ClearRoomList();
        Debug.Log("[PhotonRoomListSpawner] :: Player left lobby");
    }
    

    public override void OnRoomListUpdate(List<RoomInfo> roomList) {
        base.OnRoomListUpdate(roomList); // Perform default expected behavior
        Debug.Log("[PhotonRoomListSpawner] :: Got room list update from master server");
        hideOnRoomsFound.SetActive(true);
        ClearRoomList();
        //Build new list to refresh UI on
        foreach(RoomInfo info in roomList) {    
            if(info.RemovedFromList == true || info.IsOpen == false){
                ClearRoomFromList(info);
                continue;
            }
            else{
                if(!curRoomList.Contains(info)) //Only add it in if we don't have it already
                    curRoomList.Add(info);
            }
        }

        //Build UI from current room list
        foreach (RoomInfo info in curRoomList){
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


        if(roomList.Count > 0) //Only call once
            hideOnRoomsFound.SetActive(false);
    }
    private void ClearRoomFromList(RoomInfo room){
        curRoomList.Remove(room);
    }

    

    private void ClearRoomList(){
        foreach(GameObject g in roomPrefabs) {
            Destroy(g);
        }
        roomPrefabs.Clear();
        if(curRoomList != null && curRoomList.Count != 0) //Check if null or empty
            curRoomList.Clear();
    }
}
