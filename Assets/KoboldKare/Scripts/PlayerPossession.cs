using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldKare;
using UnityEngine.InputSystem;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

public class PlayerPossession : MonoBehaviourPun {
    public UnityEngine.InputSystem.PlayerInput controls;
    public float coyoteTime = 0.2f;
    //public Grabber grabber;
    public User user;
    public CanvasGroup chatGroup;
    public TMPro.TMP_InputField chatInput;
    public GameObject diePrefab;
    [SerializeField]
    private GameEventFloat handVisibilityEvent;
    public InputActionReference back;
    public PrecisionGrabber pGrabber;
    public GameObject dickErectionHidable;
    public Grabber grabber;
    public Camera eyes;

    public Vector2 GetEyeRot() {
        return eyeRot;
    }

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
    public KoboldCharacterController controller;
    public CharacterControllerAnimator characterControllerAnimator;
    public Rigidbody body;
    public Animator animator;
    public GameEventVector3 playerDieEvent;
    public List<GameObject> localGameObjects = new List<GameObject>();
    public GameObject grabPrompt;
    public GameObject equipmentUI;
    private bool switchedMode;
    private bool pauseInput;
    private bool rotating;
    private bool grabbing;
    private bool trackingHip;
    public UnityScriptableSettings.ScriptableSetting mouseSensitivity;
    public void OnPause() {
        if (equipmentUI.activeInHierarchy) {
            equipmentUI.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
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
        if (chatInput != null) {
            chatInput.text="";
            chatInput.onSubmit.RemoveListener(OnTextSubmit);
            chatInput.onDeselect.RemoveListener(OnTextDeselect);
        }
        if (chatGroup != null) {
            chatGroup.interactable = false;
            chatGroup.alpha = 0f;
        }
        controls.ActivateInput();
        back.action.started -= OnBack;
    }
    private void OnTextSubmit(string t) {
        if (!string.IsNullOrEmpty(t)) {
            kobold.SendChat(t);
        }
        chatInput.text="";
        chatGroup.interactable = false;
        chatGroup.alpha = 0f;
        controls.ActivateInput();
        chatInput.onSubmit.RemoveListener(OnTextSubmit);
        chatInput.onDeselect.RemoveListener(OnTextDeselect);
        back.action.started -= OnBack;
    }
    private void Start() {
        if (!isActiveAndEnabled) {
            return;
        }
        controls = GetComponent<PlayerInput>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable() {
        foreach(GameObject localGameObject in localGameObjects) {
            localGameObject.SetActive(true);
        }

        controls.actions["SwitchGrabMode"].performed += OnShiftMode;
        controls.actions["Grab"].performed += OnGrabInput;
        controls.actions["Grab"].canceled += OnGrabCancelled;
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
    }

    private void OnDisable() {
        foreach(GameObject localGameObject in localGameObjects) {
            localGameObject.SetActive(false);
        }
        controls.actions["SwitchGrabMode"].performed -= OnShiftMode;
        controls.actions["Grab"].performed -= OnGrabInput;
        controls.actions["Grab"].canceled -= OnGrabCancelled;
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
    }

    private void OnDestroy() {
        if (gameObject.scene.isLoaded) {
            if (kobold == (Kobold)PhotonNetwork.LocalPlayer.TagObject) {
                GameObject.Instantiate(diePrefab, transform.position, Quaternion.identity);
            }
            playerDieEvent.Raise(transform.position);
        }
    }
    IEnumerator PauseInputForSeconds(float delay) {
        pauseInput = true;
        yield return new WaitForSeconds(delay);
        pauseInput = false;
    }
    private Vector2 eyeRot;

    private void Look(Vector2 delta) {
        if (!rotating && !trackingHip) {
            eyeRot += delta;
        }
        eyeRot.y = Mathf.Clamp(eyeRot.y, -90f, 90f);
        eyeRot.x = Mathf.Repeat(eyeRot.x, 360f);
    }

    void PlayerProcessing() {
        float erectionUp = controls.actions["ErectionUp"].ReadValue<float>();
        float erectionDown = controls.actions["ErectionDown"].ReadValue<float>();
        if (erectionUp-erectionDown != 0f) {
            kobold.PumpUpDick((erectionUp-erectionDown)*Time.deltaTime*0.2f);
        }
        Vector2 moveInput = controls.actions["Move"].ReadValue<Vector2>();
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);

        //pGrabber.inputRotation = rotate;
        Vector2 mouseDelta = Mouse.current.delta.ReadValue() + controls.actions["Look"].ReadValue<Vector2>() * 40f;
        if (trackingHip) {
            characterControllerAnimator.SetHipVector(characterControllerAnimator.GetHipVector() + mouseDelta*0.002f);
        }

        if (!rotating || !pGrabber.TryRotate(mouseDelta * mouseSensitivity.value)) {
            Look(mouseDelta * mouseSensitivity.value);
        }
        eyes.transform.rotation = Quaternion.Euler(-eyeRot.y, eyeRot.x, 0);

        if (!pauseInput) {
            if (grabbing && !switchedMode && !pGrabber.HasGrab()) {
                grabber.TryGrab();
            }
        }

        Quaternion characterRot = Quaternion.Euler(0, eyeRot.x, 0);
        Vector3 wishDir = characterRot*Vector3.forward*move.z + characterRot*Vector3.right*move.x;
        wishDir.y = 0;
        controller.inputDir = wishDir;
    }

    public Vector3 GetEyeDir() {
        return Quaternion.Euler(-eyeRot.y, eyeRot.x, 0) * Vector3.forward;
    }

    // Update is called once per frame
    void Update() {
        if (Cursor.lockState != CursorLockMode.Locked) {
            // Clear the deltas so they don't add up.
            Vector2 mouseDelta = Mouse.current.delta.ReadValue() + controls.actions["Look"].ReadValue<Vector2>() * 40f;
            eyes.transform.rotation = Quaternion.Euler(-eyeRot.y, eyeRot.x, 0);
            controller.inputDir = Vector3.zero;
            controller.inputJump = false;
            return;
        }
        Cursor.visible = false;
        if (isActiveAndEnabled) {
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
            Vector2 mouseDelta = Mouse.current.delta.ReadValue() + controls.actions["Look"].ReadValue<Vector2>() * 40f;
            eyes.transform.rotation = Quaternion.Euler(-eyeRot.y, eyeRot.x, 0);
            controller.inputDir = Vector3.zero;
            controller.inputJump = false;
        }
        if (kobold.activeDicks.Count > 0 && !dickErectionHidable.activeInHierarchy) {
            dickErectionHidable.SetActive(true);
        }
        if (kobold.activeDicks.Count == 0 && dickErectionHidable.activeInHierarchy) {
            dickErectionHidable.SetActive(false);
        }
        characterControllerAnimator.SetEyeRot(GetEyeRot());
    }
    public void OnJump(InputValue value) {
        controller.inputJump = value.Get<float>() > 0f;
        if (!photonView.IsMine) {
            photonView.RequestOwnership();
        }
    }

    public void OnShiftMode(InputAction.CallbackContext ctx) {
        bool shift = ctx.ReadValue<float>() > 0f;
        handVisibilityEvent.Raise(ctx.ReadValue<float>());
        switchedMode = shift;
        pGrabber.SetPreviewState(shift);
        if (!shift) {
            StartCoroutine(PauseInputForSeconds(0.5f));
        }
    }

    public void OnWalk(InputValue value) {
        controller.inputWalking = value.Get<float>() > 0f;
    }
    public void OnCrouch(InputValue value) {
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
        float delta = ctx.ReadValue<float>();
        pGrabber.TryAdjustDistance(delta * 0.0005f);
    }
    
    public void OnCrouchAdjustInput(InputAction.CallbackContext ctx) {
        float delta = ctx.ReadValue<float>();
        if (!pGrabber.TryAdjustDistance(0f)) {
            controller.SetInputCrouched(controller.GetInputCrouched() - delta * 0.0005f);
        }
    }

    public void OnActivateGrabInput(InputAction.CallbackContext ctx) {
        grabber.TryActivate();
        pGrabber.TryFreeze();
        StartCoroutine(PauseInputForSeconds(0.5f));
    }
    public void OnActivateGrabCancelled(InputAction.CallbackContext ctx) {
        grabber.TryStopActivate();
    }
    public void OnUnfreezeInput(InputAction.CallbackContext ctx) {
        pGrabber.TryUnfreeze();
    }
    public void OnUnfreezeAllInput(InputAction.CallbackContext ctx) {
        pGrabber.UnfreezeAll();
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
    public void OnChat() {
        if (!enabled || GameManager.instance.isPaused) {
            return;
        }
        if (!chatGroup.interactable) {
            chatGroup.interactable = true;
            chatGroup.alpha = 1f;
            chatInput.Select();
            if (inputRagdolled) {
                kobold.ragdoller.PopRagdoll();
            }

            back.action.started += OnBack;
            StopCoroutine(nameof(WaitAndThenSubscribe));
            StartCoroutine(nameof(WaitAndThenSubscribe));
            controls.DeactivateInput();
            chatInput.onDeselect.AddListener(OnTextDeselect);
        }
    }
    IEnumerator WaitAndThenSubscribe() {
        yield return new WaitForSecondsRealtime(0.25f);
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

    public void OnViewStats( InputValue value ) {
        if (value.Get<float>() >= 0.5f) {
            if (equipmentUI.activeInHierarchy) {
                equipmentUI.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            } else {
                equipmentUI.SetActive(true);
                equipmentUI.GetComponentInChildren<Selectable>()?.Select();
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}
