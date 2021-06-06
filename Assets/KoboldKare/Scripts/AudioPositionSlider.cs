using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPositionSlider : MonoBehaviour {
    public List<Transform> points = new List<Transform>();
    private AudioListener internalListener;
    private AudioListener listener {
        get {
            if (internalListener == null || !internalListener.isActiveAndEnabled) {
                foreach (AudioListener l in GameObject.FindObjectsOfType<AudioListener>()) {
                    if (l.isActiveAndEnabled) {
                        internalListener = l;
                    }
                }
            }
            return internalListener;
        }
    }
    private Vector3 nearestPoint;
    void OnDrawGizmosSelected() {
        if (points.Count < 2 ) {
            return;
        }
        Transform lastPoint = points[0];
        for(int i=1;i<points.Count;i++) {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(lastPoint.position, points[i].position);
            lastPoint = points[i];
        }
        Gizmos.DrawCube(transform.position,Vector3.one);
    }
    void Update() {
        if (points.Count < 2 || listener == null ) {
            return;
        }
        float minDist = float.MaxValue;
        Transform lastPoint = points[0];
        nearestPoint = lastPoint.position;
        for(int i=1;i<points.Count;i++) {
            Vector3 normal = Vector3.Normalize(points[i].position - lastPoint.position);
            float dot = Vector3.Dot(Vector3.Normalize(listener.transform.position-lastPoint.position), normal);
            float otherdot = Vector3.Dot(Vector3.Normalize(listener.transform.position-points[i].position), -normal);
            Vector3 projected = Vector3.Project(listener.transform.position-lastPoint.position, normal)+lastPoint.position;
            Vector3 desiredPoint = Vector3.Lerp(lastPoint.position, projected, Mathf.Ceil(Mathf.Clamp01(dot)));
            desiredPoint = Vector3.Lerp(points[i].position, desiredPoint, Mathf.Ceil(Mathf.Clamp01(otherdot)));
            float dist = Vector3.Distance(listener.transform.position, desiredPoint);
            if (dist < minDist) {
                minDist = dist;
                nearestPoint = desiredPoint;
            }
            lastPoint = points[i];
        }
        transform.position = Vector3.Lerp(transform.position, nearestPoint, Time.deltaTime*8f);
    }
}
