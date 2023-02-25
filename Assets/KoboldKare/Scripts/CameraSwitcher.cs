using UnityEngine;
using UnityEngine.InputSystem;

public class CameraSwitcher : MonoBehaviour {
    public GameObject FPSCanvas;
    private OrbitCameraBasicConfiguration firstpersonConfiguration;
    private OrbitCameraFPSHeadPivot firstpersonPivot;
    private OrbitCameraLerpTrackBasicPivot shoulderPivot, buttPivot;
    private OrbitCameraCharacterConfiguration thirdpersonConfiguration;
    private OrbitCameraBasicConfiguration thirdpersonRagdollConfiguration;
    private OrbitCameraBasicConfiguration freecamConfiguration;
    private OrbitCameraBasicConfiguration lockedFreecamConfiguration;
    private OrbitCameraLockedOffsetPivot lockedFreecamPivot;
    private SimpleCameraController freeCamController;
    
    private Ragdoller ragdoller;
    
    public Transform uiSlider;
    private OrbitCameraConfiguration lastConfig;

    private bool initialized = false;
    [SerializeField]
    private PlayerPossession possession;
    private KoboldCharacterController controller;
    public enum CameraMode {
        FirstPerson = 0,
        ThirdPerson,
        FreeCam,
        FreeCamLocked,
    }
    private CameraMode? mode = null;

    void OnEnable() {
        controller = GetComponentInParent<KoboldCharacterController>();
        ragdoller = GetComponentInParent<Ragdoller>();
        ragdoller.RagdollEvent += OnRagdollEvent;
        if (firstpersonConfiguration == null) {
            var animator = GetComponentInParent<CharacterDescriptor>().GetDisplayAnimator();
            var fpsPivotObj = new GameObject("FPSPivot", typeof(OrbitCameraFPSHeadPivot));
            firstpersonPivot = fpsPivotObj.GetComponent<OrbitCameraFPSHeadPivot>();
            firstpersonPivot.Initialize(animator, HumanBodyBones.Head, 5f);
            firstpersonConfiguration = new OrbitCameraBasicConfiguration();
            firstpersonConfiguration.SetPivot(firstpersonPivot.GetComponent<OrbitCameraLerpTrackPivot>());
            firstpersonConfiguration.SetCullingMask(~LayerMask.GetMask("MirrorReflection"));
            var freeCamObj = new GameObject("FreeCamPivot", typeof(SimpleCameraController));
            freeCamObj.transform.SetParent(GetComponentInParent<CharacterDescriptor>().transform, false);
            freeCamObj.transform.position = transform.position;
            freeCamController = freeCamObj.GetComponent<SimpleCameraController>();
            freeCamController.SetControls(GetComponent<PlayerInput>());
            freecamConfiguration = new OrbitCameraBasicConfiguration();
            freecamConfiguration.SetPivot(freeCamController);
            freecamConfiguration.SetCullingMask(~LayerMask.GetMask("LocalPlayer"));
            shoulderPivot = new GameObject("ShoulderCamPivot", typeof(OrbitCameraLerpTrackBasicPivot)).GetComponent<OrbitCameraLerpTrackBasicPivot>();
            shoulderPivot.SetInfo(new Vector2(0.33f, 0.33f), 0.6f);
            shoulderPivot.Initialize(animator, HumanBodyBones.Head, 1f);
            buttPivot = new GameObject("ButtCamPivot", typeof(OrbitCameraLerpTrackBasicPivot)).GetComponent<OrbitCameraLerpTrackBasicPivot>();
            buttPivot.SetInfo(new Vector2(0.33f, 0.1f), 0.8f);
            buttPivot.Initialize(animator, HumanBodyBones.Hips, 1f);
            thirdpersonConfiguration = new OrbitCameraCharacterConfiguration();
            thirdpersonConfiguration.SetPivots(shoulderPivot, buttPivot);

            var basicRagdollPivot = animator.GetBoneTransform(HumanBodyBones.Spine).gameObject.AddComponent<OrbitCameraPivotBasic>();
            basicRagdollPivot.SetInfo(new Vector2(0.5f,0.33f), 1f);
            thirdpersonRagdollConfiguration = new OrbitCameraBasicConfiguration();
            thirdpersonRagdollConfiguration.SetPivot(basicRagdollPivot);
            thirdpersonRagdollConfiguration.SetCullingMask(~LayerMask.GetMask("LocalPlayer"));
            
            lockedFreecamPivot = animator.GetBoneTransform(HumanBodyBones.Chest).gameObject.AddComponent<OrbitCameraLockedOffsetPivot>();
            lockedFreecamConfiguration = new OrbitCameraBasicConfiguration();
            lockedFreecamConfiguration.SetPivot(lockedFreecamPivot);
            lockedFreecamConfiguration.SetCullingMask(~LayerMask.GetMask("LocalPlayer"));
        }
        initialized = false;
        OrbitCamera.AddConfiguration(firstpersonConfiguration);
        lastConfig = firstpersonConfiguration;
        if (!FPSCanvas.activeInHierarchy) {
            FPSCanvas.SetActive(true);
        }
        mode = CameraMode.FirstPerson;
    }

    void OnRagdollEvent(bool ragdolled) {
        if (mode == CameraMode.ThirdPerson) {
            if (ragdolled) {
                OrbitCamera.ReplaceConfiguration(lastConfig, thirdpersonRagdollConfiguration);
                lastConfig = thirdpersonRagdollConfiguration;
            } else {
                shoulderPivot.SnapInstant();
                buttPivot.SnapInstant();
                OrbitCamera.ReplaceConfiguration(lastConfig, thirdpersonConfiguration, 0.4f);
                lastConfig = thirdpersonConfiguration;
            }
        }
    }

    void OnDisable() {
        OrbitCamera.RemoveConfiguration(lastConfig);
        ragdoller.RagdollEvent -= OnRagdollEvent;
    }

    void Update() {
        uiSlider.transform.localPosition = Vector3.Lerp(uiSlider.transform.localPosition, -Vector3.right * (30f * ((int)mode+0.5f)), Time.deltaTime*2f);
    }

    public void OnSwitchCamera() {
        if (mode == null) {
            return;
        }

        int index = ((int)mode.Value + 1) % 4;
        SwitchCamera((CameraMode)index);
    }

    public void OnFirstPerson() {
        SwitchCamera(CameraMode.FirstPerson);
    }
    public void OnThirdPerson() {
        SwitchCamera(CameraMode.ThirdPerson);
    }
    public void OnFreeCamera() {
        SwitchCamera(CameraMode.FreeCam);
    }
    public void OnLockedCamera() {
        SwitchCamera(CameraMode.FreeCamLocked);
    }

    public void SwitchCamera(CameraMode cameraMode) {
        if (Cursor.lockState != CursorLockMode.Locked && initialized) {
            return;
        }

        if (mode == cameraMode) {
            return;
        }

        if (mode == CameraMode.FreeCam) {
            lockedFreecamPivot.Lock(freeCamController.transform.position, Quaternion.Inverse(ragdoller.transform.rotation));
        }

        initialized = true;
        mode = cameraMode;
        possession.enabled = true;
        freeCamController.enabled = false;
        switch (mode) {
            case CameraMode.FirstPerson:
                firstpersonPivot.SnapInstant();
                OrbitCamera.ReplaceConfiguration(lastConfig, firstpersonConfiguration);
                lastConfig = firstpersonConfiguration;
                
                if (!FPSCanvas.activeInHierarchy) {
                    FPSCanvas.SetActive(true);
                }
                break;
            case CameraMode.ThirdPerson:
                if (ragdoller.ragdolled) {
                    OrbitCamera.ReplaceConfiguration(lastConfig, thirdpersonRagdollConfiguration);
                    lastConfig = thirdpersonRagdollConfiguration;
                } else {
                    shoulderPivot.SnapInstant();
                    buttPivot.SnapInstant();
                    OrbitCamera.ReplaceConfiguration(lastConfig, thirdpersonConfiguration);
                    lastConfig = thirdpersonConfiguration;
                }

                if (!FPSCanvas.activeInHierarchy) {
                    FPSCanvas.SetActive(true);
                }
                break;
            case CameraMode.FreeCam:
                OrbitCamera.ReplaceConfiguration(lastConfig, freecamConfiguration);
                lastConfig = freecamConfiguration;
                freeCamController.enabled = true;
                possession.enabled = false;
                controller.inputDir = Vector3.zero;
                controller.inputJump = false;
                if (FPSCanvas.activeInHierarchy) {
                    FPSCanvas.SetActive(false);
                }
                break;
            case CameraMode.FreeCamLocked:
                OrbitCamera.ReplaceConfiguration(lastConfig, lockedFreecamConfiguration);
                lastConfig = lockedFreecamConfiguration;
                freeCamController.enabled = false;
                possession.enabled = true;
                if (!FPSCanvas.activeInHierarchy) {
                    FPSCanvas.SetActive(true);
                }
                break;
        }

    }
}
