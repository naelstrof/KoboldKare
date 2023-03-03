using KoboldKare;
using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class LevelLoader : MonoBehaviour {
    public static LevelLoader instance;
    public static bool loadingLevel;
    public delegate void SceneEventAction();
    public event SceneEventAction sceneLoadStart;
    public event SceneEventAction sceneLoadEnd;

    void Awake() {
        //Check if instance already exists
        if (instance == null) {
            //if not, set instance to this
            instance = this;
        } else if (instance != this) { //If instance already exists and it's not this:
            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);
            return;
        }
    }
    public Coroutine LoadLevel(string mapName) {
        StopAllCoroutines();
        return StartCoroutine(LoadLevelRoutine(mapName));
    }

    private IEnumerator LoadLevelRoutine(string mapName) {
        try {
            GameManager.instance.Pause(false);
            sceneLoadStart?.Invoke();
            loadingLevel = true;
            yield return new WaitForSecondsRealtime(1f);
            try {
                PhotonNetwork.LoadLevel(mapName);
            } catch {
                PopupHandler.instance.SpawnPopup("FailedLoad");
                throw;
            }
            while (PhotonNetwork.LevelLoadingProgress != 1f) {
                yield return new WaitForEndOfFrame();
            }
        } finally {
            loadingLevel = false;
            GameManager.instance.Pause(false);
            sceneLoadEnd?.Invoke();
        }
    }
}
