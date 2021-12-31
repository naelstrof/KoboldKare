using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour{
    public enum AIType{ Generic };
    public AIType myAI;
    public enum AIStates{ Sleep, Idle, Hunt, Flee }
    public AIStates curAIState;
}
