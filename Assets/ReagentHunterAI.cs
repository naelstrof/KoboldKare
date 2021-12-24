using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldKare;

[RequireComponent(typeof(GenericReagentContainer)), RequireComponent(typeof(KoboldSeeker))]
public class ReagentHunterAI : Enemy{
    private GenericReagentContainer GRC;
    public float targetFluidsDesired;
    public GameEventGeneric midnightEvent, nightEvent;
    public KoboldSeeker trackingAI;
}
