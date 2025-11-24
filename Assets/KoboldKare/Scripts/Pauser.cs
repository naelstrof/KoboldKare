using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Pauser : MonoBehaviour {
    private static Pauser instance;
    private static bool paused = false;
    public delegate void PauseAction(bool paused);

    public static event PauseAction pauseChanged;
    private void Awake() {
        if (instance == null) {
            SceneManager.sceneLoaded += OnSceneLoaded;
            GameManager.GetPlayerControls().UI.Pause.performed += OnPauseButtonPressed;
            instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    public static bool GetPaused() => paused;

    public static void SetPaused(bool newPause) {
        if (paused != newPause) {
            TogglePause();
        }
    }

    private void OnDestroy() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        GameManager.GetPlayerControls().UI.Pause.performed -= OnPauseButtonPressed;
    }

    private static void TogglePause() {
        if (!GameManager.InLevel()) {
            return;
        }
        paused = !paused;
        OnPauseStateChanged(paused);
        pauseChanged?.Invoke(paused);
    }

    private void OnPauseButtonPressed(InputAction.CallbackContext callbackContext) {
        if (ActionButtonListener.HasAction()) {
            return;
        }
        TogglePause();
    }

    private static void OnPauseStateChanged(bool newPause) {
        if (newPause) {
            OrbitCamera.SetTracking(false);
            Time.timeScale = 0f;
        } else {
            OrbitCamera.SetTracking(true);
            Time.timeScale = 1f;
        }
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (paused) {
            TogglePause();
        }
    }
}
