using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MultiplayerButton : MonoBehaviour {
    private void Awake() {
        GetComponent<Button>().onClick.AddListener(OnClick);
        SceneManager.activeSceneChanged += OnSceneChange;
    }
    private void OnDestroy() {
        SceneManager.activeSceneChanged -= OnSceneChange;
    }

    private void OnSceneChange(Scene arg0, Scene arg1) {
        gameObject.SetActive(!LevelLoader.InLevel());
    }


    void OnClick() {
        MainMenu.ShowMenuStatic(MainMenu.MainMenuMode.Multiplayer);
    }
}
