using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class RaymarchShape : MonoBehaviour
{
    [Range(0f,10f)]
    public float radius;

    public enum ShapeType {Sphere,Cube,Torus,Cylinder,Capsule};
    public enum Operation {None, Blend, Cut,Mask}

    public ShapeType shapeType;
    public Operation operation;
    public Color colour = Color.white;
    [Range(0,2)]
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
    public void OnEnable() {
        RaymarchScene.AddShape(this);
    }
    public void OnDisable() {
        RaymarchScene.RemoveShape(this);
    }
}
