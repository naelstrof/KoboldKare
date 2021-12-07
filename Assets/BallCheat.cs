using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallCheat : MonoBehaviour{
    Rigidbody rb;
    Pachinko pachinko;
    public float sensitivity;

    void Awake(){
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collisionInfo){
        pachinko.HitPin();
    }

    public void SetMachine(Pachinko Pachinko){
        pachinko = Pachinko;
    }

    void FixedUpdate(){
        if(rb.velocity.magnitude <= sensitivity){
            pachinko.BallStuck();
        }
    }
}
