using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.Events;
using Vilar.AnimationStation;
using Photon.Pun;
using System.IO;

[RequireComponent(typeof(KoboldCharacterController))]
public class CharacterControllerAnimator : GenericUsable {
    [SerializeField]
    private Sprite displaySprite;
    private Kobold internalKobold;
    public Kobold kobold {
        get {
            if (internalKobold == null) {
                internalKobold = GetComponentInParent<Kobold>();
            }
            return internalKobold;
        }
    }
    public float standingAnimationSpeedMultiplier = 0.1f;
    public float crouchedAnimationSpeedMultiplier = 0.1f;
    private int stationViewID;
    private byte currentStationID;
    public float walkingAnimationSpeedMultiplier = 0.1f;
    public Animator playerModel;
    public Transform lookDir;
    private Vector3 tempDir = new Vector3(0, 0, 0);
    private KoboldCharacterController controller;
    private bool jumped;
    private bool isInAir;
    private float crouchLerper;
    [Range(1f,10f)]
    public float animatableRange = 1f;
    public VisualEffect jumpDust;
    public VisualEffect walkDust;
    //public ParticleSystem walkDust;
    private float distanceCounter = 0;
    public Transform headTransform;
    public LayerMask actionLayer;
    //private Vector3 lookPosition;
    private Collider[] colliderArray = new Collider[5];
    private Vector3 lastPosition;
    private AnimationStation activeStation;
    private bool useRandomSample = false;
    public Vilar.IK.PhysicsIK physicsSolver;
    public Rigidbody body;
    public UnityEvent onBeginStation;
    public UnityEvent onEndStation;
    private WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();
    public float randomSample {
        get {
            return 1f+Mathf.SmoothStep(0f, 1f, Mathf.PerlinNoise(0f, Time.timeSinceLevelLoad*0.08f))*2f;
        }
    }
    public LookAtHandler handler;
    public AnimationCurve sizeDifferenceCurve;
    private List<Kobold> activeKobolds = new List<Kobold>(4);
    public float blend = 0f;
    [HideInInspector]
    public bool animating = false;
    public bool notAnimating { get { return !animating; } }
    public bool CanAnimate(Kobold user) {
        if (user == null) {
            return false;
        }
        activeKobolds.Clear();
        activeKobolds.Add(kobold);
        activeKobolds.Add(user);
        if (activeStation) {
            foreach (AnimationStation s in activeStation.linkedStations.hashSet) {
                if (s.info.user == user) {
                    // You're already animating with me!!
                    return false;
                }
                if (s.info.user != null && !activeKobolds.Contains((Kobold)s.info.user)) {
                    activeKobolds.Add((Kobold)s.info.user);
                }
            }
        }
        var station = GetClosestValidAnimationStations(animatableRange, activeKobolds, user.transform.position);
        if (station == null) {
            return false;
        }
        return true;
    }
    public override bool CanUse(Kobold k) {
        return CanAnimate(k);
    }
    public override void Use(Kobold k) {
        if (k != null) {
            SnapToNearestAnimationStation(k, k.transform.position);
        }
    }
    void Start() {
        //lookPosition = lookDir.position;
        controller = GetComponent<KoboldCharacterController>();
    }
    public IEnumerator WaitThenAdvanceProgress() {
        foreach( Rigidbody r in kobold.ragdollBodies) {
            r.detectCollisions = false;
        }
        if (kobold.activeDicks != null) {
            foreach( var dickSet in kobold.activeDicks) {
                dickSet.dick.body.detectCollisions = false;
            }
        }
        yield return new WaitForSeconds(5f);
        foreach( Rigidbody r in kobold.ragdollBodies) {
            r.detectCollisions = true;
        }
        if (kobold.activeDicks != null) {
            foreach( var dickSet in kobold.activeDicks) {
                dickSet.dick.body.detectCollisions = true;
            }
        }
        float endTransitionTime = Time.timeSinceLevelLoad + 3f;
        int oldLayer = kobold.ragdollBodies[0].gameObject.layer;
        while (activeStation != null && Time.timeSinceLevelLoad < endTransitionTime) {
            activeStation.progress = Mathf.MoveTowards(activeStation.progress, randomSample, 1f-(endTransitionTime-Time.timeSinceLevelLoad)/3f);
            yield return endOfFrame;
        }
        useRandomSample = true;
    }
    void OnBeginStation(AnimationStation station) {
        kobold.KnockOver(9999f);
        StopCoroutine("WaitThenAdvanceProgress");
        blend = 0f;
        activeStation = station;
        station.OnStart(kobold);
        animating = true;
        physicsSolver.Initialize();
        //body.collisionDetectionMode = CollisionDetectionMode.Discrete;
        useRandomSample = false;
        body.isKinematic = true;
        //playerModel.SetTrigger("TPose");

        transform.position = station.transform.position;
        //body.position = transform.position;
        transform.rotation = station.transform.rotation;

        onBeginStation.Invoke();
        StartCoroutine("WaitThenAdvanceProgress");
        activeStation.progress = 0f;
        //playerModel.updateMode = AnimatorUpdateMode.Normal;
    }
    public override Sprite GetSprite(Kobold k) {
        return displaySprite;
    }
    public void OnEndStation() {
        if (!animating) {
            return;
        }
        stationViewID = 0;
        currentStationID = 0;
        body.isKinematic = false;
        kobold.StandUp();
        animating = false;
        physicsSolver.CleanUp();
        // Stop everyone else from having the fun, sorry!
        foreach(AnimationStation s in activeStation.linkedStations.hashSet) {
            if (s.info.user && s != activeStation) {
                ((Kobold)s.info.user).GetComponentInChildren<CharacterControllerAnimator>().OnEndStation();
            }
            s.info.user = null;
        }
        StopCoroutine("WaitThenAdvanceProgress");
        if (isActiveAndEnabled) {
            blend = 0f;
            activeStation.OnEnd();
            activeStation = null;
            animating = false;
            onEndStation.Invoke();
        }
        useRandomSample = false;
        foreach( Rigidbody r in kobold.ragdollBodies) {
            r.detectCollisions = true;
        }
        if (kobold.activeDicks != null) {
            foreach( var dickSet in kobold.activeDicks) {
                dickSet.dick.body.detectCollisions = true;
            }
        }
    }
    private void OnDestroy() {
        if (animating && activeStation) {
            animating = false;
            foreach (AnimationStation s in activeStation.linkedStations.hashSet) {
                if (s.info.user) {
                    ((Kobold)s.info.user).GetComponentInChildren<CharacterControllerAnimator>().OnEndStation();
                }
                s.info.user = null;
            }
        }
        if (activeStation) {
            activeStation.OnEnd();
            activeStation = null;
            animating = false;
            body.isKinematic = false;
            body.collisionDetectionMode = CollisionDetectionMode.Continuous;
            //playerModel.updateMode = AnimatorUpdateMode.AnimatePhysics;
        }
    }

    public static float Compatibility(List<Kobold> kobolds, HashSet<AnimationStation> stationGroup) {
        float baseCompatibility = 0f;
        foreach (Kobold k in kobolds) {
            AnimationStation bestStation = null;
            float bestCompatibilty = float.MinValue;
            foreach (AnimationStation s in stationGroup) {
                float compat = CharacterControllerAnimator.Compatibility(k, s);
                if (bestStation == null || compat > bestCompatibilty) {
                    bestStation = s;
                    bestCompatibilty = compat;
                }
            }
            baseCompatibility += bestCompatibilty;
        }
        return baseCompatibility;
    }

    public static float Compatibility(Kobold k, AnimationStation s) {
        if (s.info.user != null) {
            return float.MinValue;
        }
        float baseCompatibility = 1f;
        // If we've got a penetrator role, we'd want a dick
        if (s.info.needsPenetrator && (k.activeDicks.Count == 0)) {
            baseCompatibility -= 0.5f;
        }
        // Kobolds with dicks will prefer to be penetrators, but only by a little.
        if (s.info.needsPenetrator && (k.activeDicks.Count != 0)) {
            baseCompatibility += 1f;
        }
        // Kobolds without dicks will prefer to be recievers/cuddlers, but only by a little.
        if (!s.info.needsPenetrator && (k.activeDicks.Count == 0)) {
            baseCompatibility += 0.1f;
        }
        // If we're drastically the wrong scale from a custom scaled station, damage the fitness score.
        float x = 1f + Mathf.Abs(k.transform.lossyScale.x - s.transform.lossyScale.x)*0.9f;
        baseCompatibility -= x*x-1f;
        return baseCompatibility;
    }
    private HashSet<AnimationStation> GetClosestValidAnimationStations(float range, List<Kobold> kobolds, Vector3 position) {
        HashSet<AnimationStation> best = null;
        float bestFitness = float.MinValue;
        int hitCount = Physics.OverlapSphereNonAlloc(position, range, colliderArray, actionLayer, QueryTriggerInteraction.Collide);
        for (int i=0;i<hitCount;i++) {
            Collider c = colliderArray[i];
            // Never use stations that have the wrong number of participants
            AnimationStation s = c.GetComponentInChildren<AnimationStation>();
            if (s == null) {
                continue;
            }
            HashSet<AnimationStation> stations = s.linkedStations.hashSet;
            if (stations.Count < kobolds.Count) {
                continue;
            }
            float dist = Vector3.Distance(position, c.transform.position);
            float fitness = 1f - dist;
            fitness += CharacterControllerAnimator.Compatibility(kobolds, stations);
            // If fitness is too low, we skip the station, this could mean that we're targeting a macro v micro station and just don't have the right people.
            //if ((best == null || fitness > bestFitness) && fitness > 0f) {
                best = stations;
                bestFitness = fitness;
            //}
        }
        return best;
    }
    public void SnapToStation(int photonViewID, byte stationNumber) {
        //if (activeStation != null) {
        //OnEndStation();
        //}
        PhotonView view = PhotonView.Find(photonViewID);
        if (view == null) {
            return;
        }
        stationViewID = photonViewID;
        currentStationID = stationNumber;
        AnimationStation[] stations = view.GetComponentsInChildren<AnimationStation>();
        if (stationNumber >= 0 && stationNumber < stations.Length) {
            OnBeginStation(stations[stationNumber]);
            stations[stationNumber].info.user = kobold;
        }
    }
    public void SnapToNearestAnimationStation(Kobold OtherPlayer, Vector3 position) {
        if (OtherPlayer == null || !photonView.IsMine) {
            return;
        }
        // Find all the active bolds.
        activeKobolds.Clear();
        activeKobolds.Add(kobold);
        activeKobolds.Add(OtherPlayer);
        if (activeStation) {
            foreach (AnimationStation s in activeStation.linkedStations.hashSet) {
                if (s.info.user != null && !activeKobolds.Contains((Kobold)s.info.user)) {
                    activeKobolds.Add((Kobold)s.info.user);
                }
            }
        }

        var closestValidAnimationStations = GetClosestValidAnimationStations(animatableRange, activeKobolds, position);
        if (closestValidAnimationStations == null) {
            return;
        }

        foreach (Kobold k in activeKobolds) {
            AnimationStation bestStation = null;
            float bestCompatibilty = float.MinValue;
            foreach (AnimationStation s in closestValidAnimationStations) {
                if (s.info.user == k) {
                    bestStation = s;
                    bestCompatibilty = float.MaxValue;
                }
                if (s.info.user != null) {
                    continue;
                }
                float compat = CharacterControllerAnimator.Compatibility(k, s);
                if (bestStation == null || compat > bestCompatibilty) {
                    bestStation = s;
                    bestCompatibilty = compat;
                }
            }
            if (bestStation != null) {
                PhotonView stationView = bestStation.photonView;
                AnimationStation[] stations = stationView.GetComponentsInChildren<AnimationStation>();
                for(int i = 0; i < stations.Length; i++) {
                    if (stations[i] == bestStation) {
                        //k.photonView.RPC("SnapToStation", RpcTarget.AllBuffered, new object[] { stationView.ViewID, i });
                        k.GetComponent<CharacterControllerAnimator>().SnapToStation(stationView.ViewID, (byte)i);
                    }
                }
                //k.GetComponentInChildren<CharacterControllerAnimator>().SnapToStation(bestStation);
            }
        }
    }
    private void Update() {
        if (activeStation == null) {
            return;
        }
        activeStation.lookAtPosition = lookDir.position;
        if (animating) {
            kobold.PumpUpDick(Time.deltaTime);
            blend = Mathf.MoveTowards(blend, 1f, Time.deltaTime*2f);
            if (useRandomSample && activeStation != null) {
                activeStation.progress = randomSample;
            }
        }
        physicsSolver.ForceBlend(blend);
        activeStation.SetCharacter(physicsSolver);
    }
    void FixedUpdate()
    {
        if (kobold != null) {
            float maxPen = 0f;
            // Kill all position changing stuff while penetrated, otherwise causes unwanted oscillations.
            foreach (var hole in kobold.penetratables) {
                foreach( var dick in hole.penetratable.GetPenetrators()) {
                    if (dick.IsInside()) {
                        lastPosition = transform.position;
                    }
                }
            }
            playerModel.SetFloat("PenetrationSize", Mathf.Clamp01(maxPen * 4f));
            if (maxPen > 0f) {
                playerModel.SetFloat("SexFace", Mathf.Lerp(playerModel.GetFloat("SexFace"), 1f, Time.deltaTime * 2f));
            } else {
                playerModel.SetFloat("SexFace", Mathf.Lerp(playerModel.GetFloat("SexFace"), 0f, Time.deltaTime));
            }
            foreach (var dickSet in kobold.activeDicks) {
                if (dickSet.dick.holeTarget != null || dickSet.dick.cumActive > 0) {
                    playerModel.SetFloat("SexFace", 1f);
                }
            }
            playerModel.SetFloat("Orgasm", Mathf.Clamp01(Mathf.Abs(kobold.stimulation / kobold.stimulationMax)));
            playerModel.SetFloat("MadHappy", Mathf.Clamp01(Mathf.Abs(kobold.stimulation / kobold.stimulationMax)));
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
        Vector3 velocity = (transform.position - lastPosition) / Time.fixedDeltaTime;
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
    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(stationViewID);
            stream.SendNext(currentStationID);
        } else {
            int newViewID = (int)stream.ReceiveNext();
            byte newStationID = (byte)stream.ReceiveNext();
            if (newViewID != 0 && stationViewID != newViewID) {
                OnEndStation();
                SnapToStation(newViewID, newStationID);
            } else if (newViewID == 0 && stationViewID != 0) {
                OnEndStation();
            }
        }
    }
    public override void Save(BinaryWriter writer, string version) {
        writer.Write(stationViewID);
        writer.Write(currentStationID);
    }

    public override void Load(BinaryReader reader, string version) {
        int newViewID = (int)reader.ReadInt32();
        byte newStationID = (byte)reader.ReadByte();
        if (newViewID != 0 && stationViewID != newViewID) {
            OnEndStation();
            SnapToStation(newViewID, newStationID);
        } else if (newViewID == 0 && stationViewID != 0) {
            OnEndStation();
        }
    }
}