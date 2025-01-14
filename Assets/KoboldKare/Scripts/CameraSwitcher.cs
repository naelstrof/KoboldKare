using UnityEngine;
using UnityEngine.InputSystem;

public class CameraSwitcher : MonoBehaviour {
    public GameObject FPSCanvas;
    private OrbitCameraBasicConfiguration firstpersonConfiguration;
    private OrbitCameraFPSHeadPivot firstpersonPivot;
    private OrbitCameraLerpTrackBasicPivot shoulderPivot, buttPivot;
    private OrbitCameraCharacterHitmanConfiguration thirdpersonConfiguration;
    private OrbitCameraBasicConfiguration thirdpersonRagdollConfiguration;
    private OrbitCameraBasicConfiguration freecamConfiguration;
    private OrbitCameraBasicConfiguration lockedFreecamConfiguration;
    //private OrbitCameraLockedOffsetPivot lockedFreecamPivot;
    private OrbitRagdollPivot basicRagdollPivot;
    private SimpleCameraController freeCamController;
    
    private OrbitCameraPivotBasic headPivot;
    private OrbitCameraPivotBasic crouchPivot;
    
    private Ragdoller ragdoller;
    
    public Transform uiSlider;
    private OrbitCameraConfiguration lastConfig;

    private bool initialized = false;
    [SerializeField]
    private PlayerPossession possession;
    private PrecisionGrabber precisionGrabber;
    private KoboldCharacterController controller;
    private CharacterControllerAnimator koboldAnimator;
    private Animator animator;
    public enum CameraMode {
        FirstPerson = 0,
        ThirdPerson,
        FreeCam,
    }
    private CameraMode? mode = null;

    void OnKoboldSizeChange(float newSize) {
        //shoulderPivot.SetInfo(new Vector2(0.33f, 0.33f), 0.6f*newSize);
        //buttPivot.SetInfo(new Vector2(0.33f, 0.1f), 0.8f*newSize);
        //basicRagdollPivot.SetInfo(new Vector2(0.5f,0.33f), 1f*newSize, 65f);
    }

    void OnEnable() {
        controller = GetComponentInParent<KoboldCharacterController>();
        var kobold = GetComponentInParent<Kobold>();
        koboldAnimator = GetComponentInParent<CharacterControllerAnimator>();
        ragdoller = GetComponentInParent<Ragdoller>();
        precisionGrabber = GetComponentInParent<PrecisionGrabber>();
        precisionGrabber.grabChanged += OnGrabChanged;
        ragdoller.RagdollEvent += OnRagdollEvent;
        if (firstpersonConfiguration == null) {
            firstpersonConfiguration = CreateFPSConfig(kobold);
            firstpersonConfiguration.SetCullingMask(~LayerMask.GetMask("MirrorReflection"));
            var animator = GetComponentInParent<CharacterDescriptor>().GetDisplayAnimator();
            
            thirdpersonConfiguration = CreateShoulderConfig(kobold);
            thirdpersonConfiguration.SetCullingMask( ~LayerMask.GetMask("LocalPlayer"));
            
            
            var freeCamObj = new GameObject("FreeCamPivot");
            freeCamObj.transform.SetParent(GetComponentInParent<CharacterDescriptor>().transform);
            freeCamObj.transform.position = transform.position;
            freeCamController = freeCamObj.AddComponent<SimpleCameraController>();
            freeCamController.SetControls(GetComponent<PlayerInput>());
            freeCamController.enabled = false;
            
            freecamConfiguration = new OrbitCameraBasicConfiguration();
            freecamConfiguration.SetPivot(freeCamController);
            freecamConfiguration.SetCullingMask(~LayerMask.GetMask("LocalPlayer"));


            basicRagdollPivot = animator.GetBoneTransform(HumanBodyBones.Spine).gameObject.AddComponent<OrbitRagdollPivot>();
            basicRagdollPivot.SetInfo(new Vector2(0.5f,0.33f), 1.75f, 65f);
            
            thirdpersonRagdollConfiguration = new OrbitCameraBasicConfiguration();
            thirdpersonRagdollConfiguration.SetPivot(basicRagdollPivot);
            thirdpersonRagdollConfiguration.SetCullingMask(~LayerMask.GetMask("LocalPlayer"));
            
            //lockedFreecamPivot = animator.GetBoneTransform(HumanBodyBones.Chest).gameObject.AddComponent<OrbitCameraLockedOffsetPivot>();
            //lockedFreecamConfiguration = new OrbitCameraBasicConfiguration();
            //lockedFreecamConfiguration.SetPivot(lockedFreecamPivot);
            //lockedFreecamConfiguration.SetCullingMask(~LayerMask.GetMask("LocalPlayer"));
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
    
    private OrbitCameraBasicConfiguration CreateFPSConfig(Kobold character) {
        var animator = character.GetComponentInChildren<CharacterControllerAnimator>()?.GetPlayerModel() ?? character.GetComponentInChildren<Animator>();
        
        var fpsPivotObj = new GameObject("FPSPivot", typeof(OrbitCameraFPSHeadPivot));
        firstpersonPivot = fpsPivotObj.GetComponent<OrbitCameraFPSHeadPivot>();
        firstpersonPivot.Initialize(animator, HumanBodyBones.Head, 5f);
        
        var config = new OrbitCameraBasicConfiguration();
        config.SetPivot(firstpersonPivot);
        return config;
    }

    private OrbitCameraCharacterHitmanConfiguration CreateShoulderConfig(Kobold character) {
        var animator = character.GetComponentInChildren<CharacterControllerAnimator>()?.GetPlayerModel() ?? character.GetComponentInChildren<Animator>();
        
        Vector3 headLocalPos = character.transform.InverseTransformPoint(animator.GetBoneTransform(HumanBodyBones.Head).position);
        
        GameObject headPivotObj = new GameObject("HeadPivot", typeof(OrbitCameraPivotBasic));
        headPivotObj.transform.SetParent(character.transform);
        headPivotObj.transform.localPosition = headLocalPos.With(x: 0f, z: 0f);
        headPivot = headPivotObj.GetComponent<OrbitCameraPivotBasic>();
        headPivot.SetInfo(new Vector2(0.3f, 0.5f), 1.75f, 65f);
        
        GameObject crouchPivotObj = new GameObject("CrouchPivot", typeof(OrbitCameraPivotBasic));
        crouchPivotObj.transform.SetParent(character.transform);
        crouchPivotObj.transform.localPosition = (headLocalPos).With(x: 0f, z: 0f);
        crouchPivot = crouchPivotObj.GetComponent<OrbitCameraPivotBasic>();
        crouchPivot.SetInfo(new Vector2(0.3f, 0.5f), 1.75f, 60f);
        
        GameObject buttPivotObj = new GameObject("ButtPivot", typeof(OrbitCameraPivotBasic));
        buttPivotObj.transform.SetParent(animator.GetBoneTransform(HumanBodyBones.Hips));
        buttPivotObj.transform.localPosition = Vector3.zero;
        var buttPivot = buttPivotObj.GetComponent<OrbitCameraPivotBasic>();
        buttPivot.SetInfo(new Vector2(0.5f, 0.25f), 1.75f, 70f);
        
        var config = new OrbitCameraCharacterHitmanConfiguration();
        config.SetPivots(character, headPivot, crouchPivot, buttPivot);
        return config;
    }

    void OnGrabChanged(GameObject grab) {
        if (grab == null) {
            basicRagdollPivot.SetFreeze(false);
        } else {
            basicRagdollPivot.SetFreeze(grab.transform.IsChildOf(ragdoller.transform));
        }
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
        OrbitCamera.RemoveConfiguration(lastConfig);
        ragdoller.RagdollEvent -= OnRagdollEvent;
        precisionGrabber.grabChanged -= OnGrabChanged;
        if (GetComponentInParent<Kobold>() != null) {
            GetComponentInParent<Kobold>().sizeInflater.changed -= OnKoboldSizeChange;
        }
    }

    void Update() {
        uiSlider.transform.localPosition = Vector3.Lerp(uiSlider.transform.localPosition, -Vector3.right * (36f * ((int)mode+0.5f)), Time.deltaTime*2f);
    }

    public void OnSwitchCamera() {
        if (mode == null) {
            return;
        }

        int index = ((int)mode.Value + 1) % 3;
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
        possession.enabled = true;
        freeCamController.enabled = false;
        switch (mode) {
            case CameraMode.FirstPerson:
                OrbitCamera.ReplaceConfiguration(lastConfig, firstpersonConfiguration);
                lastConfig = firstpersonConfiguration;
                koboldAnimator.inputShouldFaceEye = true;
                if (!FPSCanvas.activeInHierarchy) {
                    FPSCanvas.SetActive(true);
                }
                break;
            case CameraMode.ThirdPerson:
                koboldAnimator.inputShouldFaceEye = false;
                if (ragdoller.ragdolled) {
                    OrbitCamera.ReplaceConfiguration(lastConfig, thirdpersonRagdollConfiguration);
                    lastConfig = thirdpersonRagdollConfiguration;
                } else {
                    //shoulderPivot.SnapInstant();
                    //buttPivot.SnapInstant();
                    OrbitCamera.ReplaceConfiguration(lastConfig, thirdpersonConfiguration);
                    lastConfig = thirdpersonConfiguration;
                }

                if (!FPSCanvas.activeInHierarchy) {
                    FPSCanvas.SetActive(true);
                }
                break;
            case CameraMode.FreeCam:
                koboldAnimator.inputShouldFaceEye = false;
                var data = OrbitCamera.GetCurrentCameraData();
                freeCamController.SetCameraPosition(data.position+data.rotation*Vector3.back*data.distance);
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
        }

    }
}
