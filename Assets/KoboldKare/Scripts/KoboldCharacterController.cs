using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.IO;
using SimpleJSON;

[RequireComponent(typeof(Rigidbody))]
public class KoboldCharacterController : MonoBehaviourPun, IPunObservable, ISavable {
    [System.Serializable]
    public class PID {
        public float P = 1f;
        //public float I;
        public float D = 2f;
        private float error=0f;
        private float errorDifference = 0f;
        private float lastError=0f;
        //private float rollingError = 0f;
        private float timeStep = 0f;

        public PID(float initP, float initI, float initD) {
            P = initP;
            //I = initI;
            D = initD;
        }
        public void UpdatePID(float newError, float timeElapsed) {
            error = newError;
            errorDifference = error - lastError;
            //rollingError = Mathf.Lerp(rollingError, error, 0.1f);
            lastError = error;
            timeStep = timeElapsed;
        }
        public float GetCorrection() {
            return error * P * timeStep * 100f + errorDifference * D * timeStep * 200f;
            //return error * P * timeStep * 100f + rollingError * I + errorDifference * D * timeStep * 200f;
        }

    }
    [HideInInspector]
    public Rigidbody body;
    private Vector3 worldModelOffset = Vector3.zero;
    private Vector3 velocity = new Vector3(0, 0, 0);
    private float effectiveSpeed {
        get {
            float s = Mathf.Lerp(grounded?speed:speed*airSpeedMultiplier, crouchSpeed, crouchAmount);
            float f = Mathf.Lerp(s, s*walkSpeedMultiplier, (grounded && inputWalking) ? 1f : 0f);
            return f * speedMultiplier * Mathf.Lerp(transform.lossyScale.x, 1f, 0.5f);
        }
    }
    private float effectiveAccel {
        get {
            float a = Mathf.Lerp(accel, crouchAccel, crouchAmount);
            float f = Mathf.Lerp(a, a*walkSpeedMultiplier, (grounded && inputWalking) ? 1f : 0f);
            return grounded ? f : airAccel;
        }
    }
    private float effectiveStepHeight {
        get {
            //FIXME: Not sure why I need to add 0.05 here, small characters (under half size) tend to have the wrong step height! Might be due to bodyproportion changes or similar.
            return Mathf.Lerp(stepHeight, stepHeightCrouched, crouchAmount)*transform.lossyScale.x+0.06f;//-Mathf.Max(transform.lossyScale.x-1f,0f)*0.5f;
        }
    }
    // Only check below the player's feet when we're continually grounded.
    private float effectiveStepCheckDistance {
        get {
            return (jumpTimer > 0 || !grounded) ? effectiveStepHeight : stepHeightCheckDistance*transform.lossyScale.x;//-Mathf.Max(transform.lossyScale.x-1f,0f)*0.5f;
        }
    }
    private float effectiveFriction {
        get {
            return Mathf.Lerp(friction, crouchFriction, Mathf.Round(crouchAmount)) * frictionMultiplier;
        }
    }

    [HideInInspector]
    public bool grounded = false;
    [HideInInspector]
    // True for the frame that impulse was applied for a jump.
    public bool jumped = false;
    [HideInInspector]
    // This is to keep the player from instantly snapping back to the floor, a timer for when to activate the ground PID again.
    public float jumpTimer = 0f;
    [HideInInspector]
    // How far the player has crouched, analog
    public float crouchAmount = 0;
    // Used to keep track of objects the player is standing on.
    [HideInInspector]
    public Vector3 groundVelocity;
    [HideInInspector]
    public Rigidbody groundRigidbody;

    public float frictionMultiplier = 1f;
    public float speedMultiplier = 1f;
    public AudioPack footland;
    private bool groundedMemory;
    private float distanceToGround;

    // These are just used to cache the collider's original data, since we modify them to crouch.
    private float colliderFullHeight;
    private Vector3 colliderNormalCenter;

    public float airSpeedMultiplier = 1f;
    // Inputs, controlled by external player or AI script
    [HideInInspector]
    public Vector3 inputDir = new Vector3(0, 0, 0);
    [HideInInspector]
    public bool inputJump = false;
    private float inputCrouched = 0f;
    private float targetCrouched;
    private float targetCrouchedVel;

    public void SetInputCrouched(float input) {
        targetCrouched = Mathf.Clamp01(input);
    }
    public float GetInputCrouched() {
        return targetCrouched;
    }

    [HideInInspector]
    public bool inputWalking = false;

    [Tooltip("Gravity applied per second to the character, generally to make the gravity feel less floaty.")]
    public Vector3 gravityMod = new Vector3(0, -0f, 0);
    [Tooltip("Fixed impulse for how high the character jumps.")]
    public float jumpStrength = 5f;
    [Tooltip("The speed at which acceleration is no longer applied when the player is moving.")]
    public float speed = 8f;
    [Tooltip("The speed at which acceleration is no longer applied when the player is crouch-moving.")]
    public float crouchSpeed = 4f;
    [Tooltip("Speed multiplier for when inputWalking is true.")]
    public float walkSpeedMultiplier = 0.5f;
    [Tooltip("How quickly the player reaches max speed while walking.")]
    public float accel = 7f;
    [Tooltip("How quickly the player reaches max speed while crouch-walking.")]
    public float crouchAccel = 5f;
    [Tooltip("How quickly the player reaches max speed while in the air.")]
    public float airAccel = 20f;
    [SerializeField]
    [Tooltip("How high the character should float off the ground. (measured from capsule center to ground)")]
    public float stepHeight = 1f;
    [Tooltip("How high the character should float off the ground while crouched. (measured from capsule center to ground)")]
    public float stepHeightCrouched = 0.5f;
    [Tooltip("How far to raycast in order to suck the player to the floor, necessary for walking down slopes. (measured from capsule center to ground)")]
    public float stepHeightCheckDistance = 2f;
    [Tooltip("Basically the constraint settings for keeping the player a certain distance from the floor.")]
    public PID groundingPID = new PID(1f,0f,2f);
    [Tooltip("How fat the raycast is to check for walkable ground under the capsule collider.")]
    public float groundCheckRadius = 0.14f;
    [Tooltip("How sharp the player movement is, high friction means sharper movement.")]
    public float friction = 7f;
    [Tooltip("How sharp the player movement is while crouched, high friction means sharper movement.")]
    public float crouchFriction = 10f;

    //[Tooltip("Mask of layers containing walkable ground. Should match up with whatever the capsule collides with.")]
    //public LayerMask hitLayer;

    [Tooltip("The collider of the player capsule.")]
    public new CapsuleCollider collider;
    [Tooltip("How tall the collider should be during a full crouch.")]
    public float crouchHeight = 0.6f;
    [Tooltip("Reference to the player model so we can push it up and down based on when we crouch.")]
    public Transform worldModel;
    private float airCap = 0.4f;
    private Vector3 defaultWorldModelPosition = Vector3.zero;

    private RaycastHit[] hitArray = new RaycastHit[5];

    public void SetSpeed( float speed ) {
        this.speed = speed;
    }
    public void SetJumpHeight( float jumpStrength ) {
        this.jumpStrength = jumpStrength;
    }

    //public Transform hipsTransform;
    private void Start() {
        body = GetComponent<Rigidbody>();
        body.useGravity = photonView.IsMine || !PhotonNetwork.InRoom;
        colliderFullHeight = collider.height;
        colliderNormalCenter = collider.center;
        defaultWorldModelPosition = worldModel.localPosition;
    }

    // Check if we've clipped our capsule into something.
    private bool Stuck() {
        Vector3 topPoint = collider.transform.TransformPoint(collider.center + new Vector3(0, collider.height / 2f, 0));
        Vector3 botPoint = collider.transform.TransformPoint(collider.center);
        foreach(Collider c in Physics.OverlapCapsule(topPoint, botPoint, collider.radius*collider.transform.lossyScale.x, GameManager.instance.walkableGroundMask, QueryTriggerInteraction.Ignore) ) {
            if (c.transform.root == transform.root) {
                continue;
            }
            return true;
        }
        return false;
    }
    private void CheckCrouched() {
        // Calculate height difference of just the collider
        float oldHeight = collider.height;
        collider.height = Mathf.MoveTowards(collider.height, Mathf.Lerp( colliderFullHeight, crouchHeight, inputCrouched), Time.deltaTime*2f);
        float diff = (collider.height - oldHeight) / 2f;
        // If we're uncrouching and we hit something, undo the crouch progress.
        if (diff > 0 && Stuck()) {
            collider.height = oldHeight;
        } else {
            float oldStepHeight = effectiveStepHeight;
            crouchAmount = collider.height.Remap(crouchHeight, colliderFullHeight, 1, 0);
            //collider.center = Vector3.Lerp(colliderNormalCenter, colliderNormalCenter + new Vector3(0, (colliderFullHeight - crouchHeight) * 0.5f, 0f), crouchAmount);

            diff += (effectiveStepHeight - oldStepHeight);
            worldModelOffset -= new Vector3(0,diff*0.5f,0);

            if (grounded) {
                body.MovePosition(body.position + new Vector3(0, diff, 0)/2f);
            } else {
                body.MovePosition(body.position - new Vector3(0, diff, 0)/2f);
            }
        }

        worldModel.localPosition = defaultWorldModelPosition + worldModelOffset / worldModel.parent.lossyScale.y;
    }

    private void Friction() {
        float speed = velocity.magnitude;
        if ( speed < 0.1f ) {
            velocity = Vector3.zero;
            return;
        }
        float stopSpeed = 1f;
        float control = speed < stopSpeed ? stopSpeed : speed;
        float drop = 0;
        drop += control * effectiveFriction * Time.fixedDeltaTime;
        float newspeed = speed - drop;
        if (newspeed < 0) {
            newspeed = 0;
        }
        newspeed /= speed;
        velocity *= newspeed;
    }



    private void GroundCalculate() {
        groundVelocity = Vector3.zero;
        RaycastHit hit;
        distanceToGround = float.MaxValue;
        Vector3 floorNormal = Vector3.up;
        bool actuallyHit = false;
        // Do a simple raycast before doing an array of 9 raycasts looking for walkable ground. This is to get accurate normal data, as a normal spherecast deflects the normal incorrectly at the edges.
        float radius = groundCheckRadius * transform.lossyScale.x;
        for(float x = -radius; x <= radius; x+=radius) {
            for (float y = -radius; y <= radius; y += radius) {
                if (Physics.Raycast(collider.transform.TransformPoint(collider.center) + new Vector3(x,0,y), Vector3.down, out hit, 5f*transform.lossyScale.x, GameManager.instance.walkableGroundMask)) {
                    floorNormal = hit.normal;
                    distanceToGround = hit.distance;
                    if (hit.normal.y >= 0.7f && hit.distance <= effectiveStepCheckDistance + 0.05f) {
                        if (hit.rigidbody) {
                            groundVelocity = hit.rigidbody.GetPointVelocity(hit.point);
                        }
                        groundRigidbody = hit.rigidbody;
                        actuallyHit = true;
                        break;
                    }
                }
            }
            if (actuallyHit) {
                break;
            }
        }
        if (!actuallyHit) {
            groundRigidbody = null;
        }
        if (distanceToGround <= effectiveStepCheckDistance + 0.05f && floorNormal.y >= 0.7f) {
            grounded = true;
            groundingPID.UpdatePID(-(distanceToGround - effectiveStepHeight), Time.fixedDeltaTime);
            if (jumpTimer <= 0) {
                velocity += Vector3.up * Mathf.Min(groundingPID.GetCorrection(), jumpStrength);
            }
        } else {
            floorNormal = Vector3.up;
            grounded = false;
        }
    }
    private void AirBrake(Vector3 wishdir) {
        float d = Vector3.Dot(Vector3.Normalize(wishdir), Vector3.Normalize(velocity));
        if ( d < -0.25f ) {
            velocity = Vector3.ProjectOnPlane(velocity, wishdir);
        } else if (d < 0f ) {
            float oldMag = velocity.magnitude;
            velocity = Vector3.ProjectOnPlane(velocity, wishdir);
            if (velocity.magnitude > 0f) {
                velocity *= (oldMag / velocity.magnitude);
            }
        }
    }

    private void Update() {
        if (!enabled) {
            return;
        }

        inputCrouched = Mathf.SmoothDamp(inputCrouched, targetCrouched, ref targetCrouchedVel, 0.1f);
        CheckCrouched();
    }

    private void CheckSounds() {
        if (grounded) {
            if ( grounded != groundedMemory ) {
                GameManager.instance.SpawnAudioClipInWorld(footland, transform.position + Vector3.down * distanceToGround);
            }
        }
        groundedMemory = grounded;
    }

    private void FixedUpdate() {
        if (!enabled) {
            return;
        }
        //AlignToHips();
        //body.centerOfMass = Vector3.up * 0.5f;
        //collider.transform.rotation = Quaternion.identity;
        velocity = body.velocity;
        jumped = false;
        velocity -= groundVelocity;
        groundVelocity = Vector3.zero;
        GroundCalculate();
        if ( !grounded ) {
            velocity += gravityMod * (transform.localScale.x * Time.deltaTime);
            //body.AddForce(gravityMod * transform.localScale.x, ForceMode.Acceleration);
            //velocity += gravityMod * transform.lossyScale.x;
        }
        JumpCheck();
        //AirBrake(inputDir);
        if (grounded) {
            Friction();
        }
        body.useGravity = !grounded;
        if (inputDir.magnitude == 0) {
            Accelerate(Vector3.forward, 0f, effectiveAccel);
        } else {
            Accelerate(inputDir, effectiveSpeed, effectiveAccel);
        }
        CheckSounds();
        velocity += groundVelocity;
        if (photonView.IsMine || !PhotonNetwork.InRoom) {
            body.velocity = velocity;
        }
    }

    private void JumpCheck() {
        jumpTimer -= Time.fixedDeltaTime;
        if ( grounded && inputJump ) {
            jumped = true;
            velocity.y = Mathf.Max(velocity.y, jumpStrength*Mathf.Lerp(transform.lossyScale.x,1f,0.5f));
            grounded = false;
            jumpTimer = 0.25f;
        }
    }
    
    //private void AlignToHips() {
       //colliderNormalCenter = transform.InverseTransformPoint(hipsTransform.position).With(x: 0f, z: 0f) + Vector3.up * collider.height * 0.3f;
    //}

    // Quake style acceleration
    void Accelerate(Vector3 wishdir, float wishspeed, float accel) {
        float wishspd = wishspeed;
        if (!grounded) {
            wishspd = Mathf.Min(wishspd, airCap);
        }
        float currentspeed = Vector3.Dot(velocity, wishdir);

        float addspeed = wishspd - currentspeed;
        if (addspeed <= 0) {
            return;
        }
        float accelspeed = accel * wishspeed * Time.deltaTime;

        accelspeed = Mathf.Min(accelspeed, addspeed);

        velocity += accelspeed * wishdir;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(this.inputJump);
            stream.SendNext(this.targetCrouched);
        } else {
            inputJump = (bool)stream.ReceiveNext();
            targetCrouched = (float)stream.ReceiveNext();
            PhotonProfiler.LogReceive(sizeof(bool)+sizeof(float));
        }
    }

    public void Save(JSONNode node) {
        node["targetCrouched"] = inputCrouched;
    }

    public void Load(JSONNode node) {
        targetCrouched = node.GetValueOrDefault("targetCrouched", 0f);
    }

}
