using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityScriptableSettings;

public class Follower : MonoBehaviour {
    public Transform target;
    public float distance = 0.12f;
    private Kobold kobold;
    [SerializeField]
    private SettingInt motionSicknessReducer;
    private Vector3 startingPosition;
    bool ragdoll;
    
    void Awake() {
        kobold = GetComponentInParent<Kobold>();
        startingPosition = transform.localPosition;
        transform.localPosition = transform.parent.InverseTransformPoint(target.position);
        motionSicknessReducer.changed += OnMotionSicknessReducerChanged;
        kobold.ragdoller.RagdollEvent += RagdollEvent;
        OnMotionSicknessReducerChanged(motionSicknessReducer.GetValue());
    }
    void OnDestroy() {
        if (kobold !=null) {
            kobold.ragdoller.RagdollEvent -= RagdollEvent;
        }
        motionSicknessReducer.changed -= OnMotionSicknessReducerChanged;
    }

    void OnMotionSicknessReducerChanged(int value) {
        enabled = value == 0;
        transform.localPosition = startingPosition;
    }

    void LateUpdate() {
        transform.position -= transform.up*distance;
        Vector3 a = transform.localPosition;
        Vector3 b = transform.parent.InverseTransformPoint(target.position);
        if (ragdoll) {
            transform.localPosition = b;
        } else {
            transform.localPosition = Vector3.MoveTowards(a, b, Time.deltaTime*5f);
        }
        transform.position += transform.up*distance;
    }

    public void RagdollEvent(bool ragdolled) {
        ragdoll = ragdolled;
        transform.localPosition = transform.parent.InverseTransformPoint(target.position);
        if (ragdolled && !enabled) {
            enabled = true;
        } else if (!ragdolled && motionSicknessReducer.GetValue() == 1) {
            transform.localPosition = startingPosition;
            enabled = false;
        }
    }
}
