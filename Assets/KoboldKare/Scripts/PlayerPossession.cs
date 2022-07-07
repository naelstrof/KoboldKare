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
    public GenericSpoilable spoilable;
    public GameObject dickErectionHidable;
    public Grabber grabber;
    public Camera eyes;
    private bool internalInputRagdolled;
    public bool inputRagdolled {
        get {
            if (isActiveAndEnabled) {
                return internalInputRagdolled;
            }
            return false;
        }
        set {
            internalInputRagdolled = inputRagdolled;
        }
    }
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
    //public SimpleCa
    public bool mouseAttached = true;
    //public Transform hip;
    //public Transform hipTarget;
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
        controls?.ActivateInput();
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
        //if (!photonView.IsMine) {
            //eyes.gameObject.SetActive(false);
            //return;
        //}
        //animator.updateMode = AnimatorUpdateMode.Normal;
        if (!isActiveAndEnabled) {
            return;
        }
        //rig.layers[0].active = true;
        body.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        //controller.GetComponent<Kobold>().uprightForce *= 10f;
        //controller.speed *= 2.9f;
        //controller.crouchSpeed *= 3.1f;
        //controller.airAccel *= 1.5f;
        controls = GetComponent<PlayerInput>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        foreach(GameObject localGameObject in localGameObjects) {
            localGameObject.SetActive(true);
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
    void FixedUpdate() {
        //if (!photonView.IsMine) {
            //return;
        //}
        /*if (kobold.uprightTimer <= 0f) {
            body.maxAngularVelocity = 12f;
            body.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            Quaternion bodyRot = body.rotation;
            Vector3 v = bodyRot.eulerAngles;
            body.MoveRotation(Quaternion.Euler(v.With(x : 0, z : 0)));

            Quaternion characterRot = Quaternion.Euler(0, eyeRot.x, 0);
            Vector3 fdir = characterRot * Vector3.forward;
            Vector3 dampingForce = Vector3.Project(body.angularVelocity, body.transform.up)*0.5f;
            Quaternion rotForce = Quaternion.FromToRotation(body.transform.forward, fdir);
            body.angularVelocity -= dampingForce;
            body.AddTorque(new Vector3(rotForce.x,rotForce.y,rotForce.z)*5f, ForceMode.VelocityChange);
            //rig.layers[0].active = true;
        } else {
            //rig.layers[0].active = false;
            body.maxAngularVelocity = 7f;
            body.constraints = body.constraints & ~(RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ);
        }*/
        Quaternion characterRot = Quaternion.Euler(0, eyeRot.x, 0);
        Vector3 fdir = characterRot * Vector3.forward;
        float deflectionForgivenessDegrees = 5f;
        Vector3 cross = Vector3.Cross(body.transform.forward, fdir);
        float angleDiff = Mathf.Max(Vector3.Angle(body.transform.forward, fdir) - deflectionForgivenessDegrees, 0f);
        body.AddTorque(cross*angleDiff*5f, ForceMode.Acceleration);
    }
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
        //float angle = Quaternion.Angle(rotLerp, characterRot);
        //rotLerp = Quaternion.RotateTowards(rotLerp, characterRot, Time.deltaTime * angle * 6f);
        //playerModel.transform.rotation = rotLerp;

        //if (rig.layers[0].active) {
            //hipTarget.position = hip.position;
            //hipTarget.rotation = hip.rotation;
        //}


        Quaternion characterRot = Quaternion.Euler(0, eyeRot.x, 0);
        Vector3 wishDir = characterRot*Vector3.forward*move.z + characterRot*Vector3.right*move.x;
        wishDir.y = 0;
        controller.inputDir = wishDir;
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
                    //kobold.GetComponent<CharacterControllerAnimator>().OnEndStation();
                    characterControllerAnimator.StopAnimation();
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
    }

    public IEnumerator CoyoteGrab() {
        canGrab = false;
        yield return new WaitForSeconds(coyoteTime);
        canGrab = true;
    }

    /*public bool ShouldSave() {
        return true;
    }
    public void Load(SaveObject s, int version) {
        transform.position = s.vectors["position"];
        eyeRot = new Vector2(s.vectors["EyeRot"].x, s.vectors["EyeRot"].y);
        Update();
        GetComponent<Rigidbody>().velocity = s.vectors["Velocity"];
        controller.crouchAmount = s.floats["CrouchTimer"];
    }

    public SaveObject Save(int version) {
        SaveObject s = new SaveObject();
        s.id = ScriptableSaveLibrary.SaveID.Player;
        s.vectors["Velocity"] = GetComponent<Rigidbody>().velocity;
        s.vectors["position"] = transform.position;
        s.quats["rotation"] = Quaternion.identity;
        s.vectors["EyeRot"] = new Vector3(eyeRot.x, eyeRot.y, 0);
        s.floats["CrouchTimer"] = controller.crouchAmount;
        return s;
    }*/

    //public void OnMove(InputValue value) {
        //Vector2 move2 = value.Get<Vector2>();
        //move = new Vector3(move2.x, 0, move2.y);

        //Quaternion characterRot = Quaternion.Euler(0, eyeRot.x, 0);
        //Vector3 wishDir = characterRot*Vector3.forward*move.z + characterRot*Vector3.right*move.x;
        //wishDir.y = 0;
        //controller.inputDir = wishDir.magnitude > 0 ? Vector3.Normalize(wishDir) : Vector3.zero;
    //}
    public void OnJump(InputValue value) {
        controller.inputJump = value.Get<float>() > 0f;
    }
    public void OnWalk(InputValue value) {
        controller.inputWalking = value.Get<float>() > 0f;
    }
    public void OnCrouch(InputValue value) {
        controller.inputCrouched = value.Get<float>() > 0f;
    }
    public void OnGib() {
        //playerDieEvent.Raise(transform.position);
        spoilable.spoilIntensity = 1f;
        spoilable.OnSpoilEvent.Invoke();
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
            back.action.started += OnBack;
            StopCoroutine("WaitAndThenSubscribe");
            StartCoroutine("WaitAndThenSubscribe");
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
            kobold.ragdoller.PopRagdoll();
            inputRagdolled = false;
        } else {
            kobold.ragdoller.PushRagdoll();
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
    //public void OnHold() {
    //grabber.TryGrab();
    //}
    //public void OnActivateHeldItems() {
    //grabber.TryActivate();
    //}
}
