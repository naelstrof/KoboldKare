using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GenericAdvancedInteractor : MonoBehaviour, IAdvancedInteractable {
    [System.Serializable]
    public class AdvancedInteractionCallback : SerializableCallback<bool> { }
    public bool grabbed { get; set; } = false;
    public bool physicsGrabbable;
    public List<ConditionEventPair> onInteract;
    public List<ConditionEventPair> onEndInteract;
    [System.Serializable]
    public class KoboldInteractEvent : UnityEvent<Kobold> {}
    [System.Serializable]
    public class ConditionEventPair {
        [SerializeField]
        public List<AdvancedInteractionCallback> conditions;
        public KoboldInteractEvent onGrabRelease;
    }

    public void InteractTo(Vector3 worldPosition, Quaternion worldRotation) {
    }

    public void OnEndInteract(Kobold k) {
        if (!isActiveAndEnabled) {
            return;
        }
        grabbed = false;
        foreach (var pair in onEndInteract) {
            bool passConditions = true;
            foreach (var condition in pair.conditions) {
                passConditions &= condition.Invoke();
            }
            if (passConditions) {
                pair.onGrabRelease.Invoke(k);
                return;
            }
        }
    }

    public bool PhysicsGrabbable()
    {
        return physicsGrabbable;
    }

    void IAdvancedInteractable.OnInteract(Kobold k) {
        if (!isActiveAndEnabled) {
            return;
        }
        grabbed = true;
        foreach (var pair in onInteract) {
            bool passConditions = true;
            foreach (var condition in pair.conditions) {
                passConditions &= condition.Invoke();
            }
            if (passConditions) {
                pair.onGrabRelease.Invoke(k);
                return;
            }
        }
    }
}
