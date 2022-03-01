using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingListener : MonoBehaviour {
    void Start() {
        LevelLoader.instance.sceneLoadStart += SceneLoadStart;
        LevelLoader.instance.sceneLoadEnd += SceneLoadEnd;
        gameObject.SetActive(false);
    }
    void SceneLoadStart() {
        gameObject.SetActive(true);
        gameObject.GetComponent<CanvasGroup>().alpha = 1f;
    }
    void OnDestroy() {
        LevelLoader.instance.sceneLoadStart -= SceneLoadStart;
        LevelLoader.instance.sceneLoadEnd -= SceneLoadEnd;
    }
    void SceneLoadEnd() {
        gameObject.SetActive(false);
        gameObject.GetComponent<CanvasGroup>().alpha = 0f;
    }

    public void Show(){
        gameObject.SetActive(true);
        gameObject.GetComponent<CanvasGroup>().alpha = 1f;
    }

    public void Hide(){
        gameObject.SetActive(false);
        gameObject.GetComponent<CanvasGroup>().alpha = 0f;
    }
}
