using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Squash2 : MonoBehaviour
{
    public Transform objectTransform;
    public Vector3 Position;
    public Vector3 Rotation;
    private Quaternion RotationQuaternion;
    public Vector3 Squashing;
    private Quaternion originalRot;
    private Vector3 originalPos;
    private Vector3 originalScale;
    private bool squashed_ = false;
    private bool initialized = false;
    public bool Squashed {
        get {
            return squashed_;
        }
    }
    public void Squash(){
        if(squashed_){
            objectTransform.localRotation = originalRot;
            objectTransform.localPosition = originalPos;
            objectTransform.localScale = originalScale;
        }else{
            objectTransform.localRotation = RotationQuaternion;
            objectTransform.localPosition = Position;
            objectTransform.localScale = Squashing;
        }
        squashed_ = !squashed_;
    }
    // Start is called before the first frame update
    void Start()
    {
        RotationQuaternion = Quaternion.Euler(Rotation);
        originalRot = objectTransform.localRotation;
        originalPos = objectTransform.localPosition;
        originalScale = objectTransform.localScale;
        initialized = true;
    }

    // // Update is called once per frame
    // void Update()
    // {
        
    // }
}
