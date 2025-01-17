using UnityEngine;
using UnityEngine.InputSystem;
using UnityScriptableSettings;

public class CameraSwitcher : MonoBehaviour {
    public GameObject FPSCanvas;
    private OrbitCameraBasicConfiguration firstpersonConfiguration;
    private OrbitCameraFPSHeadPivot firstpersonPivot;
    private OrbitCameraCharacterHitmanConfiguration thirdpersonConfiguration;
    private OrbitCameraBasicConfiguration thirdpersonRagdollConfiguration;
    private OrbitCameraBasicConfiguration animatingThirdpersonConfiguration;
    private OrbitCameraBasicConfiguration freecamConfiguration;
    private OrbitCameraBasicConfiguration lockedFreecamConfiguration;
    //private OrbitCameraLockedOffsetPivot lockedFreecamPivot;
    //private OrbitRagdollPivot basicRagdollPivot;
    private SimpleCameraController freeCamController;
    
    private OrbitCameraPivotBasic headPivot;
    private OrbitCameraPivotBasic crouchPivot;
    private OrbitCameraPivotBasic buttPivot;
    
    private OrbitCameraPivotBasic headOogle;
    private OrbitCameraPivotBasic buttOogle;

    private OrbitCameraPivotBasic headAnimatingPivot;
    private OrbitCameraPivotBasic buttAnimatingPivot;
    
    private Ragdoller ragdoller;
    
    private SettingFloat fovSetting;

    private Collider[] collidersMemory;
    
    public Transform uiSlider;
    private OrbitCameraConfiguration lastConfig;

    private bool initialized = false;
    [SerializeField]
    private PlayerPossession possession;
    private PrecisionGrabber precisionGrabber;
    private KoboldCharacterController controller;
    private CharacterControllerAnimator koboldAnimator;
    private Animator animator;

    private bool isAnimating = false;
    public enum CameraMode {
        FirstPerson = 0,
        ThirdPerson,
        FreeCam,
    }
    private CameraMode? mode = null;
    private const float thirdPersonCameraDistance = 1.5f;

    void OnKoboldSizeChange(float newSize) {
        crouchPivot.SetDesiredDistanceFromPivot(thirdPersonCameraDistance * newSize);
        headPivot.SetDesiredDistanceFromPivot(thirdPersonCameraDistance * newSize);
        headAnimatingPivot.SetDesiredDistanceFromPivot(thirdPersonCameraDistance * newSize);
        buttAnimatingPivot.SetDesiredDistanceFromPivot(thirdPersonCameraDistance * newSize);
        buttPivot.SetDesiredDistanceFromPivot(thirdPersonCameraDistance*0.5f * newSize);
        
        headOogle.SetDesiredDistanceFromPivot(thirdPersonCameraDistance * newSize);
        buttOogle.SetDesiredDistanceFromPivot(thirdPersonCameraDistance * newSize);
    }
    private void OnFOVChanged(float fov) {
        crouchPivot.SetBaseFOV(fov-5f);
        headPivot.SetBaseFOV(fov);
        buttPivot.SetBaseFOV(fov+3);
        headAnimatingPivot.SetBaseFOV(fov);
        buttAnimatingPivot.SetBaseFOV(fov);
        headOogle.SetBaseFOV(fov);
        buttOogle.SetBaseFOV(fov);
    }

    void OnEnable() {
        fovSetting = (SettingFloat)SettingsManager.GetSetting("CameraFOV");
        float fov;
        if (fovSetting != null) {
            fovSetting.changed += OnFOVChanged;
            fov = fovSetting.GetValue();
        } else {
            fov = 65f;
        }
        
        controller = GetComponentInParent<KoboldCharacterController>();
        var kobold = GetComponentInParent<Kobold>();
        koboldAnimator = GetComponentInParent<CharacterControllerAnimator>();
        ragdoller = GetComponentInParent<Ragdoller>();
        precisionGrabber = GetComponentInParent<PrecisionGrabber>();
        ragdoller.RagdollEvent += OnRagdollEvent;
        if (firstpersonConfiguration == null) {
            firstpersonConfiguration = CreateFPSConfig(kobold, fov);
            firstpersonConfiguration.SetCullingMask(~LayerMask.GetMask("MirrorReflection"));
            var animator = GetComponentInParent<CharacterDescriptor>().GetDisplayAnimator();
            
            thirdpersonConfiguration = CreateShoulderConfig(kobold, fov);
            thirdpersonConfiguration.SetCullingMask( ~LayerMask.GetMask("LocalPlayer"));
            
            animatingThirdpersonConfiguration = new OrbitCameraConfigurationSlide();
            
            GameObject headPivotObj = new GameObject("HeadPivotAnimating", typeof(OrbitCameraPivotBasic));
            headPivotObj.transform.SetParent(animator.GetBoneTransform(HumanBodyBones.Head));
            headPivotObj.transform.localPosition = Vector3.zero;
            headAnimatingPivot = headPivotObj.GetComponent<OrbitCameraPivotBasic>();
            headAnimatingPivot.SetScreenOffset(new Vector2(0.5f, 0.55f));
            headAnimatingPivot.SetDesiredDistanceFromPivot(thirdPersonCameraDistance);
            headAnimatingPivot.SetBaseFOV(fov);
            
            GameObject buttPivotObj = new GameObject("ButtPivotAnimating", typeof(OrbitCameraPivotBasic));
            buttPivotObj.transform.SetParent(animator.GetBoneTransform(HumanBodyBones.Hips));
            buttPivotObj.transform.localPosition = Vector3.zero;
            buttAnimatingPivot = buttPivotObj.GetComponent<OrbitCameraPivotBasic>();
            buttAnimatingPivot.SetScreenOffset(new Vector2(0.5f, 0.45f));
            buttAnimatingPivot.SetDesiredDistanceFromPivot(thirdPersonCameraDistance);
            buttAnimatingPivot.SetBaseFOV(fov);
            
            animatingThirdpersonConfiguration.SetPivot(headAnimatingPivot);
            ((OrbitCameraConfigurationSlide)animatingThirdpersonConfiguration).SetOtherPivot(buttAnimatingPivot);
            animatingThirdpersonConfiguration.SetCullingMask( ~LayerMask.GetMask("LocalPlayer"));
            
            var freeCamObj = new GameObject("FreeCamPivot");
            freeCamObj.transform.SetParent(GetComponentInParent<CharacterDescriptor>().transform);
            freeCamObj.transform.position = transform.position;
            freeCamController = freeCamObj.AddComponent<SimpleCameraController>();
            freeCamController.SetControls(GetComponent<PlayerInput>());
            freeCamController.enabled = false;
            
            freecamConfiguration = new OrbitCameraBasicConfiguration();
            freecamConfiguration.SetPivot(freeCamController);
            freecamConfiguration.SetCullingMask(~LayerMask.GetMask("LocalPlayer"));

            thirdpersonRagdollConfiguration = animatingThirdpersonConfiguration;

        }
        initialized = false;
        OrbitCamera.AddConfiguration(firstpersonConfiguration);
        lastConfig = firstpersonConfiguration;
        if (!FPSCanvas.activeInHierarchy) {
            FPSCanvas.SetActive(true);
        }
        koboldAnimator.inputShouldFaceEye = true;
        mode = CameraMode.FirstPerson;
        GetComponentInParent<Kobold>().sizeInflater.changed += OnKoboldSizeChange;
        OnKoboldSizeChange(GetComponentInParent<Kobold>().sizeInflater.GetSize());
    }

    private OrbitCameraBasicConfiguration CreateFPSConfig(Kobold character, float fov) {
        var animator = character.GetComponentInChildren<CharacterControllerAnimator>()?.GetPlayerModel() ?? character.GetComponentInChildren<Animator>();
        
        var fpsPivotObj = new GameObject("FPSPivot", typeof(OrbitCameraFPSHeadPivot));
        firstpersonPivot = fpsPivotObj.GetComponent<OrbitCameraFPSHeadPivot>();
        firstpersonPivot.Initialize(animator, HumanBodyBones.Head, 5f);
        
        var config = new OrbitCameraBasicConfiguration();
        config.SetPivot(firstpersonPivot);
        return config;
    }

    private OrbitCameraCharacterHitmanConfiguration CreateShoulderConfig(Kobold character, float fov) {
        var animator = character.GetComponentInChildren<CharacterControllerAnimator>()?.GetPlayerModel() ?? character.GetComponentInChildren<Animator>();
        
        Vector3 headLocalPos = character.transform.InverseTransformPoint(animator.GetBoneTransform(HumanBodyBones.Head).position);
        
        GameObject headPivotObj = new GameObject("HeadPivot", typeof(OrbitCameraPivotBasic));
        headPivotObj.transform.SetParent(character.transform.transform);
        //headPivotObj.transform.localPosition = Vector3.zero;
        headPivotObj.transform.localPosition = headLocalPos.With(x: 0f, z: 0f);
        headPivot = headPivotObj.GetComponent<OrbitCameraPivotBasic>();
        headPivot.SetScreenOffset(new Vector2(0.35f, 0.55f));
        headPivot.SetDesiredDistanceFromPivot(thirdPersonCameraDistance);
        headPivot.SetBaseFOV(fov);
        
        GameObject crouchPivotObj = new GameObject("CrouchPivot", typeof(OrbitCameraPivotBasic));
        crouchPivotObj.transform.SetParent(character.transform);
        //crouchPivotObj.transform.localPosition = Vector3.zero;
        crouchPivotObj.transform.localPosition = (headLocalPos).With(x: 0f, z: 0f);
        crouchPivot = crouchPivotObj.GetComponent<OrbitCameraPivotBasic>();
        crouchPivot.SetScreenOffset(new Vector2(0.35f, 0.5f));
        crouchPivot.SetDesiredDistanceFromPivot(thirdPersonCameraDistance);
        crouchPivot.SetBaseFOV(fov-5f);
        
        GameObject buttPivotObj = new GameObject("ButtPivot", typeof(OrbitCameraPivotBasic));
        buttPivotObj.transform.SetParent(animator.GetBoneTransform(HumanBodyBones.Hips));
        buttPivotObj.transform.localPosition = Vector3.zero;
        buttPivot = buttPivotObj.GetComponent<OrbitCameraPivotBasic>();
        buttPivot.SetScreenOffset(new Vector2(0.5f, 0.38f));
        buttPivot.SetDesiredDistanceFromPivot(1f);
        buttPivot.SetBaseFOV(fov+3f);
        
        GameObject buttOoglePivotObj = new GameObject("ButtOoglePivot", typeof(OrbitCameraPivotBasic));
        buttOoglePivotObj.transform.SetParent(animator.GetBoneTransform(HumanBodyBones.Hips));
        buttOoglePivotObj.transform.localPosition = Vector3.zero;
        buttOogle = buttOoglePivotObj.GetComponent<OrbitCameraPivotBasic>();
        buttOogle.SetScreenOffset(new Vector2(0.5f, 0.65f));
        buttOogle.SetDesiredDistanceFromPivot(thirdPersonCameraDistance);
        buttOogle.SetBaseFOV(fov);
        
        GameObject headOoglePivotObj = new GameObject("HeadOoglePivot", typeof(OrbitCameraPivotBasic));
        headOoglePivotObj.transform.SetParent(character.transform.transform);
        //headPivotObj.transform.localPosition = Vector3.zero;
        headOoglePivotObj.transform.localPosition = headLocalPos.With(x: 0f, z: 0f);
        headOogle = headOoglePivotObj.GetComponent<OrbitCameraPivotBasic>();
        headOogle.SetScreenOffset(new Vector2(0.5f, 0.50f));
        headOogle.SetDesiredDistanceFromPivot(thirdPersonCameraDistance);
        headOogle.SetBaseFOV(fov);
        
        var config = new OrbitCameraCharacterHitmanConfiguration();
        config.SetPivots(character, headPivot, crouchPivot, buttPivot, buttOogle, headOogle);
        return config;
    }

    void OnRagdollEvent(bool ragdolled) {
        if (mode == CameraMode.ThirdPerson) {
            if (ragdolled) {
                OrbitCamera.ReplaceConfiguration(lastConfig, thirdpersonRagdollConfiguration);
                lastConfig = thirdpersonRagdollConfiguration;
            } else {
                OrbitCamera.ReplaceConfiguration(lastConfig, thirdpersonConfiguration, 0.4f);
                lastConfig = thirdpersonConfiguration;
            }
        }
    }

    void OnDisable() {
        fovSetting = (SettingFloat)SettingsManager.GetSetting("CameraFOV");
        if (fovSetting != null) {
            fovSetting.changed -= OnFOVChanged;
        }
        OrbitCamera.RemoveConfiguration(lastConfig);
        ragdoller.RagdollEvent -= OnRagdollEvent;
        if (GetComponentInParent<Kobold>() != null) {
            GetComponentInParent<Kobold>().sizeInflater.changed -= OnKoboldSizeChange;
        }
    }

    void Update() {
        uiSlider.transform.localPosition = Vector3.Lerp(uiSlider.transform.localPosition, -Vector3.right * (36f * ((int)mode+0.5f)), Time.deltaTime*2f);
        if (koboldAnimator.IsAnimating() && !isAnimating) {
            if (mode == CameraMode.ThirdPerson) {
                OrbitCamera.ReplaceConfiguration(thirdpersonConfiguration, animatingThirdpersonConfiguration);
                lastConfig = animatingThirdpersonConfiguration;
            }
            isAnimating = true;
        }
        if (!koboldAnimator.IsAnimating() && isAnimating) {
            if (mode == CameraMode.ThirdPerson) {
                OrbitCamera.ReplaceConfiguration( animatingThirdpersonConfiguration, thirdpersonConfiguration);
                lastConfig = thirdpersonConfiguration;
            }
            isAnimating = false;
        }
    }

    public void OnSwitchCamera() {
        if (mode == null) {
            return;
        }

        int index = ((int)mode.Value + 1) % 3;
        SwitchCamera((CameraMode)index);
    }

    public void OnHideHUD() {
        if (!FPSCanvas.activeInHierarchy) {
            FPSCanvas.SetActive(true);
        } else {
            FPSCanvas.SetActive(false);
        }
    }

    public void OnPause() {
        if (!FPSCanvas.activeInHierarchy) {
            FPSCanvas.SetActive(true);
        }
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

    public void SwitchCamera(CameraMode cameraMode) {
        if (Cursor.lockState != CursorLockMode.Locked && initialized) {
            return;
        }

        if (mode == cameraMode) {
            return;
        }

        if (mode == CameraMode.FreeCam) {
            //lockedFreecamPivot.Lock(freeCamController.transform.position, Quaternion.Inverse(ragdoller.transform.rotation));
        }

        initialized = true;
        mode = cameraMode;
        possession.SetMovementEnabled(true);
        freeCamController.enabled = false;
        switch (mode) {
            case CameraMode.FirstPerson:
                if (collidersMemory != null && collidersMemory.Length > 0) {
                    precisionGrabber.SetIgnoreColliders(collidersMemory);
                }
                koboldAnimator.inputShouldIgnoreLookDirChange = false;
                OrbitCamera.ReplaceConfiguration(lastConfig, firstpersonConfiguration);
                lastConfig = firstpersonConfiguration;
                koboldAnimator.inputShouldFaceEye = true;
                break;
            case CameraMode.ThirdPerson:
                collidersMemory = precisionGrabber.GetIgnoreColliders();
                if (collidersMemory != null && collidersMemory.Length > 0) {
                    precisionGrabber.SetIgnoreColliders(new Collider[] { });
                }
                koboldAnimator.inputShouldFaceEye = false;
                koboldAnimator.inputShouldIgnoreLookDirChange = false;
                if (ragdoller.ragdolled) {
                    OrbitCamera.ReplaceConfiguration(lastConfig, thirdpersonRagdollConfiguration);
                    lastConfig = thirdpersonRagdollConfiguration;
                } else {
                    //shoulderPivot.SnapInstant();
                    //buttPivot.SnapInstant();
                    OrbitCamera.ReplaceConfiguration(lastConfig, isAnimating ? animatingThirdpersonConfiguration : thirdpersonConfiguration);
                    lastConfig = isAnimating ? animatingThirdpersonConfiguration : thirdpersonConfiguration;
                }
                break;
            case CameraMode.FreeCam:
                collidersMemory = precisionGrabber.GetIgnoreColliders();
                if (collidersMemory != null && collidersMemory.Length > 0) {
                    precisionGrabber.SetIgnoreColliders(new Collider[] { });
                }
                
                koboldAnimator.inputShouldFaceEye = false;
                koboldAnimator.inputShouldIgnoreLookDirChange = true;
                var data = OrbitCamera.GetCurrentCameraData();
                freeCamController.SetCameraPosition(data.position+data.rotation*Vector3.back*data.distance);
                OrbitCamera.ReplaceConfiguration(lastConfig, freecamConfiguration);
                lastConfig = freecamConfiguration;
                freeCamController.enabled = true;
                possession.SetMovementEnabled(false);
                controller.inputDir = Vector3.zero;
                controller.inputJump = false;
                break;
        }

    }
}
