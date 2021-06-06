using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomizeRotation : MonoBehaviour
{
    void Start()
    {
        transform.rotation *= Quaternion.AngleAxis(Random.Range(0, 360), transform.up);
    }
}
