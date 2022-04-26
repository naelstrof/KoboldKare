using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Verlet;

public class Strand : MonoBehaviour {
    public Strand() {
        particlesPerMeter = 30f;
        maxDistance = 0.9f;
        maxParticles = 50;
        gravity = new Vector3(0, -0.1f, 0);
        iterations = 4;
        decay = 1f;
        volume = 0.06f;
        targetOffset = Vector3.zero;
    }
    public Transform target;
    public Vector3 targetOffset = Vector3.zero;
    public float targetOffsetMultiplier = 0;
    [Range(0.1f, 50f)]
    public float particlesPerMeter = 30f;
    [Range(0f, 10f)]
    public float maxDistance = 1f;
    [Range(2, 50)]
    public int maxParticles = 50;
    public Vector3 gravity = new Vector3(0, -0.5f, 0);
    [Range(1, 32)]
    public int iterations = 4;
    [Range(0.85f, 1f)]
    public float decay = 1f;
    [Range(0f, 10f)]
    public float volume = 0.06f;

    public Material lineMaterial;

    private Keyframe[] frames = new Keyframe[3];

    private LineRenderer lineRenderer;
    private LineRenderer snapRenderer;

    private AnimationCurve lineCurve;

    private VerletSimulator simulator;
    private int incIndex;
    private List<Node> nodes;
    private List<Edge> edges;
    private bool snapped = false;
    //private float overallAlpha = 1f;
    public void SetAlpha(float a) {
        if (lineRenderer == null || snapRenderer == null || snapped) {
            return;
        }
        Color c;
        c = lineRenderer.material.GetColor("_Color");
        c.a = a;
        lineRenderer.material.SetColor("_Color", c);
        snapRenderer.material.SetColor("_Color", c);
    }

    public Vector3 GetTransformOffset(Transform t, Vector3 off) {
        off *= targetOffsetMultiplier;
        return t.right * off.x + t.up * off.y + t.forward * off.z;
    }

    public void Start() {
        if (lineMaterial == null || lineRenderer != null) {
            return;
        }
        frames = new Keyframe[3];
        GameObject a = new GameObject();
        a.transform.parent = transform;
        a.transform.localScale = new Vector3(1, 1, 1);
        GameObject b = new GameObject();
        b.transform.parent = transform;
        b.transform.localScale = new Vector3(1, 1, 1);
        snapRenderer = a.AddComponent<LineRenderer>();
        lineRenderer = b.AddComponent<LineRenderer>();
        snapRenderer.generateLightingData = true;
        lineRenderer.generateLightingData = true;
        lineRenderer.materials = new Material[]{ lineMaterial };
        snapRenderer.materials = new Material[]{ lineMaterial };
        lineRenderer.numCapVertices = 3;
        snapRenderer.numCapVertices = 3;
        lineRenderer.positionCount = 0;
        snapRenderer.positionCount = 0;

        edges = new List<Edge>(maxParticles-1);
        nodes = new List<Node>(maxParticles);
        for(int i=0;i<maxParticles;i++) {
            nodes.Add(new Node(Vector3.zero));
        }
        for (int i = 0; i < maxParticles - 1; i++) {
            edges.Add(new Edge(nodes[i], nodes[i + 1]));
        }
        nodes[0].position = nodes[0].prev = transform.position;
        nodes[1].position = nodes[1].prev = target.position+GetTransformOffset(target, targetOffset);
        nodes[0].Connect(edges[0]);
        nodes[1].Connect(edges[0]);
        incIndex = 2;

        simulator = new VerletSimulator(nodes);

        frames[0] = new Keyframe(0, volume);
        frames[1] = new Keyframe(0.5f, volume);
        frames[2] = new Keyframe(1, volume);
        lineCurve = new AnimationCurve(frames);
        lineRenderer.widthCurve = lineCurve;
    }

    void AddNode() {
        int nodeToWorkOn = incIndex++;
        //nodes.Add(new Node(target.position+GetTransformOffset(target, targetOffset)));
        //nodes[nodeToWorkOn].position = nodes[nodeToWorkOn].prev = (target.position + GetTransformOffset(target, targetOffset)) + UnityEngine.Random.insideUnitSphere*0.01f;
        nodes[nodeToWorkOn].position = nodes[nodeToWorkOn - 1].position + UnityEngine.Random.insideUnitSphere*0.01f;
        nodes[nodeToWorkOn].prev = nodes[nodeToWorkOn - 1].prev + UnityEngine.Random.insideUnitSphere*0.01f;
        // Reset nearby velocities.
        //nodes[nodeToWorkOn - 1].prev = nodes[nodeToWorkOn - 1].position;
        Debug.DrawLine(nodes[nodeToWorkOn].position, nodes[nodeToWorkOn].position + Vector3.up * 0.03f);
        Node a = nodes[nodeToWorkOn];
        Node b = nodes[nodeToWorkOn-1];
        a.Connect(edges[nodeToWorkOn-1]);
        b.Connect(edges[nodeToWorkOn-1]);
    }

    void RemoveNode() {
        int nodeToWorkOn = incIndex--;
        Node a = nodes[nodeToWorkOn];
        // reset velocity;
        nodes[nodeToWorkOn].prev = nodes[nodeToWorkOn].position;
        Node b = nodes[nodeToWorkOn-1];
        a.Connection.Remove(edges[nodeToWorkOn-1]);
        b.Connection.Remove(edges[nodeToWorkOn-1]);
    }

    void FixDistance() {
        float length = Vector3.Distance(transform.position, target.position+GetTransformOffset(target, targetOffset)) / (incIndex - 1);
        foreach( Node n in nodes) {
            foreach( Edge e in n.Connection ) {
                e.Length = length;
            }
        }
    }
    void ReduceDistance() {
        foreach( Node n in nodes) {
            foreach( Edge e in n.Connection ) {
                e.Length = Mathf.MoveTowards(e.Length, 0.000001f, Time.fixedDeltaTime);
            }
        }
        Color c;
        if (snapped) {
            c = snapRenderer.material.GetColor("_Color");
        } else {
            c = lineRenderer.material.GetColor("_Color");
        }
        c.a = Mathf.MoveTowards(c.a, 0, Time.fixedDeltaTime);
        if (c.a == 0 ) {
            Destroy(this);
        }
        lineRenderer.material.SetColor("_Color", c);
        snapRenderer.material.SetColor("_Color", c);
    }

    void OnDestroy() {
        Destroy(lineRenderer.gameObject);
        Destroy(snapRenderer.gameObject);
    }

    void Gravity() {
        foreach (Node p in nodes) {
            p.position += Time.deltaTime * gravity;
        }
    }

    void Update() {
        if (Time.deltaTime <= 0f) {
            return;
        }
        // Create/Destroy particles
        if (target == null) {
            Destroy(this);
            return;
        }
        float dist = Vector3.Distance(transform.position, target.position+GetTransformOffset(target, targetOffset));
        if ( dist > maxDistance && !snapped) {
            snapped = true;
            int half = incIndex / 2;
            // Cut it at the center
            nodes[half].Connection.Remove(edges[half]);
            nodes[half+1].Connection.Remove(edges[half]);

            nodes[half-1].Connection.Remove(edges[half-1]);
            nodes[half].Connection.Remove(edges[half-1]);
            Keyframe[] newFramesA = new Keyframe[2];
            Keyframe[] newFramesB = new Keyframe[2];
            newFramesA[0] = new Keyframe(0, frames[0].value);
            newFramesA[1] = new Keyframe(1, frames[1].value);
            newFramesB[0] = new Keyframe(0, frames[1].value);
            newFramesB[1] = new Keyframe(1, frames[2].value);
            lineCurve.keys = newFramesA;
            lineRenderer.widthCurve = lineCurve;
            snapRenderer.widthCurve = new AnimationCurve(newFramesB);
        }

        if (!snapped) {
            int desiredParticles = Mathf.Min(Mathf.Max(Mathf.FloorToInt(dist * particlesPerMeter), 2), maxParticles-1);
            if (desiredParticles != incIndex) {
                if (incIndex < desiredParticles) {
                    AddNode();
                }
                if (incIndex > desiredParticles) {
                    RemoveNode();
                }
                FixDistance();
            }

            //float m = Mathf.Max((1f - dist / maxDistance), 0.001f);
            float adjust = ((dist + 1f) * (dist + 1f)) - 1f;
            frames[0].value = Mathf.Clamp01((volume * 2f) / Mathf.Clamp(adjust, 0.5f, 8f));
            frames[1].value = Mathf.Clamp01((volume / 2f) / Mathf.Clamp(adjust, 0.5f, 8f));
            frames[2].value = Mathf.Clamp01((volume * 2f) / Mathf.Clamp(adjust, 0.5f, 8f));
            lineCurve.keys = frames;
            lineRenderer.widthCurve = lineCurve;
        } else {
            ReduceDistance();
        }


        // Step the simulation
        Gravity();
        simulator.Simulate(3, Time.deltaTime);

        nodes[0].position = transform.position;
        nodes[incIndex-1].position = target.position+GetTransformOffset(target, targetOffset);
        //for (int i = 0; i < incIndex; i++) {
            //if (float.IsNaN(nodes[i].position.x + nodes[i].position.y + nodes[i].position.z)) {
                //nodes[i].position = nodes[i].prev = transform.position;
            //}
        //}
        if (!snapped) {
            // Update renderer
            lineRenderer.positionCount = incIndex;
            for( int i=0;i<incIndex;i++ ) {
                lineRenderer.SetPosition(i, nodes[i].position);
            }
        } else {
            int half = incIndex / 2;
            lineRenderer.positionCount = half;
            if (incIndex % 2 == 0) {
                snapRenderer.positionCount = half-1;
            } else {
                snapRenderer.positionCount = half;
            }
            for(int i=0;i<incIndex;i++ ) {
                if (i<half) {
                    lineRenderer.SetPosition(i, nodes[i].position);
                } else if (i>half) {
                    snapRenderer.SetPosition(i-(half+1), nodes[i].position);
                }
            }
        }
    }
}
