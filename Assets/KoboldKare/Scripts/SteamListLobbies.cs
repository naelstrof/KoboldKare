using System.Threading.Tasks;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SteamListLobbies : MonoBehaviour {
    [SerializeField] private GameObject lobbyPrefab;
    [SerializeField] private GameObject noLobbiesFound;
    [SerializeField] private CanvasGroup lobbyControls;
    [SerializeField] private GameObject steamUnavailable;
    private bool refreshing = false;
    
    private void OnEnable() {
        steamUnavailable.SetActive(false);
        noLobbiesFound.SetActive(false);
        _ = RefreshLobbies();
    }

    public void RefreshLobby() {
        _ = RefreshLobbies();
    }

    private async Task RefreshLobbies() {
        if (refreshing) {
            return;
        }
        refreshing = true;
        try {
            if (!SteamManager.Initialized) {
                steamUnavailable.SetActive(true);
                Debug.LogWarning("Steam is not available. Cannot list lobbies.");
                return;
            }

            lobbyControls.interactable = false;
            for (int i = 0; i < transform.childCount; i++) {
                Destroy(transform.GetChild(i).gameObject);
            }

            steamUnavailable.SetActive(false);
            noLobbiesFound.SetActive(true);
            var matchList = await SteamMatchmaking.RequestLobbyList().AsTask<LobbyMatchList_t>();
            if (matchList.m_nLobbiesMatching == 0) {
                noLobbiesFound.SetActive(true);
                await Task.Delay(2000);
                lobbyControls.interactable = true;
                return;
            }
            noLobbiesFound.SetActive(false);
            for (int i = 0; i < matchList.m_nLobbiesMatching; i++) {
                var id = SteamMatchmaking.GetLobbyByIndex(i);
                var lobbyInstanceObject = Instantiate(lobbyPrefab, transform);
                var lobbyInstance = lobbyInstanceObject.GetComponent<LobbyInstanceDisplay>();
                lobbyInstance.SetLobby(id);
            }
            await Task.Delay(2000);
            lobbyControls.interactable = true;
        } finally {
            refreshing = false;
        }
    }
}
