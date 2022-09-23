using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsScroller : MonoBehaviour {
    [SerializeField]
    private float speed;
    [SerializeField]
    private float height;
    private float startTime = 0f;
    private void OnEnable() {
        startTime = Time.unscaledTime;
    }

    void Update() {
        float currentPosition = Mathf.Repeat((Time.unscaledTime-startTime) * speed, height+100f)-100f;
        transform.position = transform.position.With(y:currentPosition);
    }
}
