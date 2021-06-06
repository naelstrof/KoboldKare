using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verlet;

public class FluidStreamSystem : MonoBehaviour {
    private LineRenderer linerenderer;
    //private List<Vector3> velocity = new List<Vector3>();
    private List<Node> nodes = new List<Node>();
    private VerletSimulator simulator;
    [Range(0f, 1f)]
    public float particleDistance = 0.1f;
    [Range(2, 256)]
    public int particleLimit = 128;
    [Range(1, 32)]
    public int iterations = 8;
    public LayerMask hitMask;
    private int endPoint;
    private int mod(int x, int m) {
        return (x % m + m) % m;
    }
    void Start() {
        linerenderer = GetComponent<LineRenderer>();
        nodes = new List<Node>();
        nodes.Add(new Node(transform.position+transform.forward));
        nodes.Add(new Node(transform.position));
        endPoint = 1;
        //velocity.Add(Vector3.forward);
        //velocity.Add(Vector3.zero);
        Edge e = new Edge(nodes[0], nodes[1], particleDistance);
        nodes[0].Connect(e);
        nodes[1].Connect(e);
        simulator = new VerletSimulator(nodes);
    }
    void FixedUpdate() {
        simulator.Simulate(iterations, Time.fixedDeltaTime);

        int endPointMinus = endPoint;
        if (nodes.Count >= particleLimit) {
            endPoint = mod(endPoint + 1, particleLimit);
            // Destroy all connections, this is a new node now.
            foreach (Edge edge in nodes[endPoint].Connection) {
                edge.Other(nodes[endPoint]).Connection.Remove(edge);
            }
            nodes[endPoint].Connection.Clear();
        } else {
            nodes.Add(new Node(transform.position));
            endPoint++;
        }
        Edge e = new Edge(nodes[endPoint], nodes[endPointMinus], particleDistance);
        nodes[endPoint].Connect(e);
        nodes[endPointMinus].Connect(e);
        nodes[endPoint].position = transform.position;
        nodes[endPoint].prev = transform.position;

        /*nodes.Add(new Node(transform.position));
        Edge e = new Edge(nodes[nodes.Count - 1], nodes[nodes.Count - 2], 0.5f);
        nodes[nodes.Count - 1].Connect(e);
        nodes[nodes.Count - 2].Connect(e);
        velocity.Add(Vector3.zero);*/

        //velocity.Add(Vector3.forward*0.1f);
        for ( int i=0;i<nodes.Count;i++ ) {
            foreach(Collider c in Physics.OverlapSphere(nodes[i].position, 0.1f, hitMask, QueryTriggerInteraction.Ignore)) {
                if (c is MeshCollider && !((MeshCollider)c).convex) {
                    Ray ray = new Ray(nodes[i].position, Vector3.down);
                    RaycastHit info;
                    if (c.Raycast(ray, out info, 0.1f)) {
                        nodes[i].position = info.point + Vector3.up * 0.1f;
                    }
                } else {
                    nodes[i].position = c.ClosestPoint(nodes[i].position);
                }
            }
            nodes[i].position += Vector3.down * 0.1f * Time.fixedDeltaTime;
            //velocity[i] += Vector3.down * Time.fixedDeltaTime;
            //nodes[i].position += velocity[i] * Time.fixedDeltaTime;
        }
        linerenderer.positionCount = nodes.Count;
        for (int i=0;i<nodes.Count;i++ ) {
            int o = mod(i + (endPoint+1), nodes.Count);
            linerenderer.SetPosition(i, nodes[o].position);
        }
        //linerenderer.SetPositions(nodes.Select(p => p.position).ToArray());
    }
}
