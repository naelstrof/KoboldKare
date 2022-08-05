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
using Object = UnityEngine.Object;

public class PlayerPossession : MonoBehaviourPun, IPunObservable, ISavable {
    public UnityEngine.InputSystem.PlayerInput controls;
    public float coyoteTime = 0.2f;
    private bool canGrab = true;
    //public Grabber grabber;
    public User user;
    public CanvasGroup chatGroup;
    public TMPro.TMP_InputField chatInput;
    public GameObject diePrefab;
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
    public bool mouseAttached = true;
    public List<GameObject> localGameObjects = new List<GameObject>();
    public GameObject grabPrompt;
    public GameObject equipmentUI;
    private bool switchedMode = false;
    private bool freeze = false;
    private bool pauseInput = false;
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
        grabber.gameObject.SetActive(true);
        foreach(GameObject localGameObject in localGameObjects) {
            localGameObject.SetActive(true);
        }
    }

    private void OnDisable() {
        grabber.gameObject.SetActive(false);
        foreach(GameObject localGameObject in localGameObjects) {
            localGameObject.SetActive(false);
        }
    }

    private void OnDestroy() {
        if (gameObject.scene.isLoaded) {
            if (kobold == ((Kobold)PhotonNetwork.LocalPlayer.TagObject)) {
                GameObject.Instantiate(diePrefab, transform.position, Quaternion.identity);
            }
            playerDieEvent.Raise(transform.position);
        }
    }
    private Vector2 eyeSensitivity = new Vector2(2f,2f);
    IEnumerator PauseInputForSeconds(float delay) {
        pauseInput = true;
        yield return new WaitForSeconds(delay);
        pauseInput = false;
    }
    private Vector2 eyeRot = new Vector2(0, 0);
    void PlayerProcessing() {
        bool grab = controls.actions["Grab"].ReadValue<float>() > 0.5f && canGrab;
        bool activateGrab = controls.actions["ActivateGrab"].ReadValue<float>() > 0.5f && canGrab;
        bool rotate = controls.actions["Rotate"].ReadValue<float>() > 0.5f;
        bool unfreeze = controls.actions["Unfreeze"].ReadValue<float>() > 0.5f;
        bool walk = controls.actions["Walk"].ReadValue<float>() > 0.5f;
        bool switchGrabMode = controls.actions["SwitchGrabMode"].ReadValue<float>() > 0.5f;
        float erectionUp = controls.actions["ErectionUp"].ReadValue<float>();
        float erectionDown = controls.actions["ErectionDown"].ReadValue<float>();
        if (erectionUp-erectionDown != 0f) {
            kobold.PumpUpDick((erectionUp-erectionDown)*Time.deltaTime*0.2f);
        }
        Vector2 moveInput = controls.actions["Move"].ReadValue<Vector2>();
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);

        pGrabber.inputRotation = rotate;
        Vector2 mouseDelta;
        if (grab && rotate) {
            mouseDelta = Vector2.zero;
        } else {
            mouseDelta = Mouse.current.delta.ReadValue() + controls.actions["Look"].ReadValue<Vector2>() * 40f;
        }
        if (mouseAttached) {
            eyeRot += mouseDelta * mouseSensitivity.value;
        }
        eyeRot.y = Mathf.Clamp(eyeRot.y, -90f, 90f);
        while(eyeRot.x > 360 ) {
            eyeRot.x -= 360;
        }
        while (eyeRot.x < 0) {
            eyeRot.x += 360;
        }
        eyes.transform.rotation = Quaternion.Euler(-eyeRot.y, eyeRot.x, 0);

        if (!pauseInput) {
            if (switchGrabMode && !switchedMode) {
                switchedMode = true;
                //pGrabber.loosenCursor = true;
                grabPrompt.SetActive(false);
            }
            if (!switchGrabMode && switchedMode && !pGrabber.grabbing) {
                //pGrabber.loosenCursor = false;
                //pGrabber.ResetCursor();
                mouseAttached = true;
                switchedMode = false;
                grabPrompt.SetActive(true);
            }
            if (!switchGrabMode && !pGrabber.grabbing) {
                pGrabber.Ungrab();
                if (grab) {
                    grabber.TryGrab();
                }
                if (!pGrabber.HandHidden()) {
                    pGrabber.HideHand(true);
                }
            } else if (switchGrabMode && !grabber.grabbing) {
                grabber.TryDrop();
                grabber.TryStopActivate();
                if (pGrabber.HandHidden()) {
                    pGrabber.HideHand(false);
                }
                if (grab && !freeze) {
                    pGrabber.Grab();
                }
            }
            if (!grab) {
                grabber.TryDrop();
                pGrabber.Ungrab();
            }
            if (!activateGrab) {
                grabber.TryStopActivate();
            }
            if (activateGrab) {
                grabber.TryActivate();
                if (!freeze) {
                    freeze = true;
                    pGrabber.Freeze();
                    StartCoroutine(PauseInputForSeconds(0.5f));
                }
            } else {
                freeze = false;
            }
            if (unfreeze) {
                pGrabber.Unfreeze(true);
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
            if (photonView.IsMine && photonView.AmOwner) {
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

    public IEnumerator CoyoteGrab() {
        canGrab = false;
        yield return new WaitForSeconds(coyoteTime);
        canGrab = true;
    }
    public void OnJump(InputValue value) {
        controller.inputJump = value.Get<float>() > 0f;
        if (!photonView.IsMine) {
            photonView.RequestOwnership();
        }
    }
    public void OnWalk(InputValue value) {
        controller.inputWalking = value.Get<float>() > 0f;
    }
    public void OnCrouch(InputValue value) {
        controller.inputCrouched = value.Get<float>() > 0f;
    }
    public void OnGib() {
        //playerDieEvent.Raise(transform.position);
        //spoilable.spoilIntensity = 1f;
        //spoilable.OnSpoilEvent.Invoke();
    }
    public void OnUse() {
        user.Use();
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
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            bool switchGrabMode = controls.actions["SwitchGrabMode"].ReadValue<float>() > 0.5f;
            stream.SendNext(switchGrabMode);
        } else {
            pGrabber.HideHand(!(bool)stream.ReceiveNext());
        }
    }

    public void Save(BinaryWriter writer, string version) {
        bool switchGrabMode = controls.actions["SwitchGrabMode"].ReadValue<float>() > 0.5f;
        writer.Write(switchGrabMode);
        writer.Write(gameObject.activeInHierarchy);
    }

    public void Load(BinaryReader reader, string version) {
        pGrabber.HideHand(!reader.ReadBoolean());
        bool isPlayer = reader.ReadBoolean();
        gameObject.SetActive(isPlayer);
        if (isPlayer) {
            PhotonNetwork.LocalPlayer.TagObject = kobold;
            //FIXME: just need to destroy death prefab, since we have a kobold now.
            CameraOrbiter input = Object.FindObjectOfType<CameraOrbiter>();
            if (input != null) {
                Destroy(input.transform.parent.gameObject);
            }
            eyeRot = new Vector2(-eyes.transform.eulerAngles.y+180f,eyes.transform.eulerAngles.x);
        }
    }
}
