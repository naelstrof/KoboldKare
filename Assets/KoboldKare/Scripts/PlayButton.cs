using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayButton : MonoBehaviour {
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
        NetworkManager.instance.SetSelectedMap(handle.Result.playableMap);
        NetworkManager.instance.StartSinglePlayer();
        GetComponent<Button>().interactable = true;
    }
}
