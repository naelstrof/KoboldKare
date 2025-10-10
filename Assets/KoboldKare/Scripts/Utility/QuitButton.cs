using UnityEngine;
using UnityEngine.UI;

public class QuitButton : MonoBehaviour{
    public void QuitToMenu(){
        GameManager.instance.QuitToMenu();
    }
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
