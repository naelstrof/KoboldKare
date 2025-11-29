using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameEventResponseConditionalRandom : GameEventResponse
{
    public int Min;
    public int Max;
    public int MininumRequirement;
    [SerializeField, SubclassSelector, SerializeReference] private List<GameEventResponse> requirementMet = new List<GameEventResponse>();
    private int Roll;

    public override void Invoke(MonoBehaviour owner)
    {
        Roll = UnityEngine.Random.Range(Min, Max);
        if (Roll >= MininumRequirement)
        {
            foreach (var response in requirementMet)
            {
                response?.Invoke(owner);
            }
        }
    }
}
