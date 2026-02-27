using System;
using UnityEngine;

public class GravityManipulator : MonoBehaviour
{
    [Range(-200f, 0f)]
    public float Gravity = -9.81f;
    private void Start()
    {

        Physics.gravity = new(0f, Gravity, 0f);
        Physics.clothGravity = new(0f, Gravity, 0f);
    }
}
