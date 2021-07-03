using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape : MonoBehaviour
{
    [Range(0f,10f)]
    public float radius;

    public enum ShapeType {Sphere,Cube,Torus,Cylinder,Capsule};
    public enum Operation {None, Blend, Cut,Mask}

    public ShapeType shapeType;
    public Operation operation;
    public Color colour = Color.white;
    [Range(0,1)]
    public float blendStrength;
    [HideInInspector]
    public int numChildren;
    public Vector3 localCapsuleOffset;

    public Vector3 Position {
        get {
            return transform.position;
        }
    }

    public Vector3 Scale {
        get {
            if (shapeType == ShapeType.Capsule) {
                return transform.TransformPoint(localCapsuleOffset);
            }
            return transform.lossyScale;
        }
    }
}
