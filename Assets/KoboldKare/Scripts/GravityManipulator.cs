using System;
using UnityEngine;

public class GravityManipulator : MonoBehaviour
{
    public Vector3 Gravity = new(0f,-9.81f,0f);

    private void Start()
    {
        Physics.gravity = Gravity;
        Physics.clothGravity = Gravity;
    }
}