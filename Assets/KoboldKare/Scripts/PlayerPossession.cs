using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldKare;
using UnityEngine.InputSystem;
using Photon.Pun;
using Steamworks;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;
using Cursor = UnityEngine.Cursor;

public class PlayerPossession : MonoBehaviourPun {
    public PlayerInput controls;
    public float coyoteTime = 0.2f; 
    public int defaultMultiGrabSwitchFrames = 15;
    public User user;
    public CanvasGroup chatGroup;
    public CommandTextDisplay chatDisplay;
    public ScrollRect chatScrollView;
    public TMPro.TMP_InputField chatInput;
    public GameObject diePrefab;
    public InputActionReference back;
    private PrecisionGrabber pGrabber;
    public GameObject dickErectionHidable;
    private Grabber grabber;

    public bool inputRagdolled;
    private Kobold cachedKobold;
    public Kobold kobold {
        get {
            if (cachedKobold == null) {
                cachedKobold = GetComponentInParent<Kobold>();
            }
            return cachedKobold;
        }
    }
    private KoboldCharacterController controller;
    private CharacterControllerAnimator characterControllerAnimator;
    private Rigidbody body;
    private Animator animator;
    public GameEventVector3 playerDieEvent;
    public List<GameObject> localGameObjects = new List<GameObject>();
    public GameObject grabPrompt;
    public GameObject equipmentUI;
    private bool switchedMode;
    private bool pauseInput;
    private bool rotating;
    private bool grabbing;
    private bool trackingHip;
    private int multiGrabSwitchTimer;
    public bool multiGrabMode = true;
    private InputSystemUIInputModule inputModule;
    public UnityScriptableSettings.SettingFloat mouseSensitivity;

    
    [SerializeField]
    private List<GameObject> activateUI;
    [SerializeField]
    private List<GameObject> throwUI;
    
    [SerializeField]
    private List<GameObject> freezeUI;
    
    [SerializeField]
    private List<GameObject> shiftGrabUI;

    [SerializeField]
    private List<GameObject> multiGrabSwitchUi;

    void OnThrowChange(bool newThrowStatus) {
        foreach (var throwElement in throwUI) {
            if (throwElement.activeInHierarchy != newThrowStatus) {
                throwElement.SetActive(newThrowStatus);
            }
        }
    }
    void OnFreezeChange(bool newFreezeStatus) {
        foreach (var freezeElement in freezeUI) {
            if (freezeElement.activeInHierarchy != newFreezeStatus) {
                freezeElement.SetActive(newFreezeStatus);
            }
        }
    }
    void OnShiftGrabChange(bool shiftGrabUIStatus) {
        foreach (var shiftGrabUIElement in shiftGrabUI) {
            if (shiftGrabUIElement.activeInHierarchy != shiftGrabUIStatus) {
                shiftGrabUIElement.SetActive(shiftGrabUIStatus);
            }
        }
    }
    void OnActivateChange(bool newActivateStatus) {
        foreach (var activateElement in activateUI) {
            if (activateElement.activeInHierarchy != newActivateStatus) {
                activateElement.SetActive(newActivateStatus);
            }
        }
    }
    public void OnPause() {
        if (!isActiveAndEnabled) {
            return;
        }

        if (GetEquipmentUI()) {
            SetEquipmentUI(false);
            return;
        }
        GameManager.instance.Pause(!GameManager.instance.isPaused);
        if (GameManager.instance.isPaused) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        } else {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    private void OnTextDeselect(string t) {
    }

    private void OnCancelTextSubmit(InputAction.CallbackContext ctx) {
        OnTextSubmit("");
    }

    private void Awake() {
        controller = GetComponentInParent<KoboldCharacterController>();
        characterControllerAnimator = GetComponentInParent<CharacterControllerAnimator>();
        pGrabber = controller.GetComponentInChildren<PrecisionGrabber>();
        pGrabber.activeUIChanged -= OnShiftGrabChange;
        pGrabber.freezeUIChanged -= OnFreezeChange;
        pGrabber.activeUIChanged += OnShiftGrabChange;
        pGrabber.freezeUIChanged += OnFreezeChange;
        grabber = controller.GetComponentInChildren<Grabber>();
        grabber.activateUIChanged -= OnActivateChange;
        grabber.throwUIChanged -= OnThrowChange;
        grabber.activateUIChanged += OnActivateChange;
        grabber.throwUIChanged += OnThrowChange;
        body = controller.GetComponent<Rigidbody>();
        animator = controller.GetComponentInChildren<Animator>();
        inputModule = FindObjectOfType<InputSystemUIInputModule>();
    }

    private void OnTextSubmit(string t) {
        chatInput.text="";
        chatGroup.interactable = false;
        chatInput.enabled = false;
        chatGroup.alpha = 0f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (isActiveAndEnabled) {
            controls.ActivateInput();
        }

        chatInput.onSubmit.RemoveListener(OnTextSubmit);
        chatInput.onDeselect.RemoveListener(OnTextDeselect);
        chatScrollView.normalizedPosition = new Vector2(0, 0);
        back.action.started -= OnBack;
        chatDisplay.ForceVisible(false);
        if (!string.IsNullOrEmpty(t)) {
            kobold.photonView.RPC(nameof(Chatter.RPCSendChat), RpcTarget.All, t);
        }
        inputModule.cancel.action.performed -= OnCancelTextSubmit;
        StopCoroutine(nameof(MaintainFocus));
        if (SteamManager.Initialized) {
            SteamUtils.DismissFloatingGamepadTextInput();
        }
    }
    private void Start() {
        controls = GetComponent<PlayerInput>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable() {
        foreach(GameObject localGameObject in localGameObjects) {
            localGameObject.SetActive(true);
        }

        if (controls == null) {
            controls = GetComponent<PlayerInput>();
        }
        
        controls.actions["SwitchGrabMode"].performed += OnShiftMode;
        controls.actions["Grab"].performed += OnGrabInput;
        controls.actions["Grab"].canceled += OnGrabCancelled;
        controls.actions["Gib"].performed += OnGibInput;
        controls.actions["Rotate"].performed += OnRotateInput;
        controls.actions["Rotate"].canceled += OnRotateCancelled;
        controls.actions["ActivateGrab"].performed += OnActivateGrabInput;
        controls.actions["ActivateGrab"].canceled += OnActivateGrabCancelled;
        controls.actions["Unfreeze"].performed += OnUnfreezeInput;
        controls.actions["UnfreezeAll"].performed += OnUnfreezeAllInput;
        controls.actions["Grab Push and Pull"].performed += OnGrabPushPull;
        controls.actions["CrouchAdjust"].performed += OnCrouchAdjustInput;
        controls.actions["HipControl"].performed += OnActivateHipInput;
        controls.actions["HipControl"].canceled += OnCanceledHipInput;
        controls.actions["ResetHip"].performed += OnResetHipInput;
        controls.actions["Chat"].performed += OnChatInput;
        if (grabber != null) {
            grabber.activateUIChanged -= OnActivateChange;
            grabber.throwUIChanged -= OnThrowChange;
            grabber.activateUIChanged += OnActivateChange;
            grabber.throwUIChanged += OnThrowChange;
        }
        if (pGrabber != null) {
            pGrabber.activeUIChanged -= OnShiftGrabChange;
            pGrabber.freezeUIChanged -= OnFreezeChange;
            pGrabber.activeUIChanged += OnShiftGrabChange;
            pGrabber.freezeUIChanged += OnFreezeChange;
        }
        OrbitCamera.SetPlayerInput(controls);
    }

    private void OnDisable() {
        OnTextSubmit("");
        foreach(GameObject localGameObject in localGameObjects) {
            localGameObject.SetActive(false);
        }
        controls.actions["SwitchGrabMode"].performed -= OnShiftMode;
        controls.actions["Grab"].performed -= OnGrabInput;
        controls.actions["Grab"].canceled -= OnGrabCancelled;
        controls.actions["Gib"].performed -= OnGibInput;
        controls.actions["Rotate"].performed -= OnRotateInput;
        controls.actions["Rotate"].canceled -= OnRotateCancelled;
        controls.actions["ActivateGrab"].performed -= OnActivateGrabInput;
        controls.actions["ActivateGrab"].canceled -= OnActivateGrabCancelled;
        controls.actions["Unfreeze"].performed -= OnUnfreezeInput;
        controls.actions["UnfreezeAll"].performed -= OnUnfreezeAllInput;
        controls.actions["Grab Push and Pull"].performed -= OnGrabPushPull;
        controls.actions["CrouchAdjust"].performed -= OnCrouchAdjustInput;
        controls.actions["HipControl"].performed -= OnActivateHipInput;
        controls.actions["HipControl"].canceled -= OnCanceledHipInput;
        controls.actions["ResetHip"].performed -= OnResetHipInput;
        controls.actions["Chat"].performed -= OnChatInput;
        
        if (grabber != null) {
            grabber.activateUIChanged -= OnActivateChange;
            grabber.throwUIChanged -= OnThrowChange;
        }

        if (pGrabber != null) {
            pGrabber.activeUIChanged -= OnShiftGrabChange;
            pGrabber.freezeUIChanged -= OnFreezeChange;
        }
        OrbitCamera.SetPlayerInput(null);
    }

    private void OnDestroy() {
        if (gameObject.scene.isLoaded) {
            if (kobold == (Kobold)PhotonNetwork.LocalPlayer.TagObject) {
                Instantiate(diePrefab, transform.position, Quaternion.identity);
            }
            playerDieEvent.Raise(transform.position);
        }
    }
    IEnumerator PauseInputForSeconds(float delay) {
        pauseInput = true;
        yield return new WaitForSeconds(delay);
        pauseInput = false;
    }
    void PlayerProcessing() {
        float erectionUp = controls.actions["ErectionUp"].ReadValue<float>();
        float erectionDown = controls.actions["ErectionDown"].ReadValue<float>();
        if (erectionUp-erectionDown != 0f) {
            kobold.PumpUpDick((erectionUp-erectionDown*2f)*Time.deltaTime*0.3f);
        }
        Vector2 moveInput = controls.actions["Move"].ReadValue<Vector2>();
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);

        //pGrabber.inputRotation = rotate;
        //Quaternion gyro = DS4.GetRotation(3000);
        Quaternion gyro = Quaternion.identity;
        Vector3 rawRotation = gyro.eulerAngles;
        if (controls.actions["GyroEnable"].ReadValue<float>() > 0.5f) {
            rawRotation = new Vector3(Mathf.DeltaAngle(0, rawRotation.x), Mathf.DeltaAngle(0, rawRotation.y),
                Mathf.DeltaAngle(0, rawRotation.z));
        } else {
            rawRotation = Vector3.zero;
        }
        Vector2 gyroDelta = new Vector2(-rawRotation.z + rawRotation.y, -rawRotation.x);
        Vector2 mouseDelta = gyroDelta + controls.actions["Look"].ReadValue<Vector2>() + controls.actions["LookJoystick"].ReadValue<Vector2>();
        bool rotatingProp = rotating && pGrabber.TryRotate(mouseDelta * mouseSensitivity.GetValue());
        
        if (trackingHip && !rotatingProp) {
            characterControllerAnimator.SetHipVector(characterControllerAnimator.GetHipVector() + mouseDelta*0.002f);
        }

        if (rotating || rotatingProp || trackingHip) {
            OrbitCamera.SetTracking(false);
        } else {
            OrbitCamera.SetTracking(true);
        }

        if (multiGrabSwitchTimer > 0) {
            multiGrabSwitchTimer -= 1;
        }

        if (!pauseInput) {
            if (grabbing && !switchedMode && !pGrabber.HasGrab()) {
                grabber.TryGrab(multiGrabMode);
            }
        }

        Quaternion characterRot = Quaternion.Euler(0, OrbitCamera.GetPlayerIntendedScreenAim().x, 0);
        Vector3 wishDir = characterRot*Vector3.forward*move.z + characterRot*Vector3.right*move.x;
        wishDir.y = 0;
        controller.inputDir = wishDir;
    }

    // Update is called once per frame
    void Update() {
        if (Cursor.lockState != CursorLockMode.Locked) {
            // Clear the deltas so they don't add up.
            Vector2 mouseDelta = controls.actions["Look"].ReadValue<Vector2>() + controls.actions["LookJoystick"].ReadValue<Vector2>();
            controller.inputDir = Vector3.zero;
            controller.inputJump = false;
            OrbitCamera.SetTracking(false);
            return;
        }
        Cursor.visible = false;
        if (isActiveAndEnabled && !GameManager.instance.isPaused) {
            PlayerProcessing();
            bool shouldCancelAnimation = false;
            //shouldCancelAnimation = (shouldCancelAnimation | controls.actions["Rotate"].ReadValue<float>() > 0.5f);
            //shouldCancelAnimation = (shouldCancelAnimation | controls.actions["Unfreeze"].ReadValue<float>() > 0.5f);
            shouldCancelAnimation = (shouldCancelAnimation | controls.actions["Jump"].ReadValue<float>() > 0.5f);
            shouldCancelAnimation = (shouldCancelAnimation | controls.actions["Gib"].ReadValue<float>() > 0.5f);
            shouldCancelAnimation = (shouldCancelAnimation | controls.actions["Ragdoll"].ReadValue<float>() > 0.5f);
            shouldCancelAnimation = (shouldCancelAnimation | controls.actions["Cancel"].ReadValue<float>() > 0.5f);
            if (shouldCancelAnimation) {
                photonView.RPC(nameof(CharacterControllerAnimator.StopAnimationRPC), RpcTarget.All);
            }
        } else {
            Vector2 mouseDelta = controls.actions["Look"].ReadValue<Vector2>() + controls.actions["LookJoystick"].ReadValue<Vector2>();
            controller.inputDir = Vector3.zero;
            controller.inputJump = false;
        }
        if (kobold.activeDicks.Count > 0 && !dickErectionHidable.activeInHierarchy) {
            dickErectionHidable.SetActive(true);
        }
        if (kobold.activeDicks.Count == 0 && dickErectionHidable.activeInHierarchy) {
            dickErectionHidable.SetActive(false);
        }
        if (kobold.GetGenes().grabCount > 1 && !multiGrabSwitchUi[0].activeInHierarchy)
        {
            multiGrabSwitchUi[0].SetActive(true);
        }
        if (kobold.GetGenes().grabCount == 1 && multiGrabSwitchUi[0].activeInHierarchy)
        {
            multiGrabSwitchUi[0].SetActive(false);
        }
        characterControllerAnimator.SetEyeRot(OrbitCamera.GetPlayerIntendedScreenAim());
    }
    public void OnJump(InputValue value) {
        if (!isActiveAndEnabled) return;
        controller.inputJump = value.Get<float>() > 0f;
        if (!photonView.IsMine) {
            photonView.RequestOwnership();
        }
    }

    public void OnShiftMode(InputAction.CallbackContext ctx) {
        bool shift = ctx.ReadValue<float>() > 0f;
        switchedMode = shift;
        PrecisionGrabber.SetPinVisibility(shift);
        pGrabber.SetPreviewState(shift);
        if (!shift) {
            StartCoroutine(PauseInputForSeconds(0.5f));
            if (multiGrabSwitchTimer > 0 && multiGrabSwitchUi[0].activeInHierarchy) {
                multiGrabMode = !multiGrabMode;
                multiGrabSwitchUi[1].SetActive(multiGrabMode);
                multiGrabSwitchUi[2].SetActive(!multiGrabMode);
            }
        }
        else {
            multiGrabSwitchTimer = defaultMultiGrabSwitchFrames;
        }
    }

    public void OnWalk(InputValue value) {
        controller.inputWalking = value.Get<float>() > 0f;
    }
    public void OnCrouch(InputValue value) {
        if (!isActiveAndEnabled) return;
        //controller.inputCrouched = value.Get<float>();
        controller.SetInputCrouched(value.Get<float>());
    }
    public void OnGib() {
        //playerDieEvent.Raise(transform.position);
        //spoilable.spoilIntensity = 1f;
        //spoilable.OnSpoilEvent.Invoke();
    }
    public void OnUse() {
        user.Use();
    }
    public void OnGrabInput(InputAction.CallbackContext ctx) {
        grabbing = true;
        if (switchedMode) {
            pGrabber.TryGrab();
        }
    }
    
    public void OnResetHipInput(InputAction.CallbackContext ctx) {
        characterControllerAnimator.SetHipVector(Vector2.zero);
    }
    public void OnActivateHipInput(InputAction.CallbackContext ctx) {
        trackingHip = true;
    }
    
    public void OnCanceledHipInput(InputAction.CallbackContext ctx) {
        trackingHip = false;
    }

    public void OnGrabCancelled(InputAction.CallbackContext ctx) {
        grabbing = false;
        grabber.TryDrop();
        pGrabber.TryDrop();
    }

    public void OnRotateInput(InputAction.CallbackContext ctx) {
        rotating = true;
    }
    public void OnRotateCancelled(InputAction.CallbackContext ctx) {
        rotating = false;
    }

    public void OnGrabPushPull(InputAction.CallbackContext ctx) {
        if (ctx.control.device is Mouse) {
            float delta = ctx.ReadValue<float>();
            pGrabber.TryAdjustDistance(delta * 0.0005f);
        } else {
            float delta = ctx.ReadValue<float>();
            pGrabber.TryAdjustDistance(delta * 0.25f);
        }
    }
    
    public void OnCrouchAdjustInput(InputAction.CallbackContext ctx) {
        if (ctx.control.device is Mouse) {
            float delta = ctx.ReadValue<float>();
            if (!pGrabber.TryAdjustDistance(0f)) {
                controller.SetInputCrouched(controller.GetInputCrouched() - delta * 0.0005f);
            }
        } else {
            float target = ctx.ReadValue<float>();
            if (!pGrabber.TryAdjustDistance(0f)) {
                controller.SetInputCrouched(target);
            }
        }

    }

    void OnActivateGrabInput(InputAction.CallbackContext ctx) {
        grabber.TryActivate();
        pGrabber.TryFreeze();
        StartCoroutine(PauseInputForSeconds(0.5f));
    }
    void OnActivateGrabCancelled(InputAction.CallbackContext ctx) {
        grabber.TryStopActivate();
    }
    void OnUnfreezeInput(InputAction.CallbackContext ctx) {
        pGrabber.TryUnfreeze();
    }
    void OnUnfreezeAllInput(InputAction.CallbackContext ctx) {
        pGrabber.UnfreezeAll();
    }

    void OnGibInput(InputAction.CallbackContext ctx) {
        if (photonView.IsMine || (Kobold)PhotonNetwork.LocalPlayer.TagObject == kobold) {
            PhotonNetwork.Destroy(kobold.gameObject);
        }
    }

    public void OnResetCamera() {
        //cameras[1].transform.localPosition = Vector3.zero;
        //cameras[1].transform.localRotation = Quaternion.identity;
    }
    public void OnLook(InputValue value) {
    }
    public void OnBack(InputAction.CallbackContext context) {
        OnTextDeselect("");
    }
    public void OnChatInput(InputAction.CallbackContext context) {
        if (GameManager.instance.isPaused) {
            return;
        }
        if (!chatGroup.interactable) {
            StartCoroutine(nameof(MaintainFocus));

            chatInput.enabled = true;
            chatDisplay.ForceVisible(true);
            chatGroup.interactable = true;
            chatGroup.alpha = 1f;
            if (inputRagdolled) {
                photonView.RPC(nameof(Ragdoller.PopRagdoll), RpcTarget.All);
            }

            back.action.started += OnBack;
            StopCoroutine(nameof(WaitAndThenSubscribe));
            StartCoroutine(nameof(WaitAndThenSubscribe));
            
            controls.DeactivateInput();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            inputModule.cancel.action.performed += OnCancelTextSubmit;
            if (SteamManager.Initialized) {
                RectTransform rectTransform = chatInput.GetComponent<RectTransform>();
                SteamUtils.ShowFloatingGamepadTextInput(EFloatingGamepadTextInputMode.k_EFloatingGamepadTextInputModeModeSingleLine, (int)rectTransform.position.x, (int)rectTransform.position.y, (int)rectTransform.sizeDelta.x, (int)rectTransform.sizeDelta.y);
            }
            chatInput.Select();
            chatInput.onDeselect.AddListener(OnTextDeselect);
        } else {
            OnTextSubmit(chatInput.text);
        }
    }

    IEnumerator MaintainFocus() {
        yield return new WaitForSecondsRealtime(0.1f);
        while (chatGroup.interactable && isActiveAndEnabled) {
            yield return new WaitForSecondsRealtime(0.1f);
            if (EventSystem.current.currentSelectedGameObject != chatInput.gameObject) {
                EventSystem.current.SetSelectedGameObject(chatInput.gameObject);
            }
            chatInput.ActivateInputField();
        }
    }

    IEnumerator WaitAndThenSubscribe() {
        yield return new WaitForSecondsRealtime(0.1f);
        chatInput.onSubmit.AddListener(OnTextSubmit);
    }
    public void OnRagdoll( InputValue value ) {
        if (value.Get<float>() <= 0.5f) {
            photonView.RPC(nameof(Ragdoller.PopRagdoll), RpcTarget.All);
            inputRagdolled = false;
        } else {
            photonView.RPC(nameof(Ragdoller.PushRagdoll), RpcTarget.All);
            inputRagdolled = true;
        }
    }

    public void SetEquipmentUI(bool enable) {
        if (!isActiveAndEnabled) {
            return;
        }

        if (!enable) {
            equipmentUI.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        } else {
            equipmentUI.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public bool GetEquipmentUI() {
        return equipmentUI.activeInHierarchy;
    }

    public void ToggleEquipmentUI() {
        if (!isActiveAndEnabled) {
            return;
        }

        SetEquipmentUI(!GetEquipmentUI());
    }

    public void OnViewStats( InputValue value ) {
        if (value.Get<float>() >= 0.5f) {
            ToggleEquipmentUI();
        }
    }
    // This fixes a bug where OnRagdoll isn't called when the application isn't in focus.
    void OnApplicationFocus(bool hasFocus) {
        if (hasFocus && inputRagdolled) {
            photonView.RPC(nameof(Ragdoller.PopRagdoll), RpcTarget.All);
            inputRagdolled = false;
        }
    }
}
