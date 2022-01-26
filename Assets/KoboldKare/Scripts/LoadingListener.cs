using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingListener : MonoBehaviour {
    void Awake() {
        LevelLoader.instance.sceneLoadStart += SceneLoadStart;
        LevelLoader.instance.sceneLoadEnd += SceneLoadEnd;
    }
    void SceneLoadStart() {
        gameObject.SetActive(true);
    }
    void SceneLoadEnd() {
        gameObject.SetActive(false);
    }
}
