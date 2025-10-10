using UnityEngine;

public class LoadingListener : MonoBehaviour {
    void Start() {
        LevelLoader.instance.sceneLoadStart += SceneLoadStart;
        LevelLoader.instance.sceneLoadEnd += SceneLoadEnd;
    }
    void SceneLoadStart() {
        MainMenu.ShowMenuStatic(MainMenu.MainMenuMode.Loading);
    }
    void OnDestroy() {
        LevelLoader.instance.sceneLoadStart -= SceneLoadStart;
        LevelLoader.instance.sceneLoadEnd -= SceneLoadEnd;
    }
    void SceneLoadEnd() {
        if (LevelLoader.InLevel()) {
            MainMenu.ShowMenuStatic(MainMenu.MainMenuMode.None);
        } else {
            MainMenu.ShowMenuStatic(MainMenu.MainMenuMode.MainMenu);
        }
    }
}
