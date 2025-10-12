using UnityEngine;
using UnityEngine.UI;

public class QuitButton : MonoBehaviour {
    private void Awake() {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }
    void OnClick() {
        if (LevelLoader.InLevel()) {
            GameManager.instance.QuitToMenu();
        } else {
            GameManager.instance.Quit();
        }
    }
}
