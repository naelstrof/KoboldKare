using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantRotate : MonoBehaviour{
    public float rotateAmount;
    public float rotateSteps;
    public bool rotate = true;

    public void Start()    {
        StartCoroutine(rotation());
    }

    public IEnumerator rotation()    {
        while (rotate)        {
            yield return new WaitForSeconds(rotateSteps);
            transform.Rotate(0f, 0f, transform.rotation.z + rotateAmount);
        }
    }
}
