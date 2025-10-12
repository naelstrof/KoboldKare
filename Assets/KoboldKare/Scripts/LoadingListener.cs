using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingListener : MonoBehaviour {
    void Start() {
        LevelLoader.instance.sceneLoadStart += SceneLoadStart;
        LevelLoader.instance.sceneLoadEnd += SceneLoadEnd;
        SceneManager.sceneLoaded += OnSceneLoadOther;
    }
    void SceneLoadStart() {
        MainMenu.ShowMenuStatic(MainMenu.MainMenuMode.Loading);
    }
    void OnDestroy() {
        LevelLoader.instance.sceneLoadStart -= SceneLoadStart;
        LevelLoader.instance.sceneLoadEnd -= SceneLoadEnd;
        SceneManager.sceneLoaded -= OnSceneLoadOther;
    }

    private void OnSceneLoadOther(Scene arg0, LoadSceneMode arg1) {
        if (LevelLoader.InLevel()) {
            MainMenu.ShowMenuStatic(MainMenu.MainMenuMode.None);
        } else {
            MainMenu.ShowMenuStatic(MainMenu.MainMenuMode.MainMenu);
        }
    }

    void SceneLoadEnd() {
        if (LevelLoader.InLevel()) {
            MainMenu.ShowMenuStatic(MainMenu.MainMenuMode.None);
        } else {
            MainMenu.ShowMenuStatic(MainMenu.MainMenuMode.MainMenu);
        }
    }
}
