using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cloud : PooledItem {
    private Bounds bounds;
    private float speed;

    public void SetBounds(Bounds newBounds) {
        bounds = newBounds;
    }

    void Start() {
        speed = UnityEngine.Random.Range(2f, 8f);
        transform.localScale = Vector3.one * UnityEngine.Random.Range(25f, 40f);
        transform.rotation = UnityEngine.Random.rotation;
    }

    public override void Reset() {
        speed = UnityEngine.Random.Range(2f, 8f);
        transform.position = new Vector3(
            bounds.center.x+bounds.extents.x,
            UnityEngine.Random.Range(bounds.center.y-bounds.extents.y, bounds.center.y+bounds.extents.y),
            UnityEngine.Random.Range(bounds.center.z-bounds.extents.z, bounds.center.z+bounds.extents.z));
        base.Reset();
    }

    private void Update() {
        transform.position -= Vector3.right * (Time.deltaTime*speed);
        transform.rotation = Quaternion.AngleAxis(-Time.deltaTime * speed, Vector3.forward) * transform.rotation;
        if (transform.position.x < bounds.center.x-bounds.extents.x) {
            Reset();
        }
    }
}
