using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.SceneManagement;
using NetStack.Serialization;
using SimpleJSON;
using Steamworks;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "NewNetworkManager", menuName = "Data/NetworkManager", order = 1)]
public class NetworkManager : SingletonScriptableObject<NetworkManager>, IConnectionCallbacks, IMatchmakingCallbacks, IInRoomCallbacks, ILobbyCallbacks, IWebRpcCallback, IErrorInfoCallback, IPunOwnershipCallbacks, IOnEventCallback {
    private string selectedMap;
    public PrefabSelectSingleSetting selectedPlayerPrefab;
    public ServerSettings settings;
    
    public static byte CustomInstantiationEvent = (byte)'C';
    public static byte CustomCheatEvent = (byte)'H';
    public static byte CustomChatEvent = (byte)'A';

    public bool online {
        get {
            return PhotonNetwork.OfflineMode != true && PhotonNetwork.PlayerList.Length > 1;
        }
    }
    public bool offline {
        get {
            return !online;
        }
    }

    private delegate void GenericAction();

    public IEnumerator JoinLobbyRoutine(string region) {
        if (PhotonNetwork.OfflineMode) {
            PhotonNetwork.OfflineMode = false;
        }
        if (PhotonNetwork.IsConnected && settings.AppSettings.FixedRegion != region) {
            PhotonNetwork.Disconnect();
            yield return new WaitUntil(()=>!PhotonNetwork.IsConnected);
        }
        if (!PhotonNetwork.IsConnected) {
            PhotonNetwork.AutomaticallySyncScene = true;
            settings.AppSettings.FixedRegion = region;
            /*if (Application.isEditor && !settings.AppSettings.AppVersion.Contains("Editor")) {
                settings.AppSettings.AppVersion += "Editor";
            }
            if (Application.isEditor && PhotonNetwork.GameVersion != null && !PhotonNetwork.GameVersion.Contains("Editor")) {
                PhotonNetwork.GameVersion += "Editor";
            }*/
            PhotonNetwork.ConnectUsingSettings();
        }
        yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady || (PhotonNetwork.IsConnected && PhotonNetwork.InRoom));
        if (!PhotonNetwork.InLobby) {
            PhotonNetwork.JoinLobby();
        }
    }
    public void JoinLobby(string region) {
        GameManager.instance.StartCoroutine(JoinLobbyRoutine(region));
    }
    public void QuickMatch() {
        GameManager.instance.StartCoroutine(QuickMatchRoutine());
    }
    public IEnumerator QuickMatchRoutine() {
        PopupHandler.instance.SpawnPopup("Connect");
        yield return GameManager.instance.StartCoroutine(EnsureOnlineAndReadyToLoad());
        PhotonNetwork.JoinRandomRoom();
    }
    public void CreatePublicRoom() {
        GameManager.instance.StartCoroutine(CreatePublicRoomRoutine());
    }
    public IEnumerator CreatePublicRoomRoutine() {
        PopupHandler.instance.SpawnPopup("Connect");
        yield return GameManager.instance.StartCoroutine(EnsureOnlineAndReadyToLoad());
        var boxedSceneLoad = MapLoadingInterop.RequestMapLoad(selectedMap);
        yield return new WaitUntil(()=>boxedSceneLoad.IsDone);
        JSONArray modArray = new JSONArray();
        foreach (var mod in ModManager.GetModsWithLoadedAssets()) {
            JSONNode modNode = JSONNode.Parse("{}");
            modNode["title"] = mod.title;
            modNode["folderTitle"] = mod.folderTitle;
            modNode["id"] = mod.id.ToString();
            modArray.Add(modNode);
        }
        var modOptions = new Hashtable {
            ["modList"] = modArray.ToString()
        };
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 8, CleanupCacheOnLeave = false, CustomRoomProperties = modOptions});
    }

    private bool TryParseMods(Hashtable hashtable, out List<ModManager.ModStub> stubs) {
        if (hashtable.ContainsKey("modList")) {
            if (hashtable["modList"] is not string) {
                stubs = new();
                return false;
            }

            string modList = (string)hashtable["modList"];
            JSONNode modArray = JSONNode.Parse(modList);
            List<ModManager.ModStub> modsToLoad = new List<ModManager.ModStub>();
            foreach (var pair in modArray) {
                var node = pair.Value;
                if (!node.HasKey("id") || !node.HasKey("folderTitle") || !node.HasKey("title")) {
                    stubs = new();
                    return false;
                }

                if (!ulong.TryParse(node["id"], out ulong parsedID)) {
                    continue;
                }

                modsToLoad.Add(new ModManager.ModStub((string)node["title"], (PublishedFileId_t)parsedID,
                    ModManager.ModSource.Any, node["folderTitle"]));
            }
            stubs = modsToLoad;
            return true;
        }

        stubs = new();
        return false;
    } 
    public void JoinMatch(RoomInfo roomInfo) {
        MainMenu.ShowMenuStatic(MainMenu.MainMenuMode.Loading);
        PhotonNetwork.JoinRoom(roomInfo.Name);
    }
    private IEnumerator JoinMatchRoutine(string roomName, List<ModManager.ModStub> modsToLoad) {
        MainMenu.ShowMenuStatic(MainMenu.MainMenuMode.Loading);
        PopupHandler.instance.SpawnPopup("Connect");
        try {
            MainMenu.ShowMenuStatic(MainMenu.MainMenuMode.Loading);
            yield return GameManager.instance.StartCoroutine(ModManager.SetLoadedMods(modsToLoad));
        } finally {
            if (ModManager.GetFailedToLoadMods()) {
                MainMenu.ShowMenuStatic(MainMenu.MainMenuMode.MainMenu);
                PopupHandler.instance.ClearAllPopups();
                PopupHandler.instance.SpawnPopup("Disconnect", true, default,
                    "Failed to download mods set by the server.");
            }
        }

        if (!ModManager.GetFailedToLoadMods()) {
            yield return EnsureOnlineAndReadyToLoad();
            PhotonNetwork.JoinRoom(roomName);
        }
    }

    private IEnumerator PhotonDisconnectCompletely() {
        if (PhotonNetwork.InRoom) {
            PhotonNetwork.LeaveRoom();
        }
        if (PhotonNetwork.InLobby) {
            PhotonNetwork.LeaveLobby();
        }
        if (PhotonNetwork.IsConnected) {
            PhotonNetwork.Disconnect();
        }
        yield return new WaitUntil(() => PhotonNetwork.NetworkClientState != ClientState.Leaving && !PhotonNetwork.IsConnected);
    }
    private IEnumerator EnsureOfflineAndReadyToLoad() {
        /*if (Application.isEditor && !settings.AppSettings.AppVersion.Contains("Editor")) {
            settings.AppSettings.AppVersion += "Editor";
        }
        if (Application.isEditor && PhotonNetwork.GameVersion != null && !PhotonNetwork.GameVersion.Contains("Editor")) {
            PhotonNetwork.GameVersion += "Editor";
        }*/
        PhotonNetwork.IsMessageQueueRunning = false;
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonPeer.RegisterType(typeof(BitBuffer), (byte)'B', BufferPool.SerializeBitBuffer, BufferPool.DeserializeBitBuffer);
        if (PhotonNetwork.InRoom) {
            PhotonNetwork.LeaveRoom();
        }
        if (PhotonNetwork.InLobby) {
            PhotonNetwork.LeaveLobby();
        }
        PhotonNetwork.Disconnect();
        yield return new WaitUntil(() => !PhotonNetwork.IsConnected);
        PhotonNetwork.OfflineMode = true;
        PhotonNetwork.EnableCloseConnection = true;
    }
    public IEnumerator EnsureOnlineAndReadyToLoad(bool shouldLeaveRoom = true) {
        /*if (Application.isEditor && !settings.AppSettings.AppVersion.Contains("Editor")) {
            settings.AppSettings.AppVersion += "Editor";
        }
        if (Application.isEditor && PhotonNetwork.GameVersion != null && !PhotonNetwork.GameVersion.Contains("Editor")) {
            PhotonNetwork.GameVersion += "Editor";
        }*/
        if (PhotonNetwork.InRoom && shouldLeaveRoom) {
            PhotonNetwork.LeaveRoom();
            yield return LevelLoader.instance.LoadLevel("ErrorScene");
        }

        PhotonNetwork.IsMessageQueueRunning = true;
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.OfflineMode = false;
        PhotonPeer.RegisterType(typeof(BitBuffer), (byte)'B', BufferPool.SerializeBitBuffer, BufferPool.DeserializeBitBuffer);
        if (!PhotonNetwork.IsConnectedAndReady) {
            PhotonNetwork.ConnectUsingSettings();
        }
        yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady);
        
        if (!PhotonNetwork.InLobby) {
            PhotonNetwork.JoinLobby();
        }
        
        yield return new WaitUntil(()=>PhotonNetwork.NetworkClientState == ClientState.JoinedLobby);

        PhotonNetwork.EnableCloseConnection = true;
    }

    public void SetSelectedMap(string mapName) {
        selectedMap = mapName;
    }
    public string GetSelectedMap() {
        return selectedMap;
    }

    public void StartSinglePlayer() {
        GameManager.instance.StartCoroutine(SinglePlayerRoutine());
    }
    public IEnumerator SinglePlayerRoutine() {
        yield return GameManager.instance.StartCoroutine(EnsureOfflineAndReadyToLoad());
        var boxedSceneLoad = MapLoadingInterop.RequestMapLoad(selectedMap);
        yield return new WaitUntil(()=>boxedSceneLoad.IsDone);
        PhotonNetwork.OfflineMode = true;
        PhotonNetwork.JoinRandomRoom();
    }

    public void LeaveLobby() {
        if (PhotonNetwork.InLobby) {
            PhotonNetwork.LeaveLobby();
        }
    }
    public void OnConnectedToMaster() {
        Debug.Log("OnConnectedToMaster() was called by PUN.");
        Debug.Log("Using version " + PhotonNetwork.NetworkingClient.AppVersion);
    }
    public void OnDisconnected(DisconnectCause cause) {
        Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
        if (GameManager.instance != null) {
            GameManager.instance.StartCoroutine(OnDisconnectRoutine(cause));
        }
    }

    private IEnumerator OnDisconnectRoutine(DisconnectCause cause) {
        if (cause == DisconnectCause.DisconnectByClientLogic || cause == DisconnectCause.None) yield break;
        PopupHandler.instance.ClearAllPopups();
        yield return LevelLoader.instance.LoadLevel("MainMenu");
        PopupHandler.instance.SpawnPopup("Disconnect", true, default, cause.ToString());
    }

    private IEnumerator OnJoinRoomFailedRoutine(short returnCode, string message) {
        yield return GameManager.instance.StartCoroutine(EnsureOnlineAndReadyToLoad());
        PopupHandler.instance.ClearAllPopups();
        yield return LevelLoader.instance.LoadLevel("MainMenu");
        PopupHandler.instance.SpawnPopup("Disconnect", true, default, "Error " + returnCode + ": " + message);
    }

    public void TriggerDisconnect() {
        OnDisconnected(DisconnectCause.DisconnectByDisconnectMessage);
    }

    public void OnJoinRoomFailed(short returnCode, string message) {
        Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRoomFailed() was called by PUN." + message);
        GameManager.instance.StartCoroutine(OnJoinRoomFailedRoutine(returnCode, message));
        //GameManager.instance.LoadLevel("ErrorScene");
        //PhotonNetwork.CreateRoom("asdfasdfasdfasdfasdfasdf", new RoomOptions{MaxPlayers = maxPlayersPerRoom});
        //PhotonNetwork.CreateRoom(null, new RoomOptions{MaxPlayers = maxPlayers});
    }
    public void OnJoinRandomFailed(short returnCode, string message) {
        Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.");
        GameManager.instance.StartCoroutine(CreatePublicRoomRoutine());
        //if (popup != null) {
        //popup.Hide();
        //}
    }
    public IEnumerator SpawnControllablePlayerRoutine() {
        yield return new WaitUntil(() => !LevelLoader.loadingLevel && ModManager.GetFinishedLoading());
        if (PhotonNetwork.NetworkClientState != ClientState.Joined) {
            yield break;
        }
        // If our kobold exists, don't spawn another
        if (PhotonNetwork.LocalPlayer.TagObject != null && (PhotonNetwork.LocalPlayer.TagObject as Kobold) != null) {
            yield break;
        }
        
        
        BitBuffer playerData = new BitBuffer(16);
        playerData.AddKoboldGenes(PlayerKoboldLoader.GetPlayerGenes());
        playerData.AddBool(true);// Is player kobold

        SceneDescriptor.GetSpawnLocationAndRotation(out Vector3 pos, out Quaternion rot);
        Debug.Log($"Spawned player at {pos}");
        GameObject player = PhotonNetwork.Instantiate(selectedPlayerPrefab.GetPrefab(), pos, Quaternion.identity, 0, new object[]{playerData});
        player.GetComponentInChildren<CharacterDescriptor>(true).SetEyeDir(rot*Vector3.forward);
        PopupHandler.instance.ClearAllPopups();
        MainMenu.ShowMenuStatic(MainMenu.MainMenuMode.None);
        Pauser.SetPaused(false);
    }
    public void SpawnControllablePlayer() {
        GameManager.instance.StartCoroutine(SpawnControllablePlayerRoutine());
    }
    void IMatchmakingCallbacks.OnJoinedRoom() {
        Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
        GameManager.StartCoroutineStatic(HandleModListChange(PhotonNetwork.CurrentRoom.CustomProperties));
        SpawnControllablePlayer();
    }
    
    public void OnPlayerEnteredRoom(Player other) {
        Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting
    }

    public void OnPlayerLeftRoom(Player other) {
        Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects
        if (PhotonNetwork.IsMasterClient) {
            Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
        }
    }
    public void OnConnected() {
        Debug.Log("Connected.");
    }

    public void OnCreatedRoom() {
        cheatsEnabled = false;
        //GameManager.instance.StartCoroutine(WaitForLevelToLoadThenSetModOptions());
    }


    public void OnLeftRoom() {
        Debug.Log("Left room");
    }
    public void OnMasterClientSwitched(Player newMasterClient) {
        Debug.Log("Master switched!" + newMasterClient);
        //GameManager.instance.StartCoroutine(WaitForLevelToLoadThenSetModOptions());
    }

    public void OnJoinedLobby() {
        Debug.Log("Joined lobby");
    }

    public void OnLeftLobby() {
        Debug.Log("Left lobby i guess");
    }

    public void OnRegionListReceived(RegionHandler regionHandler) {
    }

    public void OnRoomListUpdate(List<RoomInfo> roomList) {
        foreach (RoomInfo i in roomList) {
            Debug.Log("Got room info list:" + i);
        }
    }
    public void OnFriendListUpdate(List<FriendInfo> friendList) {
        Debug.Log("Friends update:" + friendList);
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data) {
        Debug.Log("Custom auth i guess" + data);
    }

    public void OnCustomAuthenticationFailed(string debugMessage) {
        Debug.Log("Custom auth failed" + debugMessage);
    }

    public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics) {
        Debug.Log("lobby update " + lobbyStatistics);
    }

    public void OnErrorInfo(ErrorInfo errorInfo) {
        Debug.Log("Photon error: " + errorInfo);
    }

    public void OnCreateRoomFailed(short returnCode, string message) {
    }

    public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged) {
        GameManager.StartCoroutineStatic(HandleModListChange(propertiesThatChanged));
    }

    IEnumerator HandleModListChange(Hashtable propertiesThatChanged) {
        if (TryParseMods(propertiesThatChanged, out var stubs)) {
            if (ModManager.HasExactModConfigurationLoaded(stubs)) {
                Debug.Log("Got new mods from server, but we have the exact same configuration loaded already! Woo!");
            } else {
                MainMenu.ShowMenuStatic(MainMenu.MainMenuMode.Loading);
                var roomName = PhotonNetwork.CurrentRoom.Name;
                yield return PhotonDisconnectCompletely();
                yield return LevelLoader.instance.LoadLevel("MainMenu");
                GameManager.instance.StartCoroutine(JoinMatchRoutine(roomName, stubs));
            }
        }
    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps) {
    }
    public void OnWebRpcResponse(OperationResponse response) {
    }

    public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer) {
        Kobold k = targetView.GetComponent<Kobold>();
        if (k != (Kobold)PhotonNetwork.LocalPlayer.TagObject) {
            targetView.TransferOwnership(requestingPlayer);
        } else {
            if (!k.GetComponentInChildren<PlayerInput>().actions["Jump"].IsPressed() || ReferenceEquals(requestingPlayer, PhotonNetwork.LocalPlayer)) {
                targetView.TransferOwnership(requestingPlayer);
            }
        }
    }

    public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner) {
    }

    public void OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest) {
    }

    private bool cheatsEnabled = false;
    public bool GetCheatsEnabled() => cheatsEnabled;

    public void OnEvent(EventData photonEvent) {
        if (photonEvent.Code == CustomInstantiationEvent) {
            object[] objectData = (object[])photonEvent.CustomData;
            var existingView = PhotonNetwork.GetPhotonView((int)objectData[1]);
            if (existingView != null) {
                existingView.ViewID = 0;
                Destroy(existingView.gameObject);
            }

            GameObject obj = PhotonNetwork.PrefabPool.Instantiate((string)objectData[0], Vector3.zero, Quaternion.identity);
            var photonView = obj.GetComponent<PhotonView>();
            photonView.ViewID = (int)objectData[1];
            obj.SetActive(true);
            return;
        }

        if (photonEvent.Code == CustomChatEvent) {
            var player = PhotonNetwork.CurrentRoom.GetPlayer(photonEvent.Sender, true);
            var chatKobold = (Kobold)player.TagObject;
            var message = (string)photonEvent.CustomData;
            CheatsProcessor.AppendText($"{player.NickName}: {message}\n");
            if (chatKobold != null) {
                var chatter = chatKobold.GetComponent<Chatter>();
                chatter.DisplayMessage((string)photonEvent.CustomData, 1f);
                if (Equals(player, PhotonNetwork.LocalPlayer)) {
                    CheatsProcessor.ProcessCommand(chatKobold, message);
                }
            }
            return;
        }

        if (photonEvent.Code == CustomCheatEvent) {
            cheatsEnabled = (bool)photonEvent.CustomData;
            return;
        }

        if (photonEvent.Code == 203) {
            TriggerDisconnect();
        }
    }
}
