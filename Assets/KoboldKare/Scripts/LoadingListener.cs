using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingListener : MonoBehaviour {
    void Start() {
        MapLoadingInterop.OnMapStartLoad += OnMapLoad;
    }

    private void OnMapLoad(BoxedSceneLoad obj) {
        MainMenu.ShowMenuStatic(MainMenu.MainMenuMode.Loading);
        obj.OnCompleted += () => {
            if (GameManager.InLevel()) {
                MainMenu.ShowMenuStatic(MainMenu.MainMenuMode.None);
            } else {
                MainMenu.ShowMenuStatic(MainMenu.MainMenuMode.MainMenu);
            }
        };
    }
    void OnDestroy() {
        MapLoadingInterop.OnMapStartLoad -= OnMapLoad;
    }
}
