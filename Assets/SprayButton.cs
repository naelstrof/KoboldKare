using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SprayButton : GenericUsable{
    public FluidOutputMozzarellaSquirt squirter;
    public AudioSource aud;
    public void Fire(){
        if(squirter.isFiring)
            squirter.StopFiring();
        else
            squirter.Fire();
        if(aud.isPlaying) aud.Stop();
        else aud.Play();
    }
}
