using FishNet;
using FishNet.Broadcast;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Transporting;
using UnityEngine;

public class KoboldKareChatHandler : MonoBehaviour {
    private NetworkManager _networkManager;
    public static event System.Action<NetworkConnection, string> chatMessageReceived;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Init() {
        chatMessageReceived = null;
    }
    
    private void Awake() {
        InitializeOnce();
    }
    private void InitializeOnce() {
        _networkManager = GetComponentInParent<NetworkManager>();
        if (_networkManager == null) {
            _networkManager = InstanceFinder.NetworkManager;
        }

        if (_networkManager == null) {
            _networkManager.LogWarning($"PlayerSpawner on {gameObject.name} cannot work as NetworkManager wasn't found on this object or within parent objects.");
            return;
        }
        _networkManager.ServerManager.RegisterBroadcast<ChatBroadcast>(OnChatBroadcast);
        _networkManager.ClientManager.RegisterBroadcast<ChatBroadcast>(OnChatBroadcastClient);
    }

    private void OnChatBroadcastClient(ChatBroadcast broadcast, Channel channel) {
        CheatsProcessor.AppendText(broadcast.chatMessage);
    }

    public struct ChatBroadcast : IBroadcast {
        public string chatMessage;
    }

    private void OnChatBroadcast(NetworkConnection conn, ChatBroadcast data, Channel channel) {
        chatMessageReceived?.Invoke(conn, data.chatMessage);
        CheatsProcessor.ProcessCommand(conn, data.chatMessage);
        var koboldName = KoboldEntitySpawner.TryGetPlayerName(conn);
        _networkManager.ServerManager.Broadcast(new ChatBroadcast { chatMessage = $"{koboldName}: {data.chatMessage}\n" });
    }

}

