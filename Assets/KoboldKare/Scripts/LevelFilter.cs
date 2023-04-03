using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelFilter : MonoBehaviour {
    public string sceneName;
    public List<GameObject> enabledObjectsOnLevel;
    public List<GameObject> enabledObjectsOffLevel;
    private void OnDisable() {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }
    private void OnEnable() {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
        OnLevelFinishedLoading(SceneManager.GetActiveScene(), LoadSceneMode.Additive);
    }
    private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode) {
        foreach( GameObject g in enabledObjectsOnLevel) {
            g.SetActive(scene.name == sceneName);
        }
        foreach( GameObject g in enabledObjectsOffLevel) {
            g.SetActive(scene.name != sceneName);
        }
    }
}
