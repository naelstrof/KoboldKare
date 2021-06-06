using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JiggleBone : MonoBehaviour {
    private Renderer probableRenderer;
    public enum UpdateType {
        FixedUpdate,
        Update,
        LateUpdate,
    }
    public UpdateType updateMode = UpdateType.LateUpdate;
    public Transform root;
    public AnimationCurve elasticity;
    public AnimationCurve friction;
    public bool rotateRoot = true;

    private float internalActive = 1f;
    public float active {
        get {
            return internalActive;
        }
        set {
            if (bones != null && Mathf.Approximately(value, 0f)) {
                for (int i = 0; i < bones.Count; i++) {
                    VirtualBone b = bones[i];
                    // Purely virtual particle!
                    if (b.self == null) {
                        continue;
                    }
                    b.self.localPosition = b.localStartPos;
                    b.self.localRotation = b.localStartRot;
                }
            }
            internalActive = Mathf.Clamp01(value);
        }
    }
    public bool accelerationBased = true;
    [Range(1, 4f)]
    public float maxStretch = 1.1f;
    [Range(0, 1f)]
    public float maxSquish = 0.1f;
    public Vector3 gravity;
    public List<Transform> excludedTransforms;
    private int depth;
    private List<VirtualBone> bones;
    private List<VirtualBone> previousFrameBones;
    private Vector3 lastRootPosition;
    private Vector3 lastVelocityGuess;
    public static int GetRootDistance(Transform root, Transform t) {
        int a = 0;
        Transform findRoot = t;
        while (findRoot != root && (findRoot.parent != root || findRoot.parent == null)) {
            a++;
            findRoot = findRoot.parent;
        }
        return a;
    }
    public void BuildVirtualBoneTree(List<VirtualBone> list, Transform root, Transform t, int depth, VirtualBone parent = null) {
        if (parent == null) {
            parent = new VirtualBone(t, null, root, (float)GetRootDistance(root, t) / (float)depth);
            list.Add(parent);
        }
        for (int i = 0; i < t.childCount; i++) {
            if (excludedTransforms.Contains(t.GetChild(i))) {
                continue;
            }
            VirtualBone child = new VirtualBone(t.GetChild(i), parent, root, (float)GetRootDistance(root, t.GetChild(i)) / (float)depth);
            list.Add(child);
            BuildVirtualBoneTree(list, root, t.GetChild(i), depth, child);
        }
        if (t.childCount == 0) {
            VirtualBone child = new VirtualBone(null, parent, root, 1f);
            list.Add(child);
        }
    }
    public static int GetDeepestChild(Transform t, int currentDepth = 0) {
        if (t.childCount == 0) {
            return currentDepth;
        }
        int max = 0;
        for (int i = 0; i < t.childCount; i++) {
            max = Mathf.Max(GetDeepestChild(t.GetChild(i), currentDepth + 1), max);
        }
        return max;
    }
    public class VirtualBone {
        float endExtensionDistance = 0.25f;
        public VirtualBone(Transform s, VirtualBone parent, Transform root, float chainPos) {
            self = s;
            if (self != null) {
                position = self.position;
                localStartPos = s.localPosition;
                localStartRot = s.localRotation;
            } else {
                position = root.position + root.up * endExtensionDistance;
                localStartPos = Vector3.up * endExtensionDistance;
            }
            this.parent = parent;
            if (parent != null) {
                parent.children.Add(this);
            }
            chainPosition = chainPos;
            children = new List<VirtualBone>();
        }
        public void Friction(JiggleBone jiggle, float dt) {
            float speed = velocity.magnitude;
            if (speed < Mathf.Epsilon) {
                return;
            }
            float drop = jiggle.friction.Evaluate(chainPosition) * speed * dt;
            float newSpeed = speed - drop;
            if (newSpeed < 0) {
                newSpeed = 0;
            }
            newSpeed /= speed;
            velocity *= newSpeed;
        }
        public void Gravity(JiggleBone jiggle, float dt) {
            velocity += jiggle.gravity * dt;
        }
        public void Acceleration(JiggleBone jiggle, float dt) {
            if (parent == null) {
                return;
            }
            // Undo the rotation adjustment of our parent (so it's back to a neutral rotation), then use our localStartPos to figure out where we *should* accelerate towards.
            Vector3 wantedPos = (Quaternion.Inverse(parent.rotationAdjust) * parent.self.TransformVector(localStartPos) + parent.self.position) - position;
            Vector3 force = wantedPos * jiggle.elasticity.Evaluate(chainPosition);

            velocity += force * 100f * dt;
            //parent.velocity -= force * 50f * dt;
        }
        private void Projection(JiggleBone j) {
            if (parent != null) {
                float d = Vector3.Distance(position, parent.position);
                float wantedDistance = parent.self.TransformVector(localStartPos).magnitude;
                float bounciness = 0.1f;
                if (d > wantedDistance * j.maxStretch) {
                    // Bounce with some velocity loss!
                    Vector3 normal = (parent.position - position).normalized;
                    if (Vector3.Dot(velocity, normal) < 0f) {
                        velocity = Vector3.Lerp(Vector3.ProjectOnPlane(velocity, normal), Vector3.Reflect(velocity, normal), bounciness);
                    }
                    position = parent.position + (position - parent.position).normalized * wantedDistance * j.maxStretch;
                } else if (d < wantedDistance * (1f - j.maxSquish)) {
                    Vector3 normal = (position - parent.position).normalized;
                    // Bounce with some velocity loss!
                    if (Vector3.Dot(velocity, normal) < 0f) {
                        velocity = Vector3.Lerp(Vector3.ProjectOnPlane(velocity, normal), Vector3.Reflect(velocity, normal), bounciness);
                    }
                    position = parent.position + (position - parent.position).normalized * wantedDistance * (1f - j.maxSquish);
                }
            }
        }
        public void SetPos(JiggleBone j, float dt) {
            // Projection!
            position += velocity * dt;
            Projection(j);
        }
        public VirtualBone parent;
        public List<VirtualBone> children;
        public Transform self;
        public float chainPosition;
        public Quaternion localStartRot;
        public Vector3 localStartPos;
        public Quaternion rotationAdjust;

        public Vector3 velocity;
        public Vector3 position;
    }
    public void Regenerate() {
        depth = GetDeepestChild(root);
        bones = new List<VirtualBone>();
        BuildVirtualBoneTree(bones, root, root, depth);
    }

    public void Awake() {
        if (bones == null) {
            Regenerate();
        }
        // Assume we're jiggling a skinned mesh renderer, try to find it.
        Transform findMesh = transform;
        while (findMesh.name != "Armature" && findMesh.parent != null) {
            findMesh = findMesh.parent;
        }
        if (findMesh.name == "Armature") {
            probableRenderer = findMesh.parent.GetComponentInChildren<Renderer>();
        }
    }
    public void Start() {
        lastRootPosition = root.position;
    }
    public void Process(float dt) {
        if (probableRenderer != null && !probableRenderer.isVisible) {
            return;
        }
        if (!isActiveAndEnabled || dt == 0 || Mathf.Approximately(active, 0f)) {
            return;
        }
        Vector3 positionDiff = (root.position - lastRootPosition);
        lastRootPosition = root.position;
        Vector3 velocityGuess = positionDiff / dt;
        Vector3 accelerationGuess = (velocityGuess - lastVelocityGuess);
        lastVelocityGuess = Vector3.Lerp(velocityGuess, lastVelocityGuess, 0.5f);

        // Velocity update
        foreach (VirtualBone b in bones) {
            if (float.IsNaN(b.position.x + b.position.y + b.position.z)) {
                if (b.self != null) {
                    b.position = b.self.position;
                } else {
                    b.position = Vector3.zero;
                }
                b.velocity = Vector3.zero;
            }
            // Make sure the root bone is pinned.
            if (b.parent == null) {
                b.position = b.self.position;
                continue;
            }
            if (accelerationBased) {
                b.velocity -= accelerationGuess * 0.5f;
                b.position += positionDiff;
            }
            b.Friction(this, dt);
            b.Gravity(this, dt);
            b.Acceleration(this, dt);
        }
        // Add velocity to position (then project it to make sure it doesn't stretch too much).
        foreach (VirtualBone b in bones) {
            b.SetPos(this, dt);
        }
        // Transforms update, have to set our positions carefully down the chain since each rotation breaks all the positions of the children bones.
        for (int i = 0; i < bones.Count; i++) {
            VirtualBone b = bones[i];
            // Purely virtual particle!
            if (b.self == null) {
                continue;
            }
            if (b.parent == null) {
                b.position = b.self.position;
            } else {
                Vector3 wantedPosition = Vector3.Lerp(b.self.parent.TransformPoint(b.localStartPos), b.position, active);
                b.self.position = Vector3.Lerp(b.self.position, wantedPosition, active);
            }

            if (b.children.Count <= 0) {
                continue;
            }
            if (b.self == root && !rotateRoot) {
                continue;
            }
            b.self.localRotation = b.localStartRot;
            b.rotationAdjust = Quaternion.Lerp(Quaternion.identity, Quaternion.FromToRotation(b.self.TransformDirection(b.children[0].localStartPos.normalized), (b.children[0].position - b.self.position).normalized), active);
            b.self.rotation = b.rotationAdjust * b.self.rotation;
        }
    }
    public void LateUpdate() {
        if (updateMode != UpdateType.LateUpdate) {
            return;
        }
        float timeToPass = Mathf.Min(Time.deltaTime, Time.maximumDeltaTime);
        while (timeToPass > Time.fixedDeltaTime) {
            Process(Time.fixedDeltaTime);
            timeToPass -= Time.fixedDeltaTime;
        }
        if (timeToPass > 0f) {
            Process(timeToPass);
        }
    }
    public void FixedUpdate() {
        if (updateMode != UpdateType.FixedUpdate) {
            return;
        }
        Process(Time.deltaTime);
    }
    public void Update() {
        if (updateMode != UpdateType.Update) {
            return;
        }
        float timeToPass = Mathf.Min(Time.deltaTime, Time.maximumDeltaTime);
        while (timeToPass > Time.fixedDeltaTime) {
            Process(Time.fixedDeltaTime);
            timeToPass -= Time.fixedDeltaTime;
        }
        if (timeToPass > 0f) {
            Process(timeToPass);
        }
    }
    //public void PrepareForChange() {
        //previousFrameBones = new List<VirtualBone>();
        //BuildVirtualBoneTree(previousFrameBones, root, root, depth);
    //}
    //public void ApplyChanges() {
        //List<VirtualBone> currentTree = new List<VirtualBone>();
        //BuildVirtualBoneTree(currentTree, root, root, depth);
//
        //for (int i = 0; i < bones.Count; i++) {
            //bones[i].localStartPos += currentTree[i].localStartPos - previousFrameBones[i].localStartPos;
            //bones[i].localStartRot = (Quaternion.Inverse(previousFrameBones[i].localStartRot) * currentTree[i].localStartRot) * bones[i].localStartRot;
        //}
    //}
    public VirtualBone GetVirtualBone(Transform t) {
        foreach( VirtualBone b in bones) {
            if (b.self == t) {
                return b;
            }
        }
        return null;
    }
    public bool IsSimulatingBone(Transform t) {
        if (bones == null) {
            Regenerate();
        }
        foreach( VirtualBone b in bones) {
            if (b.self == t) {
                return true;
            }
        }
        return false;
    }
}

