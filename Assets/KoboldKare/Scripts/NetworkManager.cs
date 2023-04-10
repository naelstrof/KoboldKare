using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.SceneManagement;
using System;
using NetStack.Serialization;
using SimpleJSON;
using Steamworks;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "NewNetworkManager", menuName = "Data/NetworkManager", order = 1)]
public class NetworkManager : SingletonScriptableObject<NetworkManager>, IConnectionCallbacks, IMatchmakingCallbacks, IInRoomCallbacks, ILobbyCallbacks, IWebRpcCallback, IErrorInfoCallback, IPunOwnershipCallbacks, IOnEventCallback {
    [SerializeField]
    private PlayableMap selectedMap;
    public PrefabSelectSingleSetting selectedPlayerPrefab;
    public ServerSettings settings;

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

    private GenericAction onLeaveRoom;
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
            if (Application.isEditor && !settings.AppSettings.AppVersion.Contains("Editor")) {
                settings.AppSettings.AppVersion += "Editor";
            }
            if (Application.isEditor && PhotonNetwork.GameVersion != null && !PhotonNetwork.GameVersion.Contains("Editor")) {
                PhotonNetwork.GameVersion += "Editor";
            }
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
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 8, CleanupCacheOnLeave = false });
    }
    public void JoinMatch(string roomName) {
        GameManager.instance.StartCoroutine(JoinMatchRoutine(roomName));
    }
    public IEnumerator JoinMatchRoutine(string roomName) {
        PopupHandler.instance.SpawnPopup("Connect");
        yield return GameManager.instance.StartCoroutine(EnsureOnlineAndReadyToLoad());
        PhotonNetwork.JoinRoom(roomName);
    }
    public IEnumerator EnsureOfflineAndReadyToLoad() {
        if (Application.isEditor && !settings.AppSettings.AppVersion.Contains("Editor")) {
            settings.AppSettings.AppVersion += "Editor";
        }
        if (Application.isEditor && PhotonNetwork.GameVersion != null && !PhotonNetwork.GameVersion.Contains("Editor")) {
            PhotonNetwork.GameVersion += "Editor";
        }
        PhotonNetwork.AutomaticallySyncScene = true;
        PrefabDatabaseDatabase.SavePlayerConfig();
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
        if (Application.isEditor && !settings.AppSettings.AppVersion.Contains("Editor")) {
            settings.AppSettings.AppVersion += "Editor";
        }
        if (Application.isEditor && PhotonNetwork.GameVersion != null && !PhotonNetwork.GameVersion.Contains("Editor")) {
            PhotonNetwork.GameVersion += "Editor";
        }
        if (PhotonNetwork.InRoom && shouldLeaveRoom) {
            PhotonNetwork.LeaveRoom();
            yield return LevelLoader.instance.LoadLevel("ErrorScene");
        }

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.OfflineMode = false;
        PrefabDatabaseDatabase.SavePlayerConfig();
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

    public void SetSelectedMap(PlayableMap map) {
        selectedMap = map;
    }
    public PlayableMap GetSelectedMap() {
        return selectedMap;
    }

    public void StartSinglePlayer() {
        GameManager.instance.StartCoroutine(SinglePlayerRoutine());
    }
    public IEnumerator SinglePlayerRoutine() {
        yield return GameManager.instance.StartCoroutine(EnsureOfflineAndReadyToLoad());
        PhotonNetwork.OfflineMode = true;
        PhotonNetwork.JoinRandomRoom();
        yield return null;
        yield return new WaitUntil(() => !LevelLoader.loadingLevel);
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
    }
    public void SpawnControllablePlayer() {
        GameManager.instance.StartCoroutine(SpawnControllablePlayerRoutine());
    }
    void IMatchmakingCallbacks.OnJoinedRoom() {
        Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
        SpawnControllablePlayer();
        PopupHandler.instance.ClearAllPopups();
        GameManager.instance.Pause(false);
        //if (popup != null) {
        //popup.Hide();
        //}
        //localPlayerInstance = GameObject.Instantiate(saveLibrary.GetPrefab(ScriptableSaveLibrary.SaveID.Kobold), Vector3.zero, Quaternion.identity);
        //localPlayerInstance.GetComponent<ISavable>().SpawnOverNetwork();
    }
    public void OnPlayerEnteredRoom(Player other) {
        Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting
        if (PhotonNetwork.IsMasterClient) {// Raise a handshake event to the joining player.
            JSONNode rootNode = JSONNode.Parse("{}");
            JSONArray modArray = new JSONArray();
            foreach (var mod in ModManager.GetLoadedMods()) {
                JSONNode modNode = JSONNode.Parse("{}");
                modNode["title"] = mod.title;
                modNode["id"] = mod.id.ToString();
                modArray.Add(modNode);
            }

            rootNode["modList"] = modArray;
            rootNode["config"] = PrefabDatabase.GetJsonConfiguration().ToString();
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { CachingOption = EventCaching.DoNotCache, TargetActors = new []{other.ActorNumber}};
            PhotonNetwork.RaiseEvent((byte)'M', rootNode.ToString(), raiseEventOptions, SendOptions.SendReliable);
        }
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
        if (SceneManager.GetActiveScene().name != selectedMap.unityScene.GetName()) {
            LevelLoader.instance.LoadLevel((string)selectedMap.unityScene.RuntimeKey);
        }
    }

    public void OnLeftRoom() {
        onLeaveRoom?.Invoke();
        Debug.Log("Left room");
    }
    public void OnMasterClientSwitched(Player newMasterClient) {
        Debug.Log("Master switched!" + newMasterClient);
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

    public void OnEvent(EventData photonEvent) {
        if (photonEvent.Code != 'M') {
            return;
        }

        //if (photonEvent.Sender != 0) return; // Only accept this event from a server.
        
        string data = (string)photonEvent.CustomData;
        Debug.Log($"Received mod handshake from {photonEvent.Sender}! {data}");
        JSONNode rootNode = JSONNode.Parse(data);
        List<ModManager.ModStub> desiredMods = new List<ModManager.ModStub>();
        foreach (var node in rootNode["modList"].AsArray) {
            ulong.TryParse(node.Value["id"], out ulong result);
            desiredMods.Add(new ModManager.ModStub(node.Value["title"], (PublishedFileId_t)result, ModManager.ModSource.Any));
        }

        if (ModManager.HasModsLoaded(desiredMods)) {
            PrefabDatabaseDatabase.LoadPlayerConfig(rootNode["config"]);
        } else {
            string roomName = PhotonNetwork.CurrentRoom.Name;
            onLeaveRoom = () => {
                GameManager.StartCoroutineStatic(FinishLoadMods(desiredMods, roomName, rootNode));
            };
            PhotonNetwork.LeaveRoom();
        }
    }

    private IEnumerator FinishLoadMods(List<ModManager.ModStub> desiredMods, string roomName, JSONNode rootNode) {
        yield return ModManager.SetLoadedMods(desiredMods);
        PrefabDatabaseDatabase.LoadPlayerConfig(rootNode["config"]);
        yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady);
        PhotonNetwork.JoinRoom(roomName);
        onLeaveRoom = () => {};
    }
}
