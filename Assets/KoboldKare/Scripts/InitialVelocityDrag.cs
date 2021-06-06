using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(InitialVelocityDrag))]
public class InitialVelocityDragEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
    }
    public void OnSceneGUI(){
        InitialVelocityDrag t = (InitialVelocityDrag)target;
        Vector3 targetP = t.targetPoint;
        t.targetPoint = Handles.PositionHandle(targetP, Quaternion.identity);
        if (targetP != t.targetPoint) {
            EditorUtility.SetDirty(target);
        }
    }
}
#endif

[RequireComponent(typeof(Rigidbody))]
public class InitialVelocityDrag : MonoBehaviour
{
    public Vector3 targetPoint;
    public float travelTime = 10f;
    void Start() {
        Rigidbody body = GetComponent<Rigidbody>();
        Vector3 terminalVelocity = GetTerminalVelocity(Physics.gravity, body);
        float k = (Physics.gravity.magnitude) / (2f*terminalVelocity.magnitude);
        Vector3 windVelocity = Vector3.zero;
        Vector3 vSubInf = terminalVelocity + windVelocity;
        Vector3 initialVelocity = k * (targetPoint - transform.position - vSubInf * travelTime);
        initialVelocity += (targetPoint - transform.position) / travelTime;
        body.velocity = initialVelocity;
        StartCoroutine(WaitAndThenShow(travelTime));
    }
    //public void FixedUpdate() {
        //Rigidbody body = GetComponent<Rigidbody>();
        //Vector3 terminalVelocity = GetTerminalVelocity(Physics.gravity, body);
        //Debug.Log(body.velocity + " " + terminalVelocity);
    //}
    public IEnumerator WaitAndThenShow(float duration) {
        yield return new WaitForSeconds(duration);
        Debug.DrawLine(transform.position, transform.position + Vector3.up*10f, Color.blue, 10f);
        Start();
    }

    public static Vector3 GetTerminalVelocity(Vector3 gravity, Rigidbody body) {
        return ((gravity / body.drag) - Time.fixedDeltaTime * gravity) / body.mass;
    }
}
