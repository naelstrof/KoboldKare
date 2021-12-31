using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PachinkoExplodeOnTouch : MonoBehaviour{
    void OnCollisionStay(Collision collisionInfo){
        if(collisionInfo.gameObject.CompareTag("Bullet")){
            collisionInfo.rigidbody.AddExplosionForce(1000,transform.position,5,10,ForceMode.Impulse);
        }
    }
}
