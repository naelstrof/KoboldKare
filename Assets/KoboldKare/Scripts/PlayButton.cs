using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Managing.Scened;
using FishNet.Transporting.Multipass;
using FishNet.Transporting.Tugboat;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

public class PlayButton : MonoBehaviour {
    private void Awake() {
        GetComponent<Button>().onClick.AddListener(OnClick);
        SceneManager.activeSceneChanged += OnSceneChange;
    }
    private void OnDestroy() {
        SceneManager.activeSceneChanged -= OnSceneChange;
    }

    private void OnSceneChange(Scene arg0, Scene arg1) {
        gameObject.SetActive(!GameManager.InLevel());
    }


    void OnClick() {
        GameManager.StartCoroutineStatic(LoadSinglePlayer());
    }

    IEnumerator LoadSinglePlayer() {
        GetComponent<Button>().interactable = false;
        var handle = MapSelector.PromptForMapSelect(false);
        yield return handle;
        if (handle.Cancelled) {
            GetComponent<Button>().interactable = true;
            yield break;
        }
        MainMenu.ShowMenuStatic(MainMenu.MainMenuMode.Loading);
        
        List<ModManager.ModStub> stubs = new(ModManager.GetModsWithLoadedAssets());
        if (handle.Result.playableMap.stub.HasValue) {
            stubs.Add(handle.Result.playableMap.stub.Value);
        }
        
        var networkManager = InstanceFinder.NetworkManager;
        if (!networkManager.ServerManager.Started) {
            networkManager.ServerManager.StartConnection();
        }

        if (!networkManager.ClientManager.Started) {
            networkManager.GetComponent<Multipass>().SetClientTransport(networkManager.GetComponent<Tugboat>());
            networkManager.ClientManager.StartConnection();
        }

        KoboldKareSceneProcessor.LoadSceneGlobal(handle.Result.playableMap.GetKey(), stubs);
        
        GetComponent<Button>().interactable = true;
    }
}
