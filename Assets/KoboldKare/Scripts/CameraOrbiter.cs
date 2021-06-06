using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrbiter : MonoBehaviour
{
    public float _distance;
    public Vector3 _offset;
    public float rotspeed = 0.1f;
    private Vector3 _origin;
    // Start is called before the first frame update
    void Start()
    {
        _origin = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = _origin + new Vector3(Mathf.Sin(Time.time*rotspeed), 0, Mathf.Cos(Time.time*rotspeed)) * _distance + _offset;
        transform.rotation = Quaternion.LookRotation(_origin - transform.position);
    }
}
