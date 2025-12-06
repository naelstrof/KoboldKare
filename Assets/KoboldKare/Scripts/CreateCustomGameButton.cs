using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using FishNet;
using FishNet.Managing.Scened;
using FishNet.Transporting.Multipass;
using FishNet.Transporting.Tugboat;
using SimpleJSON;
using Steamworks;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CreateCustomGameButton : MonoBehaviour {
    private void Start() {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick() {
        _ = LoadMultiplayer();
    }

    private async Task LoadMultiplayer() {
        GetComponent<Button>().interactable = false;
        var handle = MapSelector.PromptForMapSelect(true).AsTask();
        await handle;
        if (handle.IsCanceled) {
            GetComponent<Button>().interactable = true;
            return;
        }
        if (GameManager.GetBlackListed(handle.Result.roomName, out var filtered)) {
            GetComponent<Button>().interactable = true;
            if (!Analytics.playerOptedOut) {
                UriBuilder builder = new UriBuilder("http://koboldkare.com/analytics.php");
                builder.Query += $"query={Uri.EscapeDataString(handle.Result.roomName)}";
                builder.Query += $"&filtered={Uri.EscapeDataString(filtered)}";
                var req = UnityWebRequest.Get(builder.ToString());
                var asyncreq = req.SendWebRequest();
                asyncreq.completed += (a) => { Debug.Log(req.result); };
            }

            PopupHandler.instance.SpawnPopup("InappropriateName");
            return;
        } else {
            if (!Analytics.playerOptedOut) {
                UriBuilder builder = new UriBuilder("http://koboldkare.com/analytics.php");
                builder.Query += $"query={Uri.EscapeDataString(handle.Result.roomName)}";
                builder.Query += $"&filtered={Uri.EscapeDataString(filtered)}";
                Debug.Log(builder.ToString());
                var req = UnityWebRequest.Get(builder.ToString());
                var asyncreq = req.SendWebRequest();
                asyncreq.completed += (a) => { Debug.Log(req.result); };
            }
        }
        MainMenu.ShowMenuStatic(MainMenu.MainMenuMode.Loading);
        
        var lobbyType = handle.Result.lobbyType;
        var lobbyCreateResult = await SteamMatchmaking.CreateLobby(lobbyType, handle.Result.playerCount).AsTask<LobbyCreated_t>();
    
        if (lobbyCreateResult.m_eResult != EResult.k_EResultOK) {
            Debug.LogError($"Failed to create lobby: {lobbyCreateResult.m_eResult}");
            // FIXME FISHNET needs popup
            MainMenu.ShowMenuStatic(MainMenu.MainMenuMode.Multiplayer);
            return;
        }
        
        var lobbyId = new CSteamID(lobbyCreateResult.m_ulSteamIDLobby);
        SteamMatchmaking.SetLobbyData(lobbyId, "name", handle.Result.roomName);
        SteamMatchmaking.SetLobbyMemberLimit(lobbyId, handle.Result.playerCount);
        List<ModManager.ModStub> stubs = new(ModManager.GetModsWithLoadedAssets());
        if (handle.Result.playableMap.stub.HasValue) {
            stubs.Add(handle.Result.playableMap.stub.Value);
        }
        
        // FIXME FISHNET
        //SteamMatchmaking.SetLobbyData(lobbyId, "mods", modArray.ToString());
        var networkManager = InstanceFinder.NetworkManager;
        networkManager.ServerManager.StartConnection();
        
        networkManager.GetComponent<Multipass>().SetClientTransport(networkManager.GetComponent<Tugboat>());
        networkManager.ClientManager.StartConnection(); 
        
        KoboldKareSceneProcessor.LoadSceneGlobal(handle.Result.playableMap.GetKey(), stubs);
        
        GetComponent<Button>().interactable = true;
    }
}
