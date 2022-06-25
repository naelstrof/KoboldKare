using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PenetrationTech;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.VFX;
using Random = UnityEngine.Random;


namespace Naelstrof.Mozzarella {
    public class Mozzarella : PooledItem {
        private class Particle {
            private Vector3 lastPosition;
            public Vector3 position;
            public void Spawn(Vector3 position, Vector3 velocity) {
                lastPosition = position - velocity;
                this.position = position;
            }
            public void Step(float deltaTime) {
                Vector3 newPosition = position + (position-lastPosition) + Physics.gravity*(deltaTime*deltaTime);
                lastPosition = position;
                position = newPosition;
            }
        }
        [SerializeField]
        private Material lineMaterial;
        [SerializeField]
        private AnimationCurve velocityCurve;
        [SerializeField]
        private AnimationCurve volumeCurve;

        [SerializeField]
        private float velocityMultiplier = 0.01f;

        [SerializeField] private float lifetime = 3f;
        [SerializeField] private LayerMask decalHitMask;
        [SerializeField] private Material decalProjector;
        
        private List<Particle> particles;
        private NativeArray<Vector3> particlePoints;
        private LineRenderer lineRenderer;
        private AnimationCurve widthCurve;
        private int id = 0;
        private Keyframe[] keys;
        private static int particleCount = 50;
        private float dieTime = 0f;
        private static RaycastHit[] hits = new RaycastHit[32];
        public delegate void HitCallbackAction(RaycastHit hit, Vector3 startPos, Vector3 dir, float length, float progression);
        public event HitCallbackAction hitCallback;
        private Penetrator followPenetrator;

        public void SetFollowPenetrator(Penetrator target) {
            followPenetrator = target;
        }

        private void Awake() {
            widthCurve = new AnimationCurve();
            particles = new List<Particle>();
            for (int i = 0; i < particleCount; i++) {
                particles.Add(new Particle());
            }
            particlePoints = new NativeArray<Vector3>(particles.Count, Allocator.Persistent);
            keys = new Keyframe[16];
            for (int i = 0; i < keys.Length; i++) {
                keys[i] = new Keyframe();
            }

            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = true;
            lineRenderer.material = lineMaterial;
            lineRenderer.positionCount = 0;
        }

        void OnEnable() {
            id = 0;
            dieTime = Time.time + lifetime;
        }

        private void OnDestroy() {
            particlePoints.Dispose();
        }

        public override void Reset() {
            gameObject.SetActive(false);
            lineRenderer.positionCount = 0;
            id = 0;
            hitCallback = null;
            base.Reset();
        }

        private void FixedUpdate() {
            if (id < particles.Count) {
                float t = (float)id / (float)particles.Count;
                particles[id].Spawn(transform.position, transform.forward * (velocityCurve.Evaluate(t) * velocityMultiplier));
                transform.rotation *= Quaternion.Lerp(Random.rotation, Quaternion.identity, 0.99f);
                id++;
            }


            for (int i = 0; i < id; i++) {
                particles[i].Step(Time.deltaTime);
            }
            for (int i = id; i < particles.Count; i++) {
                particles[i].Spawn(transform.position, Vector3.zero);
            }

            DoCollisions();
        }

        private void DoCollisions() {
            int skip = 5;
            for (int i = 0; i < id-skip; i+=skip) {
                Vector3 diff = particles[i + skip].position - particles[i].position;
                Vector3 dir = diff.normalized;
                float dist = diff.magnitude;
                int hitcount = Physics.RaycastNonAlloc(particles[i].position, dir, hits, dist, decalHitMask);
                for (int j = 0; j < hitcount; j++) {
                    HitCallback(hits[j], particles[i].position-dir*0.1f, dir, dist+0.1f, ((float)i/(float)particles.Count));
                }
            }
        }

        private void HitCallback(RaycastHit hit, Vector3 startPos, Vector3 dir, float length, float progression) {
            SkinnedMeshDecals.PaintDecal.RenderDecalForCollider(hit.collider, decalProjector, startPos,
                Quaternion.FromToRotation(Vector3.forward,dir), Vector2.one*(volumeCurve.Evaluate(progression)*2f), length);
            hitCallback?.Invoke(hit, startPos, dir, length, progression);
            //Debug.DrawLine(startPos, startPos + dir * length, Color.red,1f);
        }

        private void Update() {
            if (dieTime < Time.time) {
                Reset();
                return;
            }

            if (followPenetrator != null) {
                float dist = followPenetrator.GetWorldLength();
                var path = followPenetrator.GetSplinePath();
                transform.position = path.GetPositionFromDistance(dist);
                transform.rotation = Quaternion.LookRotation(path.GetVelocityFromDistance(dist), Vector3.up);
            }
            //transform.position = followTarget.transform.position + followTarget.TransformVector(followTargetOffset);
            //transform.rotation = followTarget.transform.rotation;

            for (int i = 0; i < particles.Count; i++) {
                particlePoints[i] = particles[i].position;
            }
            float done = (float)id / (float)particles.Count;
            for (int i = 0; i < keys.Length; i++) {
                float t = (float)i / (float)keys.Length;
                float volumeT = t * done;
                keys[i].time = (float)i / (float)keys.Length;
                keys[i].value = volumeCurve.Evaluate(volumeT);
            }

            lineRenderer.positionCount = particles.Count;
            lineRenderer.SetPositions(particlePoints);
            widthCurve.keys = keys;
            lineRenderer.widthCurve = widthCurve;
        }

        private void OnDrawGizmosSelected() {
            for (int i = 0; i < particles.Count; i++) {
                Gizmos.DrawWireSphere(particles[i].position, 0.05f);
                Handles.Label(particles[i].position, i.ToString());
            }
        }
    }
}
