using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameEventResponseConditionalCountEnabled : GameEventResponse
{
    [SerializeField] public GameObject[] trackedObjects;
    [SerializeField, MinAttribute(0)] public int Requirement;
    [SerializeField] public bool trackDisabledInstead;
    [SerializeField, SubclassSelector, SerializeReference] private List<GameEventResponse> requirementMet = new List<GameEventResponse>();
    [SerializeField, SubclassSelector, SerializeReference] private List<GameEventResponse> requirementFailed = new List<GameEventResponse>();
    private int enabledCount = 0;


    public override void Invoke(MonoBehaviour owner)
    {
        enabledCount = 0;
        foreach (var target in trackedObjects)
        {
            if (target.activeSelf)
            {
                enabledCount++;
            }
        }


        if (!trackDisabledInstead)
        {
            if (enabledCount >= Requirement)
            {
                foreach (var response in requirementMet)
                {
                    response?.Invoke(owner);
                }
            } else
            {
               foreach (var response in requirementFailed)
                {
                    response?.Invoke(owner);
                } 
            }
        } else
        {
            if (enabledCount <= Requirement)
            {
                foreach (var response in requirementMet)
                {
                    response?.Invoke(owner);
                }
            } else
            {
               foreach (var response in requirementFailed)
                {
                    response?.Invoke(owner);
                } 
            }
        }
    }
}
