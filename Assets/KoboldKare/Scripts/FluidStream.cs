using System;
using System.Collections;
using System.Collections.Generic;
using PenetrationTech;
using UnityEngine;

[ExecuteAlways]
public class FluidStream : CatmullDeformer {
    private ReagentContents contents;
    private ReagentContents midairContents;
    private float startClip, endClip = 0f;
    private Vector3[] points;
    private const float velocity = 6f;

    void Start() {
        points = new Vector3[8];
    }

    private void Update() {
        for (int i = 0; i < points.Length; i++) {
            float t = ((float)i / (float)points.Length)*3f;
            Vector3 pos = rootBone.position + rootBone.TransformDirection(localRootForward) * (velocity * t) + Physics.gravity * (0.5f * t * t);
            points[i] = pos;
        }
        path.SetWeightsFromPoints(points);
        if (Application.isPlaying) {
            rootBone.transform.localRotation *= Quaternion.AngleAxis(-Time.deltaTime * 100f, localRootForward);
        }
    }

    private IEnumerator Output() {
        startClip = endClip = 0f;
        while (startClip != 1f && endClip != 1f) {
            endClip = Mathf.MoveTowards(endClip, 1f, Time.deltaTime);
            if (contents.volume > 0f) {
                midairContents.AddMix(contents.Spill(Time.deltaTime * 5f));
                startClip = 0f;
            } else {
                startClip = Mathf.MoveTowards(endClip, 1f, Time.deltaTime);
            }
            yield return null;
        }
    }
}
