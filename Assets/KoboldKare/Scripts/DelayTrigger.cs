using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DelayTrigger : MonoBehaviour {
    public UnityEvent onTrigger;
    public float waitTime = 1f;
    public float waitVariance = 1f;
    private float timer = 0f;
    void Start() {
        waitTime += UnityEngine.Random.Range(-waitVariance, waitVariance);
    }
    void FixedUpdate() {
        timer += Time.fixedDeltaTime;
        if ( timer > waitTime ) {
            onTrigger.Invoke();
            Destroy(this);
        }
    }
}
