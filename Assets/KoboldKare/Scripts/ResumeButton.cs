using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResumeButton : MonoBehaviour {
    private void Awake() {
        GetComponent<Button>().onClick.AddListener(OnClick);
        SceneManager.activeSceneChanged += OnSceneChange;
        gameObject.SetActive(GameManager.InLevel());
    }
    private void OnDestroy() {
        SceneManager.activeSceneChanged -= OnSceneChange;
    }

    private void OnSceneChange(Scene arg0, Scene arg1) {
        gameObject.SetActive(GameManager.InLevel());
    }
    
    void OnClick() {
        MainMenu.ShowMenuStatic(MainMenu.MainMenuMode.None);
        Pauser.SetPaused(false);
    }
}
