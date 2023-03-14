using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityScriptableSettings;

[RequireComponent(typeof(Camera))]
public class OrbitCamera : MonoBehaviour {
    [FormerlySerializedAs("configuration")] [SerializeField, SerializeReference, SerializeReferenceButton]
    private OrbitCameraConfiguration currentConfiguration;
    
    [SerializeField]
    private SettingFloat mouseSensitivity;
    private static OrbitCamera instance;
    private Vector2 _aim;
    private Camera cam;
    private bool tweening = false;
    private PlayerInput controls;
    private bool tracking = true;
    private Quaternion postRotationOffset = Quaternion.identity;
    private OrbitCameraData currentCameraData;

    private OrbitCameraConfiguration lastConfig;
    private List<OrbitCameraConfiguration> orbitCameraConfigurations;

    public struct OrbitCameraData {
        public Vector3 position;
        public float distance;
        public float fov;
        public Vector2 screenPoint;
        public Quaternion rotation; 
        // rotation applied to the camera after aiming is completed.
        public Quaternion postRotationOffset;
        public bool clampYaw;
        public bool clampPitch;

        public OrbitCameraData(OrbitCameraPivotBase pivot, Quaternion camRotation) {
            rotation = pivot.GetRotation(camRotation);
            position = pivot.GetPivotPosition(rotation);
            distance = pivot.GetDistanceFromPivot(rotation);
            fov = pivot.GetFOV(rotation);
            screenPoint = pivot.GetScreenOffset(rotation);
            postRotationOffset = pivot.GetPostRotationOffset(rotation);
            clampPitch = pivot.GetClampPitch();
            clampYaw = pivot.GetClampYaw();
        }

        public static OrbitCameraData Lerp(OrbitCameraData pivotA, OrbitCameraData pivotB, float t) {
            return new OrbitCameraData {
                position = Vector3.Lerp(pivotA.position, pivotB.position, t),
                distance = Mathf.Lerp(pivotA.distance, pivotB.distance, t),
                fov = Mathf.Lerp(pivotA.fov, pivotB.fov, t),
                screenPoint = Vector2.Lerp(pivotA.screenPoint, pivotB.screenPoint, t),
                rotation = Quaternion.Lerp(pivotA.rotation, pivotB.rotation, t),
                postRotationOffset = Quaternion.Lerp(pivotA.postRotationOffset, pivotB.postRotationOffset, t),
                clampPitch = t<0.5f ? pivotA.clampPitch : pivotB.clampPitch,
                clampYaw = t<0.5f ? pivotA.clampYaw : pivotB.clampYaw
            };
        }
    }
    

    private void Awake() {
        orbitCameraConfigurations = new List<OrbitCameraConfiguration>();
        orbitCameraConfigurations.Add(currentConfiguration);
        
        cam = GetComponent<Camera>();
        instance = this;
    }

    private void Start() {
        mouseSensitivity = SettingsManager.GetSetting("MouseSensitivity") as SettingFloat;
    }

    private void Update() {
        // Always let player control
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        if (Gamepad.current != null) {
            mouseDelta += Gamepad.current.rightStick.ReadValue() * 40f;
        }

        if (controls != null) {
            mouseDelta = controls.actions["Look"].ReadValue<Vector2>() + controls.actions["LookJoystick"].ReadValue<Vector2>();
        }

        if (mouseSensitivity != null) {
            mouseDelta *= mouseSensitivity.GetValue();
        }

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
    private void SetOrbit(OrbitCameraData data) {
        Quaternion cameraRot = data.rotation;
        
        cam.fieldOfView = data.fov;
        float distance = data.distance;
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        Vector2 desiredScreenPosition = data.screenPoint*screenSize;

        transform.rotation = cameraRot;
        transform.position = data.position + cameraRot * Vector3.back * distance;
        
        Ray screenRay = cam.ScreenPointToRay(desiredScreenPosition);
        Vector3 desiredProjectedPoint = screenRay.GetPoint(distance);
        Vector3 currentProjectedPoint = transform.position + cameraRot * Vector3.forward * distance;

        transform.position -= (desiredProjectedPoint - currentProjectedPoint);

        postRotationOffset = data.postRotationOffset;
        currentCameraData = data;
    }

    public static void ReplaceConfiguration(OrbitCameraConfiguration oldConfig, OrbitCameraConfiguration newConfig, float tweenDuration = 0.2f) {
        int index = instance.orbitCameraConfigurations.IndexOf(oldConfig);
        if (index == -1) {
            throw new UnityException("No config found to replace!");
        }
        if (instance.orbitCameraConfigurations[index] == newConfig) {
            return;
        }
        instance.orbitCameraConfigurations[index] = newConfig;
        if (index == instance.orbitCameraConfigurations.Count - 1 && newConfig != instance.currentConfiguration) {
            instance.StartCoroutine(instance.TweenTo(newConfig, tweenDuration));
        }
    }
    
    public static void AddConfiguration(OrbitCameraConfiguration newConfig, float tweenDuration = 0.2f) {

        if (instance.orbitCameraConfigurations.Contains(newConfig)) {
            throw new UnityException("Tried to add a camera config more than once!");
        }
        
        if (instance.currentConfiguration == null) {
            instance.orbitCameraConfigurations.Add(newConfig);
            instance.currentConfiguration = newConfig;
            return;
        }

        instance.orbitCameraConfigurations.Add(newConfig);
        if (instance.currentConfiguration != newConfig) {
            instance.StartCoroutine(instance.TweenTo(newConfig, tweenDuration));
        }
    }

    public static void RemoveConfiguration(OrbitCameraConfiguration config, float tweenDuration = 0.2f) {
        if (instance == null) {
            return;
        }

        instance.orbitCameraConfigurations.Remove(config);
        if (instance.currentConfiguration == config) {
            instance.StartCoroutine(instance.TweenTo(instance.orbitCameraConfigurations[^1], tweenDuration));
        }
    }

    IEnumerator TweenTo(OrbitCameraConfiguration next, float duration) {
        yield return new WaitUntil(() => !tweening);
        if (next == instance.currentConfiguration) {
            yield break;
        }
        tweening = true;
        try {
            float startTime = Time.time;
            while (Time.time < startTime + duration) {
                float t = (Time.time - startTime) / duration;
                Quaternion cameraRotation = Quaternion.Inverse(instance.postRotationOffset)*Quaternion.Euler(-instance._aim.y, instance._aim.x, 0f);
                SetOrbit(OrbitCameraData.Lerp(currentConfiguration.GetData(cameraRotation), next.GetData(cameraRotation), t));
                yield return new WaitForEndOfFrame();
            }
            SetOrbit(next.GetData(Quaternion.Inverse(instance.postRotationOffset)*Quaternion.Euler(-instance._aim.y, instance._aim.x, 0f)));
            currentConfiguration = next;
        } finally {
            tweening = false;
        }
        if (instance.currentConfiguration != instance.orbitCameraConfigurations[^1]) {
            instance.StartCoroutine(instance.TweenTo(instance.orbitCameraConfigurations[^1], 0.2f));
        }
    }
    public static Vector2 GetPlayerIntendedScreenAim() => instance._aim;
    private void LateUpdate() {
        if (tweening || currentConfiguration == null) {
            return;
        }

        Quaternion cameraRotation = Quaternion.Inverse(instance.postRotationOffset)*Quaternion.Euler(-instance._aim.y, instance._aim.x, 0f);
        SetOrbit(currentConfiguration.GetData(cameraRotation));
        cam.cullingMask = currentConfiguration.GetCullingMask();
    }

    public static Quaternion GetPlayerIntendedRotation() {
        return Quaternion.Euler(-instance._aim.y, instance._aim.x, 0f);
    }

    public static Vector3 GetPlayerIntendedPosition() {
        return instance.transform.position;
    }

    public static void SetPlayerIntendedFacingDirection(Vector3 dir) {
        Quaternion lookDir = QuaternionExtensions.LookRotationUpPriority(dir, Vector3.up);
        var euler = lookDir.eulerAngles;
        instance._aim = new Vector2(-lookDir.y, lookDir.x);
    }

    public static void SetPlayerInput(PlayerInput input) {
        instance.controls = input;
    }

    public static void SetTracking(bool tracking) {
        instance.tracking = tracking;
    }
    public static OrbitCameraData GetCurrentCameraData() => instance.currentCameraData;
}
