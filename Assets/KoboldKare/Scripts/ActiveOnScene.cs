using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ActiveOnScene : MonoBehaviour {
    [SerializeField]
    private string activateOnSceneName;
    void OnEnable() {
        SceneManager.sceneLoaded += OnSceneChange;
    }
    void OnSceneChange(Scene scene, LoadSceneMode mode) {
        gameObject.SetActive(scene.name == activateOnSceneName);
    }
}
