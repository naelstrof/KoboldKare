using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecalLifetime : MonoBehaviour
{
    public float lifetime = 60f;
    private float dieTime;

    void Start() {
        dieTime = Time.timeSinceLevelLoad + lifetime;
    }

    // Update is called once per frame
    void Update() {
        if (Time.timeSinceLevelLoad > dieTime) {
            Destroy(gameObject);
        }
    }
}
