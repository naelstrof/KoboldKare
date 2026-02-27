using System;
using UnityEngine;

[Serializable]
public class GameEventResponseGravityChange : GameEventResponse
{
    public Vector3 Gravity = new(0f,-9.81f,0f);


    public override void Invoke(MonoBehaviour owner)
    {
        Physics.gravity = Gravity;
        Physics.clothGravity = Gravity;
    }
}