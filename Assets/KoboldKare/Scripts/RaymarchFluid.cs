using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaymarchFluid : FluidOutput {
    private WaitForSeconds wait = new WaitForSeconds(0.1f);
    public Material splatterMat;
    private class RaymarchNode {
        public RaymarchNode(Vector3 position, Vector3 vel, ReagentContents contents, Color c, Material splatterMat) {
            gameObject = new GameObject("RaymarchFluidNode", new Type[] {typeof(Rigidbody), typeof(SphereCollider), typeof(RaymarchFluidBall)});
            gameObject.layer = LayerMask.NameToLayer("Water");
            gameObject.transform.position = position;
            ball = gameObject.GetComponent<RaymarchFluidBall>();
            ball.splatterMaterial = splatterMat;
            ball.contents.Mix(contents);
            body = gameObject.GetComponent<Rigidbody>();
            body.collisionDetectionMode = CollisionDetectionMode.Continuous;
            body.velocity = vel;
            sphereCollider = gameObject.GetComponent<SphereCollider>();
            sphereCollider.radius = 0.1f;
        }
        public RaymarchFluidBall ball;
        public SphereCollider sphereCollider;
        public Rigidbody body;
        public GameObject gameObject;
        public List<RaymarchConnection> connections = new List<RaymarchConnection>();
    }
    private class RaymarchConnection {
        private static float volumeMultiplier = 0.1f;
        public RaymarchConnection(RaymarchNode a, RaymarchNode b, Color c) {
            this.a = a;
            this.b = b;
            capsuleShape = a.gameObject.AddComponent<RaymarchShape>();
            float sideLength = Vector3.Distance(a.gameObject.transform.position, b.gameObject.transform.position);
            float volume = (a.ball.contents.volume + b.ball.contents.volume)*volumeMultiplier;
            float radius = Mathf.Sqrt(volume/(Mathf.PI*sideLength));
            capsuleShape.radius = radius;
            capsuleShape.blendStrength = 0.5f;
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
            a.body.AddForce(-diff*dist*deltaTime*300f);
            b.body.AddForce(diff*dist*deltaTime*300f);
        }
        public void Tick() {
            float sideLength = Mathf.Max(Vector3.Distance(a.gameObject.transform.position, b.gameObject.transform.position),0.1f);
            float volume = (a.ball.contents.volume + b.ball.contents.volume)*volumeMultiplier;
            float radius = Mathf.Sqrt(volume/(Mathf.PI*sideLength));
            capsuleShape.radius = radius;
            capsuleShape.localCapsuleOffset = capsuleShape.transform.InverseTransformPoint(b.gameObject.transform.position);
        }
        public float desiredDistance = 0.2f;
        public RaymarchNode a,b;
        public RaymarchShape capsuleShape;
    }
    private List<RaymarchNode> nodes = new List<RaymarchNode>();
    private List<RaymarchConnection> connections = new List<RaymarchConnection>();
    private bool firing;
    private ReagentContents bucket;
    public float vps = 2f;
    public void Fire(GameObject g) {
        Fire(g.GetComponent<GenericReagentContainer>().contents, vps);
    }
    private bool running = false;
    public override void Fire(ReagentContents b, float vps) {
        this.vps = vps;
        base.Fire(b, vps);
        bucket = b;
        running = true;
        StartCoroutine("FireRoutine");
    }
    public override void StopFiring() {
        base.StopFiring();
        running = false;
        StopCoroutine("FireRoutine");
    }
    public IEnumerator FireRoutine() {
        while(running) {
            ReagentContents spilled = bucket.Spill(vps/10f);
            nodes.Add(new RaymarchNode(transform.position, transform.forward*8f + UnityEngine.Random.insideUnitSphere * 1f, spilled, spilled.GetColor(ReagentDatabase.instance), splatterMat));
            if (nodes.Count > 1 && Vector3.Distance(nodes[nodes.Count-2].gameObject.transform.position, nodes[nodes.Count-1].gameObject.transform.position) < 3f) {
                var connection = new RaymarchConnection(nodes[nodes.Count-1], nodes[nodes.Count-2], spilled.GetColor(ReagentDatabase.instance));
                connections.Add(connection);
            }
            yield return wait;
        }
    }
    public void Update() {
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
    }
    public void FixedUpdate() {
        foreach(var connection in connections) {
            connection.PhysicsTick(Time.deltaTime);
        }
    }
}
