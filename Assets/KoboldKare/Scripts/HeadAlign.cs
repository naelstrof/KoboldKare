using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadAlign : MonoBehaviour
{
    public Transform _head;
    public Transform _headTarget;
    public Vector3 _offset;
    private float length;
    private void Start() {
        length = _offset.magnitude;
    }
    void FixedUpdate() {
        Vector3 temp = _offset;
        if ( _headTarget.forward.y > 0 ) {
            temp.y *= -1f;
        }
        Vector3 target = _headTarget.position + Vector3.Normalize(_headTarget.TransformDirection(temp).GroundVector()) * length;
        Vector3 diff = (target - _head.position).GroundVector();
        transform.position += diff;
    }
}
