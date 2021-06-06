using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KoboldPickupable : MonoBehaviour
{
    public Animator KoboldAnimator;
    private float carried = 0;
    private float pickedUp = 0;
    private float transSpeed = 1.0f;

    public void OnGrab() {
        pickedUp = 1;
        transSpeed = 5.0f;
    }
    public void OnRelease() {
        pickedUp = 0;
        transSpeed = 1f;
    }

    void Update() {
        carried = Mathf.MoveTowards(carried, pickedUp, Time.deltaTime*transSpeed);
        KoboldAnimator.SetFloat("Carried", carried);
    }
}
