using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class SuckingMachine : UsableMachine {
    [SerializeField]
    private SphereCollider suckZone;
    public List<int> suckingIDs=new List<int>();
    private HashSet<Rigidbody> trackedRigidbodies;
    private bool sucking;
    private WaitForFixedUpdate waitForFixedUpdate;
    protected NetworkedEntity networkedEntity;
    private HashSet<NetworkedEntity> processingEntities;
    private float tickTime = 0f;
    protected virtual void Awake() {
        trackedRigidbodies = new HashSet<Rigidbody>();
        waitForFixedUpdate = new WaitForFixedUpdate();
        processingEntities = new HashSet<NetworkedEntity>();
    }

    private Vector3 GetSuckLocation() {
        return suckZone.transform.TransformPoint(suckZone.center);
    }
    private float GetSuckRadius() {
        return suckZone.transform.lossyScale.x*suckZone.radius;
    }

    protected override void Start() {
        base.Start();
        networkedEntity = GetComponentInParent<NetworkedEntity>();
    }

    void Update() {
        tickTime += Time.deltaTime;
        if (tickTime > 0.5f) {
            foreach (var ent in processingEntities) {
                if (ent is NetworkedKobold targetKobold && targetKobold.TryGetKobold(out var kobold)) {
                    if (KoboldEntitySpawner.GetIsPlayerKobold(targetKobold)) {
                        continue;
                    }

                    if (kobold.grabbed || !kobold.GetComponent<Ragdoller>().ragdolled) {
                        continue;
                    }

                    if (networkedEntity.CanUse(targetKobold)) {
                        networkedEntity.TryUse(targetKobold);
                    }

                    continue;
                }

                Rigidbody body = ent.GetComponentInParent<Rigidbody>();
                if (body != null && body.gameObject.GetComponent<MoneyPile>() == null) {
                    trackedRigidbodies.Add(body);
                    if (!sucking) {
                        StartCoroutine(SuckAndSwallow());
                    }
                }
            }

            tickTime = 0f;
            processingEntities.Clear();
        }
    }

    protected virtual void OnSwallowed(NetworkedEntity ent) {
        if (suckingIDs.Contains(ent.ObjectId)) {
            suckingIDs.Remove(ent.ObjectId);
        }
    }

    protected virtual bool ShouldStopTracking(Rigidbody body) {
        if (body == null) {
            return true;
        }

        float distance = Vector3.Distance(body.ClosestPointOnBounds(GetSuckLocation()), GetSuckLocation());
        if (distance > GetSuckRadius()+1f) {
            return true;
        }
        if (distance < 0.1f) {
            var networkEntity = body.gameObject.GetComponentInParent<NetworkedEntity>();
            if (networkEntity != null && networkEntity.IsOwner) {
                OnSwallowed(networkEntity);
            }
            return true;
        }
        return false;
    }

    IEnumerator SuckAndSwallow() {
        sucking = true;
        while (isActiveAndEnabled && trackedRigidbodies.Count > 0) {
            trackedRigidbodies.RemoveWhere(ShouldStopTracking);
            foreach (var body in trackedRigidbodies) {
                body.velocity = Vector3.MoveTowards(body.velocity, Vector3.zero, body.velocity.magnitude*Time.deltaTime * 10f);
                body.AddForce((GetSuckLocation()-body.transform.TransformPoint(body.centerOfMass))*30f, ForceMode.Acceleration);
            }
            yield return waitForFixedUpdate;
        }
        sucking = false;
    }
    

    protected virtual void OnTriggerEnter(Collider other) {
        var ent = other.GetComponentInParent<NetworkedEntity>();
        processingEntities.Add(ent);
    }
}
