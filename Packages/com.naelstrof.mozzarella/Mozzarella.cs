using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PenetrationTech;
using Unity.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
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
        [SerializeField]
        private float volumeMultiplier = 1f;

        [SerializeField] private float lifetime = 2.25f;
        [SerializeField] private LayerMask decalHitMask;
        
        private List<Particle> particles;
        private NativeArray<Vector3> particlePoints;
        private LineRenderer lineRenderer;
        private AnimationCurve widthCurve;
        private int id = 0;
        private Keyframe[] keys;
        private static int particleCount = 50;
        private float dieTime = 0f;
        private static RaycastHit[] hits = new RaycastHit[32];
        public delegate void HitCallbackAction(RaycastHit hit, Vector3 startPos, Vector3 dir, float length, float volume);
        public event HitCallbackAction hitCallback;
        private Penetrator followPenetrator;
        private Transform followTransform;
        private Vector3 localForward = Vector3.forward;
        private int lastFrame;

        public void SetFollowPenetrator(Penetrator target) {
            followPenetrator = target;
        }

        public void SetLocalForward(Vector3 newForward) {
            localForward = newForward;
        }

        public void SetFollowTransform(Transform target) {
            followTransform = target;
        }

        public void SetVolumeMultiplier(float multi) {
            volumeMultiplier = multi;
        }
        
        public void SetLineColor(Color color){
            lineMaterial.color = color;
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
            lineRenderer.generateLightingData = true;
            lineRenderer.material = lineMaterial;
            lineRenderer.positionCount = 0;
            lineRenderer.textureMode = LineTextureMode.Tile;
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
            followPenetrator = null;
            followTransform = null;
            hitCallback = null;
            localForward = Vector3.forward;
            base.Reset();
        }

        private void FixedUpdate() {
            if (followPenetrator != null) {
                float dist = followPenetrator.GetWorldLength();
                var path = followPenetrator.GetSplinePath();
                transform.position = path.GetPositionFromDistance(dist);
                transform.rotation = Quaternion.LookRotation(path.GetVelocityFromDistance(dist), Vector3.up);
            }

            if (followTransform != null) {
                transform.position = followTransform.position;
                transform.rotation = followTransform.rotation;
            }

            // Only spawn one particle per real-frame-- so we don't overlay particles on top one-another.
            if (id < particles.Count && lastFrame != Time.frameCount) {
                float t = (float)id / (float)particles.Count;
                if (followPenetrator != null) {
                    Rigidbody body = followPenetrator.GetComponentInParent<Rigidbody>();
                    if (body != null) {
                        particles[id].Spawn(transform.position,
                         transform.TransformDirection(localForward) *
                            (velocityCurve.Evaluate(t) * velocityMultiplier * Time.deltaTime) +
                            body.GetPointVelocity(transform.position) *
                            (0.5f * Time.deltaTime));
                    } else {
                        particles[id].Spawn(transform.position,
                            transform.TransformDirection(localForward) *
                            (velocityCurve.Evaluate(t) * velocityMultiplier * Time.deltaTime));
                    }
                } else {
                    particles[id].Spawn(transform.position,
                        transform.TransformDirection(localForward) * (velocityCurve.Evaluate(t) * velocityMultiplier * Time.deltaTime));
                }

                transform.rotation *= Quaternion.Lerp(Random.rotation, Quaternion.identity, 0.99f);
                id++;
                lastFrame = Time.frameCount;
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
            for (int i = id-1; i > skip; i-=skip) {
                Vector3 diff = particles[i - skip].position - particles[i].position;
                Vector3 dir = diff.normalized;
                float dist = diff.magnitude;
                
                if (dist > 5f) {
                    continue;
                }

                int hitcount = Physics.RaycastNonAlloc(particles[i].position, dir, hits, dist, decalHitMask, QueryTriggerInteraction.Ignore);
                for (int j = 0; j < hitcount; j++) {
                    HitCallback(hits[j], particles[i].position-dir*0.1f, dir, dist+0.1f, ((float)i/(float)particles.Count));
                }
            }
        }

        private void HitCallback(RaycastHit hit, Vector3 startPos, Vector3 dir, float length, float progression) {
            hitCallback?.Invoke(hit, startPos, dir, length, volumeCurve.Evaluate(progression)*volumeMultiplier);
        }

        private void Update() {
            if (dieTime < Time.time) {
                Reset();
                return;
            }

            for (int i = 0; i < particles.Count; i++) {
                particlePoints[i] = particles[i].position;
            }
            float done = (float)id / (float)particles.Count;
            for (int i = 0; i < keys.Length; i++) {
                float t = (float)i / (float)keys.Length;
                float volumeT = t * done;
                keys[i].time = (float)i / (float)keys.Length;
                keys[i].value = volumeCurve.Evaluate(volumeT)*volumeMultiplier;
            }

            lineRenderer.positionCount = particles.Count;
            lineRenderer.SetPositions(particlePoints);
            widthCurve.keys = keys;
            lineRenderer.widthCurve = widthCurve;
        }

        private void OnDrawGizmosSelected() {
            for (int i = 0; i < particles.Count; i++) {
                Gizmos.DrawWireSphere(particles[i].position, 0.05f);
                #if UNITY_EDITOR
                Handles.Label(particles[i].position, i.ToString());
                #endif
            }
        }
    }
}
