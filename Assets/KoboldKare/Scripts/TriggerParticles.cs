using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerParticles : MonoBehaviour
{
    public ParticleSystem rightParticles;
    public ParticleSystem leftParticles;
    public void SpawnRightFoot() {
        rightParticles.Emit(30);
    }
    public void SpawnLeftFoot() {
        leftParticles.Emit(30);
    }
}
