using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.Events;
using Vilar.AnimationStation;
using Photon.Pun;
using System.IO;
using PenetrationTech;

public class CharacterControllerAnimator : MonoBehaviourPun, IPunObservable, ISavable {
    private Kobold kobold;
    private Vilar.IK.ClassicIK solver;
    private float randomSample => 1f+Mathf.SmoothStep(0f, 1f, Mathf.PerlinNoise(0f, Time.timeSinceLevelLoad*0.08f))*2f;
    private AnimationStationSet currentStationSet;
    private AnimationStation currentStation;
    private Animator playerModel;
    private KoboldCharacterController controller;
    private bool isInAir;
    private bool jumped;
    private Vector3 tempDir;
    [SerializeField]
    private VisualEffect jumpDust;
    [SerializeField]
    private VisualEffect walkDust;
    [SerializeField]
    private Transform headTransform;
    [SerializeField]
    private Transform lookDir;
    [SerializeField]
    private AudioPack footstepPack;

    [SerializeField] private Rigidbody body;
    [SerializeField] private PlayerPossession playerPossession;

    [SerializeField] private float crouchedAnimationSpeedMultiplier = 1f;
    [SerializeField] private float walkingAnimationSpeedMultiplier = 1f;
    [SerializeField] private float standingAnimationSpeedMultiplier = 1f;
    private float crouchLerper;
    private LookAtHandler handler;

    private Vector3 lastPosition;
    private bool animating;

    public bool TryGetAnimationStationSet(out AnimationStationSet set) {
        if (!animating) {
            set = null;
            return false;
        }
        set = currentStationSet;
        return true;
    }

    private void Awake() {
        kobold = GetComponentInParent<Kobold>();
        solver = GetComponentInChildren<Vilar.IK.ClassicIK>();
        playerModel = GetComponentInChildren<Animator>();
        handler = playerModel.gameObject.AddComponent<LookAtHandler>();
        controller = GetComponentInParent<KoboldCharacterController>();
        playerModel.gameObject.AddComponent<AnimatorExtender>();
        playerModel.gameObject.AddComponent<FootIK>();
        playerModel.gameObject.AddComponent<HandIK>();
        playerModel.gameObject.AddComponent<FootstepSoundManager>().SetFootstepPack(footstepPack);
        playerModel.GetBoneTransform(HumanBodyBones.LeftFoot).gameObject.AddComponent<FootInteractor>();
        playerModel.GetBoneTransform(HumanBodyBones.RightFoot).gameObject.AddComponent<FootInteractor>();
    }

    [PunRPC]
    public void BeginAnimationRPC(int photonViewID, int animatorID) {
        PhotonView view = PhotonNetwork.GetPhotonView(photonViewID);
        AnimationStationSet set = view.GetComponent<AnimationStationSet>();
        BeginAnimation(set, set.GetAnimationStations()[animatorID]);
    }
    
    private void BeginAnimation(AnimationStationSet set, AnimationStation station) {

        StopAnimation();
        currentStationSet = set;
        currentStation = station;
        if (station.info.user != null) {
            station.info.user.GetComponent<CharacterControllerAnimator>().StopAnimation();
        }
        StartCoroutine(AnimationRoutine());
    }

    private IEnumerator AnimationRoutine() {
        animating = true;
        solver.enabled = true;
        controller.enabled = false;
        kobold.body.isKinematic = true;
        solver.Initialize();
        currentStation.SetProgress(0f);
        currentStation.OnStartAnimation(kobold);
        float startTime = Time.time;
        float blendDuration = 1f;
        while (Time.time < startTime + blendDuration) {
            float t = (Time.time - startTime) / blendDuration;
            solver.ForceBlend(t);
            yield return null;
        }
        solver.ForceBlend(1f);
        yield return new WaitForSeconds(3f);
        float transitionDuration = 3f;
        float endTransitionTime = Time.time + transitionDuration;
        while (Time.time < endTransitionTime) {
            currentStation.SetProgress(Mathf.MoveTowards(currentStation.progress, randomSample, 1f-(endTransitionTime-Time.timeSinceLevelLoad)/transitionDuration));
            yield return null;
        }
        while (animating) {
            currentStation.SetProgress(randomSample);
            yield return null;
        }
        
        StopAnimation();
    }

    void Update() {
        if (kobold != null) {
            float maxPen = 0f;
            playerModel.SetFloat("PenetrationSize", Mathf.Clamp01(maxPen * 4f));
            if (maxPen > 0f) {
                playerModel.SetFloat("SexFace", Mathf.Lerp(playerModel.GetFloat("SexFace"), 1f, Time.deltaTime * 2f));
            } else {
                playerModel.SetFloat("SexFace", Mathf.Lerp(playerModel.GetFloat("SexFace"), 0f, Time.deltaTime));
            }
            foreach (var dickSet in kobold.activeDicks) {
                if (dickSet.dick.TryGetPenetrable(out Penetrable penetrable)) {
                    playerModel.SetFloat("SexFace", 1f);
                }
            }
            playerModel.SetFloat("Orgasm", Mathf.Clamp01(Mathf.Abs(kobold.stimulation / kobold.stimulationMax)));
            playerModel.SetFloat("MadHappy", Mathf.Clamp01(Mathf.Abs(kobold.stimulation / kobold.stimulationMax)));
        }

        if (animating) {
            currentStation.SetCharacter(solver);
        }

        if (!controller.grounded) {
            isInAir = true;
        } else {
            // Landing dust
            if (isInAir) {
                jumpDust.SendEvent("TriggerPoof");
                isInAir = false;
            }
        }
        if (jumped != controller.jumped && controller.jumped) {
            jumped = controller.jumped;
            jumpDust.SendEvent("TriggerPoof");
        }
        if (jumped != controller.jumped && !controller.jumped) {
            jumped = controller.jumped;
        }
        Vector3 velocity = (transform.position - lastPosition) / Mathf.Max(Time.deltaTime,0.01f);
        lastPosition = transform.position;
        Vector3 dir = Vector3.Normalize(velocity);
        dir = Quaternion.Inverse(Quaternion.Euler(0,headTransform.rotation.eulerAngles.y,0)) * dir;
        float speed = velocity.magnitude;
        if ( speed < 1f ) {
            dir *= speed;
        }
        tempDir.x = Mathf.MoveTowards(tempDir.x, dir.x, 5f * Time.deltaTime);
        if (controller.grounded) {
            if (dir.z > 0f) {
                tempDir.z = Mathf.MoveTowards(tempDir.z, dir.z + Mathf.Abs(dir.y), 5f * Time.deltaTime);
            } else {
                tempDir.z = Mathf.MoveTowards(tempDir.z, dir.z - Mathf.Abs(dir.y), 5f * Time.deltaTime);
            }
        } else {
            tempDir.z = Mathf.MoveTowards(tempDir.z, dir.z, 5f * Time.deltaTime);
        }
        playerModel.SetFloat("MoveX", tempDir.x);
        playerModel.SetFloat("MoveY", tempDir.z);
        float s = speed;
        if (controller.inputCrouched ) {
            s *= crouchedAnimationSpeedMultiplier;
        } else {
            s *= standingAnimationSpeedMultiplier;
        }
        if (controller.inputWalking) {
            s *= walkingAnimationSpeedMultiplier;
        }
        s /= Mathf.Lerp(transform.lossyScale.x,1f,0.5f);
        playerModel.SetFloat("Speed", s == 0 ? 1f : s);
        if (controller.enabled) {
            walkDust.SetFloat("Speed", velocity.magnitude * (controller.grounded ? 1f : 0f));
        } else {
            walkDust.SetFloat("Speed", 0f);
        }
        playerModel.SetBool("Jump", controller.jumped);
        playerModel.SetBool("Grounded", controller.grounded);
        crouchLerper = Mathf.MoveTowards(crouchLerper, controller.crouchAmount, 3f*Time.deltaTime);
        playerModel.SetFloat("CrouchAmount", crouchLerper);
        //lookPosition = Vector3.Lerp(lookPosition, lookDir.position + lookDir.forward, Time.deltaTime*20f);
        //handler.SetLookAtWeight(1f, 1f, 1f, 1f, 1f);
        if (animating) {
            handler.SetLookAtWeight(1f, 0f, 1f, 1f, 1f);
        } else {
            handler.SetLookAtWeight(1f, 0.5f, 1f, 1f, 1f);
        }
        handler.SetLookAtPosition(lookDir.position);
    }

    public void StopAnimation() {
        if (!animating) {
            return;
        }
        StopAllCoroutines();
        solver.enabled = false;
        controller.enabled = true;
        kobold.body.isKinematic = false;
        solver.CleanUp();
        animating = false;
        if (currentStation.info.user == kobold) {
            currentStation.info.user = null;
        }
        currentStation = null;
        currentStationSet = null;
    }
    void FixedUpdate() {
        if (playerPossession == null) {
            return;
        }
        Quaternion characterRot = Quaternion.Euler(0, playerPossession.GetEyeRot().x, 0);
        Vector3 fdir = characterRot * Vector3.forward;
        float deflectionForgivenessDegrees = 5f;
        Vector3 cross = Vector3.Cross(body.transform.forward, fdir);
        float angleDiff = Mathf.Max(Vector3.Angle(body.transform.forward, fdir) - deflectionForgivenessDegrees, 0f);
        body.AddTorque(cross*(angleDiff*5f), ForceMode.Acceleration);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            if (animating) {
                stream.SendNext(currentStationSet.photonView.ViewID);
                stream.SendNext(currentStationSet.GetAnimationStations().IndexOf(currentStation));
            } else {
                stream.SendNext(-1);
                stream.SendNext(-1);
            }
        } else {
            int photonViewID = (int)stream.ReceiveNext();
            int animationID = (int)stream.ReceiveNext();
            if (photonViewID != -1 &&
                (!animating || currentStationSet == null || currentStationSet.photonView.ViewID != photonViewID ||
                 currentStation == null || currentStationSet.GetAnimationStations().IndexOf(currentStation) != animationID)) {
                PhotonView view = PhotonNetwork.GetPhotonView(photonViewID);
                AnimationStationSet set = view.GetComponentInChildren<AnimationStationSet>();
                if (animating) {
                    StopAnimation();
                }
                BeginAnimation(set, set.GetAnimationStations()[animationID]);
            }
        }
    }
    public void Save(BinaryWriter writer, string version) {
        if (animating) {
            writer.Write(currentStationSet.photonView.ViewID);
            writer.Write(currentStationSet.GetAnimationStations().IndexOf(currentStation));
        } else {
            writer.Write(-1);
            writer.Write(-1);
        }
    }

    public void Load(BinaryReader reader, string version) {
        int photonViewID = reader.ReadInt32();
        int animationID = reader.ReadInt32();
        if (photonViewID != -1 &&
            (currentStationSet == null || currentStationSet.photonView.ViewID != photonViewID ||
             currentStation == null || currentStationSet.GetAnimationStations().IndexOf(currentStation) != animationID)) {
            PhotonView view = PhotonNetwork.GetPhotonView(photonViewID);
            AnimationStationSet set = view.GetComponentInChildren<AnimationStationSet>();
            if (animating) {
                StopAnimation();
            }
            BeginAnimation(set, set.GetAnimationStations()[animationID]);
        }
    }
}