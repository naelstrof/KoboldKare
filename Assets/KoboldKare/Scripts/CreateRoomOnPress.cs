using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine.Localization;

public class CreateRoomOnPress : MonoBehaviour {
    public TMP_InputField roomNameField;
    public TMP_Dropdown saveDropdown;
    public Toggle isPrivate;
    public Slider maxPlayersField;
    public LocalizedString newGameString;
    public Sprite saveIcon;
    public void OnEnable() {
        if (newGameString == null || saveDropdown == null) {
            return;
        }
        //saveDropdown.options = new List<TMP_Dropdown.OptionData>();
        //saveDropdown.options.Add(new TMP_Dropdown.OptionData(newGameString.GetLocalizedString()));
        //SaveManager.SaveList list = SaveManager.GetSaveList(true);
        //foreach(string savename in list.fileNames) {
            //saveDropdown.options.Add(new TMP_Dropdown.OptionData(savename, saveIcon));
        //}
    }
    public IEnumerator CreateRoomRoutine() {
        //if (saveDropdown.value == 0) {
            yield return GameManager.instance.StartCoroutine(NetworkManager.instance.EnsureOnlineAndReadyToLoad());
            PhotonNetwork.CreateRoom(roomNameField.text, new RoomOptions { MaxPlayers = (byte)maxPlayersField.value, IsVisible = !isPrivate.isOn });
        //} else {
            //SaveManager.SaveList list = SaveManager.GetSaveList(false);
            //SaveManager.Load(list.fileNames[saveDropdown.value - 1], true, (int)maxPlayersField.value, roomNameField.text, !isPrivate.isOn);
        //}
    }
    public void CreateRoom() {
        GameManager.instance.StartCoroutine(CreateRoomRoutine());
    }
    public IEnumerator JoinRoomRoutine(string roomName) {
        yield return GameManager.instance.StartCoroutine(NetworkManager.instance.EnsureOnlineAndReadyToLoad());
        PhotonNetwork.JoinRoom(roomName);
    }
    public void JoinRoom() {
        GameManager.instance.StartCoroutine(JoinRoomRoutine(roomNameField.text));
    }
}
