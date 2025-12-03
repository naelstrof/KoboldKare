using System;
using System.Threading.Tasks;
using FishNet;
using FishNet.Managing;
using FishNet.Transporting.Multipass;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyInstanceDisplay : MonoBehaviour {
    [SerializeField] private TMP_Text lobbyNameText;
    [SerializeField] private TMP_Text lobbyStatus;
    [SerializeField] private Button joinButton;

    private CSteamID lobbyID;
    private string steamHost;

    public void SetLobby(CSteamID lobbyID) {
        this.lobbyID = lobbyID;
        lobbyNameText.text = SteamMatchmaking.GetLobbyData(lobbyID, "name");
        steamHost = SteamMatchmaking.GetLobbyOwner(lobbyID).ToString();
        var maxPlayers = SteamMatchmaking.GetLobbyMemberLimit(lobbyID);
        var currentPlayers = SteamMatchmaking.GetNumLobbyMembers(lobbyID);
        lobbyStatus.text = $"{currentPlayers}/{maxPlayers}";
    }

    private void OnEnable() {
        joinButton.onClick.AddListener(OnClick);
    }

    private void OnClick() {
        var networkManager = InstanceFinder.NetworkManager;
        var fishworks = networkManager.GetComponent<FishySteamworks.FishySteamworks>();
        fishworks.SetClientAddress(steamHost);
        networkManager.GetComponent<Multipass>().SetClientTransport(fishworks);
        networkManager.ClientManager.StartConnection();
    }

    private void OnDisable() {
        joinButton.onClick.RemoveListener(OnClick);
    }
}
