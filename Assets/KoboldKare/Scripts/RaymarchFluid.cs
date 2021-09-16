using System;
using System.Collections;
using System.Collections.Generic;
using KoboldKare;
using UnityEngine;
using UnityEngine.VFX;

public class RaymarchFluid : FluidOutput {
    private WaitForSeconds wait = new WaitForSeconds(0.1f);
    public Material splatterMat;
    public AudioClip streamSound;
    public PhysicMaterial fluidMaterial;
    public AudioClip goodhit, badhit;
    private RaymarchNode start;
    private AudioSource streamSource;
    private Rigidbody body;
    public float splashAmount = 20f;
    public VisualEffect splashes;
    public float fireVelocity = 6f;
    private bool splashing = false;
    private Bounds bounds = new Bounds();
    public bool fireAtStartFromParent = false;
    private WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();
    public bool fluidsOnly = true;
    private void OnEnable() {
        splashes.Stop();
    }
    private class RaymarchNode {
        public RaymarchNode(Vector3 position, Vector3 vel, ReagentContents contents, Color c, Material splatterMat, PhysicMaterial fluidMaterial, RaymarchFluid fluidparent, float vps) {
            gameObject = new GameObject("RaymarchFluidNode", new Type[] {typeof(Rigidbody), typeof(SphereCollider), typeof(RaymarchFluidBall), typeof(PhysicsAudio)});
            gameObject.layer = LayerMask.NameToLayer("Water");
            gameObject.transform.position = position;
            ball = gameObject.GetComponent<RaymarchFluidBall>();
            ball.splatterMaterial = splatterMat;
            ball.contents.Mix(contents);
            ball.fluid = fluidparent;
            ball.vps = vps;
            ball.emitColor = contents.GetColor(ReagentDatabase.instance);
            ball.emitColor.a = 1f;
            body = gameObject.GetComponent<Rigidbody>();
            body.collisionDetectionMode = CollisionDetectionMode.Continuous;
            body.velocity = vel;
            sphereCollider = gameObject.GetComponent<SphereCollider>();
            sphereCollider.radius = 0.1f;
            sphereCollider.sharedMaterial = fluidMaterial;
            SphereCollider triggerCollider = gameObject.AddComponent<SphereCollider>();
            triggerCollider.radius = 0.5f;
            triggerCollider.isTrigger = true;
        }
        public RaymarchFluidBall ball;
        public SphereCollider sphereCollider;
        public Rigidbody body;
        public GameObject gameObject;
        public List<RaymarchConnection> connections = new List<RaymarchConnection>();
    }
    private class RaymarchConnection {
        private static float radiusMultiplier = 0.2f;
        public RaymarchConnection(RaymarchNode a, RaymarchNode b, Color c) {
            this.a = a;
            this.b = b;
            capsuleShape = a.gameObject.AddComponent<RaymarchShape>();
            capsuleShape.radius = 0.001f;
            capsuleShape.blendStrength = 0.1f;
            capsuleShape.colour = c;
            capsuleShape.operation = RaymarchShape.Operation.Blend;
            capsuleShape.shapeType = RaymarchShape.ShapeType.Capsule;
            a.connections.Add(this);
            b.connections.Add(this);
        }
        public void Destroy() {
            GameObject.Destroy(capsuleShape);
            a.connections.Remove(this);
            b.connections.Remove(this);
        }
        public void PhysicsTick(float deltaTime) {
            Vector3 diff = a.body.position - b.body.position;
            float dist = Mathf.Max(diff.magnitude-desiredDistance, 0f);
            a.body.AddForce(-diff*dist*deltaTime*force);
            b.body.AddForce(diff*dist*deltaTime*force);
        }
        public void Tick() {
            float sideLength = Mathf.Max(Vector3.Distance(a.gameObject.transform.position, b.gameObject.transform.position),0.1f);
            float volume = (a.ball.contents.volume + b.ball.contents.volume);
            float radius = Mathf.Sqrt(volume/(Mathf.PI*sideLength));
            capsuleShape.radius = Mathf.MoveTowards(capsuleShape.radius, radius*radiusMultiplier, Time.deltaTime*4f);
            capsuleShape.blendStrength = capsuleShape.radius*1.5f;
            capsuleShape.localCapsuleOffset = capsuleShape.transform.InverseTransformPoint(b.gameObject.transform.position);
        }
        public float force = 300f;
        public float desiredDistance = 0.2f;
        public RaymarchNode a,b;
        public RaymarchShape capsuleShape;
    }
    private List<RaymarchNode> nodes = new List<RaymarchNode>();
    private List<RaymarchConnection> connections = new List<RaymarchConnection>();
    private bool firing;
    private bool connect = false;
    private Task updateTask = null;
    private Task fixedUpdateTask = null;
    private ReagentContents bucket;
    public float variance = 1.5f;
    public float vps = 2f;
    public void Fire(GameObject g) {
        Fire(g.GetComponent<GenericReagentContainer>().contents, vps);
    }
    private bool running = false;
    private float lastPlayedSoundTime = 0f;
    public override void Fire(ReagentContents b, float vps) {
        if (b.volume <= 0f) {
            return;
        }
        this.vps = vps;
        base.Fire(b, vps);
        splashes.SetVector4("Color",b.GetColor(ReagentDatabase.instance));
        bucket = b;
        running = true;
        if (updateTask == null || !updateTask.Running) {
            updateTask = new Task(UpdateTask());
        }
        if (fixedUpdateTask == null || !fixedUpdateTask.Running) {
            fixedUpdateTask = new Task(FixedUpdateTask());
        }
        StopCoroutine("FireRoutine");
        StartCoroutine("FireRoutine");
        streamSource.Play();
        connect = false;
    }
    public void Splash(GameObject g) {
        Splash(g.GetComponent<GenericReagentContainer>().contents);
    }
    public void Splash(ReagentContents b) {
        if (b.volume <= 0f || splashing) {
            return;
        }
        splashes.SetVector4("Color",b.GetColor(ReagentDatabase.instance));
        base.Fire(b, vps);
        bucket = b;
        splashing = true;
        running = true;
        if (updateTask == null || !updateTask.Running) {
            updateTask = new Task(UpdateTask());
        }
        if (fixedUpdateTask == null || !fixedUpdateTask.Running) {
            fixedUpdateTask = new Task(FixedUpdateTask());
        }
        StopCoroutine("SplashRoutine");
        StartCoroutine("SplashRoutine");
        streamSource.Play();
        connect = false;
    }
    public override void StopFiring() {
        base.StopFiring();
        running = false;
        //StopCoroutine("FireRoutine");
    }
    private void OnDestroy() {
        OnStop();
    }
    private void OnStop() {
        splashes.Stop();
        if (streamSource != null) {
            streamSource.Stop();
        }
        if (start != null) {
            start.body.isKinematic = false;
            start.body.collisionDetectionMode = CollisionDetectionMode.Continuous;
            start.body.velocity = transform.forward*fireVelocity + UnityEngine.Random.insideUnitSphere * 1.5f;
            start = null;
        }
    }
    public void TriggerGoodHit(Vector3 pos) {
        //if (goodhit != null && Time.timeSinceLevelLoad-lastPlayedSoundTime > goodhit.length) {
            //streamSource.PlayOneShot(goodhit);
            //lastPlayedSoundTime = Time.timeSinceLevelLoad;
        //}
    }
    public void TriggerBadHit(Vector3 pos) {
        //if (badhit != null && Time.timeSinceLevelLoad-lastPlayedSoundTime > goodhit.length) {
            //streamSource.PlayOneShot(badhit);
            //lastPlayedSoundTime = Time.timeSinceLevelLoad;
        //}
    }
    public IEnumerator SplashRoutine() {
        float spillAmount = Mathf.Max(Mathf.Min(bucket.maxVolume,splashAmount)/10f,splashAmount/10f);
        splashes.Play();
        for(int i=0;i<10&&(fluidsOnly?bucket.GetFluidVolume(ReagentDatabase.instance):bucket.volume) >= spillAmount;i++) {
            yield return null;
            ReagentContents spilled = fluidsOnly ? bucket.FilterFluids(spillAmount, ReagentDatabase.instance) : bucket.Spill(spillAmount);
            if (spilled.volume <= 0) {
                break;
            }
            Vector3 velocity = transform.forward*fireVelocity + UnityEngine.Random.insideUnitSphere * variance;
            if (body != null) {
                velocity += body.velocity;
            }
            nodes.Add(new RaymarchNode(transform.position, velocity, spilled, spilled.GetColor(ReagentDatabase.instance), splatterMat, fluidMaterial, this, spillAmount));
            streamSource.pitch = Math.Min(bucket.maxVolume/Mathf.Max((fluidsOnly?bucket.GetFluidVolume(ReagentDatabase.instance):bucket.volume),0.1f), 1.5f);
            if (nodes.Count > 1 && connect) {
                var connection = new RaymarchConnection(nodes[nodes.Count-1], nodes[nodes.Count-2], spilled.GetColor(ReagentDatabase.instance));
                connection.desiredDistance = 2f;
                connections.Add(connection);
            }
            // Make a connection next time.
            connect = true;
            if (start != null) {
                start.body.isKinematic = false;
                start.body.collisionDetectionMode = CollisionDetectionMode.Continuous;
                start.body.velocity = transform.forward*6f + UnityEngine.Random.insideUnitSphere * 1.5f;
                start = null;
            }
            start = nodes[nodes.Count-1];
            start.body.collisionDetectionMode = CollisionDetectionMode.Discrete;
            start.body.isKinematic = true;
        }
        splashing = false;
        streamSource.Stop();
        OnStop();
    }
    public IEnumerator FireRoutine() {
        splashes.Play();
        while(running && (fluidsOnly?bucket.GetFluidVolume(ReagentDatabase.instance):bucket.volume) > 0f || ((start == null || start.connections.Count<=0) && (fluidsOnly?bucket.GetFluidVolume(ReagentDatabase.instance):bucket.volume) > 0f)) {
            yield return wait;
            ReagentContents spilled = fluidsOnly ? bucket.FilterFluids(vps/10f, ReagentDatabase.instance) : bucket.Spill(vps/10f);
            Vector3 velocity = transform.forward*fireVelocity + UnityEngine.Random.insideUnitSphere * variance;
            if (body != null) {
                velocity += body.velocity;
            }
            nodes.Add(new RaymarchNode(transform.position, velocity, spilled, spilled.GetColor(ReagentDatabase.instance), splatterMat, fluidMaterial, this, vps*0.06f));
            streamSource.pitch = Math.Min(bucket.maxVolume/Mathf.Max((fluidsOnly?bucket.GetFluidVolume(ReagentDatabase.instance):bucket.volume),0.1f), 1.5f);
            if (nodes.Count > 1 && connect) {
                var connection = new RaymarchConnection(nodes[nodes.Count-1], nodes[nodes.Count-2], spilled.GetColor(ReagentDatabase.instance));
                connections.Add(connection);
            }
            // Make a connection next time.
            connect = true;
            if (start != null && start.body != null) {
                start.body.isKinematic = false;
                start.body.collisionDetectionMode = CollisionDetectionMode.Continuous;
                start.body.velocity = transform.forward*6f + UnityEngine.Random.insideUnitSphere * 1.5f;
                start = null;
            }
            start = nodes[nodes.Count-1];
            start.body.collisionDetectionMode = CollisionDetectionMode.Discrete;
            start.body.isKinematic = true;
        }
        OnStop();
    }
    public void Start() {
        body = GetComponentInParent<Rigidbody>();
        splashes.Stop();
        var steamAudioSetting = UnityScriptableSettings.ScriptableSettingsManager.instance.GetSetting("SteamAudio");
        streamSource = gameObject.AddComponent<AudioSource>();
        streamSource.spatialize = steamAudioSetting.value > 0f;
        streamSource.spatialBlend = 1f;
        streamSource.rolloffMode = AudioRolloffMode.Logarithmic;
        streamSource.outputAudioMixerGroup = GameManager.instance.soundEffectGroup;
        streamSource.playOnAwake = false;
        streamSource.loop = true;
        streamSource.clip = streamSound;
        SteamAudio.SteamAudioSource saudioSource = gameObject.AddComponent<SteamAudio.SteamAudioSource>();
        saudioSource.enabled = steamAudioSetting.value > 0f;
        if (fireAtStartFromParent) {
            var cont = GetComponentInParent<GenericReagentContainer>();
            if (cont != null) {
                Fire(cont.contents, vps);
            }
        }
    }
    IEnumerator UpdateTask() {
        while (nodes.Count > 0 || (this != null && this.running)) {
            foreach(var connection in connections) {
                connection.Tick();
            }

            for(int i=0;i<nodes.Count;i++) {
                var ball = nodes[i];
                if (ball.ball.contents.volume <= 0f) {
                    for(int j=0;ball.connections.Count>0;) {
                        connections.Remove(ball.connections[j]);
                        ball.connections[j].Destroy();
                    }
                    Destroy(ball.gameObject);
                    nodes.Remove(ball);
                }
            }
            if (start != null && start.gameObject && this != null && transform != null) {
                start.gameObject.transform.position = transform.position;
            }
            yield return null;
        }
    }
    IEnumerator FixedUpdateTask() {
        while(nodes.Count > 0 || (this != null && this.running)) {
            foreach(var connection in connections) {
                connection.PhysicsTick(Time.deltaTime);
            }
            yield return waitForFixedUpdate;
        }
    }
}
