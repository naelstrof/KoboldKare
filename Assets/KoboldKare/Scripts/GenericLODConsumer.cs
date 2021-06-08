using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using KoboldKare;

public class GenericLODConsumer : MonoBehaviour {
    [System.Serializable]
    public class LODCondition : SerializableCallback<bool> { };
    [HideInInspector]
    public bool isClose = false;
    [HideInInspector]
    public bool isVeryFar = false;
    public UnityEvent OnLODClose;
    public UnityEvent OnLODFar;
    public UnityEvent OnLODImposter;
    public UnityEvent OnLODUnImposter;
    public List<Rigidbody> trackedRigidbodies;

    public List<LODCondition> canLODCondition;
    public Task LODTask;
    public enum ConsumerType {
        Kobold,
        PhysicsItem,
    }
    // Start is called before the first frame update
    public ConsumerType resource;
    void Start() {
        LODManager.instance.RegisterConsumer(this, resource);
    }
    private void OnDestroy() {
        LODManager.instance.UnregisterConsumer(this, resource);
    }

    public void RaiseBodyQuality(Rigidbody r) {
        if (!r.isKinematic) {
            r.interpolation = RigidbodyInterpolation.Interpolate;
            r.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
    }
    public void LowerBodyQuality(Rigidbody r) {
        r.interpolation = RigidbodyInterpolation.None;
        r.collisionDetectionMode = CollisionDetectionMode.Discrete;
    }
    public IEnumerator UpdateLODWhenPossible() {
        yield return new WaitUntil(() => CanLOD());
        if (isVeryFar) {
            OnLODImposter.Invoke();
        } else {
            OnLODUnImposter.Invoke();
        }
        if (isClose) {
            OnLODClose.Invoke();
        } else {
            OnLODFar.Invoke();
        }
    }

    public bool CanLOD() {
        foreach(var condition in canLODCondition) {
            if (!condition.Invoke()) {
                return false;
            }
        }
        return true;
    }

    public void SetVeryFar(bool veryFar) {
        if (!CanLOD()) {
            isVeryFar = veryFar;
            if (LODTask != null && !LODTask.Running || LODTask == null) {
                LODTask = new Task(UpdateLODWhenPossible());
            }
            return;
        }
        if (isVeryFar != veryFar) {
            if (veryFar) {
                OnLODImposter.Invoke();
            } else {
                OnLODUnImposter.Invoke();
            }
            isVeryFar = veryFar;
        }
    }
    public void SetClose(bool close) {
        if (!CanLOD()) {
            isClose = close;
            if (LODTask != null && !LODTask.Running || LODTask == null) {
                LODTask = new Task(UpdateLODWhenPossible());
            }
            return;
        }
        if (isClose != close) {
            if (close) {
                foreach(var body in trackedRigidbodies) {
                    RaiseBodyQuality(body);
                }
                OnLODClose.Invoke();
            } else {
                foreach(var body in trackedRigidbodies) {
                    LowerBodyQuality(body);
                }
                OnLODFar.Invoke();
            }
        }
        isClose = close;
    }
}
