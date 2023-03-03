using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitCameraPivotBase : MonoBehaviour {
    public virtual Vector3 GetPivotPosition(Quaternion camRotation) => transform.position;
    public virtual Vector2 GetScreenOffset(Quaternion camRotation) => Vector2.one*0.5f;
    public virtual float GetDistanceFromPivot(Quaternion camRotation) => 1f;
    public virtual float GetFOV(Quaternion camRotation) => 1f;
    public virtual Quaternion GetRotation(Quaternion camRotation) => camRotation;
    public virtual Quaternion GetPostRotationOffset(Quaternion camRotation) => Quaternion.identity;
    public virtual bool GetClampPitch() => true;
    public virtual bool GetClampYaw() => false;
}
