using FishNet;
using FishNet.Managing.Scened;
using FishNet.Transporting.Multipass;
using FishNet.Transporting.Tugboat;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class LoadMainMenu : MonoBehaviour {
    void Start() {
        Addressables.LoadSceneAsync("MainMenu");
        //KoboldKareSceneProcessor.LoadSceneGlobal("MainMenu");
    }
}
