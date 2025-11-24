using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityScriptableSettings;

[RequireComponent(typeof(Camera))]
public partial class OrbitCamera : MonoBehaviour {
    [FormerlySerializedAs("configuration")] [SerializeField, SerializeReference, SubclassSelector]
    private OrbitCameraConfiguration currentConfiguration;
    
    private static OrbitCamera instance;
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Init() {
        instance = null;
        tracking = true;
        orbitCameraConfigurations = new();
        configurationChanged = null;
    }
    
    private Vector2 _aim;
    private Camera cam;
    
    private static InputAction mouseLookAction;
    private static InputAction joystickLookAction;
    
    private static bool tracking = true;
    private Quaternion postRotationOffset = Quaternion.identity;
    private OrbitCameraData currentCameraData = new(){rotation = Quaternion.identity, position = Vector3.zero, fov = 65f, screenPoint = Vector2.one*0.5f, distance = 1f};
    private Coroutine tween;
    private SettingFloat mouseSensitivity;
    private bool paused;

    private static List<OrbitCameraConfiguration> orbitCameraConfigurations = new();

    public delegate void OrbitCameraConfigurationChangedAction(OrbitCameraConfiguration previousConfiguration, OrbitCameraConfiguration newConfiguration);

    public static event OrbitCameraConfigurationChangedAction configurationChanged;
    
    public static OrbitCameraConfiguration GetConfiguration() => orbitCameraConfigurations.Count == 0 ? null : orbitCameraConfigurations[^1];

    [RuntimeInitializeOnLoadMethod]
    private static void Initialize() {
        SceneManager.sceneUnloaded += OnUnloadScene;
    }

    private static void OnUnloadScene(Scene arg0) {
        orbitCameraConfigurations = new List<OrbitCameraConfiguration>();
    }

    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }
        instance = this;
        orbitCameraConfigurations.Insert(0,currentConfiguration);
        cam = GetComponent<Camera>();
        currentConfiguration = orbitCameraConfigurations[^1];
        SetOrbit(currentConfiguration.GetData(cam));
    }

    void Start() {
        mouseSensitivity = SettingsManager.GetSetting("MouseSensitivity") as SettingFloat;
    }

    private void Update() {
        if (paused) {
            return;
        }
        // Always let player control
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        if (Gamepad.current != null) {
            Vector2 gamepadLook = Gamepad.current.rightStick.ReadValue();
            float deadzone = 0.15f;
            gamepadLook = new Vector2(Mathf.MoveTowards(gamepadLook.x, 0f, 0.15f)/(1f-deadzone),
                                      Mathf.MoveTowards(gamepadLook.y, 0f, 0.15f)/(1f-deadzone));
            mouseDelta += gamepadLook * 40f;
        }

        if (mouseLookAction != null && joystickLookAction != null) {
            mouseDelta = mouseLookAction.ReadValue<Vector2>() + joystickLookAction.ReadValue<Vector2>();
        }

        var sensitivity = mouseSensitivity?.GetValue() ?? 0.01f;
        mouseDelta *= sensitivity;

        if (tracking) {
            _aim += mouseDelta;
            if (currentCameraData.clampYaw) {
                _aim.x = Mathf.Clamp(_aim.x, -180f, 180f);
            } else {
                _aim.x = Mathf.Repeat(_aim.x+180f, 360f)-180f;
            }
            if (currentCameraData.clampPitch) {
                _aim.y = Mathf.Clamp(_aim.y, -89f, 89f);
            }
        }
    }

    public static Ray GetScreenRay(Camera cam, Vector2 screenPoint) {
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        Vector2 desiredScreenPosition = screenPoint*screenSize;
        return cam.ScreenPointToRay(desiredScreenPosition);
    }

    private void SetOrbit(OrbitCameraData data) {
        if (paused) {
            return;
        }
        
        Quaternion cameraRot = data.rotation;
        
        cam.fieldOfView = data.fov;
        float distance = data.distance;
        transform.rotation = cameraRot;
        Ray screenRay = GetScreenRay(cam, data.screenPoint);
        transform.position = data.position - screenRay.direction * distance;
        
        currentCameraData = data;
    }

    public static void SetPaused(bool paused) {
        instance.paused = paused;
    }
    
    public static void AddConfiguration(OrbitCameraConfiguration newConfig, float tweenDuration = 0.4f) {
        if (orbitCameraConfigurations.Contains(newConfig)) {
            throw new UnityException("Tried to add a camera config more than once!");
        }

        orbitCameraConfigurations.Add(newConfig);
        
        if (instance == null) {
            return;
        }
        if (instance.currentConfiguration != newConfig) {
            instance.BeginTween(GetCurrentCameraData(), newConfig, tweenDuration);
        }
    }

    public static void ReplaceConfiguration(OrbitCameraConfiguration oldConfig, OrbitCameraConfiguration newConfig, float tweenDuration = 0.4f) {
        if (instance == null) {
            return;
        }

        int configLocation = orbitCameraConfigurations.IndexOf(oldConfig);
        Assert.AreNotEqual(configLocation, -1);

        orbitCameraConfigurations[configLocation] = newConfig;
        if (instance == null) {
            return;
        }
        if (instance.currentConfiguration == oldConfig) {
            instance.BeginTween(GetCurrentCameraData(), orbitCameraConfigurations[^1], tweenDuration);
        }
    }

    public static void RemoveConfiguration(OrbitCameraConfiguration config, float tweenDuration = 0.4f) {
        if (instance == null) {
            return;
        }

        if (!orbitCameraConfigurations.Contains(config)) {
            return;
        }

        orbitCameraConfigurations.Remove(config);
        if (instance == null) {
            return;
        }

        if (instance.currentConfiguration == config) {
            instance.BeginTween(GetCurrentCameraData(), orbitCameraConfigurations[^1], tweenDuration);
        }
    }

    private void BeginTween(OrbitCameraData from, OrbitCameraConfiguration next, float duration) {
        if (tween != null) {
            StopCoroutine(tween);
            tween = null;
        }

        currentConfiguration = next;
        configurationChanged?.Invoke(currentConfiguration, next);
        tween = StartCoroutine(TweenTo(from, next, duration));
    }

    IEnumerator TweenTo(OrbitCameraData from, OrbitCameraConfiguration next, float duration) {
        try {
            float timer = 0f;
            while (timer < duration) {
                if (!paused) {
                    timer += Time.deltaTime;
                    float t = timer / duration;
                    cam.transform.rotation = Quaternion.Inverse(instance.postRotationOffset) * Quaternion.Euler(-instance._aim.y, instance._aim.x, 0f);
                    SetOrbit(OrbitCameraData.Lerp(from, next.GetData(cam), t));
                }
                yield return new WaitForEndOfFrame();
            }
            SetOrbit(next.GetData(cam));
        } finally {
            tween = null;
        }
    }
    
    public static Vector2 GetPlayerIntendedScreenAim() => instance._aim;
    private void LateUpdate() {
        if (tween != null || currentConfiguration == null || paused) {
            return;
        }

        cam.transform.rotation = GetPlayerIntendedRotation();
        SetOrbit(currentConfiguration.GetData(cam));
        cam.cullingMask = currentConfiguration.GetCullingMask();
    }

    public static Quaternion GetPlayerIntendedRotation() {
        return Quaternion.Euler(-instance._aim.y, instance._aim.x, 0f);
    }

    public static Vector3 GetPlayerIntendedPosition() {
        return instance.currentConfiguration.GetData(instance.cam).position;
    }

    public static void SetPlayerIntendedFacingDirection(Vector3 dir) {
        Quaternion lookDir = QuaternionExtensions.LookRotationUpPriority(dir, Vector3.up);
        var euler = lookDir.eulerAngles;
        instance._aim = new Vector2(-lookDir.y, lookDir.x);
    }

    public static void SetLookActions(InputAction mouseLookActionNew, InputAction joystickLookActionNew) {
        mouseLookAction = mouseLookActionNew;
        joystickLookAction = joystickLookActionNew;
    }

    public static Camera GetCamera() {
        if (instance == null) {
            return null;
        }
        return instance.cam;
    }

    public static void SetTracking(bool newTracking) {
        tracking = newTracking;
    }
    public static OrbitCameraData GetCurrentCameraData() => instance.currentCameraData;
}
